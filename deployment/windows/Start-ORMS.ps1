param(
    [string]$InstallPath = "C:\Outfitters",
    [int]$ApiPort = 8080,
    [int]$WebPort = 8081
)

$ErrorActionPreference = "Stop"
$apiExe = Join-Path $InstallPath "api\Outfitters.API.exe"
$webExe = Join-Path $InstallPath "web\Outfitters.Web.exe"
if (-not (Test-Path $apiExe)) { throw "API executable not found: $apiExe" }
if (-not (Test-Path $webExe)) { throw "Web executable not found: $webExe" }

Get-Process Outfitters.API,Outfitters.Web -ErrorAction SilentlyContinue | Stop-Process -Force

$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ASPNETCORE_URLS = "http://0.0.0.0:$ApiPort"
$api = Start-Process $apiExe -WorkingDirectory (Split-Path $apiExe) -PassThru
Start-Sleep -Seconds 3
if ($api.HasExited) { throw "OUTFITTERS API exited during startup. Run it manually from $InstallPath\api to view the error." }

$env:ASPNETCORE_URLS = "http://0.0.0.0:$WebPort"
$web = Start-Process $webExe -WorkingDirectory (Split-Path $webExe) -PassThru
Start-Sleep -Seconds 3
if ($web.HasExited) { throw "OUTFITTERS Web exited during startup." }

$principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if ($principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    New-NetFirewallRule -DisplayName "OUTFITTERS ORMS API" -Direction Inbound -Action Allow -Protocol TCP -LocalPort $ApiPort -Profile Private -ErrorAction SilentlyContinue | Out-Null
    New-NetFirewallRule -DisplayName "OUTFITTERS ORMS Web" -Direction Inbound -Action Allow -Protocol TCP -LocalPort $WebPort -Profile Private -ErrorAction SilentlyContinue | Out-Null
}

Write-Host "OUTFITTERS ORMS started." -ForegroundColor Green
Write-Host "This computer: http://localhost:$WebPort"
Write-Host "Other devices on the same private LAN: http://<THIS-PC-IP>:$WebPort"
