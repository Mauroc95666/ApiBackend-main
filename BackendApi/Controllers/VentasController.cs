using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public VentasController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerVentas()
    {
        var ventas = await _context.Ventas.ToListAsync();
        return Ok(ventas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerVentaPorId(int id)
    {
        var venta = await _context.Ventas.FindAsync(id);

        if (venta == null)
            return NotFound(new { mensaje = $"Venta con id {id} no encontrada" });

        return Ok(venta);
    }

    [HttpPost]
    public async Task<IActionResult> CrearVenta([FromBody] Venta nuevaVenta)
    {
        if (nuevaVenta == null)
            return BadRequest(new { mensaje = "Los datos de la venta son requeridos" });

        _context.Ventas.Add(nuevaVenta);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerVentaPorId), new { id = nuevaVenta.Id }, nuevaVenta);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarVenta(int id, [FromBody] Venta ventaActualizada)
    {
        var venta = await _context.Ventas.FindAsync(id);

        if (venta == null)
            return NotFound(new { mensaje = $"Venta con id {id} no encontrada" });

        venta.SucursalId = ventaActualizada.SucursalId;
        venta.ClienteId = ventaActualizada.ClienteId;
        venta.UsuarioId = ventaActualizada.UsuarioId;
        venta.MetodoPagoId = ventaActualizada.MetodoPagoId;
        venta.Fecha = ventaActualizada.Fecha;
        venta.Total = ventaActualizada.Total;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Venta actualizada correctamente", venta });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarVenta(int id)
    {
        var venta = await _context.Ventas.FindAsync(id);

        if (venta == null)
            return NotFound(new { mensaje = $"Venta con id {id} no encontrada" });

        _context.Ventas.Remove(venta);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Venta {id} eliminada correctamente" });
    }
}