#!/usr/bin/env node

/**
 * Cookidoo MCP Client
 * Model Context Protocol client pro Cursor
 */

const { Server } = require('@modelcontextprotocol/sdk/server/index.js');
const { StdioServerTransport } = require('@modelcontextprotocol/sdk/server/stdio.js');

// Konfigurace
const COOKIDOO_API_URL = process.env.COOKIDOO_API_URL || 'http://localhost:5000/api/v1';
const COOKIDOO_TOKEN = process.env.COOKIDOO_TOKEN;

if (!COOKIDOO_TOKEN) {
  console.error('❌ COOKIDOO_TOKEN environment variable is required');
  process.exit(1);
}

// MCP Server instance
const server = new Server(
  {
    name: "cookidoo-mcp-server",
    version: "1.0.0"
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
        description: "Vytvoří nový recept",
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

console.error('🚀 Cookidoo MCP Server je spuštěn...'); 