using Microsoft.AspNetCore.Mvc; // Permite utilizar controladores, rutas y respuestas HTTP

namespace BackendApi.Controllers;

[ApiController] // Indica que la clase funcionará como un controlador de una API
[Route("api/[controller]")] // Define la ruta de la API -> api/Pedidos
public class PedidosController : ControllerBase // Proporciona funciones de respuesta (Ok(), NotFound(), BadRequest())
{
    [HttpGet] // Indica que el método responderá a solicitudes GET
    public IActionResult ObtenerPedidos()
    {
        var pedidos = new[]
        {
            new
            {
                Id = 1,
                Cliente = "Juan Perez",
                Producto = "Marraqueta",
                Cantidad = 10,
                Total = 5.00,
                Estado = "Preparando"
            },
            new
            {
                Id = 2,
                Cliente = "Maria Becerra",
                Producto = "Torta de chocolate",
                Cantidad = 1,
                Total = 12.00,
                Estado = "Listo para entrega"
            },
            new
            {
                Id = 3,
                Cliente = "Carlos Martinez",
                Producto = "Empanada de queso",
                Cantidad = 4,
                Total = 20.00,
                Estado = "Entregado"
            }
        };

        return Ok(pedidos);
    }

    [HttpGet("{id}")] // Responde a GET api/Pedidos/{id}
    public IActionResult ObtenerPedidoPorId(int id)
    {
        var pedido = new
        {
            Id = id,
            Cliente = "Cliente Ejemplo",
            Producto = "Pan de campo",
            Cantidad = 3,
            Total = 24.00,
            Estado = "En preparación"
        };

        return Ok(pedido);
    }
}
