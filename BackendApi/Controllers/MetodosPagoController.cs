using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetodosPagoController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MetodosPagoController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerMetodosPago()
    {
        var metodosPago = await _context.MetodosPago.ToListAsync();
        return Ok(metodosPago);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerMetodoPagoPorId(int id)
    {
        var metodoPago = await _context.MetodosPago.FindAsync(id);

        if (metodoPago == null)
            return NotFound(new { mensaje = $"Método de pago con id {id} no encontrado" });

        return Ok(metodoPago);
    }

    [HttpPost]
    public async Task<IActionResult> CrearMetodoPago([FromBody] MetodoPago nuevoMetodoPago)
    {
        if (nuevoMetodoPago == null)
            return BadRequest(new { mensaje = "Los datos del método de pago son requeridos" });

        _context.MetodosPago.Add(nuevoMetodoPago);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerMetodoPagoPorId), new { id = nuevoMetodoPago.Id }, nuevoMetodoPago);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarMetodoPago(int id, [FromBody] MetodoPago metodoPagoActualizado)
    {
        var metodoPago = await _context.MetodosPago.FindAsync(id);

        if (metodoPago == null)
            return NotFound(new { mensaje = $"Método de pago con id {id} no encontrado" });

        metodoPago.Nombre = metodoPagoActualizado.Nombre;
        metodoPago.Descripcion = metodoPagoActualizado.Descripcion;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Método de pago actualizado correctamente", metodoPago });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarMetodoPago(int id)
    {
        var metodoPago = await _context.MetodosPago.FindAsync(id);

        if (metodoPago == null)
            return NotFound(new { mensaje = $"Método de pago con id {id} no encontrado" });

        _context.MetodosPago.Remove(metodoPago);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Método de pago {id} eliminado correctamente" });
    }
}