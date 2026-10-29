using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using SICOP.Contexts;
using SICOP.Services;

var builder = WebApplication.CreateBuilder(args);

// Registra los controladores de nuestra Web API.
builder.Services.AddControllers();

// Registra el servicio de facturas.
// Permitir la comunicación entre Vue y nuestra API .NET.
builder.Services.AddCors(options =>
{
    options.AddPolicy("SICOPFrontend", policy =>
    {
        policy.WithOrigins(
            // Frontend local.
            "http://localhost:5173",

            // Frontend local en modo Preview.
            "http://localhost:4173",

            // Frontend desplegado en OpenShift.
            "https://sicop-frontend-sicop-dev-dev.apps.rm1.0a51.p1.openshiftapps.com"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});


// Registra el servicio de facturas para utilizarlo
// mediante inyección de dependencias.
builder.Services.AddScoped<FacturaService>();

// Configura la documentación OpenAPI.
builder.Services.AddOpenApi();

// Configura la conexión a SQL Server.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Obtiene la configuración JWT desde appsettings.json.
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// Obtiene la clave secreta desde User Secrets.
var jwtKey = builder.Configuration["Jwt:Key"];

// Verifica que la configuración JWT esté completa.
if (string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience) ||
    string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "La configuración JWT está incompleta."
    );
}

// Configura la autenticación mediante tokens JWT.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Establece las reglas para validar los tokens.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Verifica quién emitió el token.
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            // Verifica para quién fue emitido.
            ValidateAudience = true,
            ValidAudience = jwtAudience,

            // Verifica que el token no esté vencido.
            ValidateLifetime = true,

            // Verifica la firma del token.
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),

            // No permite minutos adicionales después del vencimiento.
            ClockSkew = TimeSpan.Zero
        };
    });

// Registra los servicios de autorización.
builder.Services.AddAuthorization();

var app = builder.Build();

// Habilita OpenAPI durante el desarrollo.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Redirige HTTP hacia HTTPS.
app.UseHttpsRedirection();

// Primero verifica la identidad del usuario.
app.UseAuthentication();

// Después verifica sus permisos.
// Habilitar CORS para nuestro Frontend Vue.
app.UseCors("SICOPFrontend");

// Habilita la autorización.
app.UseAuthorization();

// Habilita las rutas de los controladores.
app.MapControllers();

// Inicia la aplicación.
app.Run();