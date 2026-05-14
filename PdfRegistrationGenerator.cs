using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Globalization;

namespace RoomRegistrationPdfApp;

public sealed class PdfRegistrationGenerator
{
    public void Generate(string templatePath, string outputPath, GuestRegistrationData data)
    {
        if (!File.Exists(templatePath))
            throw new FileNotFoundException("Template PDF was not found.", templatePath);

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        using PdfDocument document = PdfReader.Open(templatePath, PdfDocumentOpenMode.Modify);
        PdfPage page = document.Pages[0];

        using XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
        var font = new XFont("Arial", 10, XFontStyleEx.Regular);
        var smallFont = new XFont("Arial", 9, XFontStyleEx.Regular);
        var checkFont = new XFont("Arial", 9, XFontStyleEx.Bold);

        DrawLeft(gfx, smallFont, "Print Date/Time:", 30, 77);
        DrawLeft(gfx, smallFont, data.PrintDateTime.ToString("MM/dd/yyyy  hh:mm tt", CultureInfo.InvariantCulture).ToLowerInvariant(), 30, 89);

        DrawLeft(gfx, font, data.RoomNo, 150, 114);
        DrawLeft(gfx, font, data.ConfirmationNo, 378, 114);
        DrawLeft(gfx, font, data.ArrivalDate, 150, 132);
        DrawLeft(gfx, font, data.ArrivalTime, 204, 132);
        DrawLeft(gfx, font, data.MealPlan, 378, 132);
        DrawLeft(gfx, font, FormatCurrency(data.RoomRate, data.Currency), 494, 132);
        DrawLeft(gfx, font, data.DepartureDate, 150, 150);
        DrawLeft(gfx, font, data.DepartureTime, 204, 150);
        DrawLeft(gfx, font, data.NoOfGuests, 378, 150);
        DrawLeft(gfx, font, data.RoomType, 150, 168);
        DrawLeft(gfx, font, data.AdvanceDeposit, 378, 168);

        DrawCenter(gfx, font, data.GuestName, 125, 192, 335, 14);
        DrawCenter(gfx, font, data.CompanyName, 125, 216, 335, 14);
        DrawCenter(gfx, font, data.LoyaltyNumber, 125, 240, 335, 14);
        DrawCenter(gfx, font, data.GuestAddress, 125, 258, 335, 14);
        DrawCenter(gfx, font, data.Email, 125, 276, 335, 14);
        DrawCenter(gfx, font, data.Telephone, 125, 294, 335, 14);
        DrawCenter(gfx, font, data.BillingInstructions, 125, 306, 335, 14);

        DrawPaymentCheck(gfx, checkFont, data.ModeOfPayment);

        DrawCenter(gfx, font, data.PassportNo, 125, 354, 335, 14);
        DrawCenter(gfx, font, data.Nationality, 125, 372, 335, 14);
        DrawCenter(gfx, font, data.DateOfBirth, 125, 390, 335, 14);
        DrawTermsActualArrivalTime(gfx, smallFont, data.ArrivalTime);
        DrawCenter(gfx, font, data.ReceptionistName, 45, 756, 90, 14);

        // Adjust this box if your template signature field is in a different place.
        DrawGuestSignature(gfx, data.GuestSignatureImageDataUrl, new XRect(360, 720, 190, 45));

        document.Save(outputPath);
    }
    private static void DrawTermsActualArrivalTime(XGraphics gfx, XFont font, string? arrivalTime)
    {
        if (string.IsNullOrWhiteSpace(arrivalTime))
            return;

        // English terms: replace 15:00PM
        gfx.DrawRectangle(XBrushes.Transparent, 88, 425, 38, 12);
        gfx.DrawString(arrivalTime, font, XBrushes.Black,
            new XRect(90, 426, 50, 12),
            XStringFormats.TopLeft);

        // Arabic terms: replace 15:00 عصراً
        gfx.DrawRectangle(XBrushes.Transparent, 452, 427, 48, 12);
        gfx.DrawString(arrivalTime, font, XBrushes.Black,
            new XRect(475, 428, 50, 12),
            XStringFormats.TopLeft);
    }


    private static void DrawGuestSignature(XGraphics gfx, string? signatureDataUrl, XRect signatureBox)
    {
        var bytes = TryReadDataUrlImage(signatureDataUrl);
        if (bytes.Length == 0) return;

        using var stream = new MemoryStream(bytes);
        using var image = XImage.FromStream(stream);

        var scale = Math.Min(signatureBox.Width / image.PixelWidth, signatureBox.Height / image.PixelHeight);
        var width = image.PixelWidth * scale;
        var height = image.PixelHeight * scale;
        var x = signatureBox.X + (signatureBox.Width - width) / 2;
        var y = signatureBox.Y + (signatureBox.Height - height) / 2;

        gfx.DrawImage(image, x, y, width, height);
    }

    private static byte[] TryReadDataUrlImage(string? dataUrl)
    {
        if (string.IsNullOrWhiteSpace(dataUrl)) return Array.Empty<byte>();
        var comma = dataUrl.IndexOf(',');
        var raw = comma >= 0 ? dataUrl[(comma + 1)..] : dataUrl;
        try { return Convert.FromBase64String(raw); }
        catch { return Array.Empty<byte>(); }
    }

    private static string FormatCurrency(string roomRate, string currency)
    {
        if (string.IsNullOrWhiteSpace(roomRate)) return string.Empty;

        var cleanCurrency = string.IsNullOrWhiteSpace(currency) ? "SAR" : currency.Trim();
        if (roomRate.StartsWith(cleanCurrency, StringComparison.OrdinalIgnoreCase))
            return roomRate;

        if (roomRate.StartsWith("SAR", StringComparison.OrdinalIgnoreCase))
            return roomRate;

        return $"{cleanCurrency} {roomRate}";
    }

    private static void DrawLeft(XGraphics gfx, XFont font, string? text, double x, double y)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        gfx.DrawString(text, font, XBrushes.Black, new XRect(x, y, 200, 14), XStringFormats.TopLeft);
    }

    private static void DrawCenter(XGraphics gfx, XFont font, string? text, double x, double y, double width, double height)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        gfx.DrawString(text, font, XBrushes.Black, new XRect(x, y, width, height), XStringFormats.Center);
    }

    private static void DrawPaymentCheck(XGraphics gfx, XFont font, string? paymentMode)
    {
        if (string.IsNullOrWhiteSpace(paymentMode)) return;

        var value = paymentMode.ToLowerInvariant();
        if (value.Contains("cash")) DrawLeft(gfx, font, "X", 132, 336);
        else if (value.Contains("credit")) DrawLeft(gfx, font, "X", 180, 336);
        else if (value.Contains("cheque") || value.Contains("check")) DrawLeft(gfx, font, "X", 258, 336);
        else if (value.Contains("company")) DrawLeft(gfx, font, "X", 318, 336);
        else if (value.Contains("travel")) DrawLeft(gfx, font, "X", 384, 336);
    }
}
