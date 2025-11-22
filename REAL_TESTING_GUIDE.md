# 🧪 Průvodce skutečným testováním MCP serveru

Tento průvodce vysvětluje, jak otestovat Cookidoo MCP Server s vaším reálným Cookidoo účtem.

## 🎯 Přehled

Pro skutečné testování potřebujete spustit backend API server, který komunikuje s Cookidoo API. Máte dvě možnosti:

1. **.NET Backend** (doporučeno pro produkci)
2. **Python Proxy Server** (jednodušší pro testování)

## 🚀 Možnost 1: .NET Backend

### Předpoklady
- .NET 8 SDK
- Visual Studio 2022 nebo VS Code

### Instalace a spuštění

```bash
# 1. Přejděte do API projektu
cd src/Cookidoo.MCP.Api

# 2. Obnovte NuGet balíčky
dotnet restore

# 3. Spusťte backend server
dotnet run
```

Backend poběží na `http://localhost:5555`.

### Konfigurace

Backend API používá `appsettings.json` pro konfiguraci Cookidoo API:

```json
{
  "Cookidoo": {
    "BaseUrl": "https://cookidoo.thermomix.com",
    "ApiVersion": "v1",
    "DefaultLanguage": "cs-CZ",
    "TimeoutSeconds": 30
  }
}
```

## 🐍 Možnost 2: Python Proxy Server

### Předpoklady
- Python 3.11+
- pip

### Instalace dependencies

```bash
# Nainstalujte cookidoo-api dependencies
cd cookidoo-api-master
pip install -r requirements.txt
cd ..
```

### Spuštění

```bash
# Spusťte Python proxy server
python3 python-proxy-server.py
```

Server poběží na `http://localhost:5555`.

## 📝 Testování vytvoření receptu

Po spuštění backendu (buď .NET nebo Python) můžete otestovat vytvoření receptu:

### Test 1: Krtkův dort (hotový test script)

```bash
# Spusťte test
node test-create-krtkov-dort.js
```

Tento test:
- ✅ Přihlásí se pomocí vašeho emailu a hesla
- ✅ Vytvoří recept "Krtkův dort" s 13 ingrediencemi
- ✅ Přidá 12 kroků s Thermomix parametry
- ✅ Zobrazí kompletní detail vytvořeného receptu

### Test 2: Vlastní recept přes MCP client

Pokud máte nakonfigurovaný Cursor s MCP serverem:

```
@cookidoo Vytvoř recept "Bramborová polévka"

Ingredience:
- 500g brambor
- 1 cibule
- 1l vývaru
- 200ml smetany
- sůl, pepř

Kroky:
1. Nakrájej cibuli - 5 sec / Stufe 5
2. Opraz cibuli - 3 min / 100°C / Stufe 2
3. Přidej brambory a vývar - 20 min / 100°C / Stufe 2 / Linkslauf
4. Rozmixuj - 30 sec / Stufe 8
5. Přidej smetanu a dochut
```

## 🔧 Konfigurace MCP Client

Upravte `.cursor-mcp.json` pro použití s reálným backendem:

```json
{
  "mcpServers": {
    "cookidoo": {
      "command": "node",
      "args": ["mcp-client.js"],
      "env": {
        "COOKIDOO_API_URL": "http://localhost:5555/api/v1",
        "COOKIDOO_EMAIL": "hnizdiljan@gmail.com",
        "COOKIDOO_PASSWORD": "Krel1991"
      }
    }
  }
}
```

**⚠️  BEZPEČNOST:** Nikdy necommitujte `.cursor-mcp.json` s heslem do gitu!

## 🧪 Ověření funkčnosti

Po vytvoření receptu se přihlaste na [cookidoo.thermomix.com](https://cookidoo.thermomix.com) a zkontrolujte:

1. **Moje recepty** - Měl by tam být nový recept
2. **Detail receptu** - Zkontrolujte ingredience a kroky
3. **Thermomix parametry** - Kroky by měly mít formátované parametry jako:
   - `<nobr>3 Min./100°C/Stufe 2</nobr> opražit cibuli`
   - `<nobr>20 Min./100°C/Stufe 2 Linkslauf</nobr> vařit`

## 📊 Testovací scénáře

### Scénář 1: Základní recept

```bash
# Spusťte test
node test-create-krtkov-dort.js
```

Očekávaný výsledek:
- ✅ Recept vytvořen v Cookidoo
- ✅ 13 ingrediencí
- ✅ 12 kroků s Thermomix parametry
- ✅ Viditelný v aplikaci Cookidoo

### Scénář 2: Nákupní seznam

```javascript
// Přes MCP client v Cursoru
@cookidoo Přidej recept "Krtkův dort" do nákupního seznamu
@cookidoo Zobraz můj nákupní seznam
```

### Scénář 3: Plánování jídel

```javascript
// Přes MCP client v Cursoru
@cookidoo Přidej "Krtkův dort" do plánu na zítřek
@cookidoo Zobraz týdenní plán jídel
```

## 🔍 Debugging

### Problém: Backend se nepřipojí k Cookidoo

**Řešení:**
1. Zkontrolujte internetové připojení
2. Ověřte přihlašovací údaje
3. Zkontrolujte logy backendu

### Problém: Recept se nevytváří

**Řešení:**
1. Zkontrolujte, že backend běží na `http://localhost:5555`
2. Ověřte, že MCP client je správně nakonfigurován
3. Zkontrolujte logy v konzoli backendu

### Problém: Chybí Thermomix parametry

**Řešení:**
1. Ujistěte se, že používáte nejnovější verzi mcp-client.js
2. Zkontrolujte, že kroky obsahují `timeSeconds`, `temperature`, `speed` parametry
3. Ověřte formátování v Cookidoo aplikaci

## 📚 Další čtení

- [QUICK_START.md](QUICK_START.md) - Rychlý start průvodce
- [THERMOMIX_GUIDE.md](THERMOMIX_GUIDE.md) - Průvodce Thermomix parametry
- [AUTO_LOGIN_GUIDE.md](AUTO_LOGIN_GUIDE.md) - Automatické přihlášení

## 💡 Tipy

1. **Automatické přihlášení** - Token se cachuje do `.cookidoo-token.json`, takže se nemusíte přihlašovat při každém startu

2. **Test s mock serverem** - Pro rychlé testování bez reálného Cookidoo použijte:
   ```bash
   node mock-api-server.js
   ```

3. **Verifikace tools** - Ověřte, že všechny MCP tools jsou implementované:
   ```bash
   node verify-tools.js
   ```

## ❓ FAQ

**Q: Můžu testovat bez .NET SDK?**
A: Ano! Použijte Python proxy server (`python-proxy-server.py`) nebo mock server (`mock-api-server.js`).

**Q: Je bezpečné ukládat heslo do .cursor-mcp.json?**
A: Heslo je uloženo pouze lokálně a není synchronizováno. Nicméně doporučujeme použít environment proměnné nebo secrets manager pro produkci.

**Q: Jak často expiruje token?**
A: Token je platný 1 hodinu. MCP client automaticky obnovuje token z cache nebo se znovu přihlásí.

**Q: Podporuje MCP server všechny Thermomix funkce?**
A: Ano! Podporuje: čas, teplotu, rychlost (1-10), Turbo, reverse rotation (Linkslauf) a Varoma režim.

---

**🎉 Pokud máte dotazy nebo problémy, kontaktujte vývojový tým!**
