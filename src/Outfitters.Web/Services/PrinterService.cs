using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Outfitters.Web.Models;

namespace Outfitters.Web.Services;

public interface IPrinterService
{
    IReadOnlyCollection<InstalledPrinter> GetInstalledPrinters();

    PrinterProfile LoadProfile();

    void SaveProfile(PrinterProfile profile);

    bool TestPrint(PrinterProfile profile, out string message);
    bool PrintReceipt(PrinterProfile profile, string receiptText, out string message);
}

public sealed class PrinterService : IPrinterService
{
    private static readonly string SettingsFolder =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Outfitters");

    private static readonly string SettingsFile =
        Path.Combine(SettingsFolder, "printer-profile.json");

    public IReadOnlyCollection<InstalledPrinter> GetInstalledPrinters()
    {
        if (!OperatingSystem.IsWindows())
        {
            return Array.Empty<InstalledPrinter>();
        }

        var printers = new List<InstalledPrinter>();

        var flags = PrinterEnumFlags.PRINTER_ENUM_LOCAL |
                    PrinterEnumFlags.PRINTER_ENUM_CONNECTIONS;

        EnumPrinters(
            flags,
            null,
            4,
            IntPtr.Zero,
            0,
            out var bytesNeeded,
            out _);

        if (bytesNeeded == 0)
        {
            return printers;
        }

        var buffer = Marshal.AllocHGlobal((int)bytesNeeded);

        try
        {
            if (!EnumPrinters(
                    flags,
                    null,
                    4,
                    buffer,
                    bytesNeeded,
                    out _,
                    out var returned))
            {
                return printers;
            }

            var structSize = Marshal.SizeOf<PRINTER_INFO_4>();

            for (var i = 0; i < returned; i++)
            {
                var itemPtr = IntPtr.Add(buffer, i * structSize);
                var info = Marshal.PtrToStructure<PRINTER_INFO_4>(itemPtr);

                if (!string.IsNullOrWhiteSpace(info.pPrinterName))
                {
                    printers.Add(new InstalledPrinter
                    {
                        Name = info.pPrinterName,
                        IsDefault = false
                    });
                }
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }

        return printers
            .OrderBy(x => x.Name)
            .ToArray();
    }

    public PrinterProfile LoadProfile()
    {
        try
        {
            if (!File.Exists(SettingsFile))
            {
                return new PrinterProfile();
            }

            var json = File.ReadAllText(SettingsFile);

            return JsonSerializer.Deserialize<PrinterProfile>(json)
                   ?? new PrinterProfile();
        }
        catch
        {
            return new PrinterProfile();
        }
    }

    public void SaveProfile(PrinterProfile profile)
    {
        Directory.CreateDirectory(SettingsFolder);

        var json = JsonSerializer.Serialize(
            profile,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(SettingsFile, json);
    }

    public bool TestPrint(PrinterProfile profile, out string message)
    {
        if (!OperatingSystem.IsWindows())
        {
            message = "Printing is available only on Windows.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(profile.PrinterName))
        {
            message = "Select a printer first.";
            return false;
        }

        var text =
            "OUTFITTERS APPAREL STORE\n" +
            "Printer Test\n" +
            "------------------------------\n" +
            $"Printer: {profile.PrinterName}\n" +
            $"Paper: {profile.PaperWidthMm} mm\n" +
            "Universal printer setup OK\n" +
            "------------------------------\n\n\n";

        byte[] bytes;

        if (profile.UseEscPos)
        {
            var content = new List<byte>();

            // ESC @ - initialize printer
            content.AddRange(new byte[] { 0x1B, 0x40 });

            // Left alignment
            content.AddRange(new byte[] { 0x1B, 0x61, 0x00 });

            content.AddRange(Encoding.UTF8.GetBytes(text));

            if (profile.AutoCut)
            {
                // GS V 0 - full cut
                content.AddRange(new byte[] { 0x1D, 0x56, 0x00 });
            }

            if (profile.OpenCashDrawer)
            {
                // ESC p - drawer pulse
                content.AddRange(new byte[] { 0x1B, 0x70, 0x00, 0x19, 0xFA });
            }

            bytes = content.ToArray();
        }
        else
        {
            bytes = Encoding.UTF8.GetBytes(text);
        }

        if (!SendRawBytes(profile.PrinterName, bytes))
        {
            var error = Marshal.GetLastWin32Error();

            message =
                $"Windows could not send the test print. Error code: {error}";

            return false;
        }

        message = $"Test print sent to {profile.PrinterName}.";
        return true;
    }
        public bool PrintReceipt(
    PrinterProfile profile,
    string receiptText,
    out string message)
{
    if (!OperatingSystem.IsWindows())
    {
        message = "Printing is available only on Windows.";
        return false;
    }

    if (string.IsNullOrWhiteSpace(profile.PrinterName))
    {
        message = "No receipt printer has been selected.";
        return false;
    }

    if (string.IsNullOrWhiteSpace(receiptText))
    {
        message = "Receipt text is empty.";
        return false;
    }

    try
    {
        var bytes = new List<byte>();

        // ESC/POS initialize printer
        bytes.AddRange(new byte[] { 0x1B, 0x40 });

        // Left alignment
        bytes.AddRange(new byte[] { 0x1B, 0x61, 0x00 });

        // Receipt text
        bytes.AddRange(Encoding.UTF8.GetBytes(receiptText));

        // Feed paper after receipt
        bytes.AddRange(Encoding.UTF8.GetBytes("\n\n\n"));

        // Full cut
        bytes.AddRange(new byte[] { 0x1D, 0x56, 0x00 });

        if (!SendRawBytes(profile.PrinterName, bytes.ToArray()))
        {
            var error = Marshal.GetLastWin32Error();
            message = $"Windows could not print the receipt. Error {error}.";
            return false;
        }

        message = $"Receipt sent to {profile.PrinterName}.";
        return true;
    }
    catch (Exception ex)
    {
        message = $"Receipt printing failed: {ex.Message}";
        return false;
    }
}

    private static bool SendRawBytes(string printerName, byte[] bytes)
    {
        if (!OpenPrinter(printerName, out var printerHandle, IntPtr.Zero))
        {
            return false;
        }

        try
        {
            var docInfo = new DOC_INFO_1
            {
                pDocName = "OUTFITTERS Printer Test",
                pOutputFile = null,
                pDatatype = "RAW"
            };

            if (StartDocPrinter(printerHandle, 1, ref docInfo) == 0)
            {
                return false;
            }

            try
            {
                if (!StartPagePrinter(printerHandle))
                {
                    return false;
                }

                try
                {
                    return WritePrinter(
                        printerHandle,
                        bytes,
                        bytes.Length,
                        out var written)
                        && written == bytes.Length;
                }
                finally
                {
                    EndPagePrinter(printerHandle);
                }
            }
            finally
            {
                EndDocPrinter(printerHandle);
            }
        }
        finally
        {
            ClosePrinter(printerHandle);
        }
    }

    [Flags]
    private enum PrinterEnumFlags : uint
    {
        PRINTER_ENUM_LOCAL = 0x00000002,
        PRINTER_ENUM_CONNECTIONS = 0x00000004
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct PRINTER_INFO_4
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pPrinterName;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string pServerName;

        public uint Attributes;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DOC_INFO_1
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pDocName;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? pOutputFile;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string pDatatype;
    }

    [DllImport(
        "winspool.drv",
        EntryPoint = "EnumPrintersW",
        SetLastError = true,
        CharSet = CharSet.Unicode)]
    private static extern bool EnumPrinters(
        PrinterEnumFlags Flags,
        string? Name,
        uint Level,
        IntPtr pPrinterEnum,
        uint cbBuf,
        out uint pcbNeeded,
        out uint pcReturned);

    [DllImport(
        "winspool.drv",
        EntryPoint = "OpenPrinterW",
        SetLastError = true,
        CharSet = CharSet.Unicode)]
    private static extern bool OpenPrinter(
        string pPrinterName,
        out IntPtr phPrinter,
        IntPtr pDefault);

    [DllImport(
        "winspool.drv",
        EntryPoint = "ClosePrinter",
        SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport(
        "winspool.drv",
        EntryPoint = "StartDocPrinterW",
        SetLastError = true,
        CharSet = CharSet.Unicode)]
    private static extern int StartDocPrinter(
        IntPtr hPrinter,
        int level,
        ref DOC_INFO_1 di);

    [DllImport(
        "winspool.drv",
        EntryPoint = "EndDocPrinter",
        SetLastError = true)]
    private static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport(
        "winspool.drv",
        EntryPoint = "StartPagePrinter",
        SetLastError = true)]
    private static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport(
        "winspool.drv",
        EntryPoint = "EndPagePrinter",
        SetLastError = true)]
    private static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport(
        "winspool.drv",
        EntryPoint = "WritePrinter",
        SetLastError = true)]
    private static extern bool WritePrinter(
        IntPtr hPrinter,
        byte[] pBytes,
        int dwCount,
        out int dwWritten);
}