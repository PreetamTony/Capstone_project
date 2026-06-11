namespace HospitalManagement.BusinessLogic.Services.Providers;

public interface ISmsProvider
{
    Task SendSmsAsync(string toPhoneNumber, string message);
}
