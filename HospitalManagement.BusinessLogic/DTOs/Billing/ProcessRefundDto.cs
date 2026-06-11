namespace HospitalManagement.BusinessLogic.DTOs.Billing;

public class ProcessRefundDto
{
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}
