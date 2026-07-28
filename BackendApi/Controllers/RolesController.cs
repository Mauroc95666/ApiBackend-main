using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RolesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerRoles()
    {
        var roles = await _context.Roles.ToListAsync();
        return Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerRolPorId(int id)
    {
        var rol = await _context.Roles.FindAsync(id);

        if (rol == null)
            return NotFound(new { mensaje = $"Rol con id {id} no encontrado" });

        return Ok(rol);
    }

    [HttpPost]
    public async Task<IActionResult> CrearRol([FromBody] Rol nuevoRol)
    {
        if (nuevoRol == null)
            return BadRequest(new { mensaje = "Los datos del rol son requeridos" });

        _context.Roles.Add(nuevoRol);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerRolPorId), new { id = nuevoRol.Id }, nuevoRol);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarRol(int id, [FromBody] Rol rolActualizado)
    {
        var rol = await _context.Roles.FindAsync(id);

        if (rol == null)
            return NotFound(new { mensaje = $"Rol con id {id} no encontrado" });

        rol.Nombre = rolActualizado.Nombre;
        rol.Descripcion = rolActualizado.Descripcion;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Rol actualizado correctamente", rol });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarRol(int id)
    {
        var rol = await _context.Roles.FindAsync(id);

        if (rol == null)
            return NotFound(new { mensaje = $"Rol con id {id} no encontrado" });

        _context.Roles.Remove(rol);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Rol {id} eliminado correctamente" });
    }

    [HttpGet("{id}/permisos")]
    public async Task<IActionResult> ObtenerPermisosDeRol(int id)
    {
        var rolExistente = await _context.Roles.FindAsync(id);

        if (rolExistente == null)
            return NotFound(new { mensaje = $"Rol con id {id} no encontrado" });

        var permisos = await _context.RolPermisos
            .Where(rp => rp.RolId == id)
            .Include(rp => rp.Permiso)
            .Select(rp => rp.Permiso)
            .ToListAsync();

        return Ok(permisos);
    }

    [HttpPost("{id}/permisos")]
    public async Task<IActionResult> AgregarPermisoARol(int id, [FromBody] RolPermiso rolPermiso)
    {
        var rolExistente = await _context.Roles.FindAsync(id);

        if (rolExistente == null)
            return NotFound(new { mensaje = $"Rol con id {id} no encontrado" });

        var permisoExistente = await _context.Permisos.FindAsync(rolPermiso.PermisoId);

        if (permisoExistente == null)
            return NotFound(new { mensaje = $"Permiso con id {rolPermiso.PermisoId} no encontrado" });

        var existe = await _context.RolPermisos
            .AnyAsync(rp => rp.RolId == id && rp.PermisoId == rolPermiso.PermisoId);

        if (existe)
            return BadRequest(new { mensaje = "El permiso ya está asignado a este rol" });

        rolPermiso.RolId = id;
        _context.RolPermisos.Add(rolPermiso);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPermisosDeRol), new { id }, rolPermiso);
    }

    [HttpDelete("{id}/permisos/{permisoId}")]
    public async Task<IActionResult> EliminarPermisoDeRol(int id, int permisoId)
    {
        var rolExistente = await _context.Roles.FindAsync(id);

        if (rolExistente == null)
            return NotFound(new { mensaje = $"Rol con id {id} no encontrado" });

        var rolPermiso = await _context.RolPermisos
            .FirstOrDefaultAsync(rp => rp.RolId == id && rp.PermisoId == permisoId);

        if (rolPermiso == null)
            return NotFound(new { mensaje = "La asignación del permiso al rol no existe" });

        _context.RolPermisos.Remove(rolPermiso);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Permiso eliminado del rol correctamente" });
    }
}