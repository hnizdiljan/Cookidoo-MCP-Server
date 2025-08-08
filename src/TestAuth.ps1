# Test OAuth2 autentizace pro Cookidoo MCP Server
Write-Host "🍳 Cookidoo MCP Server - OAuth2 Test" -ForegroundColor Yellow
Write-Host "====================================" -ForegroundColor Yellow
Write-Host ""

# Kontrola, zda běží server
$serverUrl = "http://localhost:5555"
try {
    $response = Invoke-WebRequest -Uri "$serverUrl/swagger" -Method HEAD -TimeoutSec 5 -ErrorAction Stop
    Write-Host "✅ Server běží na $serverUrl" -ForegroundColor Green
} catch {
    Write-Host "❌ Server neběží. Spusťte nejprve:" -ForegroundColor Red
    Write-Host "   cd Cookidoo.MCP.Api" -ForegroundColor Cyan
    Write-Host "   dotnet run" -ForegroundColor Cyan
    exit 1
}

Write-Host ""
Write-Host "🔧 Test 1: Mock přihlašovací údaje (očekáváme chybu 401)" -ForegroundColor Cyan

$mockBody = @{
    email = "test@example.com"
    password = "wrongpassword"
} | ConvertTo-Json

try {
    $mockResponse = Invoke-RestMethod -Uri "$serverUrl/api/v1/auth/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body $mockBody `
        -ErrorAction Stop
    
    Write-Host "❌ Neočekávaný úspěch!" -ForegroundColor Red
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 401) {
        Write-Host "✅ OAuth2 komunikace funguje - získali jsme očekávanou chybu 401" -ForegroundColor Green
        Write-Host "   API správně odmítlo neplatné přihlašovací údaje" -ForegroundColor Gray
    } else {
        Write-Host "⚠️  Jiná chyba: HTTP $statusCode" -ForegroundColor Yellow
        Write-Host $_.Exception.Message -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "🔧 Test 2: Chcete vyzkoušet s reálnými Cookidoo údaji? (y/n)" -ForegroundColor Cyan
$response = Read-Host

if ($response -eq "y" -or $response -eq "yes" -or $response -eq "Y") {
    Write-Host ""
    $email = Read-Host "📧 Email"
    $securePassword = Read-Host "🔐 Heslo" -AsSecureString
    $password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword))
    
    if ($email -and $password) {
        Write-Host ""
        Write-Host "🔄 Přihlašuji..." -ForegroundColor Yellow
        
        $realBody = @{
            email = $email
            password = $password
        } | ConvertTo-Json
        
        try {
            $realResponse = Invoke-RestMethod -Uri "$serverUrl/api/v1/auth/login" `
                -Method POST `
                -ContentType "application/json" `
                -Body $realBody `
                -ErrorAction Stop
            
            Write-Host "✅ Přihlášení úspěšné!" -ForegroundColor Green
            Write-Host "   Access Token: $($realResponse.accessToken.Substring(0, 20))..." -ForegroundColor Gray
            Write-Host "   Token Type: $($realResponse.tokenType)" -ForegroundColor Gray
            Write-Host "   Expires In: $($realResponse.expiresIn) sekund" -ForegroundColor Gray
            Write-Host "   User ID: $($realResponse.userId)" -ForegroundColor Gray
            
            # Test načtení receptů
            Write-Host ""
            Write-Host "🔄 Testuji načtení receptů..." -ForegroundColor Yellow
            
            $headers = @{
                "Authorization" = "Bearer $($realResponse.accessToken)"
            }
            
            try {
                $recipesResponse = Invoke-RestMethod -Uri "$serverUrl/api/v1/recipes/my-recipes" `
                    -Method GET `
                    -Headers $headers `
                    -ErrorAction Stop
                
                Write-Host "✅ Recepty úspěšně načteny!" -ForegroundColor Green
                Write-Host "   Počet receptů: $($recipesResponse.Count)" -ForegroundColor Gray
            } catch {
                Write-Host "⚠️  Nepodařilo se načíst recepty:" -ForegroundColor Yellow
                Write-Host "   $($_.Exception.Message)" -ForegroundColor Yellow
            }
            
        } catch {
            $statusCode = $_.Exception.Response.StatusCode.value__
            if ($statusCode -eq 401) {
                Write-Host "❌ Neplatné přihlašovací údaje" -ForegroundColor Red
            } else {
                Write-Host "❌ Chyba při přihlašování: HTTP $statusCode" -ForegroundColor Red
                Write-Host $_.Exception.Message -ForegroundColor Red
            }
        }
    }
}

Write-Host ""
Write-Host "🎉 Test dokončen!" -ForegroundColor Green
Write-Host ""
Write-Host "💡 Pro další testování:" -ForegroundColor Cyan
Write-Host "   • Otevřete Swagger: $serverUrl/swagger" -ForegroundColor Gray
Write-Host "   • Použijte Postman nebo curl" -ForegroundColor Gray
Write-Host "   • Implementujte klientskou aplikaci" -ForegroundColor Gray 