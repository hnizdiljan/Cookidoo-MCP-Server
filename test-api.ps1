# Test Script pro Cookidoo MCP Server API
# PowerShell script pro testování všech hlavních endpointů

param(
    [string]$BaseUrl = "http://localhost:5555",
    [string]$Email = "",
    [string]$Password = ""
)

Write-Host "Cookidoo MCP Server API Test" -ForegroundColor Yellow
Write-Host "=============================" -ForegroundColor Yellow
Write-Host ""

# Test 1: Health Check
Write-Host "Test 1: Health Check" -ForegroundColor Cyan
try {
    $health = Invoke-RestMethod -Uri "$BaseUrl/health" -Method GET
    Write-Host "OK - Health check passed" -ForegroundColor Green
} catch {
    Write-Host "FAIL - Health check failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 2: API Info
Write-Host ""
Write-Host "Test 2: API Info" -ForegroundColor Cyan
try {
    $apiInfo = Invoke-RestMethod -Uri "$BaseUrl/api" -Method GET
    Write-Host "OK - API Info: $($apiInfo.message)" -ForegroundColor Green
    Write-Host "   Version: $($apiInfo.version)" -ForegroundColor Gray
} catch {
    Write-Host "FAIL - API Info failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Swagger JSON
Write-Host ""
Write-Host "Test 3: Swagger Documentation" -ForegroundColor Cyan
try {
    $swagger = Invoke-RestMethod -Uri "$BaseUrl/swagger/v1/swagger.json" -Method GET
    $endpointCount = ($swagger.paths | Get-Member -Type NoteProperty).Count
    Write-Host "OK - Swagger JSON loaded with $endpointCount endpoints" -ForegroundColor Green
} catch {
    Write-Host "FAIL - Swagger JSON failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Authentication (pokud jsou poskytnuty credentials)
if ($Email -and $Password) {
    Write-Host ""
    Write-Host "Test 4: Authentication" -ForegroundColor Cyan
    
    $loginRequest = @{
        email = $Email
        password = $Password
    } | ConvertTo-Json
    
    try {
        $authResponse = Invoke-RestMethod -Uri "$BaseUrl/api/v1/auth/login" -Method POST -Body $loginRequest -ContentType "application/json"
        Write-Host "OK - Login successful" -ForegroundColor Green
        Write-Host "   User: $($authResponse.email)" -ForegroundColor Gray
        Write-Host "   Token expires: $($authResponse.expiresAt)" -ForegroundColor Gray
        
        $token = $authResponse.accessToken
        
        # Test 5: Protected endpoint s tokenem
        Write-Host ""
        Write-Host "Test 5: Protected Endpoint (Recipes)" -ForegroundColor Cyan
        
        $headers = @{
            "Authorization" = "Bearer $token"
        }
        
        try {
            $recipes = Invoke-RestMethod -Uri "$BaseUrl/api/v1/recipes" -Method GET -Headers $headers
            Write-Host "OK - Recipes endpoint returned $($recipes.totalCount) recipes" -ForegroundColor Green
        } catch {
            Write-Host "FAIL - Recipes endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
        }
        
        # Test 6: Collections endpoint
        Write-Host ""
        Write-Host "Test 6: Collections Endpoint" -ForegroundColor Cyan
        
        try {
            $collections = Invoke-RestMethod -Uri "$BaseUrl/api/v1/collections" -Method GET -Headers $headers
            Write-Host "OK - Collections endpoint returned $($collections.totalCount) collections" -ForegroundColor Green
        } catch {
            Write-Host "FAIL - Collections endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
        }
        
    } catch {
        Write-Host "FAIL - Login failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "   Check your Cookidoo email and password" -ForegroundColor Yellow
    }
} else {
    Write-Host ""
    Write-Host "WARNING - For authentication testing use:" -ForegroundColor Yellow
    Write-Host "   .\test-api.ps1 -Email 'your@email.com' -Password 'password'" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Test Results Summary:" -ForegroundColor Yellow
Write-Host "• Welcome page: $BaseUrl" -ForegroundColor White
Write-Host "• API documentation: $BaseUrl/swagger" -ForegroundColor White
Write-Host "• Health check: $BaseUrl/health" -ForegroundColor White
Write-Host ""
Write-Host "Testing completed!" -ForegroundColor Green 