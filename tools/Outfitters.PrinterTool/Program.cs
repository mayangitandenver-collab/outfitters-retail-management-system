using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

if (args.Length < 2)
{
    PrintUsage();
    return 2;
}

var command = args[0].Trim().ToLowerInvariant();
var printerName = args[1];

try
{
    switch (command)
    {
        case "test":
            var width = args.Length >= 3 && int.TryParse(args[2], out var parsedWidth)
                ? parsedWidth
                : 80;
            RawPrinter.Send(printerName, EscPos.CreateTestReceipt(width));
            Console.WriteLine($"Test receipt sent to '{printerName}'.");
            break;

        case "drawer":
            RawPrinter.Send(printerName, EscPos.OpenDrawer());
            Console.WriteLine($"Cash-drawer command sent through '{printerName}'.");
            break;

        case "text":
            if (args.Length < 3)
            {
                Console.Error.WriteLine("Text is required.");
                return 2;
            }

            RawPrinter.Send(
                printerName,
                EscPos.CreatePlainReceipt(string.Join(" ", args.Skip(2))));
            Console.WriteLine($"Receipt text sent to '{printerName}'.");
            break;

        default:
            PrintUsage();
            return 2;
    }

    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}

static void PrintUsage()
{
    Console.WriteLine("OUTFITTERS Printer Tool");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  Outfitters.PrinterTool.exe test <PrinterName> [58|80]");
    Console.WriteLine("  Outfitters.PrinterTool.exe drawer <PrinterName>");
    Console.WriteLine("  Outfitters.PrinterTool.exe text <PrinterName> <Text>");
}

static class EscPos
{
    private static readonly byte[] Initialize = [0x1B, 0x40];
    private static readonly byte[] AlignCenter = [0x1B, 0x61, 0x01];
    private static readonly byte[] AlignLeft = [0x1B, 0x61, 0x00];
    private static readonly byte[] BoldOn = [0x1B, 0x45, 0x01];
    private static readonly byte[] BoldOff = [0x1B, 0x45, 0x00];
    private static readonly byte[] Cut = [0x1D, 0x56, 0x00];

    public static byte[] OpenDrawer() =>
        [0x1B, 0x70, 0x00, 0x19, 0xFA];

    public static byte[] CreateTestReceipt(int paperWidth)
    {
        var columns = paperWidth == 58 ? 32 : 42;
        var separator = new string('-', columns);
        var text = new List<byte>();

        text.AddRange(Initialize);
        text.AddRange(AlignCenter);
        text.AddRange(BoldOn);
        text.AddRange(Encode("OUTFITTERS APPAREL STORE\n"));
        text.AddRange(BoldOff);
        text.AddRange(Encode("Printer and Cash Drawer Test\n"));
        text.AddRange(Encode($"{separator}\n"));
        text.AddRange(AlignLeft);
        text.AddRange(Encode($"Paper Width: {paperWidth} mm\n"));
        text.AddRange(Encode($"Currency: Philippine Peso (PHP)\n"));
        text.AddRange(Encode($"Sample Total: ₱1,234.56\n"));
        text.AddRange(Encode($"Date: {DateTime.Now:MM/dd/yyyy hh:mm tt}\n"));
        text.AddRange(Encode($"{separator}\n"));
        text.AddRange(Encode("If this receipt is clear, printing works.\n"));
        text.AddRange(Encode("The cash drawer will be triggered next.\n\n\n"));
        text.AddRange(OpenDrawer());
        text.AddRange(Cut);

        return text.ToArray();
    }

    public static byte[] CreatePlainReceipt(string value)
    {
        var bytes = new List<byte>();
        bytes.AddRange(Initialize);
        bytes.AddRange(AlignLeft);
        bytes.AddRange(Encode(value + "\n\n\n"));
        bytes.AddRange(Cut);
        return bytes.ToArray();
    }

    private static byte[] Encode(string value)
    {
        // Most ESC/POS Windows drivers accept UTF-8 or map the peso symbol.
        // The printer profile may need a code-page adjustment for older models.
        return Encoding.UTF8.GetBytes(value);
    }
}

static class RawPrinter
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private sealed class DocInfo
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string DocumentName = "OUTFITTERS ESC POS";

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? OutputFile;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string DataType = "RAW";
    }

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool OpenPrinter(
        string printerName,
        out IntPtr printerHandle,
        IntPtr defaults);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr printerHandle);

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int StartDocPrinter(
        IntPtr printerHandle,
        int level,
        [In] DocInfo documentInfo);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndDocPrinter(IntPtr printerHandle);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool StartPagePrinter(IntPtr printerHandle);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndPagePrinter(IntPtr printerHandle);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool WritePrinter(
        IntPtr printerHandle,
        byte[] bytes,
        int count,
        out int written);

    public static void Send(string printerName, byte[] bytes)
    {
        if (!OpenPrinter(printerName, out var handle, IntPtr.Zero))
        {
            throw new Win32Exception(
                Marshal.GetLastWin32Error(),
                $"Unable to open printer '{printerName}'.");
        }

        try
        {
            var document = new DocInfo();

            if (StartDocPrinter(handle, 1, document) == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            try
            {
                if (!StartPagePrinter(handle))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                try
                {
                    if (!WritePrinter(
                        handle,
                        bytes,
                        bytes.Length,
                        out var written))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    if (written != bytes.Length)
                    {
                        throw new IOException(
                            $"Printer accepted {written} of {bytes.Length} bytes.");
                    }
                }
                finally
                {
                    EndPagePrinter(handle);
                }
            }
            finally
            {
                EndDocPrinter(handle);
            }
        }
        finally
        {
            ClosePrinter(handle);
        }
    }
}
