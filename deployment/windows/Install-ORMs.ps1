param(
    [string]$InstallPath = "C:\Outfitters",
    [string]$DatabasePassword = "",
    [string]$PrinterName = "",
    [ValidateSet(58, 80)]
    [int]$PaperWidth = 80
)

$ErrorActionPreference = "Stop"

Write-Host "OUTFITTERS POS Setup" -ForegroundColor Cyan

if ([string]::IsNullOrWhiteSpace($DatabasePassword)) {
    $secure = Read-Host "Create a PostgreSQL password" -AsSecureString
    $DatabasePassword = [System.Net.NetworkCredential]::new("", $secure).Password
}

New-Item -ItemType Directory -Force -Path $InstallPath | Out-Null
New-Item -ItemType Directory -Force -Path "$InstallPath\data\backups" | Out-Null
New-Item -ItemType Directory -Force -Path "$InstallPath\logs" | Out-Null

Copy-Item "$PSScriptRoot\api" "$InstallPath\api" -Recurse -Force
Copy-Item "$PSScriptRoot\web" "$InstallPath\web" -Recurse -Force
Copy-Item "$PSScriptRoot\printer-tool" "$InstallPath\printer-tool" -Recurse -Force
Copy-Item "$PSScriptRoot\scripts" "$InstallPath\scripts" -Recurse -Force

$settings = @{
    Culture = "en-PH"
    CurrencyCode = "PHP"
    CurrencySymbol = "₱"
    TimeZone = "Asia/Manila"
    PrinterName = $PrinterName
    PaperWidth = $PaperWidth
    OpenCashDrawer = $true
} | ConvertTo-Json

$settings | Set-Content -Encoding UTF8 "$InstallPath\orms-machine-settings.json"

Write-Host ""
Write-Host "Files installed to $InstallPath" -ForegroundColor Green
Write-Host "PostgreSQL must be installed before starting ORMS."
Write-Host "Run Configure-Database.ps1 after PostgreSQL installation."
Write-Host "Run Test-Printer.ps1 after installing the XPrinter Windows driver."
