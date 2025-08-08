# Technické zadání: MCP Server pro Cookidoo

**Verze:** 1.0
**Datum:** 5. června 2025

## 1. Úvod a přehled 🚀

Projekt MCP Server (dále jen "server") má za cíl poskytnout backendové služby pro správu vlastních receptů a kolekcí receptů uživatelů platformy Cookidoo® od společnosti Vorwerk (pro zařízení Thermomix®). Server umožní vytvářet nové recepty, editovat existující (vlastní) recepty, vytvářet kolekce receptů, editovat je a spravovat obsah těchto kolekcí. Server bude implementován v **.NET 8** a bude komunikovat s oficiálním API Cookidoo pro synchronizaci dat. 

**Autentizace:** Server používá JWT token z Cookidoo (cookie `_oauth2_proxy`) pro autentizaci, podobně jako projekt `croeer/cookiput`. Uživatelé jsou odpovědní za získání tohoto tokenu z webového rozhraní Cookidoo (přihlášení do Cookidoo → Developer Tools (F12) → nalezení hodnoty `_oauth2_proxy` cookie).

Funkčnost bude inspirována existujícími projekty jako `miaucl/cookidoo-api` a `croeer/cookiput`, které demonstrují možnosti napojení na Cookidoo.

## 2. Cíle projektu 🎯

* Vyvinout robustní a škálovatelný backend server.
* Umožnit uživatelům **vytvářet nové recepty** kompatibilní s formátem Cookidoo.
* Umožnit uživatelům **editovat své existující vlastní recepty** synchronizované s Cookidoo.
* Umožnit uživatelům **vytvářet vlastní kolekce receptů**.
* Umožnit uživatelům **editovat detaily svých kolekcí receptů**.
* Umožnit uživatelům **přidávat recepty do kolekcí a odebírat je z nich**.
* Zajistit bezpečnou autentizaci a autorizaci vůči Cookidoo API.
* Poskytnout dobře dokumentované API pro klientské aplikace.

---

## 3. Funkční požadavky 🛠️

### 3.1. Správa receptů

* **FR1.1: Vytvoření nového receptu:**
    * Server musí umožnit zadání všech potřebných detailů receptu (název, ingredience, postup, časy přípravy/vaření, porce, nutriční hodnoty, tagy, obrázek atd.) ve formátu kompatibilním s Cookidoo.
    * Server musí být schopen odeslat nově vytvořený recept na Cookidoo platformu jménem autentizovaného uživatele.
* **FR1.2: Editace existujícího receptu:**
    * Server musí umožnit načtení detailů existujícího *vlastního* receptu uživatele z Cookidoo.
    * Uživatel musí mít možnost modifikovat veškeré atributy receptu.
    * Změny musí být synchronizovány zpět na Cookidoo platformu.
* **FR1.3: Načtení detailu receptu:**
    * Server musí umožnit načtení detailu vlastního receptu (pro účely zobrazení nebo editace).
* **FR1.4: Smazání vlastního receptu (Volitelné - zvážit dle Cookidoo API možností):**
    * Pokud API Cookidoo umožňuje, server by měl podporovat smazání vlastního receptu.

### 3.2. Správa kolekcí receptů

* **FR2.1: Vytvoření nové kolekce:**
    * Server musí umožnit vytvoření nové uživatelské kolekce receptů s názvem a popisem.
    * Nová kolekce musí být vytvořena na Cookidoo platformě jménem autentizovaného uživatele.
* **FR2.2: Editace kolekce:**
    * Server musí umožnit změnu názvu a popisu existující uživatelské kolekce.
    * Změny musí být synchronizovány s Cookidoo.
* **FR2.3: Načtení seznamu vlastních kolekcí:**
    * Server musí umožnit načtení seznamu vlastních kolekcí receptů uživatele z Cookidoo.
* **FR2.4: Načtení detailu kolekce (včetně receptů v ní):**
    * Server musí umožnit načtení detailů kolekce a seznamu receptů, které obsahuje.
* **FR2.5: Smazání vlastní kolekce (Volitelné - zvážit dle Cookidoo API možností):**
    * Pokud API Cookidoo umožňuje, server by měl podporovat smazání vlastní kolekce.

### 3.3. Správa receptů v kolekcích

* **FR3.1: Přidání receptu do kolekce:**
    * Server musí umožnit přidání existujícího (vlastního nebo i oficiálního, pokud API dovolí) receptu do uživatelské kolekce.
    * Změna musí být synchronizována s Cookidoo.
* **FR3.2: Odebrání receptu z kolekce:**
    * Server musí umožnit odebrání receptu z uživatelské kolekce.
    * Změna musí být synchronizována s Cookidoo.

### 3.4. Interakce s Cookidoo API

