#!/usr/bin/env node

/**
 * Cookidoo MCP Client
 * Model Context Protocol client pro Cursor s automatickým přihlášením
 */

import { Server } from '@modelcontextprotocol/sdk/server/index.js';
import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js';
import fs from 'fs/promises';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Konfigurace
const COOKIDOO_API_URL = process.env.COOKIDOO_API_URL || 'http://localhost:5555/api/v1';
const COOKIDOO_EMAIL = process.env.COOKIDOO_EMAIL;
const COOKIDOO_PASSWORD = process.env.COOKIDOO_PASSWORD;
const TOKEN_CACHE_FILE = path.join(__dirname, '.cookidoo-token.json');

// Globální proměnná pro token
let currentToken = null;
let tokenExpiresAt = null;

/**
 * Načte uložený token ze souboru
 */
async function loadCachedToken() {
  try {
    const data = await fs.readFile(TOKEN_CACHE_FILE, 'utf-8');
    const cached = JSON.parse(data);

    // Ověř, že token není expirován
    if (cached.expiresAt && new Date(cached.expiresAt) > new Date()) {
      console.error('✅ Načten cachovaný token');
      return cached;
    } else {
      console.error('⚠️  Cachovaný token expiroval');
      return null;
    }
  } catch (error) {
    // Soubor neexistuje nebo je poškozený
    return null;
  }
}

/**
 * Uloží token do souboru
 */
async function saveCachedToken(token, expiresIn) {
  try {
    const expiresAt = new Date(Date.now() + expiresIn * 1000);
    const cached = {
      accessToken: token,
      expiresAt: expiresAt.toISOString(),
      savedAt: new Date().toISOString()
    };

    await fs.writeFile(TOKEN_CACHE_FILE, JSON.stringify(cached, null, 2), 'utf-8');
    console.error('💾 Token uložen do cache');
  } catch (error) {
    console.error('⚠️  Nepodařilo se uložit token:', error.message);
  }
}

/**
 * Přihlásí se do Cookidoo pomocí emailu a hesla
 */
async function login() {
  console.error('🔐 Přihlašování do Cookidoo...');

  if (!COOKIDOO_EMAIL || !COOKIDOO_PASSWORD) {
    console.error('❌ COOKIDOO_EMAIL a COOKIDOO_PASSWORD environment proměnné jsou povinné');
    process.exit(1);
  }

  try {
    const response = await fetch(`${COOKIDOO_API_URL}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        email: COOKIDOO_EMAIL,
        password: COOKIDOO_PASSWORD
      })
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Přihlášení selhalo (${response.status}): ${errorText}`);
    }

    const data = await response.json();

    currentToken = data.accessToken;
    tokenExpiresAt = new Date(Date.now() + data.expiresIn * 1000);

    // Uložit do cache
    await saveCachedToken(data.accessToken, data.expiresIn);

    console.error(`✅ Přihlášení úspěšné (token vyprší: ${tokenExpiresAt.toLocaleString()})`);

    return data.accessToken;
  } catch (error) {
    console.error(`❌ Chyba při přihlášení: ${error.message}`);
    process.exit(1);
  }
}

/**
 * Získá platný token (z cache nebo novým přihlášením)
 */
async function getValidToken() {
  // Pokud máme platný token v paměti, použij ho
  if (currentToken && tokenExpiresAt && tokenExpiresAt > new Date()) {
    return currentToken;
  }

  // Zkus načíst z cache
  const cached = await loadCachedToken();
  if (cached) {
    currentToken = cached.accessToken;
    tokenExpiresAt = new Date(cached.expiresAt);
    return currentToken;
  }

  // Přihlas se
  return await login();
}

// MCP Server instance
const server = new Server(
  {
    name: "cookidoo-mcp-server",
    version: "2.0.0"
  },
  {
    capabilities: {
      tools: {},
      resources: {}
    }
  }
);

