using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificacionesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NotificacionesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerNotificaciones()
    {
        var notificaciones = await _context.Notificaciones.ToListAsync();
        return Ok(notificaciones);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerNotificacionPorId(int id)
    {
        var notificacion = await _context.Notificaciones.FindAsync(id);

        if (notificacion == null)
            return NotFound(new { mensaje = $"Notificación con id {id} no encontrada" });

        return Ok(notificacion);
    }

    [HttpPost]
    public async Task<IActionResult> CrearNotificacion([FromBody] Notificacion nuevaNotificacion)
    {
        if (nuevaNotificacion == null)
            return BadRequest(new { mensaje = "Los datos de la notificación son requeridos" });

        _context.Notificaciones.Add(nuevaNotificacion);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerNotificacionPorId), new { id = nuevaNotificacion.Id }, nuevaNotificacion);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarNotificacion(int id, [FromBody] Notificacion notificacionActualizada)
    {
        var notificacion = await _context.Notificaciones.FindAsync(id);

        if (notificacion == null)
            return NotFound(new { mensaje = $"Notificación con id {id} no encontrada" });

        notificacion.UsuarioId = notificacionActualizada.UsuarioId;
        notificacion.Mensaje = notificacionActualizada.Mensaje;
        notificacion.Leido = notificacionActualizada.Leido;
        notificacion.Fecha = notificacionActualizada.Fecha;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Notificación actualizada correctamente", notificacion });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarNotificacion(int id)
    {
        var notificacion = await _context.Notificaciones.FindAsync(id);

        if (notificacion == null)
            return NotFound(new { mensaje = $"Notificación con id {id} no encontrada" });

        _context.Notificaciones.Remove(notificacion);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Notificación {id} eliminada correctamente" });
    }
}