* **FR4.1: Autentizace pomocí JWT tokenu:**
    * Server musí přijímat JWT token (`_oauth2_proxy` cookie z Cookidoo) jako vstupní parametr pro autentizaci.
    * JWT token bude předáván v cookies (`_oauth2_proxy`) při volání Cookidoo API, podobně jako v projektu cookiput.
    * Server neuchovává ani nespravuje přihlašovací údaje uživatelů - odpovědnost za získání a poskytnutí platného JWT tokenu leží na klientské aplikaci.
    * Platnost JWT tokenu musí být ověřována při každém požadavku na Cookidoo API.
* **FR4.2: Synchronizace dat:**
    * Veškeré změny provedené přes MCP server musí být reflektovány na Cookidoo platformě.
    * Server by měl umět zpracovat případné konflikty nebo chyby při synchronizaci.

---

## 4. Nefunkční požadavky ⚙️

* **NFR1.1: Výkon:** Server musí poskytovat rychlou odezvu, typicky pod 500ms pro většinu operací (mimo operace závislé na rychlosti Cookidoo API).
* **NFR1.2: Škálovatelnost:** Architektura by měla umožnit horizontální škálování pro zvládnutí rostoucího počtu uživatelů a požadavků.
* **NFR1.3: Bezpečnost:**
    * Veškerá komunikace s klientskými aplikacemi musí být šifrována (HTTPS).
    * JWT tokeny z Cookidoo musí být zpracovávány bezpečně a neukládány trvale na serveru.
    * Ochrana proti běžným webovým zranitelnostem (OWASP Top 10).
* **NFR1.4: Spolehlivost:** Server by měl být vysoce dostupný.
* **NFR1.5: Udržovatelnost:** Kód by měl být čistý, dobře strukturovaný, komentovaný a testovatelný.
* **NFR1.6: Logování:** Podrobné logování požadavků, odpovědí a chyb pro účely monitoringu a ladění.
* **NFR1.7: Konfigurovatelnost:** Možnost konfigurace klíčových parametrů (např. URL Cookidoo API, časové limity) bez nutnosti změny kódu.

---

## 5. Architektura systému 🏗️

* Server bude navržen jako **API-first** (RESTful API).
* Bude se skládat z následujících hlavních komponent:
    * **API vrstva (ASP.NET Core Web API):** Zpracování HTTP požadavků, validace vstupů, autentizace/autorizace klientů MCP serveru.
    * **Servisní vrstva:** Obsahuje business logiku pro správu receptů a kolekcí.
    * **Integrační vrstva (Cookidoo Client):** Komunikace s externím Cookidoo API. Tato vrstva bude zodpovědná za překlad požadavků z MCP serveru na požadavky Cookidoo API a zpracování odpovědí. Bude inspirována projekty `miaucl/cookidoo-api` a `croeer/cookiput`.
    * **(Volitelné) Perzistentní vrstva (Databáze):** Může být zvážena pro dočasné ukládání dat, caching, nebo pro ukládání uživatelských preferencí specifických pro MCP server. Pokud bude použita, doporučuje se Entity Framework Core.

---

## 6. Technologický stack 💻

* **Framework:** .NET 8 (ASP.NET Core pro Web API)
* **Programovací jazyk:** C#
* **Databáze (pokud bude potřeba):** PostgreSQL, SQL Server, nebo SQLite (pro jednodušší scénáře/vývoj). Výběr dle preferencí a požadavků na škálovatelnost.
* **ORM (pokud bude potřeba databáze):** Entity Framework Core 8.
* **Knihovny pro HTTP komunikaci:** `HttpClientFactory` z .NET.
* **Logování:** Serilog nebo NLog.
* **Autentizace/Autorizace:** Cookidoo JWT token (`_oauth2_proxy` cookie) předávaný klientskou aplikací.
* **Kontejnerizace (doporučeno):** Docker.

---

## 7. Návrh API (Vysokoúrovňový přehled) 🌐

Následuje příklad klíčových endpointů. Detailní specifikace (OpenAPI/Swagger) bude součástí vývojového procesu.

### 7.1. Recepty

* `POST /api/v1/recipes`
    * Tělo: JSON s detailem nového receptu.
    * Odpověď: 201 Created, JSON s vytvořeným receptem (včetně ID z Cookidoo).
* `PUT /api/v1/recipes/{recipeId}`
    * `recipeId`: ID receptu na Cookidoo.
    * Tělo: JSON s aktualizovanými detaily receptu.
    * Odpověď: 200 OK, JSON s aktualizovaným receptem.
* `GET /api/v1/recipes/{recipeId}`
    * `recipeId`: ID receptu na Cookidoo.
    * Odpověď: 200 OK, JSON s detailem receptu.
* `GET /api/v1/recipes/my-recipes`
    * Odpověď: 200 OK, JSON pole vlastních receptů uživatele.

### 7.2. Kolekce

* `POST /api/v1/collections`
    * Tělo: JSON s názvem a popisem nové kolekce.
    * Odpověď: 201 Created, JSON s vytvořenou kolekcí (včetně ID z Cookidoo).