// Registrace tools
server.setRequestHandler('tools/list', async () => {
  return {
    tools: [
      {
        name: "get_recipes",
        description: "Získá seznam všech receptů uživatele",
        inputSchema: {
          type: "object",
          properties: {
            search: {
              type: "string",
              description: "Vyhledávací text pro filtrování receptů"
            },
            limit: {
              type: "number",
              description: "Maximální počet receptů (výchozí 10)"
            }
          }
        }
      },
      {
        name: "get_recipe",
        description: "Získá detail konkrétního receptu podle ID",
        inputSchema: {
          type: "object",
          properties: {
            id: {
              type: "string",
              description: "ID receptu"
            }
          },
          required: ["id"]
        }
      },
      {
        name: "create_recipe",
        description: "Vytvoří nový recept s Thermomix parametry",
        inputSchema: {
          type: "object",
          properties: {
            name: {
              type: "string",
              description: "Název receptu"
            },
            description: {
              type: "string",
              description: "Popis receptu"
            },
            ingredients: {
              type: "array",
              description: "Seznam ingrediencí",
              items: {
                type: "object",
                properties: {
                  text: { type: "string" },
                  name: { type: "string" },
                  quantity: { type: "number" },
                  unit: { type: "string" }
                }
              }
            },
            steps: {
              type: "array",
              description: "Postup přípravy - kroky receptu s Thermomix parametry",
              items: {
                type: "object",
                properties: {
                  text: {
                    type: "string",
                    description: "Popis kroku (např. 'zerkleinern', 'kochen', 'vermischen')"
                  },
                  order: {
                    type: "number",
                    description: "Pořadí kroku (1, 2, 3...)"
                  },
                  timeSeconds: {
                    type: "number",
                    description: "Čas v sekundách (např. 90 pro 1,5 minuty, 360 pro 6 minut)"
                  },
                  temperature: {
                    type: "number",
                    description: "Teplota v °C (0-120, vynechte pro bez ohřevu)"
                  },
                  speed: {
                    type: "number",
                    description: "Rychlost mixéru (1-10, např. 2 pro pomalé míchání, 8 pro sekání)"
                  },
                  useTurbo: {
                    type: "boolean",
                    description: "Použít Turbo režim (true/false)"
                  },
                  useReverseRotation: {
                    type: "boolean",
                    description: "Použít levo-otáčky - šetrné míchání (true/false)"
                  },
                  useVaroma: {
                    type: "boolean",
                    description: "Použít Varoma režim pro vaření v páře (true/false)"
                  }
                },
                required: ["text", "order"]
              }
            },
            preparationTimeMinutes: {
              type: "number",
              description: "Čas přípravy v minutách"
            },
            cookingTimeMinutes: {
              type: "number",
              description: "Čas vaření v minutách"
            },
            portions: {
              type: "number",
              description: "Počet porcí"
            },
            difficulty: {
              type: "number",
              description: "Obtížnost (1-5)"
            },
            tags: {
              type: "array",
              description: "Tagy/štítky",
              items: { type: "string" }
            }
          },
          required: ["name", "ingredients", "steps"]
        }
      },
      {
        name: "get_collections",
        description: "Získá seznam kolekcí receptů",
        inputSchema: {
          type: "object",
          properties: {
            limit: {
              type: "number",
              description: "Maximální počet kolekcí (výchozí 10)"
            }
          }
        }
      },
      {
        name: "create_collection",
        description: "Vytvoří novou kolekci receptů",
        inputSchema: {
          type: "object",
          properties: {
            name: {
              type: "string",
              description: "Název kolekce"
            },
            description: {
              type: "string",
              description: "Popis kolekce"
            },
            tags: {
              type: "array",
              description: "Tagy/štítky",
              items: { type: "string" }
            }
          },
          required: ["name"]
        }
      },
      {
        name: "add_recipe_to_collection",
        description: "Přidá recept do kolekce",
        inputSchema: {
          type: "object",
          properties: {
            collectionId: {
              type: "string",
              description: "ID kolekce"
            },
            recipeId: {
              type: "string",
              description: "ID receptu"
            }
          },
          required: ["collectionId", "recipeId"]
        }
      },
      {
        name: "search_recipes",
        description: "Vyhledá recepty podle zadaných kritérií",
        inputSchema: {
          type: "object",
          properties: {
            query: {
              type: "string",
              description: "Vyhledávací dotaz"
            },
            tags: {
              type: "array",
              description: "Filtrování podle tagů",
              items: { type: "string" }
            },
            difficulty: {
              type: "number",
              description: "Filtrování podle obtížnosti (1-5)"
            },
            maxTime: {
              type: "number",
              description: "Maximální celkový čas přípravy v minutách"
            }
          },
          required: ["query"]
        }
      },
      // === NÁKUPNÍ SEZNAM ===
      {
        name: "get_shopping_list",
        description: "Získá kompletní nákupní seznam s ingrediencemi z receptů a vlastními položkami",
        inputSchema: {
          type: "object",
          properties: {}
        }
      },
      {
        name: "add_recipes_to_shopping_list",
        description: "Přidá ingredience z receptů do nákupního seznamu",
        inputSchema: {
          type: "object",
          properties: {
            recipeIds: {
              type: "array",
              description: "ID receptů k přidání",
              items: { type: "string" }
            }
          },
          required: ["recipeIds"]
        }
      },
      {
        name: "remove_recipes_from_shopping_list",
        description: "Odebere ingredience receptů z nákupního seznamu",
        inputSchema: {
          type: "object",
          properties: {
            recipeIds: {
              type: "array",
              description: "ID receptů k odebrání",
              items: { type: "string" }
            }
          },
          required: ["recipeIds"]
        }
      },
      {
        name: "mark_ingredients_as_owned",
        description: "Označí ingredience jako již zakoupené (zaškrtne je)",
        inputSchema: {
          type: "object",
          properties: {
            ingredientIds: {
              type: "array",
              description: "ID ingrediencí k označení",
              items: { type: "string" }
            }
          },
          required: ["ingredientIds"]
        }
      },
      {
        name: "add_shopping_items",
        description: "Přidá vlastní položky do nákupního seznamu (ne z receptu)",
        inputSchema: {
          type: "object",
          properties: {
            items: {
              type: "array",
              description: "Názvy položek k přidání",
              items: { type: "string" }
            }
          },
          required: ["items"]
        }
      },
      {
        name: "mark_shopping_items_as_owned",
        description: "Označí vlastní položky jako zakoupené",
        inputSchema: {
          type: "object",
          properties: {
            itemIds: {
              type: "array",
              description: "ID položek k označení",
              items: { type: "string" }
            }
          },
          required: ["itemIds"]
        }
      },
      {
        name: "remove_shopping_items",
        description: "Odebere vlastní položky z nákupního seznamu",
        inputSchema: {
          type: "object",
          properties: {
            itemIds: {
              type: "array",
              description: "ID položek k odebrání",
              items: { type: "string" }
            }
          },
          required: ["itemIds"]
        }
      },
      {
        name: "clear_shopping_list",
        description: "Vymaže celý nákupní seznam",
        inputSchema: {
          type: "object",
          properties: {}
        }
      },
      // === PLÁNOVÁNÍ JÍDEL ===
      {
        name: "get_weekly_meal_plan",
        description: "Získá plán jídel pro daný týden",
        inputSchema: {
          type: "object",
          properties: {
            date: {
              type: "string",
              description: "Datum v týdnu (formát YYYY-MM-DD), volitelné - výchozí je tento týden"
            }
          }
        }
      },
      {
        name: "add_recipes_to_meal_plan",
        description: "Přidá recepty do kalendáře na konkrétní den",
        inputSchema: {
          type: "object",
          properties: {
            date: {
              type: "string",
              description: "Datum ve formátu YYYY-MM-DD"
            },
            recipeIds: {
              type: "array",
              description: "ID receptů k přidání",
              items: { type: "string" }
            },
            mealType: {
              type: "string",
              description: "Typ jídla: Snídaně, Oběd, Večeře (volitelné)"
            }
          },
          required: ["date", "recipeIds"]
        }
      },
      {
        name: "remove_recipe_from_meal_plan",
        description: "Odebere recept z kalendáře z konkrétního dne",
        inputSchema: {
          type: "object",
          properties: {
            recipeId: {
              type: "string",
              description: "ID receptu k odebrání"
            },
            date: {
              type: "string",
              description: "Datum ve formátu YYYY-MM-DD"
            }
          },
          required: ["recipeId", "date"]
        }
      }
    ]
  };
});

