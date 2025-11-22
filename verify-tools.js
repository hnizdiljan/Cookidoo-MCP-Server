#!/usr/bin/env node

/**
 * Verifikace MCP tools v mcp-client.js
 *
 * Tento script parsuje mcp-client.js a ověřuje, že obsahuje
 * všechny očekávané MCP tools.
 */

import fs from 'fs';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

console.log('🔍 Verifikace MCP Tools v mcp-client.js\n');
console.log('─'.repeat(60));

// Načtení mcp-client.js
const mcpClientPath = join(__dirname, 'mcp-client.js');
const mcpClientCode = fs.readFileSync(mcpClientPath, 'utf-8');

// Očekávané tools
const expectedTools = {
  // Původní tools (7)
  'get_recipes': { category: 'Recepty', description: 'Získání seznamu receptů' },
  'get_recipe': { category: 'Recepty', description: 'Získání detailu receptu' },
  'create_recipe': { category: 'Recepty', description: 'Vytvoření receptu s Thermomix parametry' },
  'get_collections': { category: 'Recepty', description: 'Získání kolekcí' },
  'create_collection': { category: 'Recepty', description: 'Vytvoření kolekce' },
  'add_recipe_to_collection': { category: 'Recepty', description: 'Přidání receptu do kolekce' },
  'search_recipes': { category: 'Recepty', description: 'Vyhledání receptů' },

  // Shopping list tools (8) - NOVÉ
  'get_shopping_list': { category: 'Nákupní seznam', description: 'Získání nákupního seznamu', new: true },
  'add_recipes_to_shopping_list': { category: 'Nákupní seznam', description: 'Přidání receptů do seznamu', new: true },
  'remove_recipes_from_shopping_list': { category: 'Nákupní seznam', description: 'Odebrání receptů ze seznamu', new: true },
  'mark_ingredients_as_owned': { category: 'Nákupní seznam', description: 'Označení ingrediencí', new: true },
  'add_shopping_items': { category: 'Nákupní seznam', description: 'Přidání vlastních položek', new: true },
  'mark_shopping_items_as_owned': { category: 'Nákupní seznam', description: 'Označení položek', new: true },
  'remove_shopping_items': { category: 'Nákupní seznam', description: 'Odebrání položek', new: true },
  'clear_shopping_list': { category: 'Nákupní seznam', description: 'Vymazání seznamu', new: true },

  // Meal planning tools (3) - NOVÉ
  'get_weekly_meal_plan': { category: 'Plánování jídel', description: 'Týdenní plán', new: true },
  'add_recipes_to_meal_plan': { category: 'Plánování jídel', description: 'Přidání do plánu', new: true },
  'remove_recipe_from_meal_plan': { category: 'Plánování jídel', description: 'Odebrání z plánu', new: true }
};

// Funkce pro kontrolu přítomnosti tool v kódu
function checkToolInCode(toolName) {
  // Kontrola v tools/list (inputSchema)
  const toolDefinitionRegex = new RegExp(`name:\\s*["']${toolName}["']`, 'g');
  const hasDefinition = toolDefinitionRegex.test(mcpClientCode);

  // Kontrola v switch case handleru
  const handlerRegex = new RegExp(`case\\s+['"]${toolName}['"]\\s*:`, 'g');
  const hasHandler = handlerRegex.test(mcpClientCode);

  // Kontrola implementační funkce
  const functionName = toolName.replace(/_([a-z])/g, (_, letter) => letter.toUpperCase());
  const functionRegex = new RegExp(`async\\s+function\\s+${functionName}\\s*\\(`, 'g');
  const hasFunction = functionRegex.test(mcpClientCode);

  return {
    hasDefinition,
    hasHandler,
    hasFunction,
    complete: hasDefinition && hasHandler && hasFunction
  };
}

// Kategorizace tools
const categories = {
  'Recepty': [],
  'Nákupní seznam': [],
  'Plánování jídel': []
};

const results = {
  total: Object.keys(expectedTools).length,
  complete: 0,
  incomplete: 0,
  newTools: 0
};

// Kontrola všech tools
for (const [toolName, toolInfo] of Object.entries(expectedTools)) {
  const check = checkToolInCode(toolName);
  const status = check.complete ? '✅' : '❌';

  categories[toolInfo.category].push({
    name: toolName,
    status,
    check,
    isNew: toolInfo.new || false
  });

  if (check.complete) {
    results.complete++;
    if (toolInfo.new) {
      results.newTools++;
    }
  } else {
    results.incomplete++;
  }
}

// Výpis výsledků
console.log('\n📋 Kategorie MCP Tools:\n');

for (const [category, tools] of Object.entries(categories)) {
  const categoryIcon = category === 'Recepty' ? '🍽️' : category === 'Nákupní seznam' ? '🛒' : '📅';
  const newCount = tools.filter(t => t.isNew).length;
  const categoryHeader = newCount > 0 ? `${categoryIcon}  ${category} (${tools.length} tools, ${newCount} nových):` : `${categoryIcon}  ${category} (${tools.length} tools):`;

  console.log(categoryHeader);

  tools.forEach(tool => {
    const newBadge = tool.isNew ? ' 🆕' : '';
    const details = !tool.check.complete
      ? ` (❌ ${!tool.check.hasDefinition ? 'def ' : ''}${!tool.check.hasHandler ? 'handler ' : ''}${!tool.check.hasFunction ? 'func' : ''})`
      : '';
    console.log(`   ${tool.status} ${tool.name}${newBadge}${details}`);
  });

  console.log('');
}

// Celkový výsledek
console.log('─'.repeat(60));
console.log('🎯 Celkový výsledek:\n');
console.log(`   📊 Celkem tools:        ${results.total}`);
console.log(`   ✅ Kompletní:          ${results.complete}`);
console.log(`   ❌ Nekompletní:        ${results.incomplete}`);
console.log(`   🆕 Nové tools:         ${results.newTools}`);
console.log('');
console.log(`   📈 Úspěšnost:          ${Math.round((results.complete / results.total) * 100)}%`);

// Kontrola verzí
const versionMatch = mcpClientCode.match(/version:\s*["']([^"']+)["']/);
if (versionMatch) {
  console.log(`   📌 Verze MCP serveru:  ${versionMatch[1]}`);
}

if (results.incomplete === 0) {
  console.log('\n🎉 Skvělé! Všechny MCP tools jsou správně implementované!');
  console.log('\n📝 Implementováno:');
  console.log('   • 7 tools pro recepty a kolekce');
  console.log('   • 8 tools pro nákupní seznam (NOVÉ)');
  console.log('   • 3 tools pro plánování jídel (NOVÉ)');
  console.log('   ━━━━━━━━━━━━━━━━━━━━━━━━━━━');
  console.log(`   • ${results.total} tools celkem`);
  console.log(`   • ${results.newTools} nových tools přidáno`);
  process.exit(0);
} else {
  console.log('\n⚠️  Některé tools nejsou kompletně implementované.');
  console.log('   Zkontrolujte výstup výše pro detaily.');
  process.exit(1);
}
