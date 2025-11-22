#!/usr/bin/env node

/**
 * Mock Cookidoo API Server pro testování
 *
 * Tento server simuluje Cookidoo backend API a vrací fake data,
 * aby bylo možné otestovat MCP client bez nutnosti spouštět skutečný backend.
 */

import http from 'http';

const PORT = 5555;

// Mock data
const mockData = {
  auth: {
    accessToken: 'mock-jwt-token-12345',
    refreshToken: 'mock-refresh-token',
    expiresIn: 3600,
    tokenType: 'Bearer'
  },
  recipes: {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 20
  },
  collections: {
    items: [],
    totalCount: 0
  },
  shoppingList: {
    recipeIngredients: [],
    additionalItems: []
  },
  mealPlan: {
    weekStart: new Date().toISOString(),
    weekEnd: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
    days: []
  }
};

const server = http.createServer((req, res) => {
  // CORS headers
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, PATCH, OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization');

  if (req.method === 'OPTIONS') {
    res.writeHead(200);
    res.end();
    return;
  }

  const url = req.url;
  const method = req.method;

  console.log(`📡 ${method} ${url}`);

  // Login endpoint
  if (url === '/api/v1/auth/login' && method === 'POST') {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify(mockData.auth));
    return;
  }

  // Auth verify
  if (url === '/api/v1/auth/verify' && method === 'GET') {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ valid: true }));
    return;
  }

  // Recipes
  if (url.startsWith('/api/v1/recipes') && method === 'GET') {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify(mockData.recipes));
    return;
  }

  // Shopping list
  if (url === '/api/v1/shoppinglist' && method === 'GET') {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify(mockData.shoppingList));
    return;
  }

  if (url === '/api/v1/shoppinglist/recipes' && method === 'POST') {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ message: 'Přidáno receptů do nákupního seznamu' }));
    return;
  }

  // Meal plan
  if (url.startsWith('/api/v1/mealplan/week') && method === 'GET') {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify(mockData.mealPlan));
    return;
  }

  // Default response
  res.writeHead(200, { 'Content-Type': 'application/json' });
  res.end(JSON.stringify({ success: true, message: 'Mock response' }));
});

server.listen(PORT, () => {
  console.log(`🚀 Mock API Server běží na http://localhost:${PORT}`);
  console.log(`📡 Připraven přijímat požadavky od MCP clienta\n`);
});

// Graceful shutdown
process.on('SIGTERM', () => {
  console.log('\n👋 Ukončování mock serveru...');
  server.close(() => {
    console.log('✅ Server ukončen');
    process.exit(0);
  });
});

process.on('SIGINT', () => {
  console.log('\n👋 Ukončování mock serveru...');
  server.close(() => {
    console.log('✅ Server ukončen');
    process.exit(0);
  });
});
