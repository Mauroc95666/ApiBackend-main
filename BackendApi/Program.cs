using BackendApi.Models;
using BackendApi.Middleware;
using BackendApi.Servicios;
using BackendApi.Services;
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

// Emisor y verificador de los tokens de sesion.
builder.Services.AddSingleton<Tokens>();

// Respaldo y restauracion de la base, con la tarea que lo corre solo.
builder.Services.AddSingleton<Respaldos>();
builder.Services.AddHostedService<RespaldoProgramado>();

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

// Control de acceso del lado del servidor: va antes de los controllers para
// que ninguna ruta de /api responda sin pasar por el.
app.UseMiddleware<ControlDeAcceso>();

app.UseAuthorization();
app.MapControllers();

// 6. Cualquier ruta que no sea /api ni un archivo la maneja Vue (SPA)
app.MapFallbackToFile("index.html");

// Clave del super-usuario. Se aplica al arrancar, no se escribe en el
// repositorio. Si la variable de entorno esta vacia y el usuario todavia no
// tiene una clave utilizable, se genera una al azar y se deja en el log para
// que el administrador la lea una sola vez.
using (var alcance = app.Services.CreateScope())
{
    var db = alcance.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var registro = alcance.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        await RolEmpleadoInicializador.AsegurarAsync(db);

        var superusuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == 1);

        if (superusuario != null)
        {
            var claveConfigurada = builder.Configuration["Seguridad:ClaveSuperadmin"];

            if (!string.IsNullOrWhiteSpace(claveConfigurada))
            {
                superusuario.PasswordHash = Passwords.Hashear(claveConfigurada);
                superusuario.DebeCambiarPassword = false;
                await db.SaveChangesAsync();
                registro.LogInformation("Clave del super-usuario tomada de la configuracion.");
            }
            else if (!Passwords.EstaHasheada(superusuario.PasswordHash))
            {
                // Base recien creada y sin variable de entorno: se genera una
                // clave al azar para que la cuenta no quede sin dueño.
                var generada = Guid.NewGuid().ToString("N")[..12];
                superusuario.PasswordHash = Passwords.Hashear(generada);
                superusuario.DebeCambiarPassword = true;
                await db.SaveChangesAsync();

                registro.LogWarning(
                    "Se genero la clave inicial del super-usuario: {Clave}. Cambiala en el primer ingreso.",
                    generada);
            }
        }
    }
    catch (Exception ex)
    {
        // Que esto falle no puede impedir que el sistema levante.
        registro.LogError(ex, "No se pudo inicializar el rol Empleado o revisar la clave del super-usuario.");
    }
}

app.Run();
