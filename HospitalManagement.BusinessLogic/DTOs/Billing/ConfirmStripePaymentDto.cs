namespace HospitalManagement.BusinessLogic.DTOs.Billing;

public class ConfirmStripePaymentDto
{
    public string PaymentIntentId { get; set; } = string.Empty;
}
