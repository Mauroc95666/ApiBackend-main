using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;
using BackendApi.Services;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventariosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InventariosController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Inventario, creando las filas faltantes para todos los productos y
    /// sucursales antes de aplicar el filtro opcional.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerInventarios([FromQuery] int? sucursalId = null)
    {
        await InventarioInicializador.AsegurarFilasAsync(_context);

        var consulta = _context.Inventarios.AsQueryable();

        if (sucursalId.HasValue && sucursalId.Value > 0)
            consulta = consulta.Where(i => i.SucursalId == sucursalId.Value);

        var inventarios = await consulta.ToListAsync();
        return Ok(inventarios);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerInventarioPorId(int id)
    {
        var inventario = await _context.Inventarios.FindAsync(id);

        if (inventario == null)
            return NotFound(new { mensaje = $"Inventario con id {id} no encontrado" });

        return Ok(inventario);
    }

    [HttpPost]
    public async Task<IActionResult> CrearInventario([FromBody] Inventario nuevoInventario)
    {
        if (nuevoInventario == null)
            return BadRequest(new { mensaje = "Los datos del inventario son requeridos" });

        var productoExiste = await _context.Productos.AnyAsync(p => p.Id == nuevoInventario.ProductoId);

        if (!productoExiste)
            return BadRequest(new { mensaje = $"No existe el Producto con ID {nuevoInventario.ProductoId}" });

        // Un producto no puede estar dos veces en la misma sucursal: serian
        // dos saldos para la misma cosa.
        var duplicado = await _context.Inventarios.AnyAsync(i =>
            i.ProductoId == nuevoInventario.ProductoId &&
            i.SucursalId == nuevoInventario.SucursalId);

        if (duplicado)
            return BadRequest(new { mensaje = "Ese producto ya está cargado en esa sucursal" });

        nuevoInventario.StockActual = 0;
        _context.Inventarios.Add(nuevoInventario);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerInventarioPorId), new { id = nuevoInventario.Id }, nuevoInventario);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarInventario(int id, [FromBody] Inventario inventarioActualizado)
    {
        var inventario = await _context.Inventarios.FindAsync(id);

        if (inventario == null)
            return NotFound(new { mensaje = $"Inventario con id {id} no encontrado" });

        if (inventarioActualizado.StockMinimo < 0)
            return BadRequest(new { mensaje = "El stock mínimo no puede ser negativo" });

        inventario.StockMinimo = inventarioActualizado.StockMinimo;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Inventario actualizado correctamente", inventario });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarInventario(int id)
    {
        var inventario = await _context.Inventarios.FindAsync(id);

        if (inventario == null)
            return NotFound(new { mensaje = $"Inventario con id {id} no encontrado" });

        if (inventario.StockActual > 0)
            return BadRequest(new { mensaje = "Registrá una salida antes de quitar un ítem con stock." });

        _context.Inventarios.Remove(inventario);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Inventario {id} eliminado correctamente" });
    }
}