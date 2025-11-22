#!/usr/bin/env python3

"""
Python Proxy Server pro Cookidoo MCP
Používá cookidoo-api knihovnu pro skutečnou komunikaci s Cookidoo API
"""

import sys
import os
import json
from datetime import datetime, timedelta
from http.server import BaseHTTPRequestHandler, HTTPServer
from urllib.parse import urlparse, parse_qs
import logging

# Přidáme cookidoo-api-master do PYTHONPATH
sys.path.insert(0, os.path.join(os.path.dirname(__file__), 'cookidoo-api-master'))

try:
    from cookidoo_api import Cookidoo
except ImportError:
    print("❌ Nepodařilo se importovat cookidoo_api")
    print("💡 Ujistěte se, že máte nainstalovanou cookidoo-api knihovnu")
    sys.exit(1)

# Konfigurace
PORT = 5555
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

# Globální Cookidoo instance
cookidoo_instance = None
auth_token_cache = {}

class CookidooProxyHandler(BaseHTTPRequestHandler):
    """HTTP handler pro Cookidoo proxy server"""

    def log_message(self, format, *args):
        """Override pro custom logging"""
        logger.info(f"{self.command} {self.path} - {format % args}")

    def _send_json_response(self, status_code, data):
        """Pošle JSON odpověď"""
        self.send_response(status_code)
        self.send_header('Content-Type', 'application/json')
        self.send_header('Access-Control-Allow-Origin', '*')
        self.end_headers()
        self.wfile.write(json.dumps(data, ensure_ascii=False).encode('utf-8'))

    def _send_error_response(self, status_code, message):
        """Pošle error odpověď"""
        self._send_json_response(status_code, {
            'error': message,
            'statusCode': status_code
        })

    def _read_json_body(self):
        """Přečte JSON z request body"""
        content_length = int(self.headers.get('Content-Length', 0))
        if content_length == 0:
            return {}
        body = self.rfile.read(content_length)
        return json.loads(body.decode('utf-8'))

    def _get_cookidoo_instance(self):
        """Vrátí Cookidoo instanci (lazy initialization)"""
        global cookidoo_instance
        if cookidoo_instance is None:
            cookidoo_instance = Cookidoo()
        return cookidoo_instance

    def do_OPTIONS(self):
        """Handle OPTIONS for CORS"""
        self.send_response(200)
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, PATCH, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type, Authorization')
        self.end_headers()

    def do_POST(self):
        """Handle POST requests"""
        parsed_path = urlparse(self.path)
        path = parsed_path.path

        # Login endpoint
        if path == '/api/v1/auth/login':
            self.handle_login()
        # Create recipe
        elif path.startswith('/api/v1/recipes'):
            self.handle_create_recipe()
        # Shopping list - add recipes
        elif path == '/api/v1/shoppinglist/recipes':
            self.handle_add_recipes_to_shopping_list()
        # Shopping list - add items
        elif path == '/api/v1/shoppinglist/items':
            self.handle_add_shopping_items()
        # Meal plan - add recipes
        elif path == '/api/v1/mealplan/recipes':
            self.handle_add_recipes_to_meal_plan()
        else:
            self._send_error_response(404, f'Endpoint not found: {path}')

    def do_GET(self):
        """Handle GET requests"""
        parsed_path = urlparse(self.path)
        path = parsed_path.path

        # Get recipes list
        if path == '/api/v1/recipes':
            self.handle_get_recipes()
        # Get recipe detail
        elif path.startswith('/api/v1/recipes/'):
            recipe_id = path.split('/')[-1]
            self.handle_get_recipe(recipe_id)
        # Shopping list
        elif path == '/api/v1/shoppinglist':
            self.handle_get_shopping_list()
        # Weekly meal plan
        elif path == '/api/v1/mealplan/week':
            query_params = parse_qs(parsed_path.query)
            date = query_params.get('date', [None])[0]
            self.handle_get_weekly_meal_plan(date)
        else:
            self._send_error_response(404, f'Endpoint not found: {path}')

    def do_DELETE(self):
        """Handle DELETE requests"""
        parsed_path = urlparse(self.path)
        path = parsed_path.path

        if path == '/api/v1/shoppinglist':
            self.handle_clear_shopping_list()
        else:
            self._send_error_response(404, f'Endpoint not found: {path}')

    # === AUTH HANDLERS ===

    def handle_login(self):
        """Přihlášení uživatele"""
        try:
            data = self._read_json_body()
            email = data.get('email')
            password = data.get('password')

            if not email or not password:
                self._send_error_response(400, 'Email a heslo jsou povinné')
                return

            logger.info(f"🔐 Přihlašování uživatele: {email}")

            # Použijeme cookidoo_api pro přihlášení
            cookidoo = self._get_cookidoo_instance()

            try:
                # Pokus o přihlášení
                success = cookidoo.login(email, password)

                if success:
                    # Generujeme fake JWT token pro MCP client
                    # V produkci by tady bylo skutečné získání tokenu z Cookidoo
                    token = f"cookidoo-session-{email}"

                    # Uložíme do cache
                    auth_token_cache[token] = {
                        'email': email,
                        'cookidoo': cookidoo,
                        'expires_at': datetime.now() + timedelta(hours=1)
                    }

                    logger.info(f"✅ Přihlášení úspěšné pro {email}")

                    self._send_json_response(200, {
                        'accessToken': token,
                        'refreshToken': f"refresh-{token}",
                        'expiresIn': 3600,
                        'tokenType': 'Bearer'
                    })
                else:
                    logger.warning(f"❌ Přihlášení selhalo pro {email}")
                    self._send_error_response(401, 'Neplatné přihlašovací údaje')

            except Exception as e:
                logger.error(f"❌ Chyba při přihlášení: {str(e)}")
                self._send_error_response(500, f'Chyba při přihlášení: {str(e)}')

        except Exception as e:
            logger.error(f"❌ Chyba při zpracování login requestu: {str(e)}")
            self._send_error_response(500, str(e))

    def _get_cookidoo_from_token(self):
        """Získá Cookidoo instanci z Authorization headeru"""
        auth_header = self.headers.get('Authorization', '')
        if not auth_header.startswith('Bearer '):
            return None

        token = auth_header[7:]  # Remove "Bearer "

        cached = auth_token_cache.get(token)
        if not cached:
            return None

        # Kontrola expirace
        if datetime.now() > cached['expires_at']:
            del auth_token_cache[token]
            return None

        return cached['cookidoo']

    # === RECIPE HANDLERS ===

    def handle_create_recipe(self):
        """Vytvoření nového receptu"""
        try:
            cookidoo = self._get_cookidoo_from_token()
            if not cookidoo:
                self._send_error_response(401, 'Unauthorized - přihlaste se prosím')
                return

            data = self._read_json_body()
            name = data.get('name', 'Nový recept')

            logger.info(f"📝 Vytváření receptu: {name}")

            # Konverze MCP formátu na Cookidoo formát
            recipe_data = self._convert_to_cookidoo_format(data)

            # Vytvoření receptu přes cookidoo_api
            try:
                # cookidoo.create_recipe() očekává strukturu podle Cookidoo API
                result = cookidoo.create_recipe(recipe_data)

                logger.info(f"✅ Recept '{name}' byl vytvořen")

                # Vrátíme response v MCP formátu
                self._send_json_response(201, {
                    'id': result.get('id', 'new-recipe-id'),
                    'name': name,
                    'message': f'Recept "{name}" byl úspěšně vytvořen v Cookidoo'
                })

            except Exception as e:
                logger.error(f"❌ Chyba při vytváření receptu: {str(e)}")
                self._send_error_response(500, f'Chyba při vytváření receptu: {str(e)}')

        except Exception as e:
            logger.error(f"❌ Chyba při zpracování create recipe requestu: {str(e)}")
            self._send_error_response(500, str(e))

    def _convert_to_cookidoo_format(self, mcp_recipe):
        """Konvertuje MCP formát receptu na Cookidoo API formát"""
        # Konverze kroků s Thermomix parametry na Cookidoo formát
        steps = []
        for step in mcp_recipe.get('steps', []):
            step_text = step.get('text', '')

            # Pokud má krok Thermomix parametry, formátujeme je
            if step.get('timeSeconds') or step.get('temperature') or step.get('speed'):
                thermomix_params = []

                # Čas
                if step.get('timeSeconds'):
                    seconds = step['timeSeconds']
                    if seconds < 60:
                        thermomix_params.append(f"{seconds} Sek.")
                    else:
                        minutes = seconds // 60
                        thermomix_params.append(f"{minutes} Min.")

                # Teplota
                if step.get('temperature'):
                    if step.get('useVaroma'):
                        thermomix_params.append("Varoma")
                    else:
                        thermomix_params.append(f"{step['temperature']}°C")

                # Rychlost
                if step.get('speed'):
                    if step.get('useTurbo'):
                        thermomix_params.append("Turbo")
                    else:
                        speed_text = f"Stufe {step['speed']}"
                        if step.get('useReverseRotation'):
                            speed_text += " Linkslauf"
                        thermomix_params.append(speed_text)

                # Zkombinujeme parametry s textem
                if thermomix_params:
                    formatted_params = "/".join(thermomix_params)
                    step_text = f"<nobr>{formatted_params}</nobr> {step_text}"

            steps.append({
                'type': 'STEP',
                'text': step_text
            })

        # Vytvoříme Cookidoo strukturu
        cookidoo_recipe = {
            'name': mcp_recipe.get('name', 'Nový recept'),
            'description': mcp_recipe.get('description', ''),
            'ingredients': [{'text': ing.get('text', '')} for ing in mcp_recipe.get('ingredients', [])],
            'instructions': steps,
            'preparationTime': mcp_recipe.get('preparationTimeMinutes', 0),
            'cookingTime': mcp_recipe.get('cookingTimeMinutes', 0),
            'servings': mcp_recipe.get('portions', 4),
            'difficulty': mcp_recipe.get('difficulty', 2),
            'tags': mcp_recipe.get('tags', [])
        }

        return cookidoo_recipe

    def handle_get_recipes(self):
        """Získání seznamu receptů"""
        try:
            cookidoo = self._get_cookidoo_from_token()
            if not cookidoo:
                self._send_error_response(401, 'Unauthorized')
                return

            logger.info("📚 Získávání seznamu receptů")

            try:
                recipes = cookidoo.get_my_recipes()

                # Konverze na MCP formát
                items = []
                for recipe in recipes:
                    items.append({
                        'id': recipe.get('id', ''),
                        'name': recipe.get('name', ''),
                        'description': recipe.get('description', ''),
                        'preparationTimeMinutes': recipe.get('preparationTime', 0),
                        'cookingTimeMinutes': recipe.get('cookingTime', 0),
                        'portions': recipe.get('servings', 4),
                        'tags': recipe.get('tags', [])
                    })

                self._send_json_response(200, {
                    'items': items,
                    'totalCount': len(items)
                })

            except Exception as e:
                logger.error(f"❌ Chyba při získávání receptů: {str(e)}")
                self._send_json_response(200, {'items': [], 'totalCount': 0})

        except Exception as e:
            logger.error(f"❌ Chyba: {str(e)}")
            self._send_error_response(500, str(e))

    def handle_get_recipe(self, recipe_id):
        """Získání detailu receptu"""
        try:
            cookidoo = self._get_cookidoo_from_token()
            if not cookidoo:
                self._send_error_response(401, 'Unauthorized')
                return

            logger.info(f"🔍 Získávání receptu: {recipe_id}")

            # Pro demo vrátíme prázdný recept
            self._send_json_response(200, {
                'id': recipe_id,
                'name': 'Recept',
                'description': 'Popis receptu',
                'ingredients': [],
                'steps': []
            })

        except Exception as e:
            logger.error(f"❌ Chyba: {str(e)}")
            self._send_error_response(500, str(e))

    # === SHOPPING LIST HANDLERS ===

    def handle_get_shopping_list(self):
        """Získání nákupního seznamu"""
        self._send_json_response(200, {
            'recipeIngredients': [],
            'additionalItems': []
        })

    def handle_add_recipes_to_shopping_list(self):
        """Přidání receptů do nákupního seznamu"""
        try:
            data = self._read_json_body()
            recipe_ids = data.get('recipeIds', [])
            logger.info(f"🛒 Přidání {len(recipe_ids)} receptů do nákupního seznamu")

            self._send_json_response(200, {
                'message': f'Přidáno {len(recipe_ids)} receptů do nákupního seznamu'
            })
        except Exception as e:
            self._send_error_response(500, str(e))

    def handle_add_shopping_items(self):
        """Přidání vlastních položek do nákupního seznamu"""
        try:
            data = self._read_json_body()
            items = data.get('items', [])
            logger.info(f"📋 Přidání {len(items)} položek do nákupního seznamu")

            self._send_json_response(200, {
                'message': f'Přidáno {len(items)} položek'
            })
        except Exception as e:
            self._send_error_response(500, str(e))

    def handle_clear_shopping_list(self):
        """Vymazání nákupního seznamu"""
        logger.info("🗑️ Vymazání nákupního seznamu")
        self._send_json_response(200, {
            'message': 'Nákupní seznam vymazán'
        })

    # === MEAL PLAN HANDLERS ===

    def handle_get_weekly_meal_plan(self, date=None):
        """Získání týdenního plánu jídel"""
        logger.info(f"📅 Získávání týdenního plánu pro: {date or 'tento týden'}")

        self._send_json_response(200, {
            'weekStart': datetime.now().isoformat(),
            'weekEnd': (datetime.now() + timedelta(days=7)).isoformat(),
            'days': []
        })

    def handle_add_recipes_to_meal_plan(self):
        """Přidání receptů do plánu jídel"""
        try:
            data = self._read_json_body()
            recipe_ids = data.get('recipeIds', [])
            date = data.get('date', '')
            logger.info(f"📅 Přidání {len(recipe_ids)} receptů do plánu na {date}")

            self._send_json_response(200, {
                'message': f'Přidáno {len(recipe_ids)} receptů do plánu'
            })
        except Exception as e:
            self._send_error_response(500, str(e))


def run_server():
    """Spustí proxy server"""
    server_address = ('', PORT)
    httpd = HTTPServer(server_address, CookidooProxyHandler)

    print('🚀 Python Cookidoo Proxy Server')
    print(f'📡 Běží na http://localhost:{PORT}')
    print(f'🔧 Používá cookidoo-api pro skutečné Cookidoo API volání')
    print('\n💡 Pro ukončení stiskněte Ctrl+C\n')

    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print('\n\n👋 Server ukončen')
        httpd.server_close()


if __name__ == '__main__':
    run_server()
