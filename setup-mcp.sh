#!/bin/bash

# 🍳 Cookidoo MCP Setup Script
# Instaluje a konfiguruje MCP client pro Cursor

echo "🚀 Instalace Cookidoo MCP Client..."

# Kontrola Node.js
if ! command -v node &> /dev/null; then
    echo "❌ Node.js není nainstalován. Prosím nainstalujte Node.js 18+."
    exit 1
fi

# Kontrola verze Node.js
NODE_VERSION=$(node -v | cut -d'v' -f2)
REQUIRED_VERSION="18.0.0"

if [ "$(printf '%s\n' "$REQUIRED_VERSION" "$NODE_VERSION" | sort -V | head -n1)" != "$REQUIRED_VERSION" ]; then
    echo "❌ Potřebujete Node.js verzi 18.0.0 nebo vyšší. Máte verzi $NODE_VERSION."
    exit 1
fi

echo "✅ Node.js verze $NODE_VERSION je v pořádku."

# Instalace dependencies
echo "📦 Instalace MCP SDK..."
npm install

# Nastavení executable permissions
chmod +x mcp-client.js

echo ""
echo "🎉 Instalace dokončena!"
echo ""
echo "📋 Další kroky:"
echo "1. Spusťte Cookidoo MCP Server:"
echo "   dotnet run --project src/Cookidoo.MCP.Api --urls \"http://localhost:5555\""
echo ""
echo "2. Přihlaste se a získejte JWT token:"
echo "   curl -X POST http://localhost:5555/api/v1/auth/login \\"
echo "        -H \"Content-Type: application/json\" \\"
echo "        -d '{\"email\":\"vas@email.com\",\"password\":\"heslo\"}'"
echo ""
echo "3. Nastavte token v .cursor-mcp.json:"
echo "   Nahraďte 'YOUR_JWT_TOKEN_HERE' skutečným tokenem"
echo ""
echo "4. Restartujte Cursor a použijte:"
echo "   @cookidoo Najdi recepty s kuřetem"
echo ""
echo "🔗 Pro více informací viz MCP_GUIDE.md" 