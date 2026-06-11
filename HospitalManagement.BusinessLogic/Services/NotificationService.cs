using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Repositories;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.BusinessLogic.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using HospitalManagement.BusinessLogic.Services.Providers;

namespace HospitalManagement.BusinessLogic.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IUnitOfWork _uow;
    private readonly SmtpSettings _smtpSettings;
    private readonly ISmsProvider _smsProvider;

    public NotificationService(ILogger<NotificationService> logger, IUnitOfWork uow, IOptions<SmtpSettings> smtpSettings, ISmsProvider smsProvider)
    {
        _logger = logger;
        _uow = uow;
        _smtpSettings = smtpSettings.Value;
        _smsProvider = smsProvider;
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_smtpSettings.Server))
        {
            _logger.LogWarning("SMTP Server is not configured. Email to {To} was skipped.", to);
            return;
        }

        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body, TextBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls, ct);
            await smtp.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password, ct);
            await smtp.SendAsync(email, ct);
            await smtp.DisconnectAsync(true, ct);

            _logger.LogInformation("Successfully sent email to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            // Don't throw, we don't want a notification failure to break the main transaction flow
        }
    }

    public Task SendSmsAsync(string to, string message, CancellationToken ct = default)
    {
        // Stub implementation
        _logger.LogInformation("Sending SMS to {To}. Message: {Message}", to, message);
        return Task.CompletedTask;
    }

    public Task SendPushNotificationAsync(Guid userId, string title, string message, CancellationToken ct = default)
    {
        // Stub implementation
        _logger.LogInformation("Sending PUSH to User {UserId}. Title: {Title}. Message: {Message}", userId, title, message);
        return Task.CompletedTask;
    }

    public async Task CreateNotificationAsync(CreateNotificationRequestDto request, CancellationToken ct = default)
    {
        var notification = new Notification
        {
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            Type = Enum.TryParse<NotificationType>(request.Type, true, out var type) ? type : NotificationType.System,
            Priority = Enum.TryParse<NotificationPriority>(request.Priority, true, out var priority) ? priority : NotificationPriority.Normal,
            Channel = Enum.TryParse<NotificationChannel>(request.Channel, true, out var channel) ? channel : NotificationChannel.InApp,
            ReferenceId = request.ReferenceId,
            ReferenceType = request.ReferenceType,
            IsRead = false
        };

        await _uow.Notifications.AddAsync(notification, ct);
        await _uow.CompleteAsync(ct);
    }

    public async Task BroadcastNotificationAsync(BroadcastNotificationRequestDto request, CancellationToken ct = default)
    {
        var activeUsers = await _uow.Users.Query().Where(u => u.IsActive).ToListAsync(ct);
        
        var priority = Enum.TryParse<NotificationPriority>(request.Priority, true, out var p) ? p : NotificationPriority.High;
        
        var notifications = activeUsers.Select(u => new Notification
        {
            UserId = u.Id,
            Title = request.Title,
            Message = request.Message,
            Type = NotificationType.System,
            Priority = priority,
            Channel = NotificationChannel.InApp,
            IsRead = false
        }).ToList();

        // In a real system, you'd use AddRange and batch saving to handle thousands of records
        foreach (var n in notifications)
        {
            await _uow.Notifications.AddAsync(n, ct);
        }
        await _uow.CompleteAsync(ct);
        
        _logger.LogInformation("Broadcast notification sent to {Count} users.", activeUsers.Count);
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, CancellationToken ct = default)
    {
        var notifications = await _uow.Notifications.Query()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(100) 
            .ToListAsync(ct);

        return notifications.Select(MapToDto).ToList();
    }

    public async Task<List<NotificationDto>> GetUnreadNotificationsAsync(Guid userId, CancellationToken ct = default)
    {
        var notifications = await _uow.Notifications.Query()
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);

        return notifications.Select(MapToDto).ToList();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        return await _uow.Notifications.Query()
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default)
    {
        var notification = await _uow.Notifications.GetByIdAsync(notificationId, ct) 
            ?? throw new NotFoundException("Notification", notificationId);

        if (notification.UserId != userId)
            throw new BusinessRuleViolationException("Unauthorized", "Notification does not belong to the user.");

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        _uow.Notifications.Update(notification);
        await _uow.CompleteAsync(ct);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        var unread = await _uow.Notifications.Query()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        if (!unread.Any()) return;

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
            _uow.Notifications.Update(n);
        }
        await _uow.CompleteAsync(ct);
    }

    public async Task DeleteNotificationAsync(Guid notificationId, Guid userId, CancellationToken ct = default)
    {
        var notification = await _uow.Notifications.GetByIdAsync(notificationId, ct) 
            ?? throw new NotFoundException("Notification", notificationId);

        if (notification.UserId != userId)
            throw new BusinessRuleViolationException("Unauthorized", "Notification does not belong to the user.");

        _uow.Notifications.Delete(notification);
        await _uow.CompleteAsync(ct);
    }

    public async Task NotifyAppointmentBookedAsync(Guid patientId, Guid appointmentId, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.Query().Include(p => p.User).FirstOrDefaultAsync(p => p.Id == patientId, ct);
        if (patient != null && patient.User != null)
        {
            await _smsProvider.SendSmsAsync(patient.User.PhoneNumber, $"Your appointment {appointmentId} has been booked.");
            await SendEmailAsync(patient.User.Email, "Appointment Confirmed", $"Your appointment {appointmentId} is confirmed.", ct);
            await SendPushNotificationAsync(patient.UserId, "Appointment Confirmed", "Check your schedule for details.", ct);
            
            await CreateNotificationAsync(new CreateNotificationRequestDto
            {
                UserId = patient.UserId,
                Title = "Appointment Booked",
                Message = "Your appointment has been successfully booked.",
                Type = NotificationType.AppointmentBooked.ToString(),
                Priority = NotificationPriority.Normal.ToString(),
                ReferenceId = appointmentId,
                ReferenceType = "Appointment"
            }, ct);
        }
    }

    public async Task NotifyAppointmentCancelledAsync(Guid patientId, Guid appointmentId, string reason, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.Query().Include(p => p.User).FirstOrDefaultAsync(p => p.Id == patientId, ct);
        if (patient != null && patient.User != null)
        {
            await SendEmailAsync(patient.User.Email, "Appointment Cancelled", $"Your appointment {appointmentId} was cancelled. Reason: {reason}", ct);
            await SendPushNotificationAsync(patient.UserId, "Appointment Cancelled", "Your appointment has been cancelled.", ct);
            
            await CreateNotificationAsync(new CreateNotificationRequestDto
            {
                UserId = patient.UserId,
                Title = "Appointment Cancelled",
                Message = $"Your appointment was cancelled. Reason: {reason}",
                Type = NotificationType.AppointmentCancelled.ToString(),
                Priority = NotificationPriority.High.ToString(),
                ReferenceId = appointmentId,
                ReferenceType = "Appointment"
            }, ct);
        }
    }

    public async Task NotifyReportUploadedAsync(Guid patientId, Guid reportId, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.Query().Include(p => p.User).FirstOrDefaultAsync(p => p.Id == patientId, ct);
        if (patient != null && patient.User != null)
        {
            await SendEmailAsync(patient.User.Email, "Lab Report Available", $"Your lab report is now available.", ct);
            
            await CreateNotificationAsync(new CreateNotificationRequestDto
            {
                UserId = patient.UserId,
                Title = "Lab Report Ready",
                Message = "Your new lab report has been uploaded.",
                Type = NotificationType.LabReportReady.ToString(),
                Priority = NotificationPriority.Normal.ToString(),
                ReferenceId = reportId,
                ReferenceType = "LabReport"
            }, ct);
        }
    }

    public async Task NotifyAppointmentConfirmedAsync(Guid patientId, Guid appointmentId, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.Query().Include(p => p.User).FirstOrDefaultAsync(p => p.Id == patientId, ct);
        if (patient != null && patient.User != null)
        {
            await SendEmailAsync(patient.User.Email, "Appointment Confirmed", $"Your appointment {appointmentId} is confirmed.", ct);
            await SendPushNotificationAsync(patient.UserId, "Appointment Confirmed", "Check your schedule for details.", ct);
            
            await CreateNotificationAsync(new CreateNotificationRequestDto
            {
                UserId = patient.UserId,
                Title = "Appointment Confirmed",
                Message = "Your appointment has been confirmed.",
                Type = NotificationType.System.ToString(),
                Priority = NotificationPriority.Normal.ToString(),
                ReferenceId = appointmentId,
                ReferenceType = "Appointment"
            }, ct);
        }
    }

    public async Task NotifyQueueCalledAsync(Guid patientId, int tokenNumber, string doctorName, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.Query().Include(p => p.User).FirstOrDefaultAsync(p => p.Id == patientId, ct);
        if (patient != null && patient.User != null)
        {
            await _smsProvider.SendSmsAsync(patient.User.PhoneNumber, $"It is your turn (Token {tokenNumber}) to see {doctorName}.");
            await SendPushNotificationAsync(patient.UserId, "Token Called", $"It is your turn (Token {tokenNumber}) to see {doctorName}.", ct);
            
            await CreateNotificationAsync(new CreateNotificationRequestDto
            {
                UserId = patient.UserId,
                Title = "Token Called",
                Message = $"It is your turn. Please proceed to {doctorName}'s room. Token: {tokenNumber}.",
                Type = NotificationType.QueueCalled.ToString(),
                Priority = NotificationPriority.High.ToString(),
                ReferenceId = null,
                ReferenceType = "Queue"
            }, ct);
        }
    }

    public async Task NotifyConsultationStartedAsync(Guid patientId, Guid consultationId, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.Query().Include(p => p.User).FirstOrDefaultAsync(p => p.Id == patientId, ct);
        if (patient != null && patient.User != null)
        {
            await CreateNotificationAsync(new CreateNotificationRequestDto
            {
                UserId = patient.UserId,
                Title = "Consultation Started",
                Message = "Your consultation has started.",
                Type = NotificationType.ConsultationStarted.ToString(),
                Priority = NotificationPriority.Normal.ToString(),
                ReferenceId = consultationId,
                ReferenceType = "Consultation"
            }, ct);
        }
    }

    public async Task NotifyConsultationCompletedAsync(Guid patientId, Guid consultationId, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.Query().Include(p => p.User).FirstOrDefaultAsync(p => p.Id == patientId, ct);
        if (patient != null && patient.User != null)
        {
            await CreateNotificationAsync(new CreateNotificationRequestDto
            {
                UserId = patient.UserId,
                Title = "Consultation Completed",
                Message = "Your consultation is complete.",
                Type = NotificationType.ConsultationCompleted.ToString(),
                Priority = NotificationPriority.Normal.ToString(),
                ReferenceId = consultationId,
                ReferenceType = "Consultation"
            }, ct);
        }
    }

    public async Task NotifyPrescriptionCreatedAsync(Guid patientId, Guid prescriptionId, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.Query().Include(p => p.User).FirstOrDefaultAsync(p => p.Id == patientId, ct);
        if (patient != null && patient.User != null)
        {
            await SendEmailAsync(patient.User.Email, "Prescription Created", $"A new prescription has been generated for you.", ct);
            
            await CreateNotificationAsync(new CreateNotificationRequestDto
            {
                UserId = patient.UserId,
                Title = "Prescription Created",
                Message = "A new prescription has been generated.",
                Type = NotificationType.PrescriptionCreated.ToString(),
                Priority = NotificationPriority.Normal.ToString(),
                ReferenceId = prescriptionId,
                ReferenceType = "Prescription"
            }, ct);
        }
    }

    public async Task NotifyInvoiceGeneratedAsync(Guid patientId, Guid invoiceId, decimal amount, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.Query().Include(p => p.User).FirstOrDefaultAsync(p => p.Id == patientId, ct);
        if (patient != null && patient.User != null)
        {
            await SendEmailAsync(patient.User.Email, "Invoice Generated", $"A new invoice for {amount:C} has been generated.", ct);
            
            await CreateNotificationAsync(new CreateNotificationRequestDto
            {
                UserId = patient.UserId,
                Title = "Invoice Generated",
                Message = $"A new invoice for {amount:C} has been generated.",
                Type = NotificationType.InvoiceGenerated.ToString(),
                Priority = NotificationPriority.Normal.ToString(),
                ReferenceId = invoiceId,
                ReferenceType = "Invoice"
            }, ct);
        }
    }

    public async Task NotifyPaymentReceivedAsync(Guid patientId, Guid invoiceId, decimal amount, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.Query().Include(p => p.User).FirstOrDefaultAsync(p => p.Id == patientId, ct);
        if (patient != null && patient.User != null)
        {
            await SendEmailAsync(patient.User.Email, "Payment Received", $"We have received your payment of {amount:C}.", ct);
            
            await CreateNotificationAsync(new CreateNotificationRequestDto
            {
                UserId = patient.UserId,
                Title = "Payment Received",
                Message = $"We have received your payment of {amount:C}.",
                Type = NotificationType.PaymentReceived.ToString(),
                Priority = NotificationPriority.Normal.ToString(),
                ReferenceId = invoiceId,
                ReferenceType = "Invoice"
            }, ct);
        }
    }

    public async Task NotifyInsuranceApprovedAsync(Guid patientId, Guid invoiceId, decimal amount, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.Query().Include(p => p.User).FirstOrDefaultAsync(p => p.Id == patientId, ct);
        if (patient != null && patient.User != null)
        {
            await CreateNotificationAsync(new CreateNotificationRequestDto
            {
                UserId = patient.UserId,
                Title = "Insurance Approved",
                Message = $"Insurance claim approved for {amount:C}.",
                Type = NotificationType.InsuranceApproved.ToString(),
                Priority = NotificationPriority.Normal.ToString(),
                ReferenceId = invoiceId,
                ReferenceType = "Invoice"
            }, ct);
        }
    }

    public async Task NotifyRefundIssuedAsync(Guid patientId, Guid invoiceId, decimal amount, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.Query().Include(p => p.User).FirstOrDefaultAsync(p => p.Id == patientId, ct);
        if (patient != null && patient.User != null)
        {
            await SendEmailAsync(patient.User.Email, "Refund Issued", $"A refund of {amount:C} has been issued.", ct);
            
            await CreateNotificationAsync(new CreateNotificationRequestDto
            {
                UserId = patient.UserId,
                Title = "Refund Issued",
                Message = $"A refund of {amount:C} has been issued.",
                Type = NotificationType.RefundIssued.ToString(),
                Priority = NotificationPriority.Normal.ToString(),
                ReferenceId = invoiceId,
                ReferenceType = "Invoice"
            }, ct);
        }
    }

    public async Task NotifyUserCreatedAsync(Guid userId, string tempPassword, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct);
        if (user == null || string.IsNullOrEmpty(user.Email)) return;

        string subject = "Welcome to NexBus Hospital Management System";
        string body = $@"
            <h2>Welcome to the team!</h2>
            <p>Your new account has been successfully created by the administrator.</p>
            <p><strong>Username:</strong> {user.Email}</p>
            <p><strong>Temporary Password:</strong> {tempPassword}</p>
            <p>Please log in and change your password immediately.</p>
        ";
        await SendEmailAsync(user.Email, subject, body, ct);
    }

    public async Task NotifyPasswordResetAsync(Guid userId, string newPassword, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct);
        if (user == null || string.IsNullOrEmpty(user.Email)) return;

        string subject = "Your Password Has Been Reset";
        string body = $@"
            <h2>Password Reset</h2>
            <p>Your password has been securely reset by the administrator.</p>
            <p><strong>Temporary Password:</strong> {newPassword}</p>
            <p>Please log in and change your password immediately.</p>
        ";
        await SendEmailAsync(user.Email, subject, body, ct);
    }

    public async Task SendPasswordResetTokenAsync(string email, string token, CancellationToken ct = default)
    {
        string subject = "Your Password Reset Code";
        string body = $@"
            <h2>Password Reset Request</h2>
            <p>We received a request to reset your password. Use the code below to reset it:</p>
            <p><strong style=""font-size:24px; letter-spacing: 5px; padding: 10px; background-color: #f4f4f4; border-radius: 4px;"">{token}</strong></p>
            <p>This code will expire in 15 minutes.</p>
            <p>If you did not request this, please ignore this email.</p>
        ";
        await SendEmailAsync(email, subject, body, ct);
    }

    private NotificationDto MapToDto(Notification n)
    {
        return new NotificationDto
        {
            Id = n.Id,
            UserId = n.UserId,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type.ToString(),
            Priority = n.Priority.ToString(),
            Channel = n.Channel.ToString(),
            ReferenceId = n.ReferenceId,
            ReferenceType = n.ReferenceType,
            IsRead = n.IsRead,
            ReadAt = n.ReadAt,
            CreatedAt = n.CreatedAt
        };
    }
}
