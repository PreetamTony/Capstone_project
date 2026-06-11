namespace HospitalManagement.DataAccess.Models.Enums;

public enum NotificationType
{
    AppointmentBooked,
    AppointmentReminder,
    AppointmentCancelled,
    AppointmentRescheduled,
    QueueCalled,
    QueueDelayed,
    ConsultationStarted,
    ConsultationCompleted,
    PrescriptionCreated,
    LabReportReady,
    InvoiceGenerated,
    PaymentReceived,
    InsuranceApproved,
    InsuranceRejected,
    RefundIssued,
    System
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Critical
}

public enum NotificationChannel
{
    InApp,
    Email,
    SMS,
    Push
}
