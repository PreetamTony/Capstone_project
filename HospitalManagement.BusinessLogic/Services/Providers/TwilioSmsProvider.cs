using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace HospitalManagement.BusinessLogic.Services.Providers;

public class TwilioSmsProvider : ISmsProvider
{
    private readonly TwilioSettings _settings;
    private readonly ILogger<TwilioSmsProvider> _logger;

    public TwilioSmsProvider(IOptions<TwilioSettings> options, ILogger<TwilioSmsProvider> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendSmsAsync(string toPhoneNumber, string message)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_settings.AccountSid) || string.IsNullOrWhiteSpace(_settings.AuthToken))
            {
                _logger.LogWarning("Twilio settings are not configured. SMS not sent.");
                return;
            }

            // Ensure E.164 format (Add +91 country code if missing)
            if (!string.IsNullOrWhiteSpace(toPhoneNumber) && !toPhoneNumber.StartsWith("+"))
            {
                toPhoneNumber = "+91" + toPhoneNumber;
            }

            TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);

            var messageOptions = new CreateMessageOptions(new PhoneNumber(toPhoneNumber))
            {
                From = new PhoneNumber(_settings.FromPhoneNumber),
                Body = message
            };

            var messageResource = await MessageResource.CreateAsync(messageOptions);
            _logger.LogInformation("Sent SMS via Twilio to {To}. Status: {Status}, SID: {Sid}", toPhoneNumber, messageResource.Status, messageResource.Sid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {To}", toPhoneNumber);
        }
    }
}
