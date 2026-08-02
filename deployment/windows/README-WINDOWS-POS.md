# OUTFITTERS Windows POS Deployment

This bundle is configured for:

- Windows 10 or Windows 11, 64-bit
- Philippine Peso (`PHP`, symbol `₱`)
- Culture `en-PH`
- Timezone `Asia/Manila`
- PostgreSQL database
- ESC/POS receipt printers
- Cash drawers connected through the receipt printer

## Installation order

1. Install PostgreSQL 17 for Windows.
2. Install the Windows driver supplied for the exact XPrinter model.
3. Connect the cash drawer to the printer's drawer port.
4. Run PowerShell as Administrator.
5. Run `Install-ORMs.ps1`.
6. Configure the database credentials in the API production settings.
7. Run the API and web applications.
8. Run `Test-Printer.ps1 -PrinterName "Windows Printer Name" -PaperWidth 80`.
9. Complete a test sale using sample data before using live transactions.

## Hardware limitation

ESC/POS support is included, but the exact XPrinter driver, code page,
cutter behavior, paper width, and cash-drawer pulse must be verified
with the physical printer and drawer. The printer name must match the
name shown in Windows Settings > Bluetooth & devices > Printers & scanners.
