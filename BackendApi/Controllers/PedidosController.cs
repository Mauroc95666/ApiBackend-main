using Microsoft.AspNetCore.Mvc; // Allows drivers, routes and HTTP responses to be used
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController] // Indica que la clase funcionará como un controlador de una API
[Route("api/[controller]")] // Define la ruta de la API -> api/Pedidos
public class PedidosController : ControllerBase // Proporciona funciones de respuesta (Ok(), NotFound(), BadRequest())
{
    // Base de datos simulada
    private static List<Pedido> pedidos = new()
    {
        new Pedido { Id = 1, Cliente = "Juan Perez", Producto = "Marraqueta", Cantidad = 10, Total = 5.00, Estado = "Preparando" },
        new Pedido { Id = 2, Cliente = "Maria Becerra", Producto = "Torta de chocolate", Cantidad = 1, Total = 12.00, Estado = "Listo para entrega" },
        new Pedido { Id = 3, Cliente = "Carlos Martinez", Producto = "Empanada de queso", Cantidad = 4, Total = 20.00, Estado = "Entregado" }
    };

    [HttpGet] 
    public IActionResult ObtenerPedidos()
    {
        return Ok(pedidos);
    }

    [HttpGet("{id}")] // Responde a GET api/Pedidos/{id}
    public IActionResult ObtenerPedidoPorId(int id)
    {
        var pedido = pedidos.FirstOrDefault(p => p.Id == id);
        
        if (pedido == null)
            return NotFound(new { mensaje = $"Pedido con id {id} no encontrado" });

        return Ok(pedido);
    }

    [HttpPost] // Responde a POST api/Pedidos
    public IActionResult CrearPedido([FromBody] Pedido nuevoPedido)
    {
        if (nuevoPedido == null)
            return BadRequest(new { mensaje = "Los datos del pedido son requeridos" });

        if (string.IsNullOrWhiteSpace(nuevoPedido.Cliente) || 
            string.IsNullOrWhiteSpace(nuevoPedido.Producto))
            return BadRequest(new { mensaje = "Cliente y Producto son campos obligatorios" });

        // Generar nuevo ID
        var nuevoId = pedidos.Max(p => p.Id) + 1;
        nuevoPedido.Id = nuevoId;

        pedidos.Add(nuevoPedido);

        return CreatedAtAction(nameof(ObtenerPedidoPorId), new { id = nuevoPedido.Id }, nuevoPedido);
    }

    [HttpPut("{id}")] // Responde a PUT api/Pedidos/{id}
    public IActionResult ActualizarPedido(int id, [FromBody] Pedido pedidoActualizado)
    {
        var pedido = pedidos.FirstOrDefault(p => p.Id == id);

        if (pedido == null)
            return NotFound(new { mensaje = $"Pedido con id {id} no encontrado" });

        if (pedidoActualizado == null)
            return BadRequest(new { mensaje = "Los datos del pedido son requeridos" });

        // Actualizar los campos
        pedido.Cliente = pedidoActualizado.Cliente ?? pedido.Cliente;
        pedido.Producto = pedidoActualizado.Producto ?? pedido.Producto;
        pedido.Cantidad = pedidoActualizado.Cantidad > 0 ? pedidoActualizado.Cantidad : pedido.Cantidad;
        pedido.Total = pedidoActualizado.Total > 0 ? pedidoActualizado.Total : pedido.Total;
        pedido.Estado = pedidoActualizado.Estado ?? pedido.Estado;

        return Ok(new { mensaje = "Pedido actualizado correctamente", pedido });
    }

    [HttpDelete("{id}")] // Responde a DELETE api/Pedidos/{id}
    public IActionResult EliminarPedido(int id)
    {
        var pedido = pedidos.FirstOrDefault(p => p.Id == id);

        if (pedido == null)
            return NotFound(new { mensaje = $"Pedido con id {id} no encontrado" });

        pedidos.Remove(pedido);

        return Ok(new { mensaje = $"Pedido {id} eliminado correctamente" });
    }
}
