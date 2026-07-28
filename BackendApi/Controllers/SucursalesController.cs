using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SucursalesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SucursalesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerSucursales()
    {
        var sucursales = await _context.Sucursales.ToListAsync();
        return Ok(sucursales);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerSucursalPorId(int id)
    {
        var sucursal = await _context.Sucursales.FindAsync(id);

        if (sucursal == null)
            return NotFound(new { mensaje = $"Sucursal con id {id} no encontrada" });

        return Ok(sucursal);
    }

    [HttpPost]
    public async Task<IActionResult> CrearSucursal([FromBody] Sucursal nuevaSucursal)
    {
        if (nuevaSucursal == null)
            return BadRequest(new { mensaje = "Los datos de la sucursal son requeridos" });

        _context.Sucursales.Add(nuevaSucursal);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerSucursalPorId), new { id = nuevaSucursal.Id }, nuevaSucursal);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarSucursal(int id, [FromBody] Sucursal sucursalActualizada)
    {
        var sucursal = await _context.Sucursales.FindAsync(id);

        if (sucursal == null)
            return NotFound(new { mensaje = $"Sucursal con id {id} no encontrada" });

        sucursal.Nombre = sucursalActualizada.Nombre;
        sucursal.Direccion = sucursalActualizada.Direccion;
        sucursal.CiudadId = sucursalActualizada.CiudadId;
        sucursal.Telefono = sucursalActualizada.Telefono;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Sucursal actualizada correctamente", sucursal });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarSucursal(int id)
    {
        var sucursal = await _context.Sucursales.FindAsync(id);

        if (sucursal == null)
            return NotFound(new { mensaje = $"Sucursal con id {id} no encontrada" });

        _context.Sucursales.Remove(sucursal);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Sucursal {id} eliminada correctamente" });
    }
}