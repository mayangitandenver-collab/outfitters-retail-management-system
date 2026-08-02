param(
    [Parameter(Mandatory = $true)]
    [string]$BackupFile,
    [string]$PgBin = "C:\Program Files\PostgreSQL\17\bin",
    [string]$DatabaseName = "outfitters",
    [string]$DatabaseUser = "outfitters"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $BackupFile)) {
    throw "Backup file not found: $BackupFile"
}

$pgRestore = Join-Path $PgBin "pg_restore.exe"

if (-not (Test-Path $pgRestore)) {
    throw "pg_restore.exe was not found at $pgRestore"
}

Write-Warning "This will overwrite objects in database '$DatabaseName'."
$confirmation = Read-Host "Type RESTORE to continue"

if ($confirmation -ne "RESTORE") {
    Write-Host "Restore cancelled."
    exit 1
}

& $pgRestore -U $DatabaseUser -d $DatabaseName --clean --if-exists $BackupFile

if ($LASTEXITCODE -ne 0) {
    throw "PostgreSQL restore failed."
}

Write-Host "Restore completed." -ForegroundColor Green
