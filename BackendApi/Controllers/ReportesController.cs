using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReportesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerReportes()
    {
        var reportes = await _context.Reportes.ToListAsync();
        return Ok(reportes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerReportePorId(int id)
    {
        var reporte = await _context.Reportes.FindAsync(id);

        if (reporte == null)
            return NotFound(new { mensaje = $"Reporte con id {id} no encontrado" });

        return Ok(reporte);
    }

    [HttpPost]
    public async Task<IActionResult> CrearReporte([FromBody] Reporte nuevoReporte)
    {
        if (nuevoReporte == null)
            return BadRequest(new { mensaje = "Los datos del reporte son requeridos" });

        _context.Reportes.Add(nuevoReporte);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerReportePorId), new { id = nuevoReporte.Id }, nuevoReporte);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarReporte(int id, [FromBody] Reporte reporteActualizado)
    {
        var reporte = await _context.Reportes.FindAsync(id);

        if (reporte == null)
            return NotFound(new { mensaje = $"Reporte con id {id} no encontrado" });

        reporte.EmprendimientoId = reporteActualizado.EmprendimientoId;
        reporte.Tipo = reporteActualizado.Tipo;
        reporte.FechaGeneracion = reporteActualizado.FechaGeneracion;
        reporte.Datos = reporteActualizado.Datos;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Reporte actualizado correctamente", reporte });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarReporte(int id)
    {
        var reporte = await _context.Reportes.FindAsync(id);

        if (reporte == null)
            return NotFound(new { mensaje = $"Reporte con id {id} no encontrado" });

        _context.Reportes.Remove(reporte);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Reporte {id} eliminado correctamente" });
    }
}