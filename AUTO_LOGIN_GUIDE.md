# 🔐 Automatické Přihlášení - Průvodce

## ✨ Co je nového?

MCP server nyní podporuje **automatické přihlášení** pomocí emailu a hesla! Už nemusíte ručně kopírovat JWT token z browseru.

## 🎯 Jak to funguje

### 1. **Konfigurace**
Místo JWT tokenu zadáte email a heslo v `.cursor-mcp.json`:

```json
{
  "mcpServers": {
    "cookidoo": {
      "command": "node",
      "args": ["mcp-client.js"],
      "env": {
        "COOKIDOO_API_URL": "http://localhost:5555/api/v1",
        "COOKIDOO_EMAIL": "vas-email@cookidoo.com",
        "COOKIDOO_PASSWORD": "vase-heslo"
      }
    }
  }
}
```

### 2. **Automatické přihlášení**
Při prvním spuštění:
1. MCP server zavolá backend API `/auth/login`
2. Backend se přihlásí do Cookidoo pomocí emailu a hesla
3. Získá JWT token
4. Token se uloží do cache `.cookidoo-token.json`

### 3. **Cache tokenu**
Token se ukládá do souboru `.cookidoo-token.json`:

```json
{
  "accessToken": "eyJhbGci...",
  "expiresAt": "2025-11-22T12:00:00Z",
  "savedAt": "2025-11-21T10:00:00Z"
}
```

### 4. **Automatické obnovení**
- Při příštím spuštění se načte token z cache
- Pokud token expiroval, automaticky se přihlásí znovu
- Vše probíhá na pozadí bez vašeho zásahu

## 📁 Struktura autentizace

```
┌──────────────┐
│   Cursor     │
│  (MCP Client)│
└──────┬───────┘
       │ COOKIDOO_EMAIL + COOKIDOO_PASSWORD
       ▼
┌──────────────────┐
│  mcp-client.js   │ ← Automatické přihlášení
│  ┌────────────┐  │
│  │   Cache    │  │ ← .cookidoo-token.json
│  └────────────┘  │
└──────┬───────────┘
       │ Bearer token
       ▼
┌──────────────────┐
│  Backend API     │
│  /auth/login     │ ← POST email + password
└──────┬───────────┘
       │
       ▼
┌──────────────────┐
│  Cookidoo API    │ ← OAuth2 autentizace
└──────────────────┘
```

## 🔑 Přihlašovací flow

### První spuštění

```
1. Cursor spustí mcp-client.js
   ↓
2. mcp-client kontroluje cache (.cookidoo-token.json)
   ↓ (cache neexistuje)
3. mcp-client volá POST /api/v1/auth/login
   {
     "email": "user@example.com",
     "password": "password123"
   }
   ↓
4. Backend AuthController přihlásí uživatele
   ↓
5. Backend vrátí token:
   {
     "accessToken": "eyJhbGci...",
     "expiresIn": 43200  // 12 hodin
   }
   ↓
6. mcp-client uloží token do cache
   ↓
7. mcp-client používá token pro API volání
```

### Další spuštění

```
1. Cursor spustí mcp-client.js
   ↓
2. mcp-client kontroluje cache (.cookidoo-token.json)
   ↓ (cache existuje a je platná)
3. mcp-client načte token z cache
   ↓
4. mcp-client používá token pro API volání
   ↓
5. (pokud token expiruje, automaticky se přihlásí znovu)
```

## 🔒 Bezpečnost

### Co se ukládá

- **Email**: Pouze v `.cursor-mcp.json` (local)
- **Heslo**: Pouze v `.cursor-mcp.json` (local)
- **Token**: V `.cookidoo-token.json` (local, ignorovaný v gitu)

### Co se NEukládá

- ❌ Email a heslo se **nikdy** neodesílají do Gitu
- ❌ Token se **nikdy** neodesílá do Gitu
- ❌ Citlivé údaje nejsou v kódu

### Doporučení