// Handler pro volání tools
server.setRequestHandler('tools/call', async (request) => {
  const { name, arguments: args } = request.params;

  try {
    switch (name) {
      case 'get_recipes':
        return await getRecipes(args);
      case 'get_recipe':
        return await getRecipe(args);
      case 'create_recipe':
        return await createRecipe(args);
      case 'get_collections':
        return await getCollections(args);
      case 'create_collection':
        return await createCollection(args);
      case 'add_recipe_to_collection':
        return await addRecipeToCollection(args);
      case 'search_recipes':
        return await searchRecipes(args);
      // Shopping list
      case 'get_shopping_list':
        return await getShoppingList(args);
      case 'add_recipes_to_shopping_list':
        return await addRecipesToShoppingList(args);
      case 'remove_recipes_from_shopping_list':
        return await removeRecipesFromShoppingList(args);
      case 'mark_ingredients_as_owned':
        return await markIngredientsAsOwned(args);
      case 'add_shopping_items':
        return await addShoppingItems(args);
      case 'mark_shopping_items_as_owned':
        return await markShoppingItemsAsOwned(args);
      case 'remove_shopping_items':
        return await removeShoppingItems(args);
      case 'clear_shopping_list':
        return await clearShoppingList(args);
      // Meal planning
      case 'get_weekly_meal_plan':
        return await getWeeklyMealPlan(args);
      case 'add_recipes_to_meal_plan':
        return await addRecipesToMealPlan(args);
      case 'remove_recipe_from_meal_plan':
        return await removeRecipeFromMealPlan(args);
      default:
        throw new Error(`Neznámý nástroj: ${name}`);
    }
  } catch (error) {
    return {
      content: [
        {
          type: "text",
          text: `❌ Chyba při volání ${name}: ${error.message}`
        }
      ],
      isError: true
    };
  }
});

