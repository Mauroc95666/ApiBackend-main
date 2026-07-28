using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsuariosController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerUsuarios()
    {
        var usuarios = await _context.Usuarios.ToListAsync();
        return Ok(usuarios);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerUsuarioPorId(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound(new { mensaje = $"Usuario con id {id} no encontrado" });

        return Ok(usuario);
    }

    [HttpPost]
    public async Task<IActionResult> CrearUsuario([FromBody] Usuario nuevoUsuario)
    {
        if (nuevoUsuario == null)
            return BadRequest(new { mensaje = "Los datos del usuario son requeridos" });

        _context.Usuarios.Add(nuevoUsuario);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerUsuarioPorId), new { id = nuevoUsuario.Id }, nuevoUsuario);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarUsuario(int id, [FromBody] Usuario usuarioActualizado)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound(new { mensaje = $"Usuario con id {id} no encontrado" });

        usuario.Nombre = usuarioActualizado.Nombre;
        usuario.Email = usuarioActualizado.Email;
        usuario.PasswordHash = usuarioActualizado.PasswordHash;
        usuario.RolId = usuarioActualizado.RolId;
        usuario.Activo = usuarioActualizado.Activo;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Usuario actualizado correctamente", usuario });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound(new { mensaje = $"Usuario con id {id} no encontrado" });

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Usuario {id} eliminado correctamente" });
    }
}