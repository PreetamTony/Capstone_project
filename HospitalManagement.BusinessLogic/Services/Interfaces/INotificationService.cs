using HospitalManagement.BusinessLogic.DTOs.Common;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface INotificationService
{
    // Channels
    Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default);
    Task SendSmsAsync(string to, string message, CancellationToken ct = default);
    Task SendPushNotificationAsync(Guid userId, string title, string message, CancellationToken ct = default);

    // Business Logic Endpoints
    Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, CancellationToken ct = default);
    Task<List<NotificationDto>> GetUnreadNotificationsAsync(Guid userId, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    
    Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
    Task DeleteNotificationAsync(Guid notificationId, Guid userId, CancellationToken ct = default);

    // Creators
    Task CreateNotificationAsync(CreateNotificationRequestDto request, CancellationToken ct = default);
    Task BroadcastNotificationAsync(BroadcastNotificationRequestDto request, CancellationToken ct = default);

    // Event Triggers
    Task NotifyAppointmentBookedAsync(Guid patientId, Guid appointmentId, CancellationToken ct = default);
    Task NotifyAppointmentCancelledAsync(Guid patientId, Guid appointmentId, string reason, CancellationToken ct = default);
    Task NotifyReportUploadedAsync(Guid patientId, Guid reportId, CancellationToken ct = default);

    Task NotifyAppointmentConfirmedAsync(Guid patientId, Guid appointmentId, CancellationToken ct = default);
    Task NotifyQueueCalledAsync(Guid patientId, int tokenNumber, string doctorName, CancellationToken ct = default);
    Task NotifyConsultationStartedAsync(Guid patientId, Guid consultationId, CancellationToken ct = default);
    Task NotifyConsultationCompletedAsync(Guid patientId, Guid consultationId, CancellationToken ct = default);
    Task NotifyPrescriptionCreatedAsync(Guid patientId, Guid prescriptionId, CancellationToken ct = default);
    Task NotifyInvoiceGeneratedAsync(Guid patientId, Guid invoiceId, decimal amount, CancellationToken ct = default);
    Task NotifyPaymentReceivedAsync(Guid patientId, Guid invoiceId, decimal amount, CancellationToken ct = default);
    Task NotifyInsuranceApprovedAsync(Guid patientId, Guid invoiceId, decimal amount, CancellationToken ct = default);
    Task NotifyRefundIssuedAsync(Guid patientId, Guid invoiceId, decimal amount, CancellationToken ct = default);
    
    // User Account Triggers
    Task NotifyUserCreatedAsync(Guid userId, string tempPassword, CancellationToken ct = default);
    Task NotifyPasswordResetAsync(Guid userId, string newPassword, CancellationToken ct = default);
    Task SendPasswordResetTokenAsync(string email, string token, CancellationToken ct = default);
}
