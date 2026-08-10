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
2. Install the Windows driver supplied for the exact receipt printer model.
3. Connect the cash drawer to the printer's drawer port.
4. Run PowerShell as Administrator.
5. Run `Install-ORMs.ps1`.
6. Run `C:\Outfitters\scripts\Configure-Database.ps1` and enter the PostgreSQL administrator password plus the ORMS database-user password when prompted.
7. Run `C:\Outfitters\scripts\Test-Printer.ps1 -PrinterName "XP-58 (copy 1)" -PaperWidth 58`.
8. Run `C:\Outfitters\scripts\Start-ORMS.ps1`.
9. Open `http://localhost:8081` on the POS computer.
10. Complete a test sale using sample data before using live transactions.

## Hardware limitation

ESC/POS support is included, but the exact receipt printer driver, code page,
cutter behavior, paper width, and cash-drawer pulse must be verified
with the physical printer and drawer. The printer name must match the
name shown in Windows Settings > Bluetooth & devices > Printers & scanners.
