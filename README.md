# Cookidoo MCP Server

MCP Server pro správu vlastních receptů a kolekcí receptů uživatelů platformy Cookidoo® od společnosti Vorwerk (pro zařízení Thermomix®).

## 🎯 Rychlý Start

**Nový uživatel?** Začněte s [QUICK_START.md](QUICK_START.md) - kompletní průvodce v češtině, jak nastavit a používat Cookidoo MCP Server v Cursoru!

## 🚀 Přehled

Tento projekt poskytuje backendové služby pro:
- **Vytváření nových receptů** kompatibilních s formátem Cookidoo včetně plné podpory Thermomix parametrů
- **Editaci existujících vlastních receptů** synchronizovaných s Cookidoo
- **Vytváření vlastních kolekcí receptů**
- **Editaci detailů kolekcí receptů**
- **Přidávání a odebírání receptů do/z kolekcí**
- **Správu nákupního seznamu** (ingredience z receptů + vlastní položky)
- **Plánování jídel** (týdenní kalendář receptů)
- **Automatické přihlášení** s cachováním tokenů
- **Bezpečnou autentizaci** vůči Cookidoo API

## 🏗️ Architektura

Projekt je navržen podle principů Clean Architecture a skládá se z následujících vrstev:

```
├── src/
│   ├── Cookidoo.MCP.Api/           # API vrstva (ASP.NET Core Web API)
│   ├── Cookidoo.MCP.Core/          # Doménová vrstva (entity, interfaces)
│   └── Cookidoo.MCP.Infrastructure/ # Infrastrukturní vrstva (služby, externí API)
└── tests/
    └── Cookidoo.MCP.Tests/         # Unit testy
```

### Komponenty

- **API vrstva**: Zpracování HTTP požadavků, validace vstupů, autentizace/autorizace
- **Servisní vrstva**: Business logika pro správu receptů a kolekcí
- **Integrační vrstva**: Komunikace s externím Cookidoo API
- **Autentizace**: JWT tokeny pro MCP API + správa Cookidoo tokenů

## 💻 Technologický stack

- **.NET 8** (ASP.NET Core Web API)
- **C#** s nullable reference types
- **JWT** autentizace
- **Serilog** pro logování
- **Swagger/OpenAPI** pro dokumentaci API
- **xUnit, Moq, FluentAssertions** pro testování

## 🛠️ Instalace a spuštění

### Předpoklady

- .NET 8 SDK
- Visual Studio 2022 nebo VS Code

### Spuštění

1. **Klonování repozitáře**
   ```bash
   git clone <repository-url>
   cd Cookidoo-MCP-Server
   ```

2. **Obnovení balíčků**
   ```bash
   dotnet restore
   ```

3. **Spuštění aplikace**
   ```bash
   cd src/Cookidoo.MCP.Api
   dotnet run
   ```

4. **Přístup k API**
   - Swagger UI: `https://localhost:5001`
   - API: `https://localhost:5001/api/v1/`

### Konfigurace

Hlavní konfigurace v `appsettings.json`:

```json
{
  "Cookidoo": {
    "BaseUrl": "https://cookidoo.thermomix.com",
    "ApiVersion": "v1",
    "Timeout": "00:00:30",
    "UserAgent": "Cookidoo-MCP-Server/1.0"
  },
  "Jwt": {
    "SecretKey": "SuperSecretKeyForJwtTokenGeneration123456789",
    "Issuer": "CookidooMcpServer",
    "Audience": "CookidooMcpApi",
    "ExpirationMinutes": 60
  }
}
```

## 📚 API Dokumentace

### Autentizace

#### POST `/api/v1/auth/login`
Přihlášení uživatele pomocí Cookidoo přihlašovacích údajů.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response:**
```json
{
  "token": "jwt-token-here",
  "expiresAt": "2024-01-01T12:00:00Z",
  "user": {
    "id": "user-id",
    "email": "user@example.com"
  }
}
```

#### POST `/api/v1/auth/refresh`
Obnovení JWT tokenu.

#### POST `/api/v1/auth/logout`
Odhlášení uživatele.

#### GET `/api/v1/auth/verify`
Ověření platnosti tokenu.

### Recepty

#### GET `/api/v1/recipes`
Získání seznamu receptů s filtrováním a stránkováním.

**Parametry:**
- `page` (int): Číslo stránky (výchozí: 1)
- `pageSize` (int): Velikost stránky (výchozí: 20, max: 100)
- `search` (string): Vyhledávací řetězec
- `difficulty` (enum): Filtr podle obtížnosti (Easy, Medium, Hard)
- `tags` (string): Tagy oddělené čárkou

#### GET `/api/v1/recipes/{id}`
Získání detailu konkrétního receptu.

#### POST `/api/v1/recipes`
Vytvoření nového receptu.

**Request:**
```json
{
  "name": "Název receptu",
  "description": "Popis receptu",
  "ingredients": [
    {
      "text": "200g mouky",
      "quantity": 200,
      "unit": "g"
    }
  ],
  "steps": [
    {
      "text": "Smíchejte ingredience",
      "order": 1
    }
  ],
  "preparationTimeMinutes": 30,
  "cookingTimeMinutes": 45,
  "portions": 4,
  "difficulty": "Medium",
  "tags": ["dezert", "čokoláda"]
}
```

#### PUT `/api/v1/recipes/{id}`
Aktualizace existujícího receptu.

#### DELETE `/api/v1/recipes/{id}`
Smazání receptu.

#### GET `/api/v1/recipes/search`
Vyhledání receptů podle dotazu.

### Kolekce

