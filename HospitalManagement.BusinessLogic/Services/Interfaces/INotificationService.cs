using HospitalManagement.BusinessLogic.DTOs.Common;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface INotificationService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default);
    Task SendSmsAsync(string to, string message, CancellationToken ct = default);
    Task SendPushNotificationAsync(Guid userId, string title, string message, CancellationToken ct = default);
    
    // Domain specific events
    Task NotifyAppointmentBookedAsync(Guid patientId, Guid appointmentId, CancellationToken ct = default);
    Task NotifyAppointmentCancelledAsync(Guid patientId, Guid appointmentId, string reason, CancellationToken ct = default);
    Task NotifyReportUploadedAsync(Guid patientId, Guid reportId, CancellationToken ct = default);

    // In-App Notifications
    Task CreateNotificationAsync(CreateNotificationRequestDto request, CancellationToken ct = default);
    Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, CancellationToken ct = default);
    Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default);
}
