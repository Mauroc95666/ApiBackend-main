using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovimientosInventarioController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MovimientosInventarioController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerMovimientos()
    {
        var movimientos = await _context.MovimientoInventario.ToListAsync();
        return Ok(movimientos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerMovimientoPorId(int id)
    {
        var movimiento = await _context.MovimientoInventario.FindAsync(id);

        if (movimiento == null)
            return NotFound(new { mensaje = $"Movimiento con id {id} no encontrado" });

        return Ok(movimiento);
    }

    [HttpPost]
    public async Task<IActionResult> CrearMovimiento([FromBody] MovimientoInventario nuevoMovimiento)
    {
        if (nuevoMovimiento == null)
            return BadRequest(new { mensaje = "Los datos del movimiento son requeridos" });

        _context.MovimientoInventario.Add(nuevoMovimiento);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerMovimientoPorId), new { id = nuevoMovimiento.Id }, nuevoMovimiento);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarMovimiento(int id, [FromBody] MovimientoInventario movimientoActualizado)
    {
        var movimiento = await _context.MovimientoInventario.FindAsync(id);

        if (movimiento == null)
            return NotFound(new { mensaje = $"Movimiento con id {id} no encontrado" });

        movimiento.InventarioId = movimientoActualizado.InventarioId;
        movimiento.UsuarioId = movimientoActualizado.UsuarioId;
        movimiento.Tipo = movimientoActualizado.Tipo;
        movimiento.Cantidad = movimientoActualizado.Cantidad;
        movimiento.Fecha = movimientoActualizado.Fecha;
        movimiento.Motivo = movimientoActualizado.Motivo;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Movimiento actualizado correctamente", movimiento });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarMovimiento(int id)
    {
        var movimiento = await _context.MovimientoInventario.FindAsync(id);

        if (movimiento == null)
            return NotFound(new { mensaje = $"Movimiento con id {id} no encontrado" });

        _context.MovimientoInventario.Remove(movimiento);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Movimiento {id} eliminado correctamente" });
    }
}