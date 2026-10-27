using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductosController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Controles que valen igual al crear y al editar: que la categoria y el
    /// proveedor existan, y que no se repita el nombre. Devuelve el mensaje de
    /// error, o null si esta todo bien.
    /// </summary>
    private async Task<string?> ValidarProducto(Producto producto, int? idQueSeEdita)
    {
        if (producto.CategoriaId.HasValue)
        {
            var categoriaExiste = await _context.Categorias
                .AnyAsync(cat => cat.Id == producto.CategoriaId.Value);

            if (!categoriaExiste)
                return $"No existe la Categoria con ID {producto.CategoriaId.Value}";
        }

        if (producto.ProveedorId.HasValue)
        {
            var proveedorExiste = await _context.Proveedores
                .AnyAsync(pr => pr.Id == producto.ProveedorId.Value);

            if (!proveedorExiste)
                return $"No existe el Proveedor con ID {producto.ProveedorId.Value}";
        }

        var nombre = (producto.Nombre ?? string.Empty).Trim();

        // Nombre repetido: se compara sin distinguir mayusculas ni espacios de
        // los bordes, y al editar se excluye el propio producto.
        var repetido = await _context.Productos.AnyAsync(p =>
            p.Id != (idQueSeEdita ?? 0) && p.Nombre.ToLower() == nombre.ToLower());

        if (repetido)
            return $"Ya existe un producto llamado '{nombre}'";

        return null;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerProductos()
    {
        var productos = await _context.Productos.ToListAsync();
        return Ok(productos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerProductoPorId(int id)
    {
        var producto = await _context.Productos.FindAsync(id);

        if (producto == null)
            return NotFound(new { mensaje = $"No se encontró el producto con ID {id}" });

        return Ok(producto);
    }

    [HttpPost]
    public async Task<IActionResult> RegistrarProducto([FromBody] Producto producto)
    {
        if (producto == null)
            return BadRequest(new { mensaje = "Los datos del producto son requeridos" });

        if (string.IsNullOrWhiteSpace(producto.Nombre))
            return BadRequest(new { mensaje = "El nombre del producto es requerido" });

        var problema = await ValidarProducto(producto, null);

        if (problema != null)
            return BadRequest(new { mensaje = problema });

        producto.Nombre = producto.Nombre.Trim();
        producto.Stock = 0;

        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"INSERT INTO [Producto] ([Nombre], [Precio], [Stock], [EmprendimientoId], [CategoriaId], [ProveedorId], [ImagenUrl])
                   VALUES ({producto.Nombre}, {producto.Precio}, {producto.Stock}, {producto.EmprendimientoId}, {producto.CategoriaId}, {producto.ProveedorId}, {producto.ImagenUrl})");

            var connection = _context.Database.GetDbConnection();
            var wasOpen = connection.State == System.Data.ConnectionState.Open;
            try
            {
                if (!wasOpen)
                    await connection.OpenAsync();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT CAST(SCOPE_IDENTITY() AS int)";
                var result = await cmd.ExecuteScalarAsync();
                producto.Id = result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
            finally
            {
                if (!wasOpen && connection.State == System.Data.ConnectionState.Open)
                    await connection.CloseAsync();
            }

            return CreatedAtAction(nameof(ObtenerProductoPorId), new { id = producto.Id }, producto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al guardar el producto en la base de datos", detalle = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarProducto(int id, [FromBody] Producto producto)
    {
        var existente = await _context.Productos.FindAsync(id);

        if (existente == null)
            return NotFound(new { mensaje = $"No se encontró el producto con ID {id}" });

        if (string.IsNullOrWhiteSpace(producto.Nombre))
            return BadRequest(new { mensaje = "El nombre del producto es requerido" });

        // La edicion pasa por los mismos controles que el alta: antes se podia
        // editar un producto poniendole una categoria o un proveedor que no
        // existen, o el nombre de otro producto.
        var problema = await ValidarProducto(producto, id);

        if (problema != null)
            return BadRequest(new { mensaje = problema });

        // Actualizar únicamente las propiedades reales de Producto.cs (DB_Panaderia2)
        existente.Nombre = producto.Nombre.Trim();
        existente.Precio = producto.Precio;
        existente.EmprendimientoId = producto.EmprendimientoId;
        existente.CategoriaId = producto.CategoriaId;
        existente.ProveedorId = producto.ProveedorId;
        existente.ImagenUrl = producto.ImagenUrl;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Producto actualizado correctamente",
            producto = existente
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarProducto(int id)
    {
        var producto = await _context.Productos.FindAsync(id);

        if (producto == null)
            return NotFound(new { mensaje = $"No se encontró el producto con ID {id}" });

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Producto {id} eliminado correctamente" });
    }
}