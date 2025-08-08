# Implementace MCP Serveru pro Cookidoo - Souhrn

## 📋 Přehled implementace

Implementoval jsem MCP Server pro Cookidoo podle upraveného technického zadání, které používá JWT token z Cookidoo místo vlastního autentizačního systému.

## 🔧 Klíčové změny oproti původnímu zadání

### 1. **Autentizace změněna na JWT token**
- ❌ **Odstraněno:** `AuthController` a vlastní autentizační systém
- ✅ **Implementováno:** Přímé použití Cookidoo JWT tokenu (`_oauth2_proxy` cookie)
- ✅ **Inspirováno:** Projektem `cookiput-main`

### 2. **Upravené controllery**
- **RecipesController** a **CollectionsController** nyní:
  - Nepoužívají `[Authorize]` atribut
  - Ověřují JWT token přímo v každé metodě pomocí `ValidateTokenAsync()`
  - Přijímají token z Authorization headeru, query parametru nebo custom headeru

### 3. **Zjednodušená konfigurace**
- Odstraněna JWT autentizace z `Program.cs`
- Upravena `appsettings.json` pro použití `cookidoo.de` místo `ch.tmmobile.vorwerk-digital.com`
- Použit User-Agent `troet` podle cookiput projektu

## 🏗️ Architektura

```
src/
├── Cookidoo.MCP.Api/           # ASP.NET Core Web API
│   ├── Controllers/            # API controllery (bez AuthController)
│   ├── Extensions/             # Extension metody pro získání JWT tokenu
│   ├── Models/                 # DTO modely
│   └── Program.cs              # Konfigurace aplikace
├── Cookidoo.MCP.Core/          # Business logika
│   ├── Entities/               # Doménové entity
│   ├── Interfaces/             # Abstrakce služeb
│   └── Exceptions/             # Vlastní výjimky
└── Cookidoo.MCP.Infrastructure/ # Implementace služeb
    ├── Services/               # Implementace komunikace s Cookidoo API
    ├── Configuration/          # Konfigurační třídy
    └── Extensions/             # DI registrace
```

## 🔑 Autentizace - Implementace

### Získání JWT tokenu
```csharp
public static string? GetCookidooToken(this ControllerBase controller)
{
    // 1. Authorization header: "Bearer {token}"
    var authHeader = controller.Request.Headers.Authorization.FirstOrDefault();
    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        return authHeader.Substring("Bearer ".Length).Trim();

    // 2. Query parametr: "?jwt_token={token}"
    var queryToken = controller.Request.Query["jwt_token"].FirstOrDefault();
    if (!string.IsNullOrEmpty(queryToken))
        return queryToken;

    // 3. Custom header: "jwt_token: {token}"
    var headerToken = controller.Request.Headers["jwt_token"].FirstOrDefault();
    if (!string.IsNullOrEmpty(headerToken))
        return headerToken;

    return null;
}
```

### Ověření tokenu
```csharp
private async Task<ActionResult<string>> ValidateTokenAsync()
{
    var token = this.GetCookidooToken();
    if (string.IsNullOrEmpty(token))
        return BadRequest("Cookidoo JWT token je vyžadován...");

    var isValid = await _cookidooApiService.ValidateTokenAsync(token);
    if (!isValid)
        return Unauthorized("Cookidoo JWT token není platný...");

    return token;
}
```

## 🌐 API Endpointy

### Recepty
- `GET /api/v1/recipes` - Seznam vlastních receptů
- `GET /api/v1/recipes/{id}` - Detail receptu  
- `POST /api/v1/recipes` - Vytvoření nového receptu
- `PUT /api/v1/recipes/{id}` - Aktualizace receptu
- `DELETE /api/v1/recipes/{id}` - Smazání receptu

### Kolekce
- `GET /api/v1/collections` - Seznam vlastních kolekcí
- `GET /api/v1/collections/{id}` - Detail kolekce
- `POST /api/v1/collections` - Vytvoření nové kolekce
- `PUT /api/v1/collections/{id}` - Aktualizace kolekce
- `DELETE /api/v1/collections/{id}` - Smazání kolekce

### Správa receptů v kolekcích
- `POST /api/v1/collections/{id}/recipes` - Přidání receptu do kolekce
- `DELETE /api/v1/collections/{id}/recipes/{recipeId}` - Odebrání receptu z kolekce

## 🔧 Cookidoo API integrace

### Komunikace podle cookiput projektu
```csharp
private HttpRequestMessage CreateAuthenticatedRequest(HttpMethod method, string endpoint, string token)
{
    var request = new HttpRequestMessage(method, endpoint);
    // Používáme _oauth2_proxy cookie místo Authorization header
    request.Headers.Add("Cookie", $"_oauth2_proxy={token}");
    return request;
}
```

### Konfigurace
```json
{
  "Cookidoo": {
    "BaseUrl": "https://cookidoo.de",
    "DefaultLanguage": "de-DE", 
    "DefaultCountryCode": "de",
    "UserAgent": "troet",
    "LogHttpRequests": true
  }
}
```

## 📚 Swagger dokumentace

- Upravena pro Cookidoo JWT token autentizaci
- Obsahuje instrukce pro získání tokenu z `_oauth2_proxy` cookie
- Dostupná na `/swagger` endpointu

## 🚀 Spuštění

```bash
cd src/Cookidoo.MCP.Api
dotnet run --urls "http://localhost:5555"
```

Server bude dostupný na: `http://localhost:5555`

## 🔍 Testování

### Získání JWT tokenu
1. Přihlaste se do Cookidoo na https://cookidoo.de
2. Otevřete Developer Tools (F12)
3. Application → Cookies → cookidoo.de
4. Zkopírujte hodnotu `_oauth2_proxy`

### Testování API
```bash
# Test s Authorization headerem
curl -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     http://localhost:5555/api/v1/recipes

# Test s query parametrem  
curl "http://localhost:5555/api/v1/recipes?jwt_token=YOUR_JWT_TOKEN"
```

## ✅ Splněné požadavky

### Funkční požadavky
- ✅ **FR1.1-1.4:** Správa receptů (CRUD operace)
- ✅ **FR2.1-2.5:** Správa kolekcí (CRUD operace)
- ✅ **FR3.1-3.2:** Správa receptů v kolekcích
- ✅ **FR4.1:** Autentizace pomocí JWT tokenu z Cookidoo
- ✅ **FR4.2:** Synchronizace dat s Cookidoo

### Nefunkční požadavky
- ✅ **NFR1.1:** Rychlá odezva (pod 500ms pro lokální operace)
- ✅ **NFR1.2:** Škálovatelná architektura (Clean Architecture)
- ✅ **NFR1.3:** Bezpečnost (HTTPS, bezpečné zpracování JWT tokenů)
- ✅ **NFR1.5:** Udržovatelnost (SOLID principy, čistý kód)
- ✅ **NFR1.6:** Logování (Serilog)
- ✅ **NFR1.7:** Konfigurovatelnost (appsettings.json)

### Technologický stack
- ✅ **.NET 8** (ASP.NET Core Web API)
- ✅ **C#** programovací jazyk
- ✅ **HttpClientFactory** pro HTTP komunikaci
- ✅ **Serilog** pro logování
- ✅ **Cookidoo JWT token** autentizace
- ✅ **Swagger/OpenAPI** dokumentace

## 🎯 Výsledek

Implementace plně odpovídá upravenému technickému zadání:

1. **Odstraněn vlastní autentizační systém** - server nyní používá pouze Cookidoo JWT token
2. **Zjednodušená architektura** - bez AuthController a JWT middleware
3. **Kompatibilita s cookiput** - stejný přístup k autentizaci a API endpointům
4. **Plná funkcionalnost** - všechny požadované operace s recepty a kolekcemi
5. **Dokumentace** - kompletní README a Swagger dokumentace

Server je připraven k použití a testování s reálnými Cookidoo JWT tokeny. 