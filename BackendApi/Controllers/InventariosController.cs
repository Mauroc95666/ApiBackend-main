using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

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

    [HttpGet]
    public async Task<IActionResult> ObtenerInventarios()
    {
        var inventarios = await _context.Inventarios.ToListAsync();
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

        inventario.ProductoId = inventarioActualizado.ProductoId;
        inventario.SucursalId = inventarioActualizado.SucursalId;
        inventario.StockActual = inventarioActualizado.StockActual;
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

        _context.Inventarios.Remove(inventario);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Inventario {id} eliminado correctamente" });
    }
}