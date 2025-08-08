# 🍳 Cookidoo MCP Server - Průvodce použitím

## 📋 Obsah
- [Přehled](#přehled)
- [Spuštění serveru](#spuštění-serveru)
- [API dokumentace (Swagger)](#api-dokumentace-swagger)
- [Autentizace](#autentizace)
- [Použití v Cursoru](#použití-v-cursoru)
- [Příklady API volání](#příklady-api-volání)
- [Troubleshooting](#troubleshooting)

## 🎯 Přehled

Cookidoo MCP Server poskytuje RESTful API pro správu receptů a kolekcí receptů z Cookidoo platformy. Server umožňuje:

- 🔐 **Autentizaci** s Cookidoo účtem
- 📝 **Správu receptů** (vytváření, editace, načítání, mazání)
- 📚 **Správu kolekcí** receptů
- 🔄 **Synchronizaci** s Cookidoo platformou

## 🚀 Spuštění serveru

### Lokální spuštění

```bash
# Klonování repozitáře
git clone <repository-url>
cd Cookidoo-MCP-Server

# Build projektu
dotnet build

# Spuštění serveru
dotnet run --project src/Cookidoo.MCP.Api
```

Server se spustí na:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`

### Docker spuštění

```bash
# Build Docker image
docker build -t cookidoo-mcp-server .

# Spuštění kontejneru
docker run -p 5000:5000 -p 5001:5001 cookidoo-mcp-server
```

## 📚 API dokumentace (Swagger)

Po spuštění serveru je k dispozici interaktivní Swagger dokumentace:

**URL**: `http://localhost:5000` (nebo port na kterém server běží)

### Swagger funkce:
- 📖 **Kompletní API dokumentace** všech endpointů
- 🧪 **Interaktivní testování** API volání
- 🔑 **JWT autentizace** přímo v rozhraní
- 📝 **Schéma modelů** pro requesty/responses
- ⏱️ **Měření času odpovědi**

### Jak používat Swagger:

1. **Otevřete prohlížeč** a přejděte na `http://localhost:5000`
2. **Přihlášení**: Použijte endpoint `POST /api/v1/auth/login`
3. **Kopírování tokenu**: Z odpovědi zkopírujte `accessToken`
4. **Autentizace**: Klikněte na "Authorize" a vložte `Bearer <token>`
5. **Testování**: Nyní můžete testovat všechny API endpointy

## 🔐 Autentizace

### 1. Přihlášení

```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "vas-cookidoo@email.com",
  "password": "vase-heslo"
}
```

**Odpověď:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "cookidooToken": "cookidoo_token_xyz...",
  "userId": "user-id",
  "email": "vas-cookidoo@email.com",
  "expiresAt": "2025-06-05T16:00:00Z"
}
```

### 2. Použití tokenu

Do všech dalších požadavků přidejte header:
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 3. Obnovení tokenu

```http
POST /api/v1/auth/refresh
Content-Type: application/json
Authorization: Bearer <starý-token>

{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

## 🖥️ Použití v Cursoru

### Model Context Protocol (MCP)

Cursor podporuje MCP protokol pro integraci s externími službami. Zde je návod na napojení:

#### 1. Konfigurace MCP v Cursoru

Vytvořte konfigurační soubor `.cursor-mcp.json` v root adresáři:

```json
{
  "mcpServers": {
    "cookidoo": {
      "command": "node",
      "args": ["mcp-client.js"],
      "env": {
        "COOKIDOO_API_URL": "http://localhost:5000/api/v1",
        "COOKIDOO_TOKEN": "your-jwt-token-here"
      }
    }
  }
}
```

#### 2. MCP Client Script

Vytvořte `mcp-client.js`:

```javascript
const { Server } = require('@modelcontextprotocol/sdk/server/index.js');
const { StdioServerTransport } = require('@modelcontextprotocol/sdk/server/stdio.js');

const server = new Server(
  {
    name: "cookidoo-mcp-server",
    version: "1.0.0"
  },
  {
    capabilities: {
      tools: {}
    }
  }
);

// Tool pro získání receptů
server.setRequestHandler('tools/call', async (request) => {
  const { name, arguments: args } = request.params;
  
  switch (name) {
    case 'get_recipes':
      return await getRecipes(args);
    case 'create_recipe':
      return await createRecipe(args);
    case 'get_collections':
      return await getCollections(args);
    default:
      throw new Error(`Unknown tool: ${name}`);
  }
});

async function getRecipes(args) {
  const response = await fetch(`${process.env.COOKIDOO_API_URL}/recipes`, {
    headers: {
      'Authorization': `Bearer ${process.env.COOKIDOO_TOKEN}`,
      'Content-Type': 'application/json'
    }
  });
  
  return {
    content: [
      {
        type: "text",
        text: JSON.stringify(await response.json(), null, 2)
      }
    ]
  };
}

// Spuštění serveru
const transport = new StdioServerTransport();
server.connect(transport);
```

#### 3. Použití v Cursoru

Po konfiguraci můžete v Cursoru používat příkazy jako:

```
@cookidoo Najdi všechny recepty s kuřetem
@cookidoo Vytvoř nový recept na špagety carbonara
@cookidoo Zobraz moje kolekce receptů
```

### Alternativní přístup - HTTP Client

Pokud MCP není k dispozici, můžete používat HTTP client přímo:

```typescript
// cookidoo-client.ts
export class CookidooClient {
  private baseUrl = 'http://localhost:5000/api/v1';
  private token: string | null = null;

  async login(email: string, password: string) {
    const response = await fetch(`${this.baseUrl}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    });
    
    const data = await response.json();
    this.token = data.accessToken;
    return data;
  }

  async getRecipes() {
    return this.apiCall('/recipes');
  }

  async createRecipe(recipe: any) {
    return this.apiCall('/recipes', 'POST', recipe);
  }

  private async apiCall(endpoint: string, method = 'GET', body?: any) {
    const response = await fetch(`${this.baseUrl}${endpoint}`, {
      method,
      headers: {
        'Authorization': `Bearer ${this.token}`,
        'Content-Type': 'application/json'
      },
      body: body ? JSON.stringify(body) : undefined
    });
    
    return response.json();
  }
}
```

## 📋 Příklady API volání

### Recepty

#### Získání všech receptů
```http
GET /api/v1/recipes
Authorization: Bearer <token>
```

#### Vytvoření nového receptu
```http
POST /api/v1/recipes
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Špagety Carbonara",
  "description": "Klasické italské těstoviny",
  "ingredients": [
    {
      "text": "400g špaget",
      "name": "špagety",
      "quantity": 400,
      "unit": "g"
    },
    {
      "text": "200g pancetta",
      "name": "pancetta", 
      "quantity": 200,
      "unit": "g"
    }
  ],
  "steps": [
    {
      "text": "Uvařte těstoviny podle návodu",
      "order": 1
    },
    {
      "text": "Osmažte pancettu do zlatova",
      "order": 2
    }
  ],
  "preparationTimeMinutes": 15,
  "cookingTimeMinutes": 20,
  "portions": 4,
  "difficulty": 2,
  "tags": ["pasta", "italské", "rychlé"],
  "isPublic": false
}
```

#### Editace receptu
```http
PUT /api/v1/recipes/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Špagety Carbonara - Vylepšená verze",
  "preparationTimeMinutes": 10
}
```

### Kolekce

#### Získání kolekcí
```http
GET /api/v1/collections
Authorization: Bearer <token>
```

#### Vytvoření kolekce
```http
POST /api/v1/collections
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Rychlá večeře",
  "description": "Recepty na rychlou přípravu večeře",
  "tags": ["rychlé", "večeře"],
  "isPublic": false
}
```

#### Přidání receptu do kolekce
```http
POST /api/v1/collections/{collectionId}/recipes
Authorization: Bearer <token>
Content-Type: application/json

