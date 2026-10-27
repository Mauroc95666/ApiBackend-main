using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

public class GuardarReporteRequest
{
    public int EmprendimientoId { get; set; }
    public string? Tipo { get; set; }
    public string? Datos { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReportesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Reporte de ventas por rango de fechas: el requisito RF-12. Devuelve el
    /// total del periodo, el detalle por dia y el ranking de productos, todo
    /// calculado en el servidor.
    /// </summary>
    [HttpGet("ventas")]
    public async Task<IActionResult> ReporteDeVentas(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] int? sucursalId)
    {
        // Por defecto, el mes en curso.
        var hoy = DateTime.Today;
        var inicio = (desde ?? new DateTime(hoy.Year, hoy.Month, 1)).Date;
        // El "hasta" se toma inclusivo: se compara contra el dia siguiente.
        var fin = (hasta ?? hoy).Date.AddDays(1);

        if (inicio >= fin)
            return BadRequest(new { mensaje = "La fecha desde tiene que ser anterior a la fecha hasta" });

        var consulta = _context.Ventas.Where(v => v.Fecha >= inicio && v.Fecha < fin);

        if (sucursalId.HasValue && sucursalId.Value > 0)
            consulta = consulta.Where(v => v.SucursalId == sucursalId.Value);

        var ventas = await consulta
            .Select(v => new { v.Id, v.Fecha, v.Total, v.SucursalId })
            .ToListAsync();

        var porDia = ventas
            .GroupBy(v => v.Fecha.Date)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                fecha = g.Key,
                cantidad = g.Count(),
                total = g.Sum(x => x.Total)
            })
            .ToList();

        var idsVentas = ventas.Select(v => v.Id).ToList();

        var porProducto = await _context.DetalleVenta
            .Where(d => idsVentas.Contains(d.VentaId))
            .Include(d => d.Producto)
            .GroupBy(d => new { d.ProductoId, d.Producto.Nombre })
            .Select(g => new
            {
                productoId = g.Key.ProductoId,
                producto = g.Key.Nombre,
                unidades = g.Sum(d => d.Cantidad),
                total = g.Sum(d => d.Cantidad * d.PrecioUnitario)
            })
            .OrderByDescending(x => x.total)
            .ToListAsync();

        var total = ventas.Sum(v => v.Total);

        return Ok(new
        {
            desde = inicio,
            hasta = fin.AddDays(-1),
            sucursalId,
            cantidadVentas = ventas.Count,
            total,
            promedioPorVenta = ventas.Count == 0 ? 0m : Math.Round(total / ventas.Count, 2),
            porDia,
            porProducto
        });
    }

    /// <summary>
    /// Guarda el reporte generado en la tabla Reporte, que existia sin que
    /// nadie la usara. Asi queda el historial de lo que se consulto y cuando.
    /// </summary>
    [HttpPost("ventas/guardar")]
    public async Task<IActionResult> GuardarReporteDeVentas([FromBody] GuardarReporteRequest datos)
    {
        if (datos == null || string.IsNullOrWhiteSpace(datos.Datos))
            return BadRequest(new { mensaje = "No hay datos para guardar" });

        var reporte = new Reporte
        {
            EmprendimientoId = datos.EmprendimientoId > 0 ? datos.EmprendimientoId : 1,
            Tipo = string.IsNullOrWhiteSpace(datos.Tipo) ? "VENTAS" : datos.Tipo.Trim(),
            FechaGeneracion = DateTime.Now,
            Datos = datos.Datos
        };

        _context.Reportes.Add(reporte);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Reporte guardado", id = reporte.Id, fecha = reporte.FechaGeneracion });
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