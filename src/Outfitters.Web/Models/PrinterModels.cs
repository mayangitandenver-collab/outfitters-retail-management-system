namespace Outfitters.Web.Models;

public sealed class PrinterProfile
{
    public string PrinterName { get; set; } = string.Empty;

    public int PaperWidthMm { get; set; } = 80;

    public bool UseEscPos { get; set; } = true;

    public bool AutoCut { get; set; } = true;

    public bool OpenCashDrawer { get; set; }

    public bool IsDefault { get; set; } = true;
}

public sealed class InstalledPrinter
{
    public string Name { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}