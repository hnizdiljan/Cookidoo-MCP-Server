# Technické zadání: Implementace Cookidoo AuthController v .NET 8

---

## 1. Cíl 🎯

Cílem je vytvořit v **.NET 8** API controller (`AuthController`), který bude spravovat autentizaci uživatelů vůči oficiálnímu API platformy **Cookidoo**. Controller bude sloužit jako Backend-for-Frontend (BFF), který obslouží přihlášení a odhlášení uživatele a bude spravovat autentizační tokeny.

---

## 2. Architektura a kontext 🏗️

Controller bude součástí **.NET 8 Web API** projektu. Bude přijímat požadavky od klienta (např. webová nebo mobilní aplikace), volat externí Cookidoo API a vracet klientovi výsledek, včetně JWT tokenu pro další autorizovanou komunikaci.

**Základní princip:** Naše aplikace se bude vůči Cookidoo API chovat jako standardní webový klient. Po úspěšném přihlášení získáme JWT token, který si náš klient uloží a bude ho posílat v hlavičce `Authorization` při každém dalším požadavku na naše API. Naše API pak tento token použije pro autorizovanou komunikaci s Cookidoo API.

---

## 3. Požadavky na implementaci 📋

### 3.1. Závislosti

* **ASP.NET Core 8:** Základní framework.
* **`IHttpClientFactory`:** Pro správu a efektivní využívání instancí `HttpClient` pro volání na Cookidoo API.
* **`System.Text.Json` nebo `Newtonsoft.Json`:** Pro serializaci a deserializaci JSON objektů.

### 3.2. Konfigurace

V `appsettings.json` budou uloženy základní konfigurační hodnoty:

```json
{
  "CookidooApi": {
    "BaseUrl": "[https://cookidoo.thermomix.com](https://cookidoo.thermomix.com)",
    "LoginPath": "/api/v2/authentication/login",
    "LogoutPath": "/api/v2/authentication/logout",
    "UserAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36"
  }
}