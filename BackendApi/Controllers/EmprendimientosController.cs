using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmprendimientosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EmprendimientosController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerEmprendimientos()
    {
        var emprendimientos = await _context.Emprendimientos.ToListAsync();
        return Ok(emprendimientos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerEmprendimientoPorId(int id)
    {
        var emprendimiento = await _context.Emprendimientos.FindAsync(id);

        if (emprendimiento == null)
            return NotFound(new { mensaje = $"Emprendimiento con id {id} no encontrado" });

        return Ok(emprendimiento);
    }

    [HttpPost]
    public async Task<IActionResult> CrearEmprendimiento([FromBody] Emprendimiento nuevoEmprendimiento)
    {
        if (nuevoEmprendimiento == null)
            return BadRequest(new { mensaje = "Los datos del emprendimiento son requeridos" });

        _context.Emprendimientos.Add(nuevoEmprendimiento);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerEmprendimientoPorId), new { id = nuevoEmprendimiento.Id }, nuevoEmprendimiento);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarEmprendimiento(int id, [FromBody] Emprendimiento emprendimientoActualizado)
    {
        var emprendimiento = await _context.Emprendimientos.FindAsync(id);

        if (emprendimiento == null)
            return NotFound(new { mensaje = $"Emprendimiento con id {id} no encontrado" });

        emprendimiento.Nombre = emprendimientoActualizado.Nombre;
        emprendimiento.NIT_RUC = emprendimientoActualizado.NIT_RUC;
        emprendimiento.Direccion = emprendimientoActualizado.Direccion;
        emprendimiento.Telefono = emprendimientoActualizado.Telefono;
        emprendimiento.Email = emprendimientoActualizado.Email;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Emprendimiento actualizado correctamente", emprendimiento });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarEmprendimiento(int id)
    {
        var emprendimiento = await _context.Emprendimientos.FindAsync(id);

        if (emprendimiento == null)
            return NotFound(new { mensaje = $"Emprendimiento con id {id} no encontrado" });

        _context.Emprendimientos.Remove(emprendimiento);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Emprendimiento {id} eliminado correctamente" });
    }
}