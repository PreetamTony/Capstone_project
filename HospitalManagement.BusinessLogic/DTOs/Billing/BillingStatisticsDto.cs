namespace HospitalManagement.BusinessLogic.DTOs.Billing;

public class BillingStatisticsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal InsurancePending { get; set; }
}
