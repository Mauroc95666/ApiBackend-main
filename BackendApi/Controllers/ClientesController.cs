using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClientesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerClientes()
    {
        var clientes = await _context.Clientes.ToListAsync();
        return Ok(clientes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerClientePorId(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);

        if (cliente == null)
            return NotFound(new { mensaje = $"Cliente con id {id} no encontrado" });

        return Ok(cliente);
    }

    [HttpPost]
    public async Task<IActionResult> CrearCliente([FromBody] Cliente nuevoCliente)
    {
        if (nuevoCliente == null)
            return BadRequest(new { mensaje = "Los datos del cliente son requeridos" });

        _context.Clientes.Add(nuevoCliente);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerClientePorId), new { id = nuevoCliente.Id }, nuevoCliente);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarCliente(int id, [FromBody] Cliente clienteActualizado)
    {
        var cliente = await _context.Clientes.FindAsync(id);

        if (cliente == null)
            return NotFound(new { mensaje = $"Cliente con id {id} no encontrado" });

        cliente.EmprendimientoId = clienteActualizado.EmprendimientoId;
        cliente.Nombre = clienteActualizado.Nombre;
        cliente.CI_NIT = clienteActualizado.CI_NIT;
        cliente.Telefono = clienteActualizado.Telefono;
        cliente.Email = clienteActualizado.Email;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Cliente actualizado correctamente", cliente });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarCliente(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);

        if (cliente == null)
            return NotFound(new { mensaje = $"Cliente con id {id} no encontrado" });

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Cliente {id} eliminado correctamente" });
    }
}