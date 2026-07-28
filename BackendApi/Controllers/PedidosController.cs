using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PedidosController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerPedidos()
    {
        var pedidos = await _context.Pedidos.ToListAsync();
        return Ok(pedidos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPedidoPorId(int id)
    {
        var pedido = await _context.Pedidos.FindAsync(id);

        if (pedido == null)
            return NotFound(new { mensaje = $"Pedido con id {id} no encontrado" });

        return Ok(pedido);
    }

    [HttpPost]
    public async Task<IActionResult> CrearPedido([FromBody] Pedido nuevoPedido)
    {
        if (nuevoPedido == null)
            return BadRequest(new { mensaje = "Los datos del pedido son requeridos" });

        if (nuevoPedido.VentaId <= 0)
            return BadRequest(new { mensaje = "El VentaId es obligatorio" });

        _context.Pedidos.Add(nuevoPedido);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPedidoPorId), new { id = nuevoPedido.Id }, nuevoPedido);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarPedido(int id, [FromBody] Pedido pedidoActualizado)
    {
        var pedido = await _context.Pedidos.FindAsync(id);

        if (pedido == null)
            return NotFound(new { mensaje = $"Pedido con id {id} no encontrado" });

        // Propiedades reales de la tabla Pedido
        pedido.VentaId = pedidoActualizado.VentaId > 0 ? pedidoActualizado.VentaId : pedido.VentaId;
        pedido.Estado = pedidoActualizado.Estado ?? pedido.Estado;
        pedido.FechaEntrega = pedidoActualizado.FechaEntrega ?? pedido.FechaEntrega;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Pedido actualizado correctamente", pedido });
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarPedido(int id)
    {
        var pedido = await _context.Pedidos.FindAsync(id);

        if (pedido == null)
            return NotFound(new { mensaje = $"Pedido con id {id} no encontrado" });

        _context.Pedidos.Remove(pedido);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Pedido {id} eliminado correctamente" });
    }
}