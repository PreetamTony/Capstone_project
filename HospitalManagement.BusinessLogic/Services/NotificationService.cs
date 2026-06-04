using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Repositories;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.BusinessLogic.Services;

/// <summary>
/// A stub implementation of the Notification Service.
/// In a real-world scenario, this would integrate with SendGrid, Twilio, Firebase Cloud Messaging, etc.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IUnitOfWork _uow;

    public NotificationService(ILogger<NotificationService> logger, IUnitOfWork uow)
    {
        _logger = logger;
        _uow = uow;
    }

    public Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        _logger.LogInformation("Sending EMAIL to {To}. Subject: {Subject}. Body: {Body}", to, subject, body);
        return Task.CompletedTask;
    }

    public Task SendSmsAsync(string to, string message, CancellationToken ct = default)
    {
        _logger.LogInformation("Sending SMS to {To}. Message: {Message}", to, message);
        return Task.CompletedTask;
    }

    public Task SendPushNotificationAsync(Guid userId, string title, string message, CancellationToken ct = default)
    {
        _logger.LogInformation("Sending PUSH to User {UserId}. Title: {Title}. Message: {Message}", userId, title, message);
        return Task.CompletedTask;
    }

    public async Task NotifyAppointmentBookedAsync(Guid patientId, Guid appointmentId, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.GetByIdAsync(patientId, ct);
        if (patient != null && patient.User != null)
        {
            await SendEmailAsync(patient.User.Email, "Appointment Confirmed", $"Your appointment {appointmentId} is confirmed.", ct);
            await SendPushNotificationAsync(patient.UserId, "Appointment Confirmed", "Check your schedule for details.", ct);
        }
    }

    public async Task NotifyAppointmentCancelledAsync(Guid patientId, Guid appointmentId, string reason, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.GetByIdAsync(patientId, ct);
        if (patient != null && patient.User != null)
        {
            await SendEmailAsync(patient.User.Email, "Appointment Cancelled", $"Your appointment {appointmentId} was cancelled. Reason: {reason}", ct);
            await SendPushNotificationAsync(patient.UserId, "Appointment Cancelled", "Your appointment has been cancelled.", ct);
        }
    }

    public async Task NotifyReportUploadedAsync(Guid patientId, Guid reportId, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.GetByIdAsync(patientId, ct);
        if (patient != null && patient.User != null)
        {
            await SendEmailAsync(patient.User.Email, "Lab Report Available", $"Your lab report {reportId} is now available.", ct);
            await SendSmsAsync(patient.User.PhoneNumber ?? string.Empty, "Your lab report has been uploaded.", ct);
        }
    }

    public async Task CreateNotificationAsync(CreateNotificationRequestDto request, CancellationToken ct = default)
    {
        var notification = new Notification
        {
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            IsRead = false
        };

        await _uow.Notifications.AddAsync(notification, ct);
        await _uow.CompleteAsync(ct);
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, CancellationToken ct = default)
    {
        var notifications = await _uow.Notifications.Query()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50) // Limit to latest 50
            .ToListAsync(ct);

        return notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default)
    {
        var notification = await _uow.Notifications.GetByIdAsync(notificationId, ct) 
            ?? throw new NotFoundException("Notification", notificationId);

        if (notification.UserId != userId)
            throw new BusinessRuleViolationException("Unauthorized", "Notification does not belong to the user.");

        notification.IsRead = true;
        _uow.Notifications.Update(notification);
        await _uow.CompleteAsync(ct);
    }
}