#### GET `/api/v1/collections`
Získání seznamu kolekcí.

#### GET `/api/v1/collections/{id}`
Získání detailu kolekce.

#### POST `/api/v1/collections`
Vytvoření nové kolekce.

#### PUT `/api/v1/collections/{id}`
Aktualizace kolekce.

#### DELETE `/api/v1/collections/{id}`
Smazání kolekce.

#### POST `/api/v1/collections/{id}/recipes`
Přidání receptu do kolekce.

#### DELETE `/api/v1/collections/{id}/recipes/{recipeId}`
Odebrání receptu z kolekce.

#### GET `/api/v1/collections/{id}/recipes`
Získání receptů v kolekci.

### Nákupní seznam

#### GET `/api/v1/shoppinglist`
Získá kompletní nákupní seznam s ingrediencemi z receptů a vlastními položkami.

#### POST `/api/v1/shoppinglist/recipes`
Přidá ingredience z receptů do nákupního seznamu.

**Request:**
```json
{
  "recipeIds": ["recipe-id-1", "recipe-id-2"]
}
```

#### DELETE `/api/v1/shoppinglist/recipes`
Odebere ingredience receptů z nákupního seznamu.

#### PATCH `/api/v1/shoppinglist/ingredients/ownership`
Označí ingredience jako zakoupené.

**Request:**
```json
{
  "ingredientIds": ["ing-1", "ing-2"]
}
```

#### POST `/api/v1/shoppinglist/items`
Přidá vlastní položky do nákupního seznamu.

**Request:**
```json
{
  "items": ["Toaletní papír", "Máslo"]
}
```

#### PATCH `/api/v1/shoppinglist/items/ownership`
Označí vlastní položky jako zakoupené.

#### DELETE `/api/v1/shoppinglist/items`
Odebere vlastní položky z nákupního seznamu.

#### DELETE `/api/v1/shoppinglist`
Vymaže celý nákupní seznam.

### Plánování jídel

#### GET `/api/v1/mealplan/week`
Získá plán jídel pro daný týden.

**Parametry:**
- `date` (string): Datum v týdnu (formát YYYY-MM-DD), volitelné

#### GET `/api/v1/mealplan/day`
Získá plán jídel pro konkrétní den.

**Parametry:**
- `date` (string): Datum (formát YYYY-MM-DD)

#### POST `/api/v1/mealplan/recipes`
Přidá recepty do kalendáře.

**Request:**
```json
{
  "date": "2025-11-22",
  "recipeIds": ["recipe-id-1"],
  "mealType": "Oběd"
}
```

#### DELETE `/api/v1/mealplan/recipes/{recipeId}`
Odebere recept z kalendáře.

**Parametry:**
- `date` (string): Datum (formát YYYY-MM-DD)

## 🔒 Bezpečnost

- **JWT autentizace**: Všechny endpointy (kromě přihlášení) vyžadují platný JWT token
- **HTTPS**: Veškerá komunikace je šifrována
- **Validace vstupů**: Důsledná validace všech vstupních dat
- **Rate limiting**: Ochrana proti zneužití API (doporučeno implementovat)

## 🧪 Testování

Spuštění testů:

```bash
dotnet test
```

Spuštění testů s pokrytím:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 📝 Logování

Aplikace používá Serilog pro strukturované logování:

- **Konzole**: Výstup do konzole během vývoje
- **Soubory**: Rotující soubory v `logs/` složce
- **Strukturované logy**: JSON formát pro lepší analýzu

## 🚧 Známé limitace

1. **Cookidoo API integrace**: Současná implementace používá simulované volání. Pro produkční použití je nutné implementovat skutečnou integraci s Cookidoo API.

2. **Token management**: Cookidoo tokeny jsou momentálně simulované. V produkci je nutné implementovat skutečné získávání a správu tokenů.

3. **Databáze**: Aplikace momentálně nepoužívá perzistentní úložiště. Všechna data jsou získávána z Cookidoo API.

## 📖 Dokumentace

- **[QUICK_START.md](QUICK_START.md)** - Rychlý start průvodce pro nové uživatele
- **[THERMOMIX_GUIDE.md](THERMOMIX_GUIDE.md)** - Kompletní průvodce Thermomix parametry
- **[AUTO_LOGIN_GUIDE.md](AUTO_LOGIN_GUIDE.md)** - Průvodce automatickým přihlášením
- **[NEW_TOOLS_PROPOSAL.md](NEW_TOOLS_PROPOSAL.md)** - Analýza a návrh nových MCP tools

## 🔮 Budoucí rozšíření

- **Import receptů** z jiných formátů
- **Pokročilé vyhledávání** a filtrování
- **Offline podpora** s cachingem
- **Sdílení receptů** mezi uživateli
- **Nutriční kalkulačka**
- **Oficiální Cookidoo kolekce** (managed collections)
- **Rozšířené informace o receptu** (nutriční hodnoty, nádobí, kategorie)

## 🤝 Přispívání

1. Fork projektu
2. Vytvořte feature branch (`git checkout -b feature/amazing-feature`)
3. Commitněte změny (`git commit -m 'Add amazing feature'`)
4. Push do branch (`git push origin feature/amazing-feature`)
5. Otevřete Pull Request

## 📄 Licence

Tento projekt je licencován pod MIT licencí - viz [LICENSE](LICENSE) soubor pro detaily.

## 📞 Kontakt

Pro otázky a podporu kontaktujte vývojový tým.

---

**Poznámka**: Tento projekt je nezávislý a není oficiálně spojen se společností Vorwerk nebo platformou Cookidoo®.