{
  "recipeId": "recipe-id-xyz"
}
```

## 🔧 Troubleshooting

### Časté problémy

#### 1. Server se nespustí
```bash
# Kontrola portů
netstat -an | findstr :5000

# Spuštění na jiném portu
dotnet run --project src/Cookidoo.MCP.Api --urls "http://localhost:5555"
```

#### 2. Chyba autentizace
- Zkontrolujte správnost emailu a hesla
- Ověřte, že máte platný Cookidoo účet
- Zkontrolujte expiraci JWT tokenu

#### 3. CORS chyby
Server má nastavenou CORS politiku `AllowAll` pro development. V produkci upravte v `Program.cs`:

```csharp
options.AddPolicy("Production", policy =>
{
    policy.WithOrigins("https://cursor.sh", "https://localhost:3000")
          .AllowAnyMethod()
          .AllowAnyHeader();
});
```

#### 4. SSL/TLS chyby
Pro development můžete ignorovat SSL certifikáty:

```bash
# Windows
set NODE_TLS_REJECT_UNAUTHORIZED=0

# Linux/Mac
export NODE_TLS_REJECT_UNAUTHORIZED=0
```

### Logování

Server používá Serilog pro logování. Logy najdete v:
- **Konzole**: Během běhu aplikace
- **Soubory**: `logs/cookidoo-mcp-{datum}.txt`

Úroveň logování můžete změnit v `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

### Health Check

Zkontrolujte stav serveru:
```http
GET /health
```

Mělo by vrátit `200 OK` pokud server běží správně.

## 🤝 Podpora

Pokud narazíte na problémy:

1. **Zkontrolujte logy** v `logs/` adresáři
2. **Ověřte konfiguraci** v `appsettings.json`
3. **Testujte API** přes Swagger UI
4. **Reportujte chyby** s detailním popisem a logy

---

*Tento průvodce pokrývá základní použití Cookidoo MCP serveru. Pro pokročilé funkce a konfiguraci si přečtěte hlavní README.md.* 