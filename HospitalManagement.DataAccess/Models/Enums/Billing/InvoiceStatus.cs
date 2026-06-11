namespace HospitalManagement.DataAccess.Models.Enums.Billing;

public enum InvoiceStatus
{
    Draft,
    Generated,
    Pending,
    PartiallyPaid,
    Paid,
    Cancelled,
    Refunded
}
