using System.Text.RegularExpressions;

namespace Outfitters.Infrastructure.Integrations;

public interface IBarcodeService
{
    bool IsValid(string barcode, string barcodeType);
    string Normalize(string barcode);
}

public sealed class BarcodeService : IBarcodeService
{
    public string Normalize(string barcode) =>
        Regex.Replace(barcode.Trim(), @"\s+", string.Empty);

    public bool IsValid(string barcode, string barcodeType)
    {
        var value = Normalize(barcode);
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return barcodeType.Trim().ToUpperInvariant() switch
        {
            "EAN13" => Regex.IsMatch(value, @"^\d{13}$"),
            "EAN8" => Regex.IsMatch(value, @"^\d{8}$"),
            "UPC" => Regex.IsMatch(value, @"^\d{12}$"),
            "CODE128" => value.Length is >= 4 and <= 80,
            _ => value.Length is >= 4 and <= 100
        };
    }
}
