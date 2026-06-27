using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace RoomRegistrationPdfApp;

public sealed class GuestGateConsentClient : IDisposable
{
    private readonly HttpClient _http;

    public GuestGateConsentClient(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("GuestGate BaseUrl is required.", nameof(baseUrl));

        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/")
        };
    }

    public async Task<GuestGateConsentCreateResponse> CreateConsentAsync(
        GuestRegistrationData data,
        string kid,
        string language,
        CancellationToken cancellationToken = default)
    {
        var checkInTime = string.IsNullOrWhiteSpace(data.ArrivalTime)
            ? DateTime.Now.ToString("HH:mm")
            : data.ArrivalTime.Trim();

        var identityNumber = !string.IsNullOrWhiteSpace(data.IdentityNumber)
            ? data.IdentityNumber.Trim()
            : data.PassportNo.Trim();

        var payload = new
        {
            kid,
            guestName = data.GuestName,
            identityNumber,
            language,
            checkInTime
        };

        using var response = await _http.PostAsJsonAsync("api/consents", payload, cancellationToken);
        await EnsureSuccessWithBodyAsync(response, cancellationToken);

        var created = await response.Content.ReadFromJsonAsync<GuestGateConsentCreateResponse>(cancellationToken: cancellationToken);
        return created ?? throw new InvalidOperationException("GuestGate returned an empty consent-create response.");
    }

    public async Task EndActiveSessionAsync(string kid, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(kid))
            throw new ArgumentException("GuestGate Kid is required.", nameof(kid));

        await CancelActiveConsentAsync(kid, cancellationToken);
        await CancelActiveKioskSessionAsync(kid, cancellationToken);
    }

    private async Task CancelActiveConsentAsync(string kid, CancellationToken cancellationToken)
    {
        var encodedKid = Uri.EscapeDataString(kid.Trim());
        using var response = await _http.DeleteAsync($"api/consents/active?kid={encodedKid}", cancellationToken);

        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
            return;

        if (response.StatusCode == HttpStatusCode.MethodNotAllowed)
        {
            using var fallbackResponse = await _http.PostAsync($"api/consents/cancel?kid={encodedKid}&force=true", content: null, cancellationToken);
            if (fallbackResponse.IsSuccessStatusCode || fallbackResponse.StatusCode == HttpStatusCode.NotFound)
                return;

            await EnsureSuccessWithBodyAsync(fallbackResponse, cancellationToken);
            return;
        }

        await EnsureSuccessWithBodyAsync(response, cancellationToken);
    }

    private async Task CancelActiveKioskSessionAsync(string kid, CancellationToken cancellationToken)
    {
        var encodedKid = Uri.EscapeDataString(kid.Trim());
        using var response = await _http.DeleteAsync($"api/sessions/active?kid={encodedKid}", cancellationToken);

        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
            return;

        if (response.StatusCode == HttpStatusCode.MethodNotAllowed)
        {
            using var fallbackResponse = await _http.PostAsync($"api/sessions/cancel?kid={encodedKid}", content: null, cancellationToken);
            if (fallbackResponse.IsSuccessStatusCode || fallbackResponse.StatusCode == HttpStatusCode.NotFound)
                return;

            await EnsureSuccessWithBodyAsync(fallbackResponse, cancellationToken);
            return;
        }

        await EnsureSuccessWithBodyAsync(response, cancellationToken);
    }

    public async Task<GuestGateConsentSignatureResponse> WaitForSignatureAsync(
        int consentId,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var until = DateTime.UtcNow.Add(timeout);

        while (DateTime.UtcNow < until)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var status = await _http.GetFromJsonAsync<GuestGateConsentStatusResponse>(
                $"api/consents/{consentId}",
                cancellationToken);

            if (status is not null && string.Equals(status.Status, "signed", StringComparison.OrdinalIgnoreCase))
            {
                var signature = await _http.GetFromJsonAsync<GuestGateConsentSignatureResponse>(
                    $"api/consents/{consentId}/signature",
                    cancellationToken);

                if (signature is not null && !string.IsNullOrWhiteSpace(signature.SignatureImage))
                    return signature with { PdfPath = signature.PdfPath ?? status.PdfPath };
            }

            await Task.Delay(1000, cancellationToken);
        }

        throw new TimeoutException($"GuestGate consent {consentId} was not signed before timeout.");
    }

    private static async Task EnsureSuccessWithBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException($"GuestGate request failed: {(int)response.StatusCode} {response.ReasonPhrase}. {body}");
    }

    public void Dispose() => _http.Dispose();
}

public sealed record GuestGateConsentCreateResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("kid")] string Kid,
    [property: JsonPropertyName("guestName")] string GuestName,
    [property: JsonPropertyName("identityNumber")] string IdentityNumber,
    [property: JsonPropertyName("checkInTime")] string CheckInTime,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("pdfPath")] string? PdfPath);

public sealed record GuestGateConsentStatusResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("pdfPath")] string? PdfPath);

public sealed record GuestGateConsentSignatureResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("pdfPath")] string? PdfPath,
    [property: JsonPropertyName("signatureImage")] string SignatureImage,
    [property: JsonPropertyName("signedAt")] DateTime? SignedAt);
