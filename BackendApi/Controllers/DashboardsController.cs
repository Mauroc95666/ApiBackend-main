using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerDashboards()
    {
        var dashboards = await _context.Dashboards.ToListAsync();
        return Ok(dashboards);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerDashboardPorId(int id)
    {
        var dashboard = await _context.Dashboards.FindAsync(id);

        if (dashboard == null)
            return NotFound(new { mensaje = $"Dashboard con id {id} no encontrado" });

        return Ok(dashboard);
    }

    [HttpPost]
    public async Task<IActionResult> CrearDashboard([FromBody] Dashboard nuevoDashboard)
    {
        if (nuevoDashboard == null)
            return BadRequest(new { mensaje = "Los datos del dashboard son requeridos" });

        _context.Dashboards.Add(nuevoDashboard);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerDashboardPorId), new { id = nuevoDashboard.Id }, nuevoDashboard);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarDashboard(int id, [FromBody] Dashboard dashboardActualizado)
    {
        var dashboard = await _context.Dashboards.FindAsync(id);

        if (dashboard == null)
            return NotFound(new { mensaje = $"Dashboard con id {id} no encontrado" });

        dashboard.EmprendimientoId = dashboardActualizado.EmprendimientoId;
        dashboard.MetricasJson = dashboardActualizado.MetricasJson;
        dashboard.UltimaActualizacion = dashboardActualizado.UltimaActualizacion;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Dashboard actualizado correctamente", dashboard });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarDashboard(int id)
    {
        var dashboard = await _context.Dashboards.FindAsync(id);

        if (dashboard == null)
            return NotFound(new { mensaje = $"Dashboard con id {id} no encontrado" });

        _context.Dashboards.Remove(dashboard);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Dashboard {id} eliminado correctamente" });
    }
}