* `PUT /api/v1/collections/{collectionId}`
    * `collectionId`: ID kolekce na Cookidoo.
    * Tělo: JSON s aktualizovaným názvem/popisem.
    * Odpověď: 200 OK, JSON s aktualizovanou kolekcí.
* `GET /api/v1/collections/my-collections`
    * Odpověď: 200 OK, JSON pole vlastních kolekcí uživatele.
* `GET /api/v1/collections/{collectionId}`
    * `collectionId`: ID kolekce na Cookidoo.
    * Odpověď: 200 OK, JSON s detailem kolekce (včetně seznamu receptů).

### 7.3. Recepty v kolekcích

* `POST /api/v1/collections/{collectionId}/recipes`
    * `collectionId`: ID kolekce na Cookidoo.
    * Tělo: JSON s `recipeId`, který má být přidán.
    * Odpověď: 200 OK nebo 204 No Content.
* `DELETE /api/v1/collections/{collectionId}/recipes/{recipeId}`
    * `collectionId`: ID kolekce na Cookidoo.
    * `recipeId`: ID receptu na Cookidoo, který má být odebrán.
    * Odpověď: 200 OK nebo 204 No Content.

### 7.4. Autentizace

* **Všechny API endpointy vyžadují platný JWT token z Cookidoo:**
    * JWT token musí být poskytnut v HTTP headeru `Authorization: Bearer {jwt_token}` nebo jako query parametr `jwt_token`.
    * Server ověří platnost tokenu voláním na Cookidoo API před provedením jakékoliv operace.
    * **Poznámka:** Uživatel musí získat JWT token (`_oauth2_proxy` cookie) z webového rozhraní Cookidoo. Jak získat token je popsáno v dokumentaci - uživatel se přihlásí do Cookidoo, otevře Developer Tools (F12) a najde hodnotu `_oauth2_proxy` cookie.

---

## 8. Datový model (Příklady entit) 📝

Následující entity budou pravděpodobně mapovány na struktury používané Cookidoo API.

* **Recept (Recipe):**
    * `Id` (Cookidoo ID)
    * `Name` (Název)
    * `Description` (Popis)
    * `Ingredients` (Seznam ingrediencí: `Name`, `Quantity`, `Unit`)
    * `Steps` (Seznam kroků: `Description`, `Image`)
    * `PreparationTimeMinutes`
    * `CookingTimeMinutes`
    * `Portions`
    * `Difficulty` (Obtížnost: např. easy, medium, hard)
    * `Tags` (Seznam tagů)
    * `ImageUrl`
    * `Notes` (Poznámky)
    * `NutritionalInfo` (Nutriční informace)
    * `CreatedBy` (Informace o tvůrci - např. MCP user ID)
    * `IsPublic` (Veřejný/Soukromý na Cookidoo)
* **Kolekce (RecipeCollection):**
    * `Id` (Cookidoo ID)
    * `Name` (Název)
    * `Description` (Popis)
    * `RecipeIds` (Seznam ID receptů v kolekci)

---

## 9. Správa chyb a logování ⚠️

* Standardizované chybové odpovědi (např. dle RFC 7807 Problem Details for HTTP APIs).
* Podrobné logování všech požadavků, odpovědí, interních operací a chyb.
* Logování interakcí s Cookidoo API (požadavky, odpovědi, latence).
* Implementace sledování (tracing) pro lepší diagnostiku v distribuovaném prostředí (pokud relevantní).

---

## 10. Bezpečnostní aspekty 🔒

* **Autentizace s Cookidoo:** Server přijímá JWT token z Cookidoo (`_oauth2_proxy` cookie) jako vstupní parametr. Tento token není ukládán trvale na serveru a je používán pouze pro komunikaci s Cookidoo API během zpracování jednotlivých požadavků. **Uživatelé jsou odpovědní za získání a poskytnutí platného JWT tokenu ze svého Cookidoo účtu.**
* **Ochrana API MCP serveru:** Všechny endpointy MCP serveru vyžadují platný Cookidoo JWT token pro přístup - není nutné implementovat vlastní autentizační systém.
* **Rate limiting:** Ochrana proti zneužití API.
* **Validace vstupů:** Důsledná validace všech vstupních dat.
* **Správa závislostí:** Pravidelná aktualizace knihoven a frameworků.

---

## 11. Budoucí rozšíření (Volitelné) ✨

* Import receptů z jiných formátů.
* Pokročilé vyhledávání a filtrování ve vlastních receptech.
* Možnost sdílení vlastních receptů/kolekcí s jinými MCP uživateli (pokud by MCP mělo vlastní uživatelskou základnu).
* Offline podpora (caching).

Tento dokument slouží jako výchozí bod pro vývoj MCP serveru. Během analýzy a implementace mohou být detaily upřesněny. Klíčové bude detailní prozkoumání možností a limitací Cookidoo API, ideálně s využitím poznatků z uvedených GitHub projektů.