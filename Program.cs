using Microsoft.EntityFrameworkCore;
using SICOP.Contexts;
using SICOP.Services;

var builder = WebApplication.CreateBuilder(args);

// Registra los controladores de nuestra Web API.
builder.Services.AddControllers();

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

// Registra el contexto de la base de datos.
// Obtiene la cadena de conexión desde appsettings.json.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

var app = builder.Build();

// Habilita OpenAPI en el entorno de desarrollo.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Redirige las solicitudes HTTP hacia HTTPS.
app.UseHttpsRedirection();

// Habilitar CORS para nuestro Frontend Vue.
app.UseCors("SICOPFrontend");

// Habilita la autorización.
app.UseAuthorization();

// Habilita las rutas de nuestros controladores.
app.MapControllers();

// Inicia la aplicación.
app.Run();
