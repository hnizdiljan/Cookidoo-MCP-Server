#!/usr/bin/env node

/**
 * Test script pro Cookidoo MCP Client
 *
 * Tento skript demonstruje použití MCP serveru pro vytváření receptů
 * bez nutnosti používat Cursor editor - přímo z příkazové řádky.
 *
 * Použití:
 *   COOKIDOO_TOKEN=your-token node test-mcp-client.js
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Konfigurace
const COOKIDOO_API_URL = process.env.COOKIDOO_API_URL || 'http://localhost:5555/api/v1';
const COOKIDOO_TOKEN = process.env.COOKIDOO_TOKEN || 'mock-test-token';

console.log('🧪 Cookidoo MCP Client - Test Script\n');
console.log(`📡 API URL: ${COOKIDOO_API_URL}`);
console.log(`🔑 Token: ${COOKIDOO_TOKEN.substring(0, 20)}...`);
console.log('');

/**
 * Pomocná funkce pro volání Cookidoo API
 */
async function apiCall(endpoint, method = 'GET', body = null) {
  const url = `${COOKIDOO_API_URL}${endpoint}`;

  const options = {
    method,
    headers: {
      'Authorization': `Bearer ${COOKIDOO_TOKEN}`,
      'Content-Type': 'application/json'
    }
  };

  if (body) {
    options.body = JSON.stringify(body);
  }

  try {
    const response = await fetch(url, options);

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText}`);
    }

    return response.json();
  } catch (error) {
    console.error(`❌ Chyba při volání API: ${error.message}`);
    throw error;
  }
}

/**
 * Test 1: Získání seznamu receptů
 */
async function testGetRecipes() {
  console.log('📚 Test 1: Získání seznamu receptů');
  console.log('─'.repeat(50));

  try {
    const recipes = await apiCall('/recipes?limit=5');
    console.log(`✅ Načteno receptů: ${recipes.items?.length || 0}`);

    if (recipes.items && recipes.items.length > 0) {
      console.log('\nPrvních 5 receptů:');
      recipes.items.forEach((recipe, i) => {
        console.log(`  ${i + 1}. ${recipe.name} (${recipe.id})`);
      });
    } else {
      console.log('ℹ️  Zatím nemáte žádné recepty');
    }

    return true;
  } catch (error) {
    console.error(`❌ Test selhal: ${error.message}`);
    return false;
  }
}

/**
 * Test 2: Vytvoření nového receptu
 */
async function testCreateRecipe() {
  console.log('\n\n📝 Test 2: Vytvoření nového receptu');
  console.log('─'.repeat(50));

  try {
    // Načtení příkladu receptu
    const exampleRecipePath = path.join(__dirname, 'example-recipe.json');

    if (!fs.existsSync(exampleRecipePath)) {
      console.log('⚠️  Soubor example-recipe.json nenalezen, vytvářím jednoduchý recept...');

      // Jednoduchý testovací recept
      const simpleRecipe = {
        name: `Testovací recept ${new Date().toISOString()}`,
        description: 'Testovací recept vytvořený MCP test scriptem',
        ingredients: [
          {
            text: '200g testovací ingredience',
            name: 'test',
            quantity: 200,
            unit: 'g'
          }
        ],
        steps: [
          {
            text: 'Krok 1: Testovací postup',
            order: 1
          }
        ],
        preparationTimeMinutes: 10,
        cookingTimeMinutes: 20,
        portions: 2,
        difficulty: 1,
        tags: ['test']
      };

      const createdRecipe = await apiCall('/recipes', 'POST', simpleRecipe);
      console.log(`✅ Recept vytvořen s ID: ${createdRecipe.id}`);
      console.log(`   Název: ${createdRecipe.name}`);

      return createdRecipe.id;
    }

    // Načtení receptu ze souboru
    const recipeData = JSON.parse(fs.readFileSync(exampleRecipePath, 'utf-8'));
    console.log(`📖 Načítám recept ze souboru: ${recipeData.name}`);

    // Vytvoření receptu
    const createdRecipe = await apiCall('/recipes', 'POST', recipeData);
    console.log(`✅ Recept úspěšně vytvořen!`);
    console.log(`   ID: ${createdRecipe.id}`);
    console.log(`   Název: ${createdRecipe.name}`);
    console.log(`   Ingredience: ${createdRecipe.ingredients?.length || 0}`);
    console.log(`   Kroky: ${createdRecipe.steps?.length || 0}`);
    console.log(`   Celkový čas: ${(createdRecipe.preparationTimeMinutes || 0) + (createdRecipe.cookingTimeMinutes || 0)} min`);

    return createdRecipe.id;
  } catch (error) {
    console.error(`❌ Test selhal: ${error.message}`);
    return null;
  }
}

/**
 * Test 3: Načtení detailu receptu
 */
async function testGetRecipe(recipeId) {
  console.log('\n\n🔍 Test 3: Načtení detailu receptu');
  console.log('─'.repeat(50));

  if (!recipeId) {
    console.log('⚠️  Žádný recept k načtení (předchozí test selhal)');
    return false;
  }

  try {
    const recipe = await apiCall(`/recipes/${recipeId}`);
    console.log(`✅ Recept načten: ${recipe.name}`);
    console.log(`\n📋 Detail receptu:`);
    console.log(`   Popis: ${recipe.description}`);
    console.log(`   Porce: ${recipe.portions}`);
    console.log(`   Obtížnost: ${recipe.difficulty}/5`);
    console.log(`   Tagy: ${recipe.tags?.join(', ') || 'žádné'}`);

    return true;
  } catch (error) {
    console.error(`❌ Test selhal: ${error.message}`);
    return false;
  }
}

/**
 * Test 4: Vyhledání receptů
 */
async function testSearchRecipes() {
  console.log('\n\n🔍 Test 4: Vyhledání receptů');
  console.log('─'.repeat(50));

  try {
    const results = await apiCall('/recipes/search?q=čokoláda');
    console.log(`✅ Nalezeno receptů: ${results.items?.length || 0}`);

    if (results.items && results.items.length > 0) {
      console.log('\nRecepty s čokoládou:');
      results.items.forEach((recipe, i) => {
        console.log(`  ${i + 1}. ${recipe.name}`);
      });
    } else {
      console.log('ℹ️  Žádné recepty s čokoládou nenalezeny');
    }

    return true;
  } catch (error) {
    console.error(`❌ Test selhal: ${error.message}`);
    return false;
  }
}

/**
 * Test 5: Vytvoření kolekce
 */
async function testCreateCollection() {
  console.log('\n\n📁 Test 5: Vytvoření kolekce');
  console.log('─'.repeat(50));

  try {
    const collection = {
      name: `Testovací kolekce ${new Date().toISOString()}`,
      description: 'Kolekce vytvořená MCP test scriptem',
      tags: ['test', 'automation']
    };

    const createdCollection = await apiCall('/collections', 'POST', collection);
    console.log(`✅ Kolekce vytvořena s ID: ${createdCollection.id}`);
    console.log(`   Název: ${createdCollection.name}`);

    return createdCollection.id;
  } catch (error) {
    console.error(`❌ Test selhal: ${error.message}`);
    return null;
  }
}

/**
 * Hlavní testovací funkce
 */
async function runTests() {
  console.log('🚀 Spouštím testy...\n');

  const results = {
    passed: 0,
    failed: 0,
    total: 5
  };

  // Test 1
  if (await testGetRecipes()) {
    results.passed++;
  } else {
    results.failed++;
  }

  // Test 2
  const recipeId = await testCreateRecipe();
  if (recipeId) {
    results.passed++;
  } else {
    results.failed++;
  }

  // Test 3
  if (await testGetRecipe(recipeId)) {
    results.passed++;
  } else {
    results.failed++;
  }

  // Test 4
  if (await testSearchRecipes()) {
    results.passed++;
  } else {
    results.failed++;
  }

  // Test 5
  if (await testCreateCollection()) {
    results.passed++;
  } else {
    results.failed++;
  }

  // Výsledky
  console.log('\n\n' + '='.repeat(50));
  console.log('📊 Výsledky testů');
  console.log('='.repeat(50));
  console.log(`✅ Úspěšné: ${results.passed}/${results.total}`);
  console.log(`❌ Neúspěšné: ${results.failed}/${results.total}`);
  console.log(`📈 Úspěšnost: ${Math.round((results.passed / results.total) * 100)}%`);

  if (results.failed === 0) {
    console.log('\n🎉 Všechny testy prošly!');
  } else {
    console.log('\n⚠️  Některé testy selhaly. Zkontrolujte logy výše.');
  }
}

// Spuštění testů
runTests().catch(error => {
  console.error('\n💥 Kritická chyba:', error);
  process.exit(1);
});
