using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
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

    /// <summary>
    /// Controles comunes al alta y a la edicion: el NIT no se repite y el
    /// correo y el telefono tienen formato.
    /// </summary>
    private async Task<string?> Validar(Emprendimiento e, int? idQueSeEdita)
    {
        if (string.IsNullOrWhiteSpace(e.Nombre))
            return "El nombre del emprendimiento es requerido";

        var nit = (e.NIT_RUC ?? string.Empty).Trim();

        if (nit.Length > 0)
        {
            var repetido = await _context.Emprendimientos.AnyAsync(x =>
                x.Id != (idQueSeEdita ?? 0) && x.NIT_RUC == nit);

            if (repetido)
                return $"Ya hay un emprendimiento con el NIT o RUC {nit}";
        }

        var email = (e.Email ?? string.Empty).Trim();

        if (email.Length > 0 && !Regex.IsMatch(email, @"^[^@ ]+@[^@ ]+[.][^@ ]+$"))
            return $"'{email}' no tiene formato de correo";

        var telefono = (e.Telefono ?? string.Empty).Trim();

        if (telefono.Length > 0 && !Regex.IsMatch(telefono, @"^[0-9 ()+-]{6,20}$"))
            return $"'{telefono}' no tiene formato de teléfono";

        return null;
    }

    [HttpPost]
    public async Task<IActionResult> CrearEmprendimiento([FromBody] Emprendimiento nuevoEmprendimiento)
    {
        if (nuevoEmprendimiento == null)
            return BadRequest(new { mensaje = "Los datos del emprendimiento son requeridos" });

        var problema = await Validar(nuevoEmprendimiento, null);

        if (problema != null)
            return BadRequest(new { mensaje = problema });

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