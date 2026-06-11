namespace HospitalManagement.DataAccess.Models.Enums;

public enum AppointmentStatus
{
    Scheduled = 0,
    Confirmed = 1,
    CheckedIn = 2,
    InProgress = 3,
    Completed = 4,
    Cancelled = 5,
    NoShow = 6,
    PendingPayment = 7
}

public enum ConsultationStatus
{
    Draft = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

public enum AppointmentType
{
    InPerson = 0,
    Video = 1,
    Phone = 2
}

public enum AppointmentSource
{
    PatientPortal = 0,
    Receptionist = 1,
    Doctor = 2,
    Admin = 3,
    MobileApp = 4
}

public enum AppointmentPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Emergency = 3
}

public enum QueueStatus
{
    Waiting = 0,
    Called = 1,
    InConsultation = 2,
    Completed = 3,
    Skipped = 4,
    NoShow = 5
}

public enum LabReportStatus
{
    Ordered,
    SampleCollected,
    InProgress,
    Completed,
    Reviewed,
    Cancelled
}

public enum PrescriptionStatus
{
    Draft = 0,
    Active = 1,
    Dispensed = 2,
    PartiallyDispensed = 3,
    Cancelled = 4,
    Expired = 5
}

public enum BillingStatus
{
    Pending = 0,
    Paid = 1,
    Refunded = 2,
    WrittenOff = 3,
    RefundRequested = 4
}

public enum InsuranceClaimStatus
{
    Pending,
    Approved,
    Rejected,
    PartiallyApproved
}

public enum BillingCategory
{
    Consultation = 0,
    Lab = 1,
    Pharmacy = 2,
    Procedure = 3,
    Other = 4
}

public enum Gender
{
    Male = 0,
    Female = 1,
    Other = 2,
    PreferNotToSay = 3
}

public enum BloodGroup
{
    APositive = 0,
    ANegative = 1,
    BPositive = 2,
    BNegative = 3,
    ABPositive = 4,
    ABNegative = 5,
    OPositive = 6,
    ONegative = 7
}
