# 🛠️ Návrh nových MCP Tools pro Cookidoo

## 📊 Přehled současného stavu

### ✅ Již implementováno (7 tools)
- `get_recipes` - Seznam receptů
- `get_recipe` - Detail receptu
- `create_recipe` - Vytvoření receptu
- `get_collections` - Seznam kolekcí
- `create_collection` - Vytvoření kolekce
- `add_recipe_to_collection` - Přidání receptu do kolekce
- `search_recipes` - Vyhledávání receptů

## 🆕 Navrhované nové MCP Tools (19 tools)

### 🛒 1. Správa nákupního seznamu (9 tools) - **VYSOKÁ PRIORITA**

#### `get_shopping_list`
Získá kompletní nákupní seznam s ingrediencemi z receptů a vlastními položkami.

**Příklad použití:**
```
@cookidoo Zobraz mi můj nákupní seznam
```

**Response:**
```
📝 Nákupní seznam:

🍽️ Z receptů:
  ☐ 200g mouky (Čokoládový dort)
  ☐ 3 vejce (Čokoládový dort, Omeleta)
  ☑️ 100ml mléka (Čokoládový dort) - zakoupeno

📋 Vlastní položky:
  ☐ Toaletní papír
  ☐ Máslo
  ☑️ Sůl - zakoupeno
```

---

#### `add_recipes_to_shopping_list`
Přidá ingredience z receptů do nákupního seznamu.

**Příklad použití:**
```
@cookidoo Přidej ingredience z receptu "Čokoládový dort" do nákupního seznamu
```

---

#### `remove_recipes_from_shopping_list`
Odebere ingredience receptů z nákupního seznamu.

**Příklad použití:**
```
@cookidoo Odeber všechny ingredience receptu "Polévka" z nákupního seznamu
```

---

#### `mark_ingredients_as_owned`
Označí ingredience jako již zakoupené (zaškrtne je).

**Příklad použití:**
```
@cookidoo Označ mouku a vejce jako zakoupené
```

---

#### `add_shopping_items`
Přidá vlastní položky do nákupního seznamu (ne z receptu).

**Příklad použití:**
```
@cookidoo Přidej do nákupního seznamu: toaletní papír, zubní pastu, máslo
```

---

#### `edit_shopping_items`
Upraví název vlastních položek v nákupním seznamu.

**Příklad použití:**
```
@cookidoo Přejmenuj "maslo" na "máslo 250g"
```

---

#### `mark_shopping_items_as_owned`
Označí vlastní položky jako zakoupené.

**Příklad použití:**
```
@cookidoo Označ "toaletní papír" a "máslo" jako zakoupené
```

---

#### `remove_shopping_items`
Odebere vlastní položky z nákupního seznamu.

**Příklad použití:**
```
@cookidoo Odeber "toaletní papír" z nákupního seznamu
```

---

#### `clear_shopping_list`
Vymaže celý nákupní seznam.

**Příklad použití:**
```
@cookidoo Vymaž celý nákupní seznam
```

---

### 📅 2. Plánování jídel (3 tools) - **VYSOKÁ PRIORITA**

#### `get_weekly_meal_plan`
Získá plán jídel pro daný týden.

**Příklad použití:**
```
@cookidoo Zobraz mi plán jídel na tento týden
@cookidoo Co mám naplánováno na zítřek?
```

**Response:**
```
📅 Plán jídel pro týden 22.11. - 28.11.2025:

Pondělí 22.11.:
  🍽️ Oběd: Špagety Carbonara (30 min)
  🍽️ Večeře: Zeleninová polévka (25 min)

Úterý 23.11.:
  🍽️ Oběd: Kuřecí steak (40 min)

Středa 24.11.:
  (Žádný plán)

...
```

---

#### `add_recipes_to_meal_plan`
Přidá recepty do kalendáře na konkrétní den.

**Příklad použití:**
```
@cookidoo Naplánuj "Čokoládový dort" na sobotu na oběd
@cookidoo Přidej do plánu na zítřek: Polévka (oběd) a Rizoto (večeře)
```

---

