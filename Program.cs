using Microsoft.EntityFrameworkCore;
using SICOP.Contexts;
using SICOP.Services;

var builder = WebApplication.CreateBuilder(args);

// Registra los controladores de nuestra Web API.
builder.Services.AddControllers();

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

// Habilita la autorización.
app.UseAuthorization();

// Habilita las rutas de nuestros controladores.
app.MapControllers();

// Inicia la aplicación.
app.Run();
