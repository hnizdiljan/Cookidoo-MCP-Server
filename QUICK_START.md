# 🚀 Cookidoo MCP Server - Rychlý Start

## 📝 Co je to Cookidoo MCP Server?

Cookidoo MCP Server umožňuje ovládat Cookidoo aplikaci přímo z Cursoru pomocí AI asistenta Claude. Můžete vytvářet recepty pro Thermomix, spravovat kolekce a mnohem více - vše pomocí přirozeného jazyka.

## 🎯 Hlavní funkce

- ✅ **Vytváření receptů** - Claude vytvoří recept podle vašich instrukcí
- ✅ **Správa kolekcí** - Organizujte recepty do kolekcí
- ✅ **Vyhledávání** - Najděte recepty podle ingrediencí, tagů nebo názvu
- ✅ **Úpravy receptů** - Upravujte existující recepty
- ✅ **Synchronizace** - Vše se synchronizuje s Cookidoo platformou

## 📋 Prerekvizity

1. **Node.js** (verze 18 nebo vyšší)
2. **Cursor** editor
3. **Cookidoo účet** s předplatným
4. **Thermomix** zařízení (doporučeno)

## 🔧 Instalace a nastavení

### Krok 1: Nainstalujte závislosti

```bash
npm install
```

### Krok 2: Získejte Cookidoo JWT token

Cookidoo používá OAuth2 autentizaci přes `_oauth2_proxy` cookie. Pro získání tokenu:

