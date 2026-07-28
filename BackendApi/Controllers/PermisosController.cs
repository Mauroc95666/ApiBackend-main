using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermisosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PermisosController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerPermisos()
    {
        var permisos = await _context.Permisos.ToListAsync();
        return Ok(permisos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPermisoPorId(int id)
    {
        var permiso = await _context.Permisos.FindAsync(id);

        if (permiso == null)
            return NotFound(new { mensaje = $"Permiso con id {id} no encontrado" });

        return Ok(permiso);
    }

    [HttpPost]
    public async Task<IActionResult> CrearPermiso([FromBody] Permiso nuevoPermiso)
    {
        if (nuevoPermiso == null)
            return BadRequest(new { mensaje = "Los datos del permiso son requeridos" });

        _context.Permisos.Add(nuevoPermiso);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPermisoPorId), new { id = nuevoPermiso.Id }, nuevoPermiso);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarPermiso(int id, [FromBody] Permiso permisoActualizado)
    {
        var permiso = await _context.Permisos.FindAsync(id);

        if (permiso == null)
            return NotFound(new { mensaje = $"Permiso con id {id} no encontrado" });

        permiso.Nombre = permisoActualizado.Nombre;
        permiso.Modulo = permisoActualizado.Modulo;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Permiso actualizado correctamente", permiso });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarPermiso(int id)
    {
        var permiso = await _context.Permisos.FindAsync(id);

        if (permiso == null)
            return NotFound(new { mensaje = $"Permiso con id {id} no encontrado" });

        _context.Permisos.Remove(permiso);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Permiso {id} eliminado correctamente" });
    }
}