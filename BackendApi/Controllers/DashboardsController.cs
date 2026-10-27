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

    /// <summary>
    /// Resumen del panel calculado en el servidor. Antes el panel se bajaba
    /// las tablas enteras de productos, inventario, ventas y pedidos y sacaba
    /// las cuentas en el navegador: con pocos datos funciona, pero crece con
    /// el tamaño de la base y manda al navegador informacion que no necesita.
    /// </summary>
    [HttpGet("resumen")]
    public async Task<IActionResult> ObtenerResumen([FromQuery] int? sucursalId = null)
    {
        var hoy = DateTime.Today;
        var unaSucursal = sucursalId.HasValue && sucursalId.Value > 0;

        var ventas = _context.Ventas.AsQueryable();

        if (unaSucursal)
            ventas = ventas.Where(v => v.SucursalId == sucursalId!.Value);

        var ventasDeHoy = await ventas
            .Where(v => v.Fecha >= hoy && v.Fecha < hoy.AddDays(1))
            .SumAsync(v => (decimal?)v.Total) ?? 0m;

        var cantidadProductos = await _context.Productos.CountAsync();

        var pedidos = _context.Pedidos.AsQueryable();

        // El pedido no tiene sucursal propia: se la toma de su venta.
        if (unaSucursal)
            pedidos = pedidos.Where(p => _context.Ventas
                .Any(v => v.Id == p.VentaId && v.SucursalId == sucursalId!.Value));

        var pedidosPendientes = await pedidos.CountAsync(p => p.Estado != "ENTREGADO");

        // Semana en curso, de lunes a domingo. El grafico decia "de la semana"
        // pero sumaba todas las ventas de la historia agrupadas por dia.
        var diasDesdeLunes = ((int)hoy.DayOfWeek + 6) % 7;
        var lunes = hoy.AddDays(-diasDesdeLunes);
        var finDeSemana = lunes.AddDays(7);

        var ventasSemana = await ventas
            .Where(v => v.Fecha >= lunes && v.Fecha < finDeSemana)
            .Select(v => new { v.Fecha, v.Total })
            .ToListAsync();

        var nombresDias = new[] { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };

        var barras = Enumerable.Range(0, 7).Select(i =>
        {
            var dia = lunes.AddDays(i);
            var total = ventasSemana
                .Where(v => v.Fecha.Date == dia)
                .Sum(v => v.Total);

            return new { dia = nombresDias[i], fecha = dia, total };
        }).ToList();

        // Mismo criterio de stock critico que usa la pantalla de Inventario:
        // esta en el limite o por debajo.
        var inventario = _context.Inventarios
            .Include(i => i.Producto)
            .Include(i => i.Sucursal)
            .AsQueryable();

        if (unaSucursal)
            inventario = inventario.Where(i => i.SucursalId == sucursalId!.Value);

        var alertas = await inventario
            .Where(i => i.StockMinimo != null && i.StockActual <= i.StockMinimo)
            .Select(i => new
            {
                i.Id,
                i.ProductoId,
                producto = i.Producto.Nombre,
                sucursal = i.Sucursal.Nombre,
                i.StockActual,
                i.StockMinimo
            })
            .ToListAsync();

        // Tiempo promedio de servicio de los ultimos 30 dias (HU-12). Solo
        // cuentan los pedidos entregados, que son los unicos con duracion.
        var desde = hoy.AddDays(-30);

        var entregados = await _context.Pedidos
            .Where(p => p.Estado == "ENTREGADO"
                     && p.FechaCreacion != null
                     && p.FechaEntregaReal != null
                     && p.FechaEntregaReal >= desde)
            .Select(p => new { p.FechaCreacion, p.FechaEntregaReal })
            .ToListAsync();

        double? promedioMinutos = entregados.Count == 0
            ? null
            : Math.Round(entregados.Average(
                p => (p.FechaEntregaReal!.Value - p.FechaCreacion!.Value).TotalMinutes), 1);

        var productos = await _context.Productos
            .OrderBy(p => p.Id)
            .Take(6)
            .Select(p => new { p.Id, p.Nombre, p.Precio })
            .ToListAsync();

        return Ok(new
        {
            sucursalId = unaSucursal ? sucursalId : null,
            ventasDeHoy,
            cantidadProductos,
            pedidosPendientes,
            stockCritico = alertas.Count,
            tiempoPromedioMinutos = promedioMinutos,
            pedidosMedidos = entregados.Count,
            semana = new { desde = lunes, hasta = finDeSemana.AddDays(-1), barras },
            alertas,
            productos
        });
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