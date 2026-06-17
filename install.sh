#!/usr/bin/env bash
# ============================================================
#  install.sh — Instalacja zaleznosci (Linux)
# ============================================================
set -e

echo ""
echo "=== Kalkulator IP — Instalacja ==="

check_dotnet() {
    if command -v dotnet &>/dev/null; then
        ver=$(dotnet --version)
        echo "[OK] Znaleziono .NET $ver"
        return 0
    fi
    return 1
}

if ! check_dotnet; then
    echo "[!] .NET SDK nie znaleziony. Instalowanie .NET 8..."
    curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
    chmod +x /tmp/dotnet-install.sh
    /tmp/dotnet-install.sh --channel 8.0
    export PATH="$HOME/.dotnet:$PATH"
    echo 'export PATH="$HOME/.dotnet:$PATH"' >> "$HOME/.bashrc"
    echo "[OK] .NET 8 zainstalowany."
fi

echo ""
echo ">>> dotnet restore..."
dotnet restore

echo ""
echo "[OK] Zaleznosci zainstalowane. Uruchom ./build.sh aby zbudowac aplikacje."
echo ""
