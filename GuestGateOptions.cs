using Microsoft.Extensions.Configuration;

namespace RoomRegistrationPdfApp;

public sealed class GuestGateOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string Kid { get; set; } = string.Empty;
    public string DefaultLanguage { get; set; } = "ar";
    public int SignatureWaitSeconds { get; set; } = 180;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(BaseUrl) &&
        !string.IsNullOrWhiteSpace(Kid);

    public static GuestGateOptions FromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection("GuestGate");
        var enabledText = section["Enabled"];
        var secondsText = section["SignatureWaitSeconds"];

        return new GuestGateOptions
        {
            Enabled = bool.TryParse(enabledText, out var enabled) && enabled,
            BaseUrl = section["BaseUrl"] ?? string.Empty,
            Kid = section["Kid"] ?? string.Empty,
            DefaultLanguage = string.IsNullOrWhiteSpace(section["DefaultLanguage"]) ? "ar" : section["DefaultLanguage"]!,
            SignatureWaitSeconds = int.TryParse(secondsText, out var seconds) ? seconds : 180
        };
    }
}