// API helper funkce
async function apiCall(endpoint, method = 'GET', body = null) {
  const token = await getValidToken();
  const url = `${COOKIDOO_API_URL}${endpoint}`;

  const options = {
    method,
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  };

  if (body) {
    options.body = JSON.stringify(body);
  }

  const response = await fetch(url, options);

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(`HTTP ${response.status}: ${errorText}`);
  }

  return response.json();
}

// Tool implementace
async function getRecipes(args) {
  const { search, limit = 10 } = args || {};

  let endpoint = `/recipes?limit=${limit}`;
  if (search) {
    endpoint += `&search=${encodeURIComponent(search)}`;
  }

  const data = await apiCall(endpoint);

  return {
    content: [
      {
        type: "text",
        text: `📚 Načteno ${data.items?.length || 0} receptů:\n\n` +
              (data.items || []).map(recipe =>
                `🍽️ **${recipe.name}**\n` +
                `   📝 ${recipe.description || 'Bez popisu'}\n` +
                `   ⏱️ ${recipe.preparationTimeMinutes + recipe.cookingTimeMinutes} min\n` +
                `   👥 ${recipe.portions} porcí\n` +
                `   🏷️ ${(recipe.tags || []).join(', ')}\n`
              ).join('\n')
      }
    ]
  };
}

async function getRecipe(args) {
  const { id } = args;
  const recipe = await apiCall(`/recipes/${id}`);

  return {
    content: [
      {
        type: "text",
        text: `🍽️ **${recipe.name}**\n\n` +
              `📝 **Popis:** ${recipe.description || 'Bez popisu'}\n\n` +
              `📋 **Ingredience:**\n${recipe.ingredients.map(ing => `• ${ing.text}`).join('\n')}\n\n` +
              `👨‍🍳 **Postup:**\n${recipe.steps.map((step, i) => `${i + 1}. ${step.text}`).join('\n')}\n\n` +
              `⏱️ **Časy:** ${recipe.preparationTimeMinutes} min příprava + ${recipe.cookingTimeMinutes} min vaření\n` +
              `👥 **Porce:** ${recipe.portions}\n` +
              `📊 **Obtížnost:** ${recipe.difficulty}/5\n` +
              `🏷️ **Tagy:** ${(recipe.tags || []).join(', ')}`
      }
    ]
  };
}

async function createRecipe(args) {
  const recipe = await apiCall('/recipes', 'POST', args);

  return {
    content: [
      {
        type: "text",
        text: `✅ Recept "${recipe.name}" byl úspěšně vytvořen!\n` +
              `🆔 ID: ${recipe.id}\n` +
              `🔗 Můžete jej zobrazit pomocí: get_recipe s ID ${recipe.id}`
      }
    ]
  };
}

