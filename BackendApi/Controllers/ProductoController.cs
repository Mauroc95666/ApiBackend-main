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

        if (producto.ProveedorId.HasValue)
        {
            var proveedorExiste = await _context.Proveedores.FindAsync(producto.ProveedorId.Value);
            if (proveedorExiste == null)
                return BadRequest(new { mensaje = $"No existe el Proveedor con ID {producto.ProveedorId.Value}" });
        }

        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"INSERT INTO [Producto] ([Nombre], [Precio], [Stock], [EmprendimientoId], [CategoriaId], [ProveedorId])
                   VALUES ({producto.Nombre}, {producto.Precio}, {producto.Stock}, {producto.EmprendimientoId}, {producto.CategoriaId}, {producto.ProveedorId})");

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

        // Actualizar únicamente las propiedades reales de Producto.cs (DB_Panaderia2)
        existente.Nombre = producto.Nombre;
        existente.Precio = producto.Precio;
        existente.Stock = producto.Stock;
        existente.EmprendimientoId = producto.EmprendimientoId;
        existente.CategoriaId = producto.CategoriaId;
        existente.ProveedorId = producto.ProveedorId;

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