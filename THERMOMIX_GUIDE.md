# 🤖 Thermomix Parametry - Průvodce

Tento průvodce vysvětluje, jak správně vyplňovat Thermomix parametry při vytváření receptů pomocí Cookidoo MCP Serveru.

## 📋 Obsah

- [Přehled parametrů](#přehled-parametrů)
- [Struktura kroku](#struktura-kroku)
- [Příklady použití](#příklady-použití)
- [Formátování pro Cookidoo](#formátování-pro-cookidoo)
- [Tipy a triky](#tipy-a-triky)

## 🎯 Přehled parametrů

Každý krok receptu může obsahovat následující Thermomix parametry:

### Základní parametry

| Parametr | Typ | Rozsah | Popis | Příklad |
|----------|-----|--------|-------|---------|
| **text** | string | - | Popis akce (povinné) | "zerkleinern", "kochen" |
| **order** | number | 1+ | Pořadí kroku (povinné) | 1, 2, 3... |
| **timeSeconds** | number | 0-7200 | Čas v sekundách | 90 (= 1,5 min) |
| **temperature** | number | 0-120 | Teplota v °C | 100 |
| **speed** | number | 1-10 | Rychlost mixéru | 4 |

### Speciální režimy

| Parametr | Typ | Popis |
|----------|-----|-------|
| **useTurbo** | boolean | Turbo režim (velmi vysoká rychlost) |
| **useReverseRotation** | boolean | Levo-otáčky (šetrné míchání) |
| **useVaroma** | boolean | Varoma režim (vaření v páře) |

## 📦 Struktura kroku

### Minimální krok (bez Thermomix parametrů)

```json
{
  "text": "Přidejte do nádoby ingredience",
  "order": 1
}
```

### Krok s Thermomix parametry

```json
{
  "text": "zerkleinern",
  "order": 2,
  "timeSeconds": 15,
  "speed": 8,
  "temperature": null
}
```

### Kompletní krok se všemi parametry

```json
{
  "text": "kochen",
  "order": 3,
  "timeSeconds": 360,
  "temperature": 100,
  "speed": 2,
  "useTurbo": false,
  "useReverseRotation": false,
  "useVaroma": false
}
```

## 🔥 Příklady použití

### 1. Sekání zeleniny (15 sekund, rychlost 8)

```json
{
  "text": "zerkleinern",
  "order": 1,
  "timeSeconds": 15,
  "speed": 8
}
```

**Výstup pro Cookidoo:** `<nobr>15 Sek./Stufe 8</nobr> zerkleinern`

### 2. Vaření polévky (20 minut, 100°C, rychlost 2)

```json
{
  "text": "kochen",
  "order": 2,
  "timeSeconds": 1200,
  "temperature": 100,
  "speed": 2
}
```

**Výstup pro Cookidoo:** `<nobr>20 Min./100°C/Stufe 2</nobr> kochen`

### 3. Mixování (30 sekund, rychlost 9)

```json
{
  "text": "pürieren",
  "order": 3,
  "timeSeconds": 30,
  "speed": 9
}
```

**Výstup pro Cookidoo:** `<nobr>30 Sek./Stufe 9</nobr> pürieren`

### 4. Opražení (3 minuty, 100°C, rychlost 1)

```json
{
  "text": "andünsten",
  "order": 4,
  "timeSeconds": 180,
  "temperature": 100,
  "speed": 1
}
```

**Výstup pro Cookidoo:** `<nobr>3 Min./100°C/Stufe 1</nobr> andünsten`

### 5. Turbo režim (5 sekund, Turbo)

```json
{
  "text": "zerkleinern",
  "order": 5,
  "timeSeconds": 5,
  "useTurbo": true
}
```

**Výstup pro Cookidoo:** `<nobr>5 Sek./Turbo</nobr> zerkleinern`

### 6. Šetrné míchání s levo-otáčkami (2 minuty, rychlost 2, levo)

```json
{
  "text": "vermischen",
  "order": 6,
  "timeSeconds": 120,
  "speed": 2,
  "useReverseRotation": true
}
```

**Výstup pro Cookidoo:** `<nobr>2 Min./Stufe 2 Linkslauf</nobr> vermischen`

### 7. Varoma režim (30 minut, Varoma, rychlost 1)

```json
{
  "text": "dämpfen",
  "order": 7,
  "timeSeconds": 1800,
  "useVaroma": true,
  "speed": 1
}
```

**Výstup pro Cookidoo:** `<nobr>30 Min./Varoma/Stufe 1</nobr> dämpfen`

## 🎨 Formátování pro Cookidoo

Backend automaticky formátuje Thermomix parametry do správného formátu:

### Formát

```
<nobr>{čas}/{teplota}/{rychlost}</nobr> {text}
```

### Pravidla formátování

1. **Čas**:
   - Méně než 60 sekund: `{n} Sek.`
   - 60+ sekund: `{m} Min.` nebo `{m} Min. {s} Sek.`

2. **Teplota**:
   - Normální: `{t}°C`
   - Varoma: `Varoma`
   - Bez ohřevu: vynecháno

3. **Rychlost**:
   - Normální: `Stufe {s}`
   - Turbo: `Turbo`
   - S levo-otáčkami: `Stufe {s} Linkslauf`

4. **HTML tag**: `<nobr>` zabraňuje zalomení řádku

## 🎓 Běžné Thermomix operace

### Sekání a drcení

| Operace | Čas | Rychlost | Teplota |
|---------|-----|----------|---------|
| Jemné sekání | 5-10 s | 5-6 | - |
| Hrubé sekání | 3-5 s | 4-5 | - |
| Velmi jemné sekání | 10-15 s | 7-8 | - |
| Turbo sekání | 2-5 s | Turbo | - |
| Drcení ledu | 5-10 s | 8-10 | - |

### Míchání

| Operace | Čas | Rychlost | Teplota |
|---------|-----|----------|---------|
| Jemné míchání | 30-60 s | 2-3 | - |
| Středně silné míchání | 20-40 s | 4-5 | - |
| Šetrné míchání (levo) | 30-60 s | 1-2 + Levo | - |

### Vaření

| Operace | Čas | Rychlost | Teplota |
|---------|-----|----------|---------|
| Opražení | 2-5 min | 1-2 | 100°C |
| Vaření polévky | 15-30 min | 1-2 | 100°C |
| Vaření těstovin | 8-12 min | 1 | 100°C |
| Vaření na páře (Varoma) | 20-40 min | 1 | Varoma |

### Mixování

| Operace | Čas | Rychlost | Teplota |
|---------|-----|----------|---------|
| Smoothie | 30-60 s | 8-10 | - |
| Polévka krémová | 20-40 s | 7-9 | - |
| Omáčka hladká | 15-30 s | 6-8 | - |

## 💡 Tipy a triky

### 1. Doporučené rychlosti

- **Rychlost 1-2**: Opražení, pomalé míchání, vaření
- **Rychlost 3-5**: Míchání, hnětení těsta
- **Rychlost 6-8**: Sekání, mixování
- **Rychlost 9-10**: Jemné mixování, smoothies
- **Turbo**: Velmi rychlé sekání (krátkodobě)

### 2. Teploty

- **37°C**: Aktivace kvasnic, zahřívání mléka
- **50-70°C**: Šetrné zahřívání
- **80-90°C**: Zahušťování omáček
- **100°C**: Vaření, opražení
- **Varoma (~120°C)**: Vaření v páře

### 3. Levo-otáčky (Linkslauf)

Použijte pro:
- Šetrné míchání (např. risotto)
- Hnětení těsta
- Vmíchávání křehkých ingrediencí
- Emulgaci omáček

### 4. Turbo režim

Použijte pro:
- Velmi rychlé sekání (led, ořechy)
- Krátkodobé operace (2-5 sekund)
- **POZOR**: Nikdy nepoužívejte Turbo s horkými tekutinami!

### 5. Varoma režim

Použijte pro:
- Vaření zeleniny v páře
- Přípravu ryb
- Vaření knedlíků
- Zdravější vaření bez tuku

## 📝 Příklad kompletního receptu

```json
{
  "name": "Zeleninová polévka",
  "ingredients": [
    { "text": "1 cibule", "quantity": 1, "unit": "ks" },
    { "text": "30g olivového oleje", "quantity": 30, "unit": "g" },
    { "text": "500g zeleniny", "quantity": 500, "unit": "g" },
    { "text": "800ml vývaru", "quantity": 800, "unit": "ml" }
  ],
  "steps": [
    {
      "text": "zerkleinern",
      "order": 1,
      "timeSeconds": 5,
      "speed": 5,
      "comment": "Nakrájejte cibuli"
    },
    {
      "text": "andünsten",
      "order": 2,
      "timeSeconds": 180,
      "speed": 1,
      "temperature": 100,
      "comment": "Opražte cibuli na oleji"
    },
    {
      "text": "kochen",
      "order": 3,
      "timeSeconds": 1200,
      "speed": 2,
      "temperature": 100,
      "comment": "Přidejte zeleninu a vývar, vařte"
    },
    {
      "text": "pürieren",
      "order": 4,
      "timeSeconds": 30,
      "speed": 9,
      "comment": "Rozmixujte na hladkou polévku"
    }
  ],
  "preparationTimeMinutes": 10,
  "cookingTimeMinutes": 25,
  "portions": 4
}
```

## 🔗 Související dokumentace

- [QUICK_START.md](QUICK_START.md) - Rychlý start průvodce
- [README.md](README.md) - Přehled projektu
- [example-recipe-thermomix.json](example-recipe-thermomix.json) - Ukázkový recept

## ⚠️ Důležité upozornění

Thermomix parametry jsou **volitelné**. Pokud je nevyplníte, recept se vytvoří s prostým textem. Pro maximální využití UX funkcionalit Thermomixu však doporučujeme vyplňovat všechny relevantní parametry.

---

**Vytvořeno pro komunitu Thermomix uživatelů** 🍳
