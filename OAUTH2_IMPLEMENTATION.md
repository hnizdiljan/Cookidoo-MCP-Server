# OAuth2 Autentizace podle cookidoo-api-master

## Přehled implementace

Byla úspěšně implementována OAuth2 autentizace pro Cookidoo MCP Server podle funkčního Python projektu `cookidoo-api-master`. Stávající HTML form-based přihlášení bylo nahrazeno správným OAuth2 API flow.

## Klíčové změny

### 1. Konfigurace (CookidooOptions.cs)
- ✅ Aktualizovány API endpointy podle cookidoo-api-master
- ✅ Přidány OAuth2 konstanty (ClientId, ClientSecret, AuthorizationHeader)
- ✅ Správné URL pattern pro různé země
- ✅ Endpoint pro token: `ciam/auth/token`
- ✅ Endpoint pro user info: `community/profile`

### 2. Autentizační modely (CookidooAuthModels.cs)
- ✅ `CookidooAuthResponse` - odpověď z OAuth2 API
- ✅ `CookidooUserInfo` - informace o uživateli
- ✅ `CookidooProfileResponse` - wrapper pro user info endpoint
- ✅ `CookidooLocalizationConfig` - lokalizační konfigurace

### 3. CookidooAuthService.cs - Kompletně přepsáno
- ✅ **LoginAsync()** - OAuth2 password grant flow
- ✅ **RefreshTokenAsync()** - obnovení access tokenu
- ✅ **GetUserInfoAsync()** - načtení informací o uživateli
- ✅ **ValidateTokenAsync()** - ověření platnosti tokenu
- ✅ Správné HTTP headers podle cookidoo-api-master
- ✅ Správné error handling a logování

### 4. AuthService.cs - Aktualizováno
- ✅ Integrace s novou CookidooAuthService
- ✅ Použití CookidooAuthResponse místo string tokenu
- ✅ Správné mapování user ID z OAuth2 response

### 5. Konfigurace (appsettings.json)
- ✅ Aktualizovány všechny endpointy
- ✅ Přidány OAuth2 konstanty z cookidoo-api-master
- ✅ Správné API endpoint pattern

## OAuth2 Flow podle cookidoo-api-master

### 1. Login Request
```http
POST https://ch.tmmobile.vorwerk-digital.com/ciam/auth/token
Content-Type: application/x-www-form-urlencoded
Authorization: Basic a3VwZmVyd2Vyay1jbGllbnQtbndvdDpMczUwT04xd285U3FzMWRDZEpnZQ==

grant_type=password&username={email}&password={password}&client_id=kupferwerk-client-nwot
```

### 2. Login Response
```json
{
  "access_token": "eyJ...",
  "refresh_token": "eyJ...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "sub": "user-id"
}
```

### 3. User Info Request
```http
GET https://ch.tmmobile.vorwerk-digital.com/community/profile
Authorization: Bearer {access_token}
```

### 4. User Info Response
```json
{
  "userInfo": {
    "username": "user@example.com",
    "description": null,
    "picture": "https://..."
  }
}
```

## Konstanty z cookidoo-api-master

```python
COOKIDOO_CLIENT_ID = "kupferwerk-client-nwot"
COOKIDOO_CLIENT_SECRET = "Ls50ON1woySqs1dCdJge"
COOKIDOO_AUTHORIZATION_HEADER = "Basic a3VwZmVyd2Vyay1jbGllbnQtbndvdDpMczUwT04xd285U3FzMWRDZEpnZQ=="
```

## Testování

### Sestavení projektu
```bash
cd src
dotnet build Cookidoo.MCP.Infrastructure  # ✅ Úspěšné
```

### Test OAuth2 implementace
```bash
cd src
dotnet run --project TestOAuth2.cs
```

## Bezpečnostní aspekty

### ✅ Implementováno
- OAuth2 password grant flow
- Správné HTTP headers a authorization
- Bezpečné uložení credentials v konfiguraci
- Error handling pro různé HTTP status kódy
- Logování bez citlivých dat

### 🔄 Doporučení pro produkci
- Použít Azure Key Vault pro credentials
- Implementovat rate limiting
- Přidat retry policy s exponential backoff
- Monitorování a alerting

## Kompatibilita s cookidoo-api-master

| Funkce | Python | C# | Status |
|--------|--------|----|---------| 
| OAuth2 Login | ✅ | ✅ | Implementováno |
| Refresh Token | ✅ | ✅ | Implementováno |
| User Info | ✅ | ✅ | Implementováno |
| Token Validation | ✅ | ✅ | Implementováno |
| Error Handling | ✅ | ✅ | Implementováno |
| Localization | ✅ | ✅ | Implementováno |

## Další kroky

1. **Testování s reálnými údaji** - Otestovat s platnými Cookidoo přihlašovacími údaji
2. **Implementace recipe API** - Použít access token pro volání recipe endpointů
3. **Implementace collections API** - Použít access token pro správu kolekcí
4. **Refresh token handling** - Automatické obnovení tokenů před expirací
5. **Error recovery** - Lepší handling síťových chyb a timeoutů

## Závěr

✅ **OAuth2 autentizace byla úspěšně implementována podle cookidoo-api-master**

Stávající HTML form-based přihlášení bylo nahrazeno správným OAuth2 API flow s použitím stejných konstant a endpointů jako ve funkčním Python projektu. Implementace je připravena pro testování s reálnými Cookidoo přihlašovacími údaji. 