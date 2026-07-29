using BackendApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var corsPolicyName = "AllowFrontendVue";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicyName, policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:8080")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 1. Agregar servicios al contenedor
// SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true evita que
// .NET marque como obligatorias las propiedades de navegación (Cliente, Sucursal,
// Emprendimiento, etc.). Así el frontend puede crear registros mandando solo los
// ids (ClienteId, SucursalId...) sin que la API los rechace con error 400.
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

// 4. Configurar el pipeline de solicitudes HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); <-- COMPARA O COMENTA ESTA LÍNEA PARA EVITAR FORZAR HTTPS EN HTTP LOCAL

app.UseCors(corsPolicyName);
app.UseAuthorization();
app.MapControllers();

app.Run();
