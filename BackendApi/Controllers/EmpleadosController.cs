using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmpleadosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EmpleadosController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerEmpleados()
    {
        var empleados = await _context.Empleados.ToListAsync();
        return Ok(empleados);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerEmpleadoPorId(int id)
    {
        var empleado = await _context.Empleados.FindAsync(id);

        if (empleado == null)
            return NotFound(new { mensaje = $"Empleado con id {id} no encontrado" });

        return Ok(empleado);
    }

    [HttpPost]
    public async Task<IActionResult> CrearEmpleado([FromBody] Empleado nuevoEmpleado)
    {
        if (nuevoEmpleado == null)
            return BadRequest(new { mensaje = "Los datos del empleado son requeridos" });

        _context.Empleados.Add(nuevoEmpleado);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerEmpleadoPorId), new { id = nuevoEmpleado.Id }, nuevoEmpleado);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarEmpleado(int id, [FromBody] Empleado empleadoActualizado)
    {
        var empleado = await _context.Empleados.FindAsync(id);

        if (empleado == null)
            return NotFound(new { mensaje = $"Empleado con id {id} no encontrado" });

        empleado.UsuarioId = empleadoActualizado.UsuarioId;
        empleado.SucursalId = empleadoActualizado.SucursalId;
        empleado.Cargo = empleadoActualizado.Cargo;
        empleado.Salario = empleadoActualizado.Salario;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Empleado actualizado correctamente", empleado });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarEmpleado(int id)
    {
        var empleado = await _context.Empleados.FindAsync(id);

        if (empleado == null)
            return NotFound(new { mensaje = $"Empleado con id {id} no encontrado" });

        _context.Empleados.Remove(empleado);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Empleado {id} eliminado correctamente" });
    }
}