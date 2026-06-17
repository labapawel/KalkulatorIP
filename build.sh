#!/usr/bin/env bash
# ============================================================
#  build.sh — Budowanie i publikacja (Linux + Windows)
# ============================================================
set -e

echo ""
echo "=== Kalkulator IP — Build ==="

publish() {
    local RID="$1"
    local OUT="./publish/$2"
    local BIN="$OUT/$3"

    echo ""
    echo ">>> [$RID] dotnet publish -> $OUT"
    dotnet publish -c Release \
        -r "$RID" \
        --self-contained true \
        -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -o "$OUT"

    if [ -f "$BIN" ]; then
        chmod +x "$BIN"
        SIZE=$(du -sh "$BIN" | cut -f1)
        echo "[OK] $BIN  ($SIZE)"
    fi
}

publish "linux-x64"  "linux"   "KalkulatorIP"
publish "win-x64"    "windows" "KalkulatorIP.exe"

echo ""
echo "[OK] Publikacja zakonczona — pliki w ./publish/"
echo ""