#### `remove_recipe_from_meal_plan`
Odebere recept z kalendáře z konkrétního dne.

**Příklad použití:**
```
@cookidoo Odeber "Polévku" z plánu na zítřek
```

---

### 📁 3. Rozšířená správa kolekcí (5 tools) - **STŘEDNÍ PRIORITA**

#### `get_managed_collections`
Získá oficiální Cookidoo kolekce (předpřipravené kolekce od Vorwerku).

**Příklad použití:**
```
@cookidoo Zobraz mi oficiální kolekce od Cookidoo
@cookidoo Jaké jsou nejnovější kolekce?
```

**Response:**
```
📚 Oficiální Cookidoo kolekce:

🎄 Vánoční recepty 2025 (45 receptů)
🥗 Zdravé jaro (32 receptů)
🍕 Italská kuchyně (28 receptů)
👶 Recepty pro děti (41 receptů)
```

---

#### `subscribe_to_managed_collection`
Přihlásí se k odběru oficiální Cookidoo kolekce.

**Příklad použití:**
```
@cookidoo Přihlas mě k odběru kolekce "Vánoční recepty 2025"
```

---

#### `unsubscribe_from_managed_collection`
Odhlásí se z odběru oficiální kolekce.

**Příklad použití:**
```
@cookidoo Odhlas mě z kolekce "Italská kuchyně"
```

---

#### `delete_collection`
Smaže vlastní kolekci.

**Příklad používání:**
```
@cookidoo Smaž moji kolekci "Staré recepty"
```

---

#### `remove_recipe_from_collection`
Odebere konkrétní recept z kolekce.

**Příklad použití:**
```
@cookidoo Odeber "Polévku" z kolekce "Rychlé večeře"
```

---

### 👤 4. Uživatel & Předplatné (2 tools) - **NÍZKÁ PRIORITA**

#### `get_user_profile`
Získá informace o uživatelském profilu.

**Příklad použití:**
```
@cookidoo Zobraz můj profil
```

**Response:**
```
👤 Uživatelský profil:

📧 Email: user@example.com
👤 Jméno: Jan Novák
🌍 Země: Česká republika
🗣️ Jazyk: cs-CZ
🔧 Přístroje: Thermomix TM6
```

---

#### `get_subscription_info`
Získá informace o předplatném Cookidoo.

**Příklad použití:**
```
@cookidoo Kdy mi vyprší předplatné?
@cookidoo Jaké mám předplatné?
```

**Response:**
```
📱 Cookidoo předplatné:

✅ Stav: Aktivní
📅 Platné do: 15.12.2025
🎫 Typ: Roční předplatné
💳 Zdroj: COMMERCE
🌍 Země: Česká republika
```

---

### 🍽️ 5. Rozšířené informace o receptu (1 tool) - **STŘEDNÍ PRIORITA**

#### `get_recipe_full_details`
Získá kompletní detail receptu včetně kategorií, kolekcí, nutričních informací, nádobí.

**Příklad použití:**
```
@cookidoo Zobraz kompletní detail receptu "Špagety Carbonara"
```

**Response:**
```
🍽️ Špagety Carbonara - Kompletní detail

📝 Základní info:
   Obtížnost: Střední
   Aktivní čas: 15 min
   Celkový čas: 30 min
   Porce: 4

📋 Kategorie:
   - Těstoviny
   - Italská kuchyně
   - Rychlé recepty

📚 V kolekcích:
   - Italské speciality
   - Rychlé večeře

🔧 Potřebné nádobí:
   - Thermomix TM6
   - Velký hrnec na těstoviny

🥗 Nutriční hodnoty (na porci):
   Kalorie: 520 kcal
   Bílkoviny: 24g
   Sacharidy: 65g
   Tuky: 18g
   Vláknina: 3g

📝 Poznámky:
   Pro ještě lepší chuť přidejte čerstvě nastrouhaný parmezán.
```

---

## 📊 Souhrn podle priorit

### 🔥 Vysoká priorita (12 tools)
Funkce, které uživatelé používají denně:

