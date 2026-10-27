using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;
using BackendApi.Servicios;

namespace BackendApi.Controllers;

public class LoginRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class CambioDeClaveRequest
{
    public string? Email { get; set; }
    public string? PasswordActual { get; set; }
    public string? PasswordNueva { get; set; }
}

/// <summary>
/// Ingreso al sistema. Antes no existia: la pantalla de login pedia
/// GET /api/usuarios y comparaba correo y contraseña en el navegador, asi que
/// el sistema entregaba todos los usuarios con sus contraseñas antes de que
/// nadie hubiera iniciado sesion. Ahora la verificacion ocurre aca.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly Tokens _tokens;

    public AuthController(ApplicationDbContext context, Tokens tokens)
    {
        _context = context;
        _tokens = tokens;
    }

    /// <summary>
    /// Id del usuario con el que la tienda publica registra sus ventas. La
    /// tienda lo necesita sin tener sesion, y antes lo sacaba de la lista
    /// completa de usuarios, que ya no es publica.
    /// </summary>
    [HttpGet("usuario-tienda")]
    public async Task<IActionResult> UsuarioDeLaTienda()
    {
        var usuario = await _context.Usuarios
            .Where(u => u.EsSistema && u.Email == "tienda@kiataque.bo")
            .Select(u => new { u.Id, u.Nombre })
            .FirstOrDefaultAsync();

        if (usuario == null)
            return NotFound(new { mensaje = "No está configurado el usuario de la tienda" });

        return Ok(usuario);
    }

    /// <summary>
    /// Cambio de contraseña. Pide la actual, asi tener la pantalla abierta no
    /// alcanza para cambiarsela a alguien.
    /// </summary>
    [HttpPost("cambiar-password")]
    public async Task<IActionResult> CambiarPassword([FromBody] CambioDeClaveRequest datos)
    {
        var email = (datos?.Email ?? string.Empty).Trim();
        var actual = datos?.PasswordActual ?? string.Empty;
        var nueva = datos?.PasswordNueva ?? string.Empty;

        if (email.Length == 0 || actual.Length == 0 || nueva.Length == 0)
            return BadRequest(new { mensaje = "Faltan datos para cambiar la contraseña" });

        if (nueva.Length < 8)
            return BadRequest(new { mensaje = "La contraseña nueva tiene que tener al menos 8 caracteres" });

        if (nueva == actual)
            return BadRequest(new { mensaje = "La contraseña nueva tiene que ser distinta de la actual" });

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (usuario == null || !Passwords.Verificar(actual, usuario.PasswordHash))
            return Unauthorized(new { mensaje = "La contraseña actual no es correcta" });

        usuario.PasswordHash = Passwords.Hashear(nueva);
        usuario.DebeCambiarPassword = false;
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Contraseña actualizada" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest datos)
    {
        var email = (datos?.Email ?? string.Empty).Trim();
        var password = datos?.Password ?? string.Empty;

        if (email.Length == 0 || password.Length == 0)
            return BadRequest(new { mensaje = "Completá el correo y la contraseña" });

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        // Mismo mensaje exista o no el correo: decir "ese correo no existe"
        // le confirma a cualquiera que pruebe cuales son los correos validos.
        if (usuario == null || !Passwords.Verificar(password, usuario.PasswordHash))
            return Unauthorized(new { mensaje = "El correo o la contraseña no son correctos" });

        if (!usuario.Activo)
            return Unauthorized(new { mensaje = "Este usuario está desactivado. Contactá al administrador." });

        // Si la contraseña todavia estaba en texto plano, este ingreso correcto
        // es el momento de reemplazarla por su hash. Asi las cuentas viejas se
        // migran solas, sin pedirle a nadie que la cambie.
        if (!Passwords.EstaHasheada(usuario.PasswordHash))
        {
            usuario.PasswordHash = Passwords.Hashear(password);
            await _context.SaveChangesAsync();
        }

        var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Id == usuario.RolId);

        // Los modulos que este rol tiene permitidos salen de RolPermiso, no
        // del nombre del rol. Antes el panel los deducia del nombre, asi que
        // renombrar un rol le cambiaba los permisos sin que nadie se entere.
        var permisos = await _context.RolPermisos
            .Where(rp => rp.RolId == usuario.RolId)
            .Include(rp => rp.Permiso)
            .Select(rp => rp.Permiso.Modulo)
            .ToListAsync();

        // La respuesta no incluye la contraseña en ninguna forma.
        return Ok(new
        {
            id = usuario.Id,
            nombre = usuario.Nombre,
            email = usuario.Email,
            rolId = usuario.RolId,
            rol = rol?.Nombre,
            activo = usuario.Activo,
            emprendimientoId = usuario.EmprendimientoId,
            usuario.DebeCambiarPassword,
            permisos,
            // El frontend manda este token en cada pedido. Va firmado, asi que
            // cambiarle el rol o el usuario lo invalida.
            token = _tokens.Emitir(usuario.Id, usuario.RolId, TimeSpan.FromHours(8))
        });
    }
}
