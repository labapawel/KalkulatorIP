# ============================================================
#  build.ps1 — Budowanie i publikacja (Windows)
# ============================================================

Write-Host "`n=== Kalkulator IP — Build ===" -ForegroundColor Cyan

$out = ".\publish\windows"

Write-Host ">>> dotnet publish -> $out" -ForegroundColor Cyan
dotnet publish -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $out

if ($LASTEXITCODE -ne 0) {
    Write-Host "[BLAD] Build nieudany." -ForegroundColor Red
    exit 1
}

$exe = Get-Item "$out\KalkulatorIP.exe" -ErrorAction SilentlyContinue
if ($exe) {
    $size = [math]::Round($exe.Length / 1MB, 1)
    Write-Host "`n[OK] Gotowy plik: $($exe.FullName)  ($size MB)" -ForegroundColor Green
}

Write-Host "[OK] Publikacja zakonczona.`n" -ForegroundColor Green