**Nákupní seznam (9):**
- get_shopping_list
- add_recipes_to_shopping_list
- remove_recipes_from_shopping_list
- mark_ingredients_as_owned
- add_shopping_items
- edit_shopping_items
- mark_shopping_items_as_owned
- remove_shopping_items
- clear_shopping_list

**Plánování jídel (3):**
- get_weekly_meal_plan
- add_recipes_to_meal_plan
- remove_recipe_from_meal_plan

### 🟡 Střední priorita (6 tools)
Užitečné doplňkové funkce:

**Rozšířené kolekce (5):**
- get_managed_collections
- subscribe_to_managed_collection
- unsubscribe_from_managed_collection
- delete_collection
- remove_recipe_from_collection

**Rozšířené recepty (1):**
- get_recipe_full_details

### 🔵 Nízká priorita (2 tools)
Informační funkce:

**Uživatel & Předplatné (2):**
- get_user_profile
- get_subscription_info

## 🎯 Doporučení k implementaci

### Fáze 1: Nákupní seznam (nejužitečnější)
```
1. get_shopping_list
2. add_recipes_to_shopping_list
3. mark_ingredients_as_owned
4. add_shopping_items
5. clear_shopping_list
```

**Use case:**
```
Claude: "Mám dnes uvařit Špagety Carbonara a Čokoládový dort.
        Co potřebuji nakoupit?"

→ add_recipes_to_shopping_list(["Špagety Carbonara", "Čokoládový dort"])
→ get_shopping_list()

Response: "📝 Přidáno do nákupního seznamu: mouka, vejce,
           smetana, slanina, parmezán, čokoláda..."
```

### Fáze 2: Plánování jídel
```
1. get_weekly_meal_plan
2. add_recipes_to_meal_plan
3. remove_recipe_from_meal_plan
```

**Use case:**
```
Claude: "Naplánuj mi na tento týden zdravé recepty,
        každý den jiný, max 30 minut přípravy"

→ search_recipes(tags=["zdravé"], maxTime=30)
→ add_recipes_to_meal_plan(pondělí: "Salát", úterý: "Polévka", ...)
→ get_weekly_meal_plan()

Response: "📅 Naplánováno 7 receptů na tento týden"
```

### Fáze 3: Rozšířené funkce
```
1. get_managed_collections
2. get_recipe_full_details
3. delete_collection
```

## 💡 Pokročilé use case

### Kombinace nákupního seznamu a plánování:

```
@cookidoo Naplánuj mi celý týden zdravých receptů
a přidej všechny ingredience do nákupního seznamu

→ search_recipes(tags=["zdravé"], limit=7)
→ add_recipes_to_meal_plan(7 receptů na 7 dní)
→ add_recipes_to_shopping_list(všech 7 receptů)
→ get_shopping_list()

Response: "✅ Naplánováno 7 receptů na tento týden
          📝 Přidáno 45 ingrediencí do nákupního seznamu"
```

### Inteligentní nákupní asistent:

```
@cookidoo Co mám dnes uvařit, když mám doma už:
mouku, vejce, mléko a sýr?

→ get_shopping_list() (zjistí co má doma)
→ search_recipes(obsahuje: mouku, vejce, mléko, sýr)
→ filter (co nepotřebuje moc dalších ingrediencí)

Response: "Doporučuji: Palačinky (máte vše) nebo
          Quiche (potřebujete jen špenát)"
```

## 🔗 API Endpointy (Cookidoo)

Všechny tyto funkce jsou dostupné v oficiálním Cookidoo API:

- **Shopping:** `/shopping/{language}/*`
- **Planning:** `/planning/{language}/api/my-week/*`
- **Collections:** `/organize/{language}/api/custom-list/*`
- **Profile:** `/community/profile`
- **Subscription:** `/ownership/subscriptions`
- **Recipes:** `/recipes/recipe/{language}/{id}`

## 📚 Reference

- [cookidoo-api Python knihovna](https://github.com/miaucl/cookidoo-api)
- Cookidoo API dokumentace (neoficiální)

---

**Celkem navrženo: 19 nových MCP tools**