1. Otevřete webový prohlížeč a přejděte na [Cookidoo](https://cookidoo.thermomix.com)
2. Přihlaste se svým účtem
3. Otevřete Developer Tools (F12)
4. Přejděte na záložku **Application** (Chrome) nebo **Storage** (Firefox)
5. V sekci **Cookies** najděte cookie s názvem `_oauth2_proxy`
6. Zkopírujte **hodnotu** tohoto cookie

**Příklad:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

### Krok 3: Nakonfigurujte MCP server

Otevřete soubor `.cursor-mcp.json` a vložte váš token:

```json
{
  "mcpServers": {
    "cookidoo": {
      "command": "node",
      "args": ["mcp-client.js"],
      "env": {
        "COOKIDOO_API_URL": "http://localhost:5555/api/v1",
        "COOKIDOO_TOKEN": "VÁŠ_TOKEN_ZDE"
      },
      "alwaysAllow": ["get_recipes", "get_collections", "get_recipe", "search_recipes"],
      "requireApproval": ["create_recipe", "create_collection", "add_recipe_to_collection"],
      "workspaceRoot": ".",
      "description": "Cookidoo MCP Server - správa receptů z Cookidoo platformy"
    }
  }
}
```

**Poznámka:** Backend API server v tomto projektu funguje jako proxy mezi MCP klientem a Cookidoo API. Pro produkční použití můžete buď:
- Použít tento backend (viz níže)
- Nebo implementovat přímou komunikaci s Cookidoo API

### Krok 4: (Volitelné) Spusťte backend API server

Pokud chcete používat backend server pro pokročilé funkce:

```bash
cd src/Cookidoo.MCP.Api
dotnet run
```

Server se spustí na `http://localhost:5555`.

## 🎮 Použití v Cursoru

Po nastavení můžete začít používat Cookidoo přímo v Cursoru pomocí Claude AI!

### Příklad 1: Vytvoření receptu

Napište v Cursoru:

```
@cookidoo Vytvoř recept na čokoládový dort s následujícími ingrediencemi:
- 200g mouky
- 150g cukru
- 100g kakaa
- 4 vejce
- 200ml mléka
- 100g másla

Čas přípravy: 20 minut
Čas pečení: 40 minut
Porce: 8
```

Claude automaticky:
1. Vytvoří strukturovaný recept
2. Přidá kroky přípravy
3. Nastaví správné parametry
4. Uloží recept do Cookidoo

### Příklad 2: Vyhledání receptů

```
@cookidoo Najdi všechny recepty s kuřetem, které trvají méně než 30 minut
```

### Příklad 3: Vytvoření kolekce

```
@cookidoo Vytvoř kolekci "Rychlé večeře" a přidej do ní všechny recepty, které trvají méně než 30 minut
```

### Příklad 4: Úprava receptu

```
@cookidoo Uprav recept "Špagety Carbonara" - přidej bazalku do ingrediencí a zvyš počet porcí na 6
```

## 📚 Dostupné nástroje

MCP server poskytuje následující nástroje:

### 📖 Čtení dat

- **get_recipes** - Získá seznam všech vašich receptů
- **get_recipe** - Získá detail konkrétního receptu
- **get_collections** - Získá seznam kolekcí
- **search_recipes** - Vyhledá recepty podle kritérií

### ✏️ Zápis dat

- **create_recipe** - Vytvoří nový recept
- **create_collection** - Vytvoří novou kolekci
- **add_recipe_to_collection** - Přidá recept do kolekce

## 🔑 Autentizace

### Mock token pro testování

Pro testování bez skutečného Cookidoo účtu můžete použít mock token:

```json
{
  "COOKIDOO_TOKEN": "mock-test-token"
}
```

**Upozornění:** Mock token vytvoří recepty pouze lokálně a nebude je synchronizovat s Cookidoo.

### Produkční token

Pro produkční použití je nutné použít skutečný `_oauth2_proxy` token z Cookidoo webu.

**Důležité:**
- Token má omezenou platnost (obvykle 24 hodin)
- Po expiraci je nutné získat nový token
- Token obsahuje přístup k vašemu Cookidoo účtu - nesdílejte ho!

## 🛠️ Pokročilé použití

### Thermomix specifické funkce

Při vytváření receptů můžete specifikovat Thermomix parametry:

```
@cookidoo Vytvoř recept na polévku s následujícími kroky:
1. Nakrájej cibuli: rychlost 5, 5 sekund
2. Opraž cibuli: 100°C, 3 minuty, rychlost 1
3. Přidej vodu a vař: 100°C, 20 minut, rychlost 4
4. Rozmixuj: rychlost 9, 30 sekund
```

### Strukturované recepty

Pro maximální kompatibilitu s Thermomix používejte tento formát:

```json
{
  "name": "Název receptu",
  "description": "Popis receptu",
  "ingredients": [
    {
      "text": "200g mouky",
      "name": "mouka",
      "quantity": 200,
      "unit": "g"
    }
  ],
  "steps": [
    {
      "text": "Smíchejte suché ingredience",
      "order": 1,
      "temperature": null,
      "time": null,
      "speed": 4
    }
  ],
  "preparationTimeMinutes": 15,
  "cookingTimeMinutes": 30,
  "portions": 4,
  "difficulty": 2,
  "tags": ["dezert", "pečení"]
}
```

## 🔍 Troubleshooting

### MCP server se nespustí

**Problém:** `COOKIDOO_TOKEN environment variable is required`

**Řešení:** Zkontrolujte, že jste správně nastavili token v `.cursor-mcp.json`

### Token nefunguje

**Problém:** `401 Unauthorized`

**Řešení:**
1. Zkontrolujte, že token nebyl exspirován
2. Získejte nový token z Cookidoo webu
3. Ujistěte se, že jste zkopírovali celý token včetně všech znaků

### Backend API není dostupný

**Problém:** `Connection refused to localhost:5555`

**Řešení:**
1. Spusťte backend server: `dotnet run --project src/Cookidoo.MCP.Api`
2. Nebo změňte `COOKIDOO_API_URL` v konfiguraci na jiný server

### Recepty se nevytvářejí

**Problém:** MCP volání selhává

**Řešení:**
1. Zkontrolujte logy v konzoli
2. Ověřte, že backend API běží
3. Zkuste použít mock token pro testování: `mock-test-token`

## 📖 Dokumentace

Pro více informací viz:

- [README.md](README.md) - Přehled projektu a architektura
- [MCP_GUIDE.md](MCP_GUIDE.md) - Detailní průvodce MCP protokolem
- [COOKIDOO_API_DOCUMENTATION.md](COOKIDOO_API_DOCUMENTATION.md) - Dokumentace Cookidoo API
- [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md) - Technický přehled implementace

## 💡 Tipy a triky

1. **Používejte tagy** - Označujte recepty tagy pro snadnější organizaci
2. **Vytvářejte kolekce** - Seskupujte podobné recepty do kolekcí
3. **Specifikujte obtížnost** - Pomůže vám to najít recepty podle úrovně
4. **Přidávejte poznámky** - Claude může přidat poznámky k receptům
5. **Využívejte vyhledávání** - Najděte recepty podle ingrediencí

## 🤝 Přispívání

Tento projekt je open-source. Pokud najdete chybu nebo máte nápad na vylepšení:

1. Vytvořte Issue na GitHubu
2. Navrhněte Pull Request
3. Kontaktujte vývojový tým

## 📄 Licence

MIT License - viz [LICENSE](LICENSE)

## ⚠️ Upozornění

Tento projekt je **neoficiální** a není spojen se společností Vorwerk nebo platformou Cookidoo®. Používejte na vlastní odpovědnost.

---

**Vytvořeno s ❤️ pro komunitu Thermomix uživatelů**
