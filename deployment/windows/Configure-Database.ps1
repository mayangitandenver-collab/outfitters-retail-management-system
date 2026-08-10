param(
    [string]$InstallPath = "C:\Outfitters",
    [string]$PgBin = "C:\Program Files\PostgreSQL\17\bin",
    [string]$DatabaseName = "outfitters",
    [string]$DatabaseUser = "outfitters",
    [int]$Port = 5432
)

$ErrorActionPreference = "Stop"
$psql = Join-Path $PgBin "psql.exe"
if (-not (Test-Path $psql)) {
    throw "PostgreSQL 17 psql.exe was not found at $psql"
}

$apiSettings = Join-Path $InstallPath "api\appsettings.Production.json"
$dbTool = Join-Path $InstallPath "database-tool\Outfitters.DatabaseTool.exe"
if (-not (Test-Path $apiSettings)) { throw "API settings not found: $apiSettings" }
if (-not (Test-Path $dbTool)) { throw "Database migration tool not found: $dbTool" }

Write-Host "OUTFITTERS Database Configuration" -ForegroundColor Cyan
$postgresSecure = Read-Host "Enter the PostgreSQL 'postgres' administrator password" -AsSecureString
$postgresPassword = [System.Net.NetworkCredential]::new("", $postgresSecure).Password

$appSecure = Read-Host "Create/enter the password for the ORMS 'outfitters' database user" -AsSecureString
$appPassword = [System.Net.NetworkCredential]::new("", $appSecure).Password
if ([string]::IsNullOrWhiteSpace($appPassword)) { throw "The ORMS database password cannot be empty." }

$escaped = $appPassword.Replace("'", "''")
$env:PGPASSWORD = $postgresPassword
try {
    $roleExists = (& $psql -U postgres -h localhost -p $Port -d postgres -tAc "SELECT 1 FROM pg_roles WHERE rolname='$DatabaseUser';").Trim()
    if ($LASTEXITCODE -ne 0) { throw "Unable to connect to PostgreSQL as postgres." }

    if ($roleExists -eq "1") {
        & $psql -U postgres -h localhost -p $Port -d postgres -v ON_ERROR_STOP=1 -c "ALTER USER $DatabaseUser WITH LOGIN PASSWORD '$escaped';"
    } else {
        & $psql -U postgres -h localhost -p $Port -d postgres -v ON_ERROR_STOP=1 -c "CREATE USER $DatabaseUser WITH LOGIN PASSWORD '$escaped';"
    }
    if ($LASTEXITCODE -ne 0) { throw "Unable to create/update PostgreSQL user '$DatabaseUser'." }

    $dbExists = (& $psql -U postgres -h localhost -p $Port -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname='$DatabaseName';").Trim()
    if ($dbExists -ne "1") {
        & $psql -U postgres -h localhost -p $Port -d postgres -v ON_ERROR_STOP=1 -c "CREATE DATABASE $DatabaseName OWNER $DatabaseUser;"
        if ($LASTEXITCODE -ne 0) { throw "Unable to create database '$DatabaseName'." }
    }

    & $psql -U postgres -h localhost -p $Port -d postgres -v ON_ERROR_STOP=1 -c "ALTER DATABASE $DatabaseName OWNER TO $DatabaseUser;"
    if ($LASTEXITCODE -ne 0) { throw "Unable to assign database owner." }
}
finally {
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}

$connection = "Host=localhost;Port=$Port;Database=$DatabaseName;Username=$DatabaseUser;Password=$appPassword"
$config = Get-Content $apiSettings -Raw | ConvertFrom-Json
$config.ConnectionStrings.DefaultConnection = $connection
if ($config.Receipt) {
    $config.Receipt.PaperWidth = 58
    $config.Receipt.CharactersPerLine = 32
}
$config | ConvertTo-Json -Depth 20 | Set-Content -Encoding UTF8 $apiSettings

& $dbTool $connection
if ($LASTEXITCODE -ne 0) { throw "Database migration failed." }

Write-Host "Database configured and migrations applied." -ForegroundColor Green
Write-Host "Next: run $InstallPath\scripts\Start-ORMS.ps1"