async function getCollections(args) {
  const { limit = 10 } = args || {};
  const data = await apiCall(`/collections?limit=${limit}`);

  return {
    content: [
      {
        type: "text",
        text: `📚 Načteno ${data.items?.length || 0} kolekcí:\n\n` +
              (data.items || []).map(collection =>
                `📁 **${collection.name}**\n` +
                `   📝 ${collection.description || 'Bez popisu'}\n` +
                `   📊 ${collection.recipeCount || 0} receptů\n` +
                `   🏷️ ${(collection.tags || []).join(', ')}\n`
              ).join('\n')
      }
    ]
  };
}

async function createCollection(args) {
  const collection = await apiCall('/collections', 'POST', args);

  return {
    content: [
      {
        type: "text",
        text: `✅ Kolekce "${collection.name}" byla úspěšně vytvořena!\n` +
              `🆔 ID: ${collection.id}`
      }
    ]
  };
}

async function addRecipeToCollection(args) {
  const { collectionId, recipeId } = args;
  await apiCall(`/collections/${collectionId}/recipes`, 'POST', { recipeId });

  return {
    content: [
      {
        type: "text",
        text: `✅ Recept byl úspěšně přidán do kolekce!`
      }
    ]
  };
}

async function searchRecipes(args) {
  const { query, tags, difficulty, maxTime } = args;

  let endpoint = `/recipes/search?q=${encodeURIComponent(query)}`;

  if (tags && tags.length > 0) {
    endpoint += `&tags=${tags.map(encodeURIComponent).join(',')}`;
  }
  if (difficulty) {
    endpoint += `&difficulty=${difficulty}`;
  }
  if (maxTime) {
    endpoint += `&maxTime=${maxTime}`;
  }

  const data = await apiCall(endpoint);

  return {
    content: [
      {
        type: "text",
        text: `🔍 Výsledky vyhledávání pro "${query}":\n\n` +
              `📊 Nalezeno ${data.items?.length || 0} receptů\n\n` +
              (data.items || []).map(recipe =>
                `🍽️ **${recipe.name}** (${recipe.id})\n` +
                `   📝 ${recipe.description || 'Bez popisu'}\n` +
                `   ⏱️ ${(recipe.preparationTimeMinutes || 0) + (recipe.cookingTimeMinutes || 0)} min\n` +
                `   📊 Obtížnost: ${recipe.difficulty}/5\n`
              ).join('\n')
      }
    ]
  };
}

// === SHOPPING LIST FUNCTIONS ===

async function getShoppingList(args) {
  const data = await apiCall('/shoppinglist');

  const recipeIngredientsText = (data.recipeIngredients || []).map(ing =>
    `${ing.isOwned ? '☑️' : '☐'} ${ing.text} (${ing.recipeName})`
  ).join('\n  ');

  const additionalItemsText = (data.additionalItems || []).map(item =>
    `${item.isOwned ? '☑️' : '☐'} ${item.name}`
  ).join('\n  ');

  return {
    content: [
      {
        type: "text",
        text: `📝 Nákupní seznam:\n\n` +
              `🍽️ Z receptů:\n  ${recipeIngredientsText || '(žádné ingredience)'}\n\n` +
              `📋 Vlastní položky:\n  ${additionalItemsText || '(žádné položky)'}`
      }
    ]
  };
}

async function addRecipesToShoppingList(args) {
  const { recipeIds } = args;
  const data = await apiCall('/shoppinglist/recipes', 'POST', { recipeIds });

  return {
    content: [
      {
        type: "text",
        text: `✅ ${data.message || `Přidáno ${recipeIds.length} receptů do nákupního seznamu`}`
      }
    ]
  };
}

async function removeRecipesFromShoppingList(args) {
  const { recipeIds } = args;
  const data = await apiCall('/shoppinglist/recipes', 'DELETE', { recipeIds });

  return {
    content: [
      {
        type: "text",
        text: `✅ ${data.message || `Odebráno ${recipeIds.length} receptů z nákupního seznamu`}`
      }
    ]
  };
}

