param(
    [string]$InstallPath = "C:\Outfitters",
    [string]$PrinterName = "",
    [ValidateSet(58, 80)]
    [int]$PaperWidth = 58
)

$ErrorActionPreference = "Stop"

Write-Host "OUTFITTERS POS Setup" -ForegroundColor Cyan

New-Item -ItemType Directory -Force -Path $InstallPath | Out-Null
New-Item -ItemType Directory -Force -Path "$InstallPath\data\backups" | Out-Null
New-Item -ItemType Directory -Force -Path "$InstallPath\logs" | Out-Null

Copy-Item "$PSScriptRoot\api" "$InstallPath\api" -Recurse -Force
Copy-Item "$PSScriptRoot\web" "$InstallPath\web" -Recurse -Force
Copy-Item "$PSScriptRoot\printer-tool" "$InstallPath\printer-tool" -Recurse -Force
Copy-Item "$PSScriptRoot\database-tool" "$InstallPath\database-tool" -Recurse -Force
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
Write-Host "PostgreSQL 17 must be installed before database configuration."
Write-Host "Next: run $InstallPath\scripts\Configure-Database.ps1"
Write-Host 'Then: run C:\Outfitters\scripts\Test-Printer.ps1 -PrinterName "XP-58 (copy 1)" -PaperWidth 58'
Write-Host "Finally: run $InstallPath\scripts\Start-ORMS.ps1"
