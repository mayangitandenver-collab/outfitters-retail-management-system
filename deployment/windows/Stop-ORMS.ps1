Get-Process Outfitters.API,Outfitters.Web -ErrorAction SilentlyContinue | Stop-Process -Force
Write-Host "OUTFITTERS ORMS stopped." -ForegroundColor Green
