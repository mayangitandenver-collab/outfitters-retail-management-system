param(
    [string]$PgBin = "C:\Program Files\PostgreSQL\17\bin",
    [string]$DatabaseName = "outfitters",
    [string]$DatabaseUser = "outfitters",
    [string]$BackupFolder = "C:\Outfitters\data\backups",
    [int]$RetentionDays = 30
)

$ErrorActionPreference = "Stop"
New-Item -ItemType Directory -Force -Path $BackupFolder | Out-Null

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$output = Join-Path $BackupFolder "$DatabaseName-$timestamp.dump"
$pgDump = Join-Path $PgBin "pg_dump.exe"

if (-not (Test-Path $pgDump)) {
    throw "pg_dump.exe was not found at $pgDump"
}

& $pgDump -U $DatabaseUser -d $DatabaseName -F c -f $output

if ($LASTEXITCODE -ne 0) {
    throw "PostgreSQL backup failed."
}

Get-ChildItem $BackupFolder -Filter "*.dump" |
    Where-Object LastWriteTime -lt (Get-Date).AddDays(-$RetentionDays) |
    Remove-Item -Force

Write-Host "Backup created: $output" -ForegroundColor Green
