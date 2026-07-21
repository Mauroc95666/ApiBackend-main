using Microsoft.AspNetCore.Mvc; //Permite utilizar controladores, rutas y respuestas Http

namespace BackendApi.Controllers;

[ApiController] //Indica que la clase funcionara como un controlador de una API
[Route("api/[controller]")] //Define la ruta de la API -> api/Producto
public class ProductoController : ControllerBase // Proporciona funciones de respuesta (Ok(), NotFound(), BadRequest())
{
    // Productos de la Panaderia Kiataque
    [HttpGet] //Indica que el metodo respondera a solicitudes GET
    public IActionResult ObtenerProductos()
    {
        var productos = new[]
        {
            new
            {
                Id = 1,
                Nombre = "Marraqueta",
                Descripcion = "Pan crujiente horneado cada manana",
                Precio = 0.50,
                Stock = 200
            },
            new
            {
                Id = 2,
                Nombre = "Cuernito de mantequilla",
                Descripcion = "Cuernito hojaldrado con mantequilla",
                Precio = 3.00,
                Stock = 80
            },
            new
            {
                Id = 3,
                Nombre = "Empanada de queso",
                Descripcion = "Empanada rellena de queso fundido",
                Precio = 5.00,
                Stock = 60
            },
            new
            {
                Id = 4,
                Nombre = "Torta de chocolate",
                Descripcion = "Porcion de torta humeda de chocolate",
                Precio = 12.00,
                Stock = 25
            },
            new
            {
                Id = 5,
                Nombre = "Cateto de la Panaderia Kiataque",
                Descripcion = "Pan de campo tradicional de la casa",
                Precio = 8.00,
                Stock = 40
            }
        };
        return Ok(productos);
    }

    [HttpGet("{id}")] //Responde a GET api/Producto/{id}
    public IActionResult ObtenerProductoPorId(int id)
    {
        var producto = new
        {
            Id = id,
            Nombre = "Producto Ejemplo",
            Descripcion = "Producto de la Panaderia Kiataque",
            Precio = 10.00,
            Stock = 50
        };
        return Ok(producto);
    }
}
