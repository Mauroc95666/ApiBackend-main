using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResenasController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ResenasController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerResenas()
    {
        var resenas = await _context.Resenas.ToListAsync();
        return Ok(resenas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerResenaPorId(int id)
    {
        var resena = await _context.Resenas.FindAsync(id);

        if (resena == null)
            return NotFound(new { mensaje = $"Reseña con id {id} no encontrada" });

        return Ok(resena);
    }

    [HttpPost]
    public async Task<IActionResult> CrearResena([FromBody] Resena nuevaResena)
    {
        if (nuevaResena == null)
            return BadRequest(new { mensaje = "Los datos de la reseña son requeridos" });

        _context.Resenas.Add(nuevaResena);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerResenaPorId), new { id = nuevaResena.Id }, nuevaResena);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarResena(int id, [FromBody] Resena resenaActualizada)
    {
        var resena = await _context.Resenas.FindAsync(id);

        if (resena == null)
            return NotFound(new { mensaje = $"Reseña con id {id} no encontrada" });

        resena.ClienteId = resenaActualizada.ClienteId;
        resena.ProductoId = resenaActualizada.ProductoId;
        resena.Calificacion = resenaActualizada.Calificacion;
        resena.Comentario = resenaActualizada.Comentario;
        resena.Fecha = resenaActualizada.Fecha;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Reseña actualizada correctamente", resena });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarResena(int id)
    {
        var resena = await _context.Resenas.FindAsync(id);

        if (resena == null)
            return NotFound(new { mensaje = $"Reseña con id {id} no encontrada" });

        _context.Resenas.Remove(resena);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Reseña {id} eliminada correctamente" });
    }
}