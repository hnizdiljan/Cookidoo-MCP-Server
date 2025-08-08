# Cookidoo MCP Server

RESTful API server pro správu vlastních receptů a kolekcí receptů z platformy Cookidoo® od společnosti Vorwerk (pro zařízení Thermomix®).

## 🚀 Rychlý start

### Požadavky
- .NET 8 SDK
- Platný Cookidoo účet
- Cookidoo JWT token (`_oauth2_proxy` cookie)

### Spuštění serveru

```bash
cd src/Cookidoo.MCP.Api
dotnet run
```

Server bude dostupný na: `http://localhost:5555`

### Swagger dokumentace
Interaktivní API dokumentace je dostupná na: `http://localhost:5555/swagger`

## 🔑 Autentizace

Server používá JWT token z Cookidoo (`_oauth2_proxy` cookie) pro autentizaci, podobně jako projekt `cookiput`.

### Jak získat JWT token:

1. **Přihlaste se do Cookidoo** v prohlížeči na https://cookidoo.de
2. **Otevřete Developer Tools** (F12)
3. **Přejděte na záložku Application** (nebo Storage)
4. **Najděte Cookies** pro cookidoo.de
5. **Zkopírujte hodnotu** `_oauth2_proxy` cookie

### Použití tokenu v API:

**Option 1: Authorization header**
```bash
curl -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     http://localhost:5555/api/v1/recipes
```

**Option 2: Query parametr**
```bash
curl "http://localhost:5555/api/v1/recipes?jwt_token=YOUR_JWT_TOKEN"
```

**Option 3: Custom header**
```bash
curl -H "jwt_token: YOUR_JWT_TOKEN" \
     http://localhost:5555/api/v1/recipes
```

## 📚 API Endpointy

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

## 🛠️ Příklady použití

### Vytvoření nového receptu

```bash
curl -X POST "http://localhost:5555/api/v1/recipes" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -H "Content-Type: application/json" \
     -d '{
       "name": "Můj nový recept",
       "description": "Popis receptu",
       "ingredients": [
         {
           "name": "Mouka",
           "quantity": "500",
           "unit": "g"
         }
       ],
       "steps": [
         {
           "description": "Smíchejte ingredience",
           "order": 1
         }
       ],
       "preparationTimeMinutes": 15,
       "cookingTimeMinutes": 30,
       "portions": 4
     }'
```

### Vytvoření nové kolekce

```bash
curl -X POST "http://localhost:5555/api/v1/collections" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -H "Content-Type: application/json" \
     -d '{
       "name": "Moje oblíbené recepty",
       "description": "Kolekce nejlepších receptů"
     }'
```

## ⚙️ Konfigurace

Konfiguraci lze upravit v `appsettings.json`:

```json
{
  "Cookidoo": {
    "BaseUrl": "https://cookidoo.de",
    "DefaultLanguage": "de-DE",
    "DefaultCountryCode": "de",
    "TimeoutSeconds": 30,
    "UserAgent": "troet",
    "LogHttpRequests": true
  }
}
```

## 🏗️ Architektura

Projekt je rozdělen do tří vrstev:

- **Cookidoo.MCP.Api** - ASP.NET Core Web API vrstva
- **Cookidoo.MCP.Core** - Business logika a entity
- **Cookidoo.MCP.Infrastructure** - Komunikace s Cookidoo API

## 🔧 Vývoj

### Spuštění v Development módu

```bash
cd src/Cookidoo.MCP.Api
dotnet run --environment Development
```

### Testování

```bash
dotnet test
```

### Build

```bash
dotnet build
```

## 📝 Poznámky

- Server **neukládá** žádné přihlašovací údaje
- JWT token se **neuchovává** trvale na serveru
- Všechny operace jsou **synchronizovány** s Cookidoo platformou
- Implementace je inspirována projekty `cookiput` a `cookidoo-api`

## 🐛 Řešení problémů

### Token není platný
- Ověřte, že jste přihlášeni do Cookidoo
- Zkontrolujte, že token není vypršelý
- Získejte nový token z prohlížeče

### Chyby komunikace s Cookidoo
- Zkontrolujte internetové připojení
- Ověřte, že Cookidoo služby jsou dostupné
- Zkontrolujte logy serveru pro detailní chybové zprávy

## 📄 Licence

MIT License - viz LICENSE soubor pro detaily. 