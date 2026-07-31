using BackendApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var corsPolicyName = "AllowFrontendVue";

// CORS abierto: permite que el frontend consuma la API.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicyName, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 1. Agregar servicios al contenedor
// SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true evita que
// .NET marque como obligatorias las propiedades de navegación, así el frontend
// puede crear registros mandando solo los ids sin recibir error 400.
builder.Services.AddControllers(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

// 2. Registrar el DbContext para la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionDB")));

// 3. Configurar Swagger para documentación de la API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Swagger siempre disponible (también en producción)
app.UseSwagger();
app.UseSwaggerUI();

// 5. Servir el frontend de Vue (archivos estáticos de wwwroot)
app.UseDefaultFiles();   // sirve index.html en la raíz "/"
app.UseStaticFiles();    // sirve los assets (js, css) de wwwroot

// app.UseHttpsRedirection(); <-- COMENTADA PARA EVITAR FORZAR HTTPS EN HTTP LOCAL

app.UseCors(corsPolicyName);
app.UseAuthorization();
app.MapControllers();

// 6. Cualquier ruta que no sea /api ni un archivo la maneja Vue (SPA)
app.MapFallbackToFile("index.html");

app.Run();
