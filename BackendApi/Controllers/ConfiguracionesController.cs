using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfiguracionesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ConfiguracionesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerConfiguraciones()
    {
        var configuraciones = await _context.Configuraciones.ToListAsync();
        return Ok(configuraciones);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerConfiguracionPorId(int id)
    {
        var configuracion = await _context.Configuraciones.FindAsync(id);

        if (configuracion == null)
            return NotFound(new { mensaje = $"Configuración con id {id} no encontrada" });

        return Ok(configuracion);
    }

    [HttpPost]
    public async Task<IActionResult> CrearConfiguracion([FromBody] Configuracion nuevaConfiguracion)
    {
        if (nuevaConfiguracion == null)
            return BadRequest(new { mensaje = "Los datos de la configuración son requeridos" });

        _context.Configuraciones.Add(nuevaConfiguracion);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerConfiguracionPorId), new { id = nuevaConfiguracion.Id }, nuevaConfiguracion);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarConfiguracion(int id, [FromBody] Configuracion configuracionActualizada)
    {
        var configuracion = await _context.Configuraciones.FindAsync(id);

        if (configuracion == null)
            return NotFound(new { mensaje = $"Configuración con id {id} no encontrada" });

        configuracion.EmprendimientoId = configuracionActualizada.EmprendimientoId;
        configuracion.Clave = configuracionActualizada.Clave;
        configuracion.Valor = configuracionActualizada.Valor;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Configuración actualizada correctamente", configuracion });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarConfiguracion(int id)
    {
        var configuracion = await _context.Configuraciones.FindAsync(id);

        if (configuracion == null)
            return NotFound(new { mensaje = $"Configuración con id {id} no encontrada" });

        _context.Configuraciones.Remove(configuracion);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Configuración {id} eliminada correctamente" });
    }
}