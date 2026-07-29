using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CiudadesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CiudadesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerCiudades()
    {
        var ciudades = await _context.Ciudades.ToListAsync();
        return Ok(ciudades);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerCiudadPorId(int id)
    {
        var ciudad = await _context.Ciudades.FindAsync(id);

        if (ciudad == null)
            return NotFound(new { mensaje = $"Ciudad con id {id} no encontrada" });

        return Ok(ciudad);
    }

    [HttpPost]
    public async Task<IActionResult> CrearCiudad([FromBody] Ciudad nuevaCiudad)
    {
        if (nuevaCiudad == null)
            return BadRequest(new { mensaje = "Los datos de la ciudad son requeridos" });

        _context.Ciudades.Add(nuevaCiudad);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerCiudadPorId), new { id = nuevaCiudad.Id }, nuevaCiudad);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarCiudad(int id, [FromBody] Ciudad ciudadActualizada)
    {
        var ciudad = await _context.Ciudades.FindAsync(id);

        if (ciudad == null)
            return NotFound(new { mensaje = $"Ciudad con id {id} no encontrada" });

        ciudad.Nombre = ciudadActualizada.Nombre;
        ciudad.Departamento = ciudadActualizada.Departamento;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Ciudad actualizada correctamente", ciudad });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarCiudad(int id)
    {
        var ciudad = await _context.Ciudades.FindAsync(id);

        if (ciudad == null)
            return NotFound(new { mensaje = $"Ciudad con id {id} no encontrada" });

        _context.Ciudades.Remove(ciudad);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Ciudad {id} eliminada correctamente" });
    }
}