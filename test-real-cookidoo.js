#!/usr/bin/env node

/**
 * Přímý test s Cookidoo API - skutečné vytvoření receptu
 * Tento script se připojí přímo k Cookidoo API bez prostředníka
 */

const COOKIDOO_BASE_URL = 'https://cookidoo.thermomix.com';
const EMAIL = 'hnizdiljan@gmail.com';
const PASSWORD = 'Krel1991';
const LANGUAGE = 'cs-CZ';

console.log('🧪 Přímý test s Cookidoo API\n');
console.log('─'.repeat(60));

let cookieJar = '';

/**
 * Přihlášení k Cookidoo
 */
async function login() {
  console.log('🔐 Přihlašování k Cookidoo...');
  console.log(`   Email: ${EMAIL}`);

  try {
    // Krok 1: Získání hlavní stránky pro cookies
    const homeResponse = await fetch(COOKIDOO_BASE_URL, {
      headers: {
        'User-Agent': 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36',
        'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8',
        'Accept-Language': 'cs-CZ,cs;q=0.9,en;q=0.8'
      },
      redirect: 'manual'
    });

    // Extrahovat cookies
    const setCookies = homeResponse.headers.getSetCookie?.() || [];
    cookieJar = setCookies.map(c => c.split(';')[0]).join('; ');

    console.log('   ✅ Získány session cookies');

    // Krok 2: OAuth2 login endpoint
    // Cookidoo používá Vorwerk Identity Provider
    const loginUrl = `${COOKIDOO_BASE_URL}/login`;

    console.log('   🔄 Pokus o přihlášení...');

    // Poznámka: Cookidoo používá složitý OAuth2 flow s PKCE
    // Pro skutečné testování je potřeba implementovat celý flow nebo použít
    // již existující knihovnu

    console.log('\n⚠️  UPOZORNĚNÍ:');
    console.log('   Cookidoo používá komplexní OAuth2 autentizaci s PKCE flow.');
    console.log('   Pro skutečné testování doporučuji:');
    console.log('   1. Použít .NET backend (src/Cookidoo.MCP.Api)');
    console.log('   2. Nebo získat OAuth token manuálně z prohlížeče\n');

    return null;

  } catch (error) {
    console.error('❌ Chyba při přihlášení:', error.message);
    return null;
  }
}

/**
 * Alternativní metoda - vytvoření receptu pomocí získaného OAuth tokenu
 */
async function createRecipeWithToken(oauthToken) {
  console.log('📝 Vytváření receptu "Krtkův dort"...\n');

  const recipe = {
    name: "Krtkův dort",
    description: "Tradiční český dort s banány, šlehačkovým krémem a čokoládovou polevou",
    ingredients: [
      { text: "4 vejce" },
      { text: "150 g cukru krupice" },
      { text: "150 g hladké mouky" },
      { text: "1 balíček prášku do pečiva" },
      { text: "50 ml mléka" },
      { text: "50 ml oleje" },
      { text: "2 lžíce kakaa" },
      { text: "500 ml šlehačky na šlehání" },
      { text: "2 balíčky ztužovače šlehačky" },
      { text: "3-4 banány" },
      { text: "200 g hořké čokolády" },
      { text: "100 ml šlehačky na vaření" },
      { text: "30 g másla" }
    ],
    instructions: [
      {
        type: "STEP",
        text: "oddělte žloutky od bílků"
      },
      {
        type: "STEP",
        text: "<nobr>3 Min./Stufe 4</nobr> ušlehejte bílky dotuha"
      },
      {
        type: "STEP",
        text: "<nobr>2 Min./Stufe 4</nobr> přidejte žloutky a cukr a šlehejte"
      },
      {
        type: "STEP",
        text: "<nobr>30 Sek./Stufe 3 Linkslauf</nobr> přidejte mouku, prášek do pečiva, mléko a olej a promíchejte"
      },
      {
        type: "STEP",
        text: "polovinu těsta dejte do vymazané formy, do druhé poloviny vmíchejte kakao"
      },
      {
        type: "STEP",
        text: "kakaové těsto nalijte na světlé těsto a špejlí proveďte mramorování. Pečte v troubě na 180°C 35-40 minut"
      },
      {
        type: "STEP",
        text: "nechte vychladnout a rozkrojte na 3 pláty"
      },
      {
        type: "STEP",
        text: "<nobr>2 Min./Stufe 4</nobr> ušlehejte šlehačku se ztužovačem"
      },
      {
        type: "STEP",
        text: "<nobr>15 Sek./Stufe 5</nobr> nakrájejte banány na kolečka"
      },
      {
        type: "STEP",
        text: "první plát dortu potřete šlehačkou, položte na něj banány, přikryjte druhým plátem a opakujte. Navrch položte třetí plát"
      },
      {
        type: "STEP",
        text: "<nobr>3 Min./50°C/Stufe 2</nobr> roztopte čokoládu se šlehačkou a máslem"
      },
      {
        type: "STEP",
        text: "polijte dort čokoládovou polevou a nechte ztuhnout v lednici minimálně 2 hodiny"
      }
    ],
    preparationTime: 45,
    cookingTime: 40,
    servingSize: {
      quantity: { value: 12 },
      unitNotation: "porce"
    },
    difficulty: 3
  };

  try {
    const response = await fetch(`${COOKIDOO_BASE_URL}/api/created-recipes/${LANGUAGE}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Cookie': `_oauth2_proxy=${oauthToken}`,
        'User-Agent': 'troet'
      },
      body: JSON.stringify(recipe)
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText}`);
    }

    const result = await response.json();
    console.log('✅ Recept úspěšně vytvořen!');
    console.log(`   ID receptu: ${result.recipeId || result.id}`);
    console.log(`   Název: ${recipe.name}\n`);

    return result;

  } catch (error) {
    console.error('❌ Chyba při vytváření receptu:', error.message);
    return null;
  }
}

