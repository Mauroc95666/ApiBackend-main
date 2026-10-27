using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromocionesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PromocionesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerPromociones()
    {
        var promociones = await _context.Promociones.ToListAsync();
        return Ok(promociones);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPromocionPorId(int id)
    {
        var promocion = await _context.Promociones.FindAsync(id);

        if (promocion == null)
            return NotFound(new { mensaje = $"Promoción con id {id} no encontrada" });

        return Ok(promocion);
    }

    [HttpPost]
    public async Task<IActionResult> CrearPromocion([FromBody] Promocion nuevaPromocion)
    {
        if (nuevaPromocion == null)
            return BadRequest(new { mensaje = "Los datos de la promoción son requeridos" });

        _context.Promociones.Add(nuevaPromocion);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPromocionPorId), new { id = nuevaPromocion.Id }, nuevaPromocion);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarPromocion(int id, [FromBody] Promocion promocionActualizada)
    {
        var promocion = await _context.Promociones.FindAsync(id);

        if (promocion == null)
            return NotFound(new { mensaje = $"Promoción con id {id} no encontrada" });

        promocion.EmprendimientoId = promocionActualizada.EmprendimientoId;
        promocion.Nombre = promocionActualizada.Nombre;
        promocion.Descuento = promocionActualizada.Descuento;
        promocion.FechaInicio = promocionActualizada.FechaInicio;
        promocion.FechaFin = promocionActualizada.FechaFin;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Promoción actualizada correctamente", promocion });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarPromocion(int id)
    {
        var promocion = await _context.Promociones.FindAsync(id);

        if (promocion == null)
            return NotFound(new { mensaje = $"Promoción con id {id} no encontrada" });

        _context.Promociones.Remove(promocion);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Promoción {id} eliminada correctamente" });
    }
}