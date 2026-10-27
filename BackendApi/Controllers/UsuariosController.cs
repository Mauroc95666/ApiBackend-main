using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using BackendApi.Models;
using BackendApi.Servicios;

namespace BackendApi.Controllers;

/// <summary>
/// Lo que la pantalla manda al crear o editar un usuario. Password viaja en
/// claro una sola vez, de la pantalla al servidor, y lo primero que hace el
/// servidor es hashearlo. Nunca se devuelve.
/// </summary>
public class UsuarioRequest
{
    public int EmprendimientoId { get; set; }
    public int RolId { get; set; }
    public string? Nombre { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    /// <summary>Alias por compatibilidad: la pantalla vieja mandaba este nombre.</summary>
    public string? PasswordHash { get; set; }
    public bool Activo { get; set; } = true;
}

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsuariosController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Forma en que se devuelve un usuario. No tiene campo de contraseña: el
    /// hash no sale de la base ni siquiera hasheado. Antes GET /api/usuarios
    /// devolvia la contraseña de todos.
    /// </summary>
    private static object ADto(Usuario u) => new
    {
        u.Id,
        u.EmprendimientoId,
        u.RolId,
        u.Nombre,
        u.Email,
        u.Activo
    };

    private static bool EmailValido(string email) =>
        Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    /// <summary>
    /// Controles comunes al alta y a la edicion. Devuelve el mensaje de error
    /// o null si esta todo bien.
    /// </summary>
    private async Task<string?> Validar(UsuarioRequest datos, int? idQueSeEdita)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            return "El nombre es requerido";

        var email = (datos.Email ?? string.Empty).Trim();

        if (email.Length == 0)
            return "El correo es requerido";

        if (!EmailValido(email))
            return $"'{email}' no tiene formato de correo";

        var repetido = await _context.Usuarios.AnyAsync(u =>
            u.Id != (idQueSeEdita ?? 0) && u.Email.ToLower() == email.ToLower());

        if (repetido)
            return $"Ya hay un usuario con el correo {email}";

        var rolExiste = await _context.Roles.AnyAsync(r => r.Id == datos.RolId);

        if (!rolExiste)
            return $"No existe el Rol con ID {datos.RolId}";

        var esRolCliente = await _context.Roles.AnyAsync(r =>
            r.Id == datos.RolId && r.Nombre.ToLower().Contains("cliente"));
        if (esRolCliente)
            return "El rol Cliente se asigna exclusivamente desde el registro de clientes";

        return null;
    }

    /// <summary>
    /// Un rol se considera administrativo por su nombre. Es el mismo criterio
    /// que usa el panel hoy; cuando los permisos salgan de RolPermiso habria
    /// que decidirlo por permisos y no por nombre.
    /// </summary>
    private static bool EsRolAdministrativo(string? nombreRol)
    {
        var n = (nombreRol ?? string.Empty).ToLowerInvariant();
        return n.Contains("admin") || n.Contains("gerente");
    }

    /// <summary>
    /// Usuarios administrables. Las cuentas internas del sistema quedan
    /// fuera: el super-usuario de arranque y el de la tienda no se muestran
    /// ni se administran desde esta pantalla.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerUsuarios()
    {
        var rolesCliente = await _context.Roles
            .Where(r => r.Nombre.ToLower().Contains("cliente"))
            .Select(r => r.Id)
            .ToListAsync();
        var usuarios = await _context.Usuarios
            .Where(u => !u.EsSistema && !rolesCliente.Contains(u.RolId))
            .ToListAsync();

        return Ok(usuarios.Select(ADto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerUsuarioPorId(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound(new { mensaje = $"Usuario con id {id} no encontrado" });

        return Ok(ADto(usuario));
    }

    [HttpPost]
    public async Task<IActionResult> CrearUsuario([FromBody] UsuarioRequest datos)
    {
        if (datos == null)
            return BadRequest(new { mensaje = "Los datos del usuario son requeridos" });

        var problema = await Validar(datos, null);

        if (problema != null)
            return BadRequest(new { mensaje = problema });

        var contrasena = datos.Password ?? datos.PasswordHash;

        if (string.IsNullOrWhiteSpace(contrasena))
            return BadRequest(new { mensaje = "La contraseña es requerida" });

        if (contrasena.Length < 6)
            return BadRequest(new { mensaje = "La contraseña tiene que tener al menos 6 caracteres" });

        var nuevoUsuario = new Usuario
        {
            EmprendimientoId = datos.EmprendimientoId,
            RolId = datos.RolId,
            Nombre = datos.Nombre!.Trim(),
            Email = datos.Email!.Trim(),
            // Se hashea antes de tocar la base: en la tabla nunca entra el texto.
            PasswordHash = Passwords.Hashear(contrasena),
            Activo = datos.Activo
        };

        _context.Usuarios.Add(nuevoUsuario);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerUsuarioPorId), new { id = nuevoUsuario.Id }, ADto(nuevoUsuario));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarUsuario(int id, [FromBody] UsuarioRequest datos)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound(new { mensaje = $"Usuario con id {id} no encontrado" });

        var problema = await Validar(datos, id);

        if (problema != null)
            return BadRequest(new { mensaje = problema });

        usuario.Nombre = datos.Nombre!.Trim();
        usuario.Email = datos.Email!.Trim();
        usuario.RolId = datos.RolId;
        usuario.Activo = datos.Activo;

        if (datos.EmprendimientoId > 0)
            usuario.EmprendimientoId = datos.EmprendimientoId;

        var contrasena = datos.Password ?? datos.PasswordHash;

        // El campo vacio significa "dejala como esta". Antes el PUT pisaba la
        // contraseña con lo que llegara, asi que editar el nombre de alguien
        // le borraba la clave.
        if (!string.IsNullOrWhiteSpace(contrasena))
        {
            if (contrasena.Length < 6)
                return BadRequest(new { mensaje = "La contraseña tiene que tener al menos 6 caracteres" });

            usuario.PasswordHash = Passwords.Hashear(contrasena);
        }

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Usuario actualizado correctamente", usuario = ADto(usuario) });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarUsuario(int id, [FromQuery] int? solicitanteId = null)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound(new { mensaje = $"Usuario con id {id} no encontrado" });

        if (usuario.EsSistema)
            return BadRequest(new
            {
                mensaje = "Es una cuenta del sistema y no se puede eliminar"
            });

        if (solicitanteId.HasValue && solicitanteId.Value == id)
            return BadRequest(new { mensaje = "No podés eliminar tu propio usuario" });

        var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Id == usuario.RolId);

        // No se puede dejar el sistema sin ningun administrador activo: si se
        // borra el ultimo, nadie puede volver a entrar al panel.
        if (EsRolAdministrativo(rol?.Nombre))
        {
            var rolesAdmin = await _context.Roles
                .Where(r => r.Nombre.ToLower().Contains("admin") || r.Nombre.ToLower().Contains("gerente"))
                .Select(r => r.Id)
                .ToListAsync();

            var administradoresActivos = await _context.Usuarios
                .CountAsync(u => u.Activo && rolesAdmin.Contains(u.RolId));

            if (administradoresActivos <= 1)
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar al último administrador activo del sistema"
                });
        }

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Usuario {id} eliminado correctamente" });
    }
}