async function markIngredientsAsOwned(args) {
  const { ingredientIds } = args;
  const data = await apiCall('/shoppinglist/ingredients/ownership', 'PATCH', { ingredientIds });

  return {
    content: [
      {
        type: "text",
        text: `✅ ${data.message || `Označeno ${ingredientIds.length} ingrediencí jako zakoupených`}`
      }
    ]
  };
}

async function addShoppingItems(args) {
  const { items } = args;
  const data = await apiCall('/shoppinglist/items', 'POST', { items });

  return {
    content: [
      {
        type: "text",
        text: `✅ ${data.message || `Přidáno ${items.length} položek do nákupního seznamu`}`
      }
    ]
  };
}

async function markShoppingItemsAsOwned(args) {
  const { itemIds } = args;
  const data = await apiCall('/shoppinglist/items/ownership', 'PATCH', { itemIds });

  return {
    content: [
      {
        type: "text",
        text: `✅ ${data.message || `Označeno ${itemIds.length} položek jako zakoupených`}`
      }
    ]
  };
}

async function removeShoppingItems(args) {
  const { itemIds } = args;
  const data = await apiCall('/shoppinglist/items', 'DELETE', { itemIds });

  return {
    content: [
      {
        type: "text",
        text: `✅ ${data.message || `Odebráno ${itemIds.length} položek z nákupního seznamu`}`
      }
    ]
  };
}

async function clearShoppingList(args) {
  const data = await apiCall('/shoppinglist', 'DELETE');

  return {
    content: [
      {
        type: "text",
        text: `✅ ${data.message || 'Nákupní seznam byl vymazán'}`
      }
    ]
  };
}

// === MEAL PLANNING FUNCTIONS ===

async function getWeeklyMealPlan(args) {
  const { date } = args || {};

  let endpoint = '/mealplan/week';
  if (date) {
    endpoint += `?date=${date}`;
  }

  const data = await apiCall(endpoint);

  const weekText = `📅 Plán jídel pro týden ${new Date(data.weekStart).toLocaleDateString('cs-CZ')} - ${new Date(data.weekEnd).toLocaleDateString('cs-CZ')}:\n\n`;

  const daysText = (data.days || []).map(day => {
    const dayDate = new Date(day.date).toLocaleDateString('cs-CZ', { weekday: 'long', day: 'numeric', month: 'numeric' });
    const mealsText = (day.meals || []).length > 0
      ? day.meals.map(meal => `  🍽️ ${meal.mealType}: ${meal.recipeName} (${meal.totalTime} min)`).join('\n')
      : '  (Žádný plán)';

    return `${day.dayName} ${dayDate}:\n${mealsText}`;
  }).join('\n\n');

  return {
    content: [
      {
        type: "text",
        text: weekText + daysText
      }
    ]
  };
}

async function addRecipesToMealPlan(args) {
  const { date, recipeIds, mealType } = args;
  const data = await apiCall('/mealplan/recipes', 'POST', { date, recipeIds, mealType });

  return {
    content: [
      {
        type: "text",
        text: `✅ ${data.message || `Přidáno ${recipeIds.length} receptů do plánu na ${new Date(date).toLocaleDateString('cs-CZ')}`}`
      }
    ]
  };
}

async function removeRecipeFromMealPlan(args) {
  const { recipeId, date } = args;
  const data = await apiCall(`/mealplan/recipes/${recipeId}?date=${date}`, 'DELETE');

  return {
    content: [
      {
        type: "text",
        text: `✅ ${data.message || `Recept odebrán z plánu pro ${new Date(date).toLocaleDateString('cs-CZ')}`}`
      }
    ]
  };
}

// Error handling
process.on('uncaughtException', (error) => {
  console.error('❌ Neočekávaná chyba:', error);
  process.exit(1);
});

process.on('unhandledRejection', (reason, promise) => {
  console.error('❌ Neošetřená Promise rejection:', reason);
  process.exit(1);
});

// Spuštění MCP serveru
const transport = new StdioServerTransport();
server.connect(transport).catch(error => {
  console.error('❌ Chyba při spuštění MCP serveru:', error);
  process.exit(1);
});

// Přihlásit se při startu
await getValidToken();

console.error('🚀 Cookidoo MCP Server je spuštěn s automatickým přihlášením...');
