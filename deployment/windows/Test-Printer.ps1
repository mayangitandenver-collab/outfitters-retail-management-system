param(
    [Parameter(Mandatory = $true)]
    [string]$PrinterName,
    [ValidateSet(58, 80)]
    [int]$PaperWidth = 58
)

$ErrorActionPreference = "Stop"
$tool = Join-Path $PSScriptRoot "..\printer-tool\Outfitters.PrinterTool.exe"

if (-not (Test-Path $tool)) {
    throw "Printer tool not found: $tool"
}

& $tool test $PrinterName $PaperWidth

if ($LASTEXITCODE -ne 0) {
    throw "Printer or cash-drawer test failed."
}
