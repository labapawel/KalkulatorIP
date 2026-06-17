# ============================================================
#  install.ps1 — Instalacja zaleznosci (Windows)
# ============================================================

Write-Host "`n=== Kalkulator IP — Instalacja ===" -ForegroundColor Cyan

# Sprawdz .NET 8+
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Host "[!] .NET SDK nie znaleziony. Pobieranie instalatora..." -ForegroundColor Yellow
    $installer = "$env:TEMP\dotnet-install.ps1"
    Invoke-WebRequest -Uri "https://dot.net/v1/dotnet-install.ps1" -OutFile $installer
    & $installer -Channel 8.0 -Runtime dotnet
    & $installer -Channel 8.0          # SDK
    Write-Host "[OK] .NET 8 zainstalowany." -ForegroundColor Green
} else {
    $ver = & dotnet --version
    Write-Host "[OK] Znaleziono .NET $ver" -ForegroundColor Green
}

# Przywroc pakiety NuGet
Write-Host "`n>>> dotnet restore..." -ForegroundColor Cyan
dotnet restore
if ($LASTEXITCODE -ne 0) { Write-Host "[BLAD] Restore nieudany." -ForegroundColor Red; exit 1 }

Write-Host "`n[OK] Zaleznosci zainstalowane. Uruchom build.ps1 aby zbudowac aplikacje.`n" -ForegroundColor Green
