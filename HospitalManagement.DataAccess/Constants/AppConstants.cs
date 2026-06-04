namespace HospitalManagement.DataAccess.Constants;

/// <summary>
/// Central location for all string constants used across the application.
/// Avoids magic strings and enables refactoring without regressions.
/// </summary>
public static class AppConstants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Doctor = "Doctor";
        public const string Patient = "Patient";
        public const string Receptionist = "Receptionist";
        public const string LabTechnician = "LabTechnician";
        public const string Pharmacist = "Pharmacist";
    }

    public static class Policies
    {
        public const string AdminOnly = "AdminOnly";
        public const string DoctorOrAdmin = "DoctorOrAdmin";
        public const string PatientOrAdmin = "PatientOrAdmin";
        public const string MedicalStaff = "MedicalStaff";
        public const string ViewAuditLogs = "ViewAuditLogs";
    }

    public static class Permissions
    {
        public const string CanManageUsers = "CanManageUsers";
        public const string CanViewBilling = "CanViewBilling";
        public const string CanCreatePrescription = "CanCreatePrescription";
        public const string CanUploadReports = "CanUploadReports";
        public const string CanManageAppointments = "CanManageAppointments";
        public const string CanManageInventory = "CanManageInventory";
        public const string CanAdmitPatients = "CanAdmitPatients";
        public const string CanViewSystemSettings = "CanViewSystemSettings";
        public const string CanViewReports = "CanViewReports";
    }

    public static class Appointment
    {
        public const int WorkdayStartHour = 9;
        public const int WorkdayEndHour = 18;
        public const int CancellationNoticeHours = 4;
        public const int NoShowMinutes = 30;
        public const int DefaultSlotDurationMinutes = 30;
    }

    public static class Prescription
    {
        public const int EditWindowMinutes = 30;
    }

    public static class File
    {
        public const string LabReportUploadPath = "wwwroot/uploads/labreports";
        public const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20 MB
        public static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".dcm" };
    }

    public static class Jwt
    {
        public const string ClaimUserId = "uid";
        public const string ClaimRole = "role";
        public const string ClaimEmail = "email";
        public const string ClaimPermission = "permission";
    }

    public static class Audit
    {
        public const string Create = "CREATE";
        public const string Update = "UPDATE";
        public const string Delete = "DELETE";
    }
}