1. **Nikdy** necommitujte `.cursor-mcp.json` s vašimi údaji
2. **Nikdy** necommitujte `.cookidoo-token.json`
3. Používejte **silné heslo** pro Cookidoo účet
4. Pravidelně **měňte heslo**

## 🛠️ Troubleshooting

### Token je neplatný

```bash
# Smažte cache a přihlaste se znovu
rm .cookidoo-token.json
# MCP server se automaticky přihlásí při příštím spuštění
```

### Chyba přihlášení

```
❌ Chyba při přihlášení: Přihlášení selhalo (401): {"message":"Neplatné přihlašovací údaje"}
```

**Řešení:**
1. Ověřte správnost emailu a hesla v `.cursor-mcp.json`
2. Zkontrolujte, že backend API běží na `http://localhost:5555`
3. Zkontrolujte logy v konzoli

### Backend API není dostupný

```
❌ Chyba při přihlášení: fetch failed
```

**Řešení:**
1. Spusťte backend API: `dotnet run --project src/Cookidoo.MCP.Api`
2. Zkontrolujte, že API běží na správném portu
3. Změňte `COOKIDOO_API_URL` pokud používáte jiný port

## 📝 Porovnání: Staré vs. Nové

### Staré řešení (manuální token)

```json
{
  "env": {
    "COOKIDOO_TOKEN": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

**Problémy:**
- ❌ Musíte ručně kopírovat token z browseru
- ❌ Token expiruje za 12-24 hodin
- ❌ Musíte ho ručně obnovovat
- ❌ Nepraktické pro dlouhodobé používání

### Nové řešení (automatické přihlášení)

```json
{
  "env": {
    "COOKIDOO_EMAIL": "vas-email@cookidoo.com",
    "COOKIDOO_PASSWORD": "vase-heslo"
  }
}
```

**Výhody:**
- ✅ Automatické přihlášení
- ✅ Automatické cachování tokenu
- ✅ Automatické obnovení tokenu
- ✅ Nastavte jednou, funguje stále

## 🚀 Quick Start

### 1. Nainstalujte závislosti

```bash
npm install
```

### 2. Nakonfigurujte přihlašovací údaje

Upravte `.cursor-mcp.json`:

```json
{
  "mcpServers": {
    "cookidoo": {
      "env": {
        "COOKIDOO_EMAIL": "vas-email@cookidoo.com",
        "COOKIDOO_PASSWORD": "vase-heslo"
      }
    }
  }
}
```

### 3. Spusťte backend API

```bash
cd src/Cookidoo.MCP.Api
dotnet run
```

### 4. Používejte v Cursoru

```
@cookidoo Vytvoř recept na čokoládový dort
```

MCP server se automaticky přihlásí a vytvoří recept!

## 🎓 Pokročilé použití

### Změna Cookidoo účtu

Stačí změnit email a heslo v `.cursor-mcp.json` a smazat cache:

```bash
rm .cookidoo-token.json
```

### Vícero účtů

Můžete mít více konfigurací MCP serveru:

```json
{
  "mcpServers": {
    "cookidoo-personal": {
      "env": {
        "COOKIDOO_EMAIL": "osobni@email.com",
        "COOKIDOO_PASSWORD": "heslo1"
      }
    },
    "cookidoo-work": {
      "env": {
        "COOKIDOO_EMAIL": "pracovni@email.com",
        "COOKIDOO_PASSWORD": "heslo2"
      }
    }
  }
}
```

### Debug režim

Pro zobrazení detailních logů:

```bash
NODE_ENV=development node mcp-client.js
```

## 🔗 Související dokumentace

- [QUICK_START.md](QUICK_START.md) - Rychlý start průvodce
- [README.md](README.md) - Přehled projektu
- [THERMOMIX_GUIDE.md](THERMOMIX_GUIDE.md) - Thermomix parametry

---

**Vytvořeno s ❤️ pro snadnější použití Cookidoo MCP Serveru**