/**
 * Hlavní funkce
 */
async function main() {
  console.log('🎯 Cíl: Vytvořit skutečný recept "Krtkův dort" v Cookidoo\n');

  // Pokus o přihlášení
  const token = await login();

  if (!token) {
    console.log('\n📋 INSTRUKCE PRO MANUÁLNÍ TEST:\n');
    console.log('Pro skutečné vytvoření receptu v Cookidoo postupujte takto:\n');

    console.log('📍 MOŽNOST 1: Použití .NET backendu');
    console.log('─'.repeat(60));
    console.log('1. Nainstalujte .NET 8 SDK');
    console.log('2. Spusťte backend:');
    console.log('   cd src/Cookidoo.MCP.Api');
    console.log('   dotnet run');
    console.log('3. V druhém terminálu:');
    console.log('   node test-create-krtkov-dort.js\n');

    console.log('📍 MOŽNOST 2: Manuální získání OAuth tokenu');
    console.log('─'.repeat(60));
    console.log('1. Přihlaste se na https://cookidoo.thermomix.com');
    console.log('2. Otevřete Developer Tools (F12) → Application → Cookies');
    console.log('3. Najděte cookie "_oauth2_proxy" a zkopírujte hodnotu');
    console.log('4. Spusťte:');
    console.log('   OAUTH_TOKEN="zkopírovaná-hodnota" node test-real-cookidoo.js --with-token\n');

    console.log('📍 MOŽNOST 3: Použití MCP clienta v Cursoru');
    console.log('─'.repeat(60));
    console.log('1. Nakonfigurujte .cursor-mcp.json s vašimi údaji');
    console.log('2. V Cursoru napište:');
    console.log('   @cookidoo Vytvoř recept "Krtkův dort" podle REAL_TESTING_GUIDE.md\n');

    console.log('📖 Více informací: REAL_TESTING_GUIDE.md\n');

    process.exit(1);
  }

  // Pokud máme token, vytvoříme recept
  const result = await createRecipeWithToken(token);

  if (result) {
    console.log('🎉 Test úspěšně dokončen!');
    console.log('   Zkontrolujte recept na: https://cookidoo.thermomix.com/created-recipes\n');
    process.exit(0);
  } else {
    console.log('❌ Test selhal\n');
    process.exit(1);
  }
}

// Kontrola parametrů
if (process.argv.includes('--with-token')) {
  const oauthToken = process.env.OAUTH_TOKEN;

  if (!oauthToken) {
    console.error('❌ Chyba: OAUTH_TOKEN environment proměnná není nastavena');
    console.log('   Použití: OAUTH_TOKEN="token" node test-real-cookidoo.js --with-token');
    process.exit(1);
  }

  console.log('🔑 Používám dodaný OAuth token...\n');
  createRecipeWithToken(oauthToken).then(result => {
    if (result) {
      console.log('🎉 Recept vytvořen!');
      console.log('   Zkontrolujte: https://cookidoo.thermomix.com/created-recipes\n');
      process.exit(0);
    } else {
      process.exit(1);
    }
  });
} else {
  main();
}
