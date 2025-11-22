#!/usr/bin/env node

/**
 * Test script pro verifikaci MCP tools
 *
 * Tento script ověří, že MCP server správně exportuje všechny tools
 * včetně nových pro shopping list a meal planning.
 */

import { spawn } from 'child_process';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

console.log('🧪 Test MCP Server Tools\n');
console.log('─'.repeat(60));

// Spustíme MCP server a pošleme mu request pro seznam tools
const mcpProcess = spawn('node', [join(__dirname, 'mcp-client.js')], {
  env: {
    ...process.env,
    COOKIDOO_API_URL: 'http://localhost:5555/api/v1',
    COOKIDOO_EMAIL: 'test@example.com',
    COOKIDOO_PASSWORD: 'test-password'
  },
  stdio: ['pipe', 'pipe', 'pipe']
});

let stdout = '';
let stderr = '';
let errorOccurred = false;

mcpProcess.stdout.on('data', (data) => {
  stdout += data.toString();
});

mcpProcess.stderr.on('data', (data) => {
  const message = data.toString();
  stderr += message;

  // Vypisujeme stderr live pro debugging
  if (message.includes('❌')) {
    console.error(message.trim());
    errorOccurred = true;
  } else if (message.includes('✅') || message.includes('🚀')) {
    console.log(message.trim());
  }
});

// Po 2 sekundách posíláme request pro tools/list
setTimeout(() => {
  const request = {
    jsonrpc: '2.0',
    id: 1,
    method: 'tools/list',
    params: {}
  };

  mcpProcess.stdin.write(JSON.stringify(request) + '\n');
}, 2000);

// Po 4 sekundách vyhodnocujeme
setTimeout(() => {
  mcpProcess.kill();

  console.log('\n' + '─'.repeat(60));
  console.log('📊 Vyhodnocení testů\n');

  // Parsujeme odpověď
  try {
    const lines = stdout.split('\n').filter(line => line.trim());
    let toolsList = null;

    for (const line of lines) {
      try {
        const parsed = JSON.parse(line);
        if (parsed.result && parsed.result.tools) {
          toolsList = parsed.result.tools;
          break;
        }
      } catch (e) {
        // Ignorujeme řádky, které nejsou JSON
      }
    }

    if (!toolsList) {
      console.log('❌ Nepodařilo se získat seznam tools z MCP serveru');
      console.log('\nStdout:');
      console.log(stdout);
      process.exit(1);
    }

    console.log(`✅ MCP server vrátil ${toolsList.length} tools\n`);

    // Očekávané tools
    const expectedTools = {
      // Původní tools
      'get_recipes': 'Získání seznamu receptů',
      'get_recipe': 'Získání detailu receptu',
      'create_recipe': 'Vytvoření receptu s Thermomix parametry',
      'get_collections': 'Získání kolekcí',
      'create_collection': 'Vytvoření kolekce',
      'add_recipe_to_collection': 'Přidání receptu do kolekce',
      'search_recipes': 'Vyhledání receptů',

      // Shopping list tools (nové)
      'get_shopping_list': 'Nákupní seznam',
      'add_recipes_to_shopping_list': 'Přidání receptů do nákupního seznamu',
      'remove_recipes_from_shopping_list': 'Odebrání receptů z nákupního seznamu',
      'mark_ingredients_as_owned': 'Označení ingrediencí jako zakoupených',
      'add_shopping_items': 'Přidání vlastních položek',
      'mark_shopping_items_as_owned': 'Označení položek jako zakoupených',
      'remove_shopping_items': 'Odebrání vlastních položek',
      'clear_shopping_list': 'Vymazání nákupního seznamu',

      // Meal planning tools (nové)
      'get_weekly_meal_plan': 'Týdenní plán jídel',
      'add_recipes_to_meal_plan': 'Přidání receptů do plánu',
      'remove_recipe_from_meal_plan': 'Odebrání receptu z plánu'
    };

    const toolNames = toolsList.map(t => t.name);
    const foundTools = [];
    const missingTools = [];

    // Kontrola všech očekávaných tools
    for (const [toolName, description] of Object.entries(expectedTools)) {
      if (toolNames.includes(toolName)) {
        foundTools.push(toolName);
      } else {
        missingTools.push(toolName);
      }
    }

    // Výpis výsledků
    console.log('📋 Kategorie tools:\n');

    console.log('🍽️  Recepty (7 tools):');
    ['get_recipes', 'get_recipe', 'create_recipe', 'get_collections',
     'create_collection', 'add_recipe_to_collection', 'search_recipes'].forEach(name => {
      const status = toolNames.includes(name) ? '✅' : '❌';
      console.log(`   ${status} ${name}`);
    });

    console.log('\n🛒 Nákupní seznam (8 tools):');
    ['get_shopping_list', 'add_recipes_to_shopping_list', 'remove_recipes_from_shopping_list',
     'mark_ingredients_as_owned', 'add_shopping_items', 'mark_shopping_items_as_owned',
     'remove_shopping_items', 'clear_shopping_list'].forEach(name => {
      const status = toolNames.includes(name) ? '✅' : '❌';
      console.log(`   ${status} ${name}`);
    });

    console.log('\n📅 Plánování jídel (3 tools):');
    ['get_weekly_meal_plan', 'add_recipes_to_meal_plan', 'remove_recipe_from_meal_plan'].forEach(name => {
      const status = toolNames.includes(name) ? '✅' : '❌';
      console.log(`   ${status} ${name}`);
    });

    // Celkový výsledek
    console.log('\n' + '─'.repeat(60));
    console.log('🎯 Celkový výsledek:\n');
    console.log(`   Očekáváno: ${Object.keys(expectedTools).length} tools`);
    console.log(`   Nalezeno:  ${foundTools.length} tools`);
    console.log(`   Chybí:     ${missingTools.length} tools`);

    if (missingTools.length > 0) {
      console.log('\n❌ Chybějící tools:');
      missingTools.forEach(tool => console.log(`   - ${tool}`));
      process.exit(1);
    }

    // Kontrola na extra tools
    const extraTools = toolNames.filter(name => !expectedTools[name]);
    if (extraTools.length > 0) {
      console.log('\n⚠️  Extra tools (neočekávané):');
      extraTools.forEach(tool => console.log(`   - ${tool}`));
    }

    if (!errorOccurred && missingTools.length === 0) {
      console.log('\n🎉 Všechny testy prošly! MCP server je plně funkční.');
      console.log('\n📝 Celkem implementováno:');
      console.log('   • 7 tools pro recepty a kolekce');
      console.log('   • 8 tools pro nákupní seznam');
      console.log('   • 3 tools pro plánování jídel');
      console.log('   ━━━━━━━━━━━━━━━━━━━━━━━━━━━');
      console.log('   • 18 tools celkem');
      process.exit(0);
    } else {
      console.log('\n⚠️  Test obsahoval chyby. Zkontrolujte výstup výše.');
      process.exit(1);
    }

  } catch (error) {
    console.error('❌ Chyba při parsování odpovědi:', error.message);
    console.log('\nStdout:', stdout);
    console.log('\nStderr:', stderr);
    process.exit(1);
  }
}, 4000);

mcpProcess.on('error', (error) => {
  console.error('❌ Chyba při spuštění MCP serveru:', error.message);
  process.exit(1);
});
