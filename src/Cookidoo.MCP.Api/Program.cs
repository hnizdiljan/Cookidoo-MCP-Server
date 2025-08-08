using Cookidoo.MCP.Infrastructure.Extensions;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Konfigurace Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/cookidoo-mcp-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Přidání služeb
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger konfigurace
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cookidoo MCP Server API",
        Version = "v1",
        Description = "MCP Server pro správu receptů z Cookidoo platformy. Poskytuje RESTful API pro autentizaci, správu receptů a kolekcí receptů synchronizovaných s Cookidoo platformou.",
        Contact = new OpenApiContact
        {
            Name = "Cookidoo MCP Server",
            Email = "support@cookidoo-mcp.com",
            Url = new Uri("https://github.com/your-repo/cookidoo-mcp-server")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License", 
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Přidání XML komentářů
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Cookidoo JWT token autentizace pro Swagger
    c.AddSecurityDefinition("CookidooJWT", new OpenApiSecurityScheme
    {
        Description = @"Cookidoo JWT token z _oauth2_proxy cookie. 
                        Získejte token z Cookidoo webového rozhraní (F12 → Application → Cookies → _oauth2_proxy).
                        Zadejte 'Bearer {token}' nebo použijte query parametr 'jwt_token={token}'.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "CookidooJWT"
                },
                Scheme = "Bearer",
                Name = "Authorization",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // Přidání příkladů a anotací
    c.EnableAnnotations();
    
    // Přidání tagů pro lepší organizaci
    c.TagActionsBy(api => new[] { api.GroupName ?? "Default" });
    
    // Lepší popis chyb
    c.DescribeAllParametersInCamelCase();
});

// API versioning
builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader("version"),
        new HeaderApiVersionReader("X-Version"),
        new MediaTypeApiVersionReader("ver")
    );
});

builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Registrace Cookidoo služeb
builder.Services.AddCookidooServices(builder.Configuration);

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Konfigurace HTTP pipeline
// Swagger je povolen vždy (nejen v Development)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cookidoo MCP Server API v1");
    c.RoutePrefix = "swagger"; // Swagger na /swagger
    c.DefaultModelsExpandDepth(-1); // Schovej modely ve výchozím stavu
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
    c.EnableFilter();
    c.ShowExtensions();
    c.EnableValidator();
    c.DocumentTitle = "Cookidoo MCP Server API Documentation";
    
    // Přidání custom CSS a JS
    c.InjectStylesheet("/swagger-ui/custom.css");
    c.HeadContent = @"
        <style>
            .swagger-ui .topbar { display: none; }
            .swagger-ui .info .title { color: #e67e22; }
            .swagger-ui .scheme-container { background: #f8f9fa; }
        </style>";
});

// HTTPS redirect pouze pokud není HTTP-only režim
if (!app.Environment.IsDevelopment() || app.Urls.Any(url => url.StartsWith("https")))
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowAll");
// Nepoužíváme vlastní autentizaci - JWT token se ověřuje přímo v controllerech

app.MapControllers();
app.MapHealthChecks("/health");

// Welcome page
app.MapGet("/", () => Results.Content(@"
<!DOCTYPE html>
<html lang='cs'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Cookidoo MCP Server</title>
    <style>
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; min-height: 100vh; }
        .container { max-width: 800px; margin: 0 auto; text-align: center; }
        .header { margin-bottom: 40px; }
        .logo { font-size: 3em; margin-bottom: 10px; }
        .subtitle { font-size: 1.2em; opacity: 0.9; }
        .card { background: rgba(255,255,255,0.1); border-radius: 15px; padding: 30px; margin: 20px 0; backdrop-filter: blur(10px); }
        .button { display: inline-block; background: #e67e22; color: white; padding: 12px 24px; border-radius: 8px; text-decoration: none; margin: 10px; transition: transform 0.3s; }
        .button:hover { transform: translateY(-2px); }
        .status { color: #2ecc71; font-weight: bold; }
        .endpoints { text-align: left; margin-top: 20px; }
        .endpoint { background: rgba(0,0,0,0.2); padding: 10px; margin: 5px 0; border-radius: 5px; font-family: 'Courier New', monospace; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>🍳</div>
            <h1>Cookidoo MCP Server</h1>
            <p class='subtitle'>RESTful API pro správu receptů z Cookidoo platformy</p>
        </div>
        
        <div class='card'>
            <h2>🟢 <span class='status'>Server je spuštěn</span></h2>
            <p>Verze: 1.0.0 | Port: 5555</p>
            
            <a href='/swagger' class='button'>📚 API Dokumentace (Swagger)</a>
            <a href='/health' class='button'>💚 Health Check</a>
        </div>
        
        <div class='card'>
            <h3>🚀 Rychlý start</h3>
            <div class='endpoints'>
                <div class='endpoint'>GET /swagger - Interaktivní API dokumentace</div>
                <div class='endpoint'>GET /api/v1/recipes - Seznam receptů (vyžaduje Cookidoo JWT token)</div>
                <div class='endpoint'>GET /api/v1/collections - Seznam kolekcí (vyžaduje Cookidoo JWT token)</div>
                <div class='endpoint'>POST /api/v1/recipes - Vytvoření receptu (vyžaduje Cookidoo JWT token)</div>
            </div>
        </div>
        
        <div class='card'>
            <h3>🔑 Autentizace</h3>
            <p>Server vyžaduje Cookidoo JWT token z <code>_oauth2_proxy</code> cookie.</p>
            <p><strong>Jak získat token:</strong> Přihlaste se do Cookidoo → F12 → Application → Cookies → zkopírujte hodnotu <code>_oauth2_proxy</code></p>
        </div>
        
        <div class='card'>
            <h3>🖥️ Použití v Cursoru</h3>
            <p>Viz <strong>MCP_GUIDE.md</strong> pro kompletní návod na integraci s Cursorem pomocí Model Context Protocol.</p>
        </div>
    </div>
</body>
</html>
", "text/html"));

// API info endpoint
app.MapGet("/api", () => new { 
    message = "Cookidoo MCP Server API je spuštěn", 
    version = "1.0.0", 
    swagger = "/swagger",
    health = "/health",
    authentication = "Vyžaduje Cookidoo JWT token z _oauth2_proxy cookie",
    endpoints = new {
        recipes = "/api/v1/recipes", 
        collections = "/api/v1/collections"
    }
});

// Graceful shutdown
app.Lifetime.ApplicationStopping.Register(() =>
{
    Log.Information("Cookidoo MCP Server se vypíná...");
});

try
{
    Log.Information("Spouštím Cookidoo MCP Server...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Cookidoo MCP Server se nepodařilo spustit");
}
finally
{
    Log.CloseAndFlush();
} 