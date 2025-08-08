# Cookidoo API - Dokumentace pro vývojáře

## ⚠️ Důležité upozornění

**Cookidoo NEPOUŽÍVÁ standardní REST API** pro autentizaci! Současná implementace v `CookidooAuthService` je **demonstrační a používá mockované tokeny**.

## Skutečná architektura Cookidoo

### Autentizace
Cookidoo používá **OAuth2 flow** s následujícím postupem:

1. **Webové přihlášení**: Uživatel se přihlašuje přes webový formulář na `https://cookidoo.thermomix.com/profile/en-US/login`
2. **OAuth2 Proxy**: Po úspěšném přihlášení se vytvoří cookie `_oauth2_proxy` obsahující JWT token
3. **API komunikace**: Všechna API volání používají tento cookie token v hlavičce nebo jako cookie

### Endpointy
- **Přihlášení**: Webový formulář, ne API endpoint
- **API volání**: Používají cookie `_oauth2_proxy` pro autorizaci
- **Logout**: Invalidace OAuth2 session a smazání cookie

## Produkční implementace

### Možnost 1: OAuth2 Flow (Doporučeno)
```csharp
// Implementace OAuth2 pro Cookidoo
public class CookidooOAuth2Service
{
    public async Task<string> StartAuthenticationAsync()
    {
        // Redirect na Cookidoo OAuth2 endpoint
        return "https://cookidoo.thermomix.com/oauth2/authorize?client_id=...";
    }
    
    public async Task<string> ExchangeCodeForTokenAsync(string code)
    {
        // Výměna authorization code za access token
    }
}
```

### Možnost 2: Web Scraping s Headless Browser
```csharp
// Použití Playwright nebo Selenium pro automatizované přihlášení
public class CookidooWebAuthService
{
    public async Task<string> LoginWithBrowserAsync(string email, string password)
    {
        // 1. Spustit headless browser
        // 2. Navigovat na login stránku
        // 3. Vyplnit formulář
        // 4. Získat _oauth2_proxy cookie
        // 5. Vrátit cookie hodnotu
    }
}
```

### Možnost 3: Pre-authenticated Cookie
```csharp
// Použití předem získaného cookie z browseru
public class CookidooCookieAuthService
{
    public async Task<bool> ValidatePreAuthenticatedCookieAsync(string cookieValue)
    {
        // Validace existujícího _oauth2_proxy cookie
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Cookie", $"_oauth2_proxy={cookieValue}");
        
        var response = await httpClient.GetAsync("https://cookidoo.thermomix.com/api/user/profile");
        return response.IsSuccessStatusCode;
    }
}
```

## Současná implementace

### Co je implementováno
- ✅ **Demonstrační flow**: Simuluje přihlášení s mockovanými tokeny
- ✅ **Proper error handling**: Správné vyhazování výjimek
- ✅ **Logging**: Kompletní logování všech operací
- ✅ **Validation**: Validace vstupních dat
- ✅ **JWT structure**: Mockované tokeny mají správnou JWT strukturu

### Co NENÍ implementováno
- ❌ **Skutečná komunikace s Cookidoo API**
- ❌ **OAuth2 flow**
- ❌ **Cookie handling**
- ❌ **Web scraping**

## Testování

### Testovací scénáře v současné implementaci
```bash
# Úspěšné přihlášení
curl -X POST http://localhost:5555/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "test@example.com", "password": "password"}'

# Neplatné přihlašovací údaje
curl -X POST http://localhost:5555/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "invalid@example.com", "password": "wrong"}'

# Neplatný email formát
curl -X POST http://localhost:5555/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "invalid-email", "password": "password"}'
```

## Migrace na produkční implementaci

### Krok 1: Registrace OAuth2 aplikace
1. Kontaktovat Vorwerk/Cookidoo support
2. Registrovat aplikaci pro OAuth2
3. Získat `client_id` a `client_secret`

### Krok 2: Implementace OAuth2
1. Nahradit `CookidooAuthService.LoginAsync()` skutečným OAuth2 flow
2. Implementovat callback endpoint pro authorization code
3. Upravit token storage a validation

### Krok 3: Aktualizace konfigurace
```json
{
  "CookidooOAuth2": {
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "AuthorizeUrl": "https://cookidoo.thermomix.com/oauth2/authorize",
    "TokenUrl": "https://cookidoo.thermomix.com/oauth2/token",
    "RedirectUri": "https://yourdomain.com/auth/cookidoo/callback"
  }
}
```

## Poznámky

- **Neoficiální API**: Cookidoo nemá veřejně dostupné API
- **Terms of Service**: Zkontrolujte podmínky použití před implementací
- **Rate limiting**: Implementujte proper rate limiting
- **Error handling**: Cookidoo může změnit své API bez upozornění

## Užitečné odkazy

- [cookidoo-api Python knihovna](https://github.com/miaucl/cookidoo-api)
- [cookiput - Recipe importer](https://github.com/croeer/cookiput)
- [cookidoo-scraper - Web scraper](https://github.com/tobim-dev/cookidoo-scraper) 