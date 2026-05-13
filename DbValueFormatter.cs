using System.Globalization;

namespace RoomRegistrationPdfApp;

public static class DbValueFormatter
{
    public static string Text(object value)
    {
        if (value == DBNull.Value || value is null) return string.Empty;
        return Convert.ToString(value, CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;
    }

    public static string Number(object value, int decimals = 2)
    {
        if (value == DBNull.Value || value is null) return string.Empty;

        if (decimal.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
        {
            return d.ToString($"N{decimals}", CultureInfo.InvariantCulture);
        }

        return Text(value);
    }

    public static string DateYmd(object value)
    {
        if (value == DBNull.Value || value is null) return string.Empty;

        if (value is DateTime dt)
            return dt.ToString("dd/MM/yy", CultureInfo.InvariantCulture);

        var raw = Text(value);
        var text = raw.Replace("-", "").Replace("/", "").Replace(".", "").Trim();

        if (text.Length == 8 && int.TryParse(text[..4], out var yyyy) && int.TryParse(text.Substring(4, 2), out var mm8) && int.TryParse(text.Substring(6, 2), out var dd8))
        {
            try
            {
                return new DateTime(yyyy, mm8, dd8).ToString("dd/MM/yy", CultureInfo.InvariantCulture);
            }
            catch
            {
                return raw;
            }
        }

        if (text.Length == 6 && int.TryParse(text[..2], out var yy) && int.TryParse(text.Substring(2, 2), out var mm6) && int.TryParse(text.Substring(4, 2), out var dd6))
        {
            try
            {
                var year = yy >= 80 ? 1900 + yy : 2000 + yy;
                return new DateTime(year, mm6, dd6).ToString("dd/MM/yy", CultureInfo.InvariantCulture);
            }
            catch
            {
                return raw;
            }
        }

        return raw;
    }

    public static string TimeHm(object value)
    {
        if (value == DBNull.Value || value is null) return string.Empty;

        if (value is DateTime dt)
            return dt.ToString("HH:mm", CultureInfo.InvariantCulture);

        if (value is TimeSpan ts)
            return new DateTime(1, 1, 1).Add(ts).ToString("HH:mm", CultureInfo.InvariantCulture);

        var text = Text(value);
        if (text.Contains(':'))
        {
            var parts = text.Split(':');
            if (parts.Length >= 2 && int.TryParse(parts[0], out var h) && int.TryParse(parts[1], out var m))
                return $"{h:00}:{m:00}";
        }

        if (text.Contains('.'))
        {
            var parts = text.Split('.');
            if (parts.Length >= 2 && int.TryParse(parts[0], out var h) && int.TryParse(parts[1].PadRight(2, '0')[..2], out var m))
                return $"{h:00}:{m:00}";
        }

        var digits = new string(text.Where(char.IsDigit).ToArray());
        if (digits.Length is 3 or 4)
        {
            var hText = digits.Length == 3 ? digits[..1] : digits[..2];
            var mText = digits[^2..];
            if (int.TryParse(hText, out var h) && int.TryParse(mText, out var m))
                return $"{h:00}:{m:00}";
        }

        return text;
    }
}
