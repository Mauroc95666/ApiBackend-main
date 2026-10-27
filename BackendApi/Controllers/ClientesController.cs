using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using BackendApi.Models;
using BackendApi.Servicios;

namespace BackendApi.Controllers;

public class RegistroClienteRequest
{
    public string? Nombre { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Direccion { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClientesController(ApplicationDbContext context)
    {
        _context = context;
    }

    private static string NormalizarNombre(string nombre)
    {
        var cultura = System.Globalization.CultureInfo.GetCultureInfo("es-BO");
        return cultura.TextInfo.ToTitleCase(nombre.Trim().ToLower(cultura));
    }

    private static bool NombreValido(string nombre) =>
        Regex.IsMatch(nombre, @"^[\p{L}]+(?: [\p{L}]+)*$");

    [HttpGet]
    public async Task<IActionResult> ObtenerClientes()
    {
        var clientes = await _context.Clientes.ToListAsync();
        return Ok(clientes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerClientePorId(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);

        if (cliente == null)
            return NotFound(new { mensaje = $"Cliente con id {id} no encontrado" });

        return Ok(cliente);
    }

    /// <summary>
    /// Controles comunes al alta y a la edicion. Devuelve el mensaje de error
    /// o null si esta todo bien.
    /// </summary>
    private async Task<string?> Validar(Cliente cliente, int? idQueSeEdita)
    {
        if (string.IsNullOrWhiteSpace(cliente.Nombre))
            return "El nombre del cliente es requerido";

        if (!NombreValido(cliente.Nombre.Trim()))
            return "El nombre solo puede contener letras y espacios";

        var ci = (cliente.CI_NIT ?? string.Empty).Trim();

        if (ci.Length > 0)
        {
            // Entre 5 y 15 digitos, con un complemento opcional de hasta dos
            // letras al final, que es como se escriben los CI en Bolivia.
            if (!Regex.IsMatch(ci, @"^[0-9]{5,15}([ -]?[A-Za-z]{1,2})?$"))
                return $"'{ci}' no tiene formato de CI o NIT";

            var ciRepetido = await _context.Clientes.AnyAsync(c =>
                c.Id != (idQueSeEdita ?? 0) && c.CI_NIT == ci);

            if (ciRepetido)
                return $"Ya hay un cliente con el CI o NIT {ci}";
        }

        var email = (cliente.Email ?? string.Empty).Trim();

        if (email.Length > 0)
        {
            if (!Regex.IsMatch(email, @"^[^@ ]+@[^@ ]+[.][^@ ]+$"))
                return $"'{email}' no tiene formato de correo";

            var emailRepetido = await _context.Clientes.AnyAsync(c =>
                c.Id != (idQueSeEdita ?? 0) && c.Email != null && c.Email.ToLower() == email.ToLower());

            if (emailRepetido)
                return $"Ya hay un cliente con el correo {email}";
        }

        var telefono = (cliente.Telefono ?? string.Empty).Trim();

        if (!Regex.IsMatch(telefono, @"^[0-9]{6,15}$"))
            return "El teléfono es obligatorio y debe tener entre 6 y 15 dígitos";

        return null;
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> RegistrarCuentaCliente([FromBody] RegistroClienteRequest datos)
    {
        if (datos == null)
            return BadRequest(new { mensaje = "Los datos del cliente son requeridos" });

        var nombre = (datos.Nombre ?? string.Empty).Trim();
        var telefono = (datos.Telefono ?? string.Empty).Trim();
        var email = (datos.Email ?? string.Empty).Trim();
        var password = datos.Password ?? string.Empty;
        var direccion = (datos.Direccion ?? string.Empty).Trim();

        if (nombre.Length == 0 || !NombreValido(nombre))
            return BadRequest(new { mensaje = "El nombre solo puede contener letras y espacios" });
        if (!Regex.IsMatch(telefono, @"^[0-9]{6,15}$"))
            return BadRequest(new { mensaje = "El teléfono es obligatorio y debe tener entre 6 y 15 dígitos" });
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return BadRequest(new { mensaje = "Ingresá un correo válido" });
        if (password.Length < 8)
            return BadRequest(new { mensaje = "La contraseña debe tener al menos 8 caracteres" });
        if (direccion.Length == 0)
            return BadRequest(new { mensaje = "Seleccioná una dirección en el mapa" });

        var emailNormalizado = email.ToLowerInvariant();
        if (await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == emailNormalizado))
            return Conflict(new { mensaje = "Ya existe una cuenta con ese correo" });

        var rolCliente = await _context.Roles
            .Where(r => r.Nombre.ToLower().Contains("cliente"))
            .OrderBy(r => r.Id)
            .FirstOrDefaultAsync();
        if (rolCliente == null)
            return BadRequest(new { mensaje = "No está configurado el rol Cliente. Contactá al administrador." });

        var emprendimientoId = await _context.Emprendimientos
            .OrderBy(e => e.Id)
            .Select(e => (int?)e.Id)
            .FirstOrDefaultAsync();
        if (!emprendimientoId.HasValue)
            return BadRequest(new { mensaje = "No hay un emprendimiento configurado para registrar clientes." });

        var nombreNormalizado = NormalizarNombre(nombre);
        var correoNormalizado = email;

        await using var transaccion = await _context.Database.BeginTransactionAsync();
        try
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Email != null && c.Email.ToLower() == emailNormalizado);

            if (cliente == null)
            {
                cliente = new Cliente
                {
                    EmprendimientoId = emprendimientoId.Value,
                    Nombre = nombreNormalizado,
                    Telefono = telefono,
                    Email = correoNormalizado,
                    Direccion = direccion
                };
                _context.Clientes.Add(cliente);
            }
            else
            {
                cliente.Nombre = nombreNormalizado;
                cliente.Telefono = telefono;
                cliente.Direccion = direccion;
            }

            var usuario = new Usuario
            {
                EmprendimientoId = emprendimientoId.Value,
                RolId = rolCliente.Id,
                Nombre = nombreNormalizado,
                Email = correoNormalizado,
                PasswordHash = Passwords.Hashear(password),
                Activo = true
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();

            return CreatedAtAction(nameof(ObtenerClientePorId), new { id = cliente.Id }, new
            {
                cliente = new
                {
                    id = cliente.Id,
                    emprendimientoId = cliente.EmprendimientoId,
                    nombre = cliente.Nombre,
                    telefono = cliente.Telefono,
                    email = cliente.Email,
                    direccion = cliente.Direccion
                },
                usuario = new { id = usuario.Id, usuario.Nombre, usuario.Email, usuario.RolId }
            });
        }
        catch (Exception)
        {
            await transaccion.RollbackAsync();
            return StatusCode(500, new { mensaje = "No se pudo completar el registro del cliente." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CrearCliente([FromBody] Cliente nuevoCliente)
    {
        if (nuevoCliente == null)
            return BadRequest(new { mensaje = "Los datos del cliente son requeridos" });

        var problema = await Validar(nuevoCliente, null);

        if (problema != null)
            return BadRequest(new { mensaje = problema });

        nuevoCliente.Nombre = NormalizarNombre(nuevoCliente.Nombre);
        nuevoCliente.CI_NIT = string.IsNullOrWhiteSpace(nuevoCliente.CI_NIT) ? null : nuevoCliente.CI_NIT.Trim();
        nuevoCliente.Email = string.IsNullOrWhiteSpace(nuevoCliente.Email) ? null : nuevoCliente.Email.Trim();
        nuevoCliente.Telefono = nuevoCliente.Telefono!.Trim();

        _context.Clientes.Add(nuevoCliente);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerClientePorId), new { id = nuevoCliente.Id }, nuevoCliente);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarCliente(int id, [FromBody] Cliente clienteActualizado)
    {
        var cliente = await _context.Clientes.FindAsync(id);

        if (cliente == null)
            return NotFound(new { mensaje = $"Cliente con id {id} no encontrado" });

        var problema = await Validar(clienteActualizado, id);

        if (problema != null)
            return BadRequest(new { mensaje = problema });

        cliente.EmprendimientoId = clienteActualizado.EmprendimientoId;
        cliente.Nombre = NormalizarNombre(clienteActualizado.Nombre);
        cliente.CI_NIT = string.IsNullOrWhiteSpace(clienteActualizado.CI_NIT) ? null : clienteActualizado.CI_NIT.Trim();
        cliente.Telefono = clienteActualizado.Telefono!.Trim();
        cliente.Email = string.IsNullOrWhiteSpace(clienteActualizado.Email) ? null : clienteActualizado.Email.Trim();
        cliente.Direccion = clienteActualizado.Direccion!.Trim();

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Cliente actualizado correctamente", cliente });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarCliente(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);

        if (cliente == null)
            return NotFound(new { mensaje = $"Cliente con id {id} no encontrado" });

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Cliente {id} eliminado correctamente" });
    }
}