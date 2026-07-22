using Microsoft.AspNetCore.Mvc; //Permite utlizar controladores,rutas y respuestas Http
using BackendApi.Models;
namespace BackendApi.Controllers;

[ApiController] //Inidica que la clase funcionara como un controlador de una API
[Route("api/[controller]")] //Define la ruta de una API
public class ProductosController : ControllerBase // Proporciona funciones de respuesta (Ok(), NotFound(), Badrequest())
{
    // Lista en memoria: guarda los productos mientras la aplicacion este corriendo.
    // Es "static" para que la misma lista se comparta entre todas las peticiones.
    private static List<Producto> productos = new List<Producto>
    {
        new Producto { Id = 1, Nombre = "Marraqueta", Precio = 0.50m, Categoria = "Pan", Stock = 200 },
        new Producto { Id = 2, Nombre = "Cuñape", Precio = 2.00m, Categoria = "Horneado", Stock = 80 },
        new Producto { Id = 3, Nombre = "Empanada de queso", Precio = 3.50m, Categoria = "Salado", Stock = 60 }
    };

    // Lleva el control del siguiente Id a asignar cuando se registra un producto nuevo.
    private static int siguienteId = 4;

    [HttpGet] //Indica que el metodo respondera a solicitudes GET
    public IActionResult ObtenerProductos()
    {
        return Ok(productos);
    }

    [HttpGet("{id}")]
    public IActionResult ObtenerProductoPorId(int id)
    {
        // Busca en la lista el producto con ese Id
        var producto = productos.FirstOrDefault(p => p.Id == id);
        if (producto == null)
        {
            return NotFound(new { mensaje = $"No se encontro el producto {id}" });
        }
        return Ok(producto);
    }

    [HttpPost]
    public IActionResult RegistrarProducto([FromBody] Producto producto)//FromBody significa tomar la informacion que llega en formato JSON
    {
        // Le asignamos un Id nuevo y lo agregamos a la lista
        producto.Id = siguienteId;
        siguienteId++;
        productos.Add(producto);

        return Ok(new
        {
            mensaje = "Producto registrado correctamente",
            producto
        });
    }

    [HttpPut("{id}")]
    public IActionResult ActualizarProducto(int id, [FromBody] Producto producto)
    {
        // Buscamos el producto existente para modificarlo
        var existente = productos.FirstOrDefault(p => p.Id == id);
        if (existente == null)
        {
            return NotFound(new { mensaje = $"No se encontro el producto {id}" });
        }

        // Actualizamos sus datos con lo que llego en el JSON
        existente.Nombre = producto.Nombre;
        existente.Precio = producto.Precio;
        existente.Categoria = producto.Categoria;
        existente.Stock = producto.Stock;

        return Ok(new
        {
            mensaje = "Producto actualizado",
            producto = existente
        });
    }

    [HttpDelete("{id}")]
    public IActionResult EliminarProducto(int id)
    {
        // Buscamos y quitamos el producto de la lista
        var producto = productos.FirstOrDefault(p => p.Id == id);
        if (producto == null)
        {
            return NotFound(new { mensaje = $"No se encontro el producto {id}" });
        }

        productos.Remove(producto);

        return Ok(new
        {
            mensaje = $"Producto {id} eliminado correctamente"
        });
    }
}
