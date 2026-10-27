using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

/// <summary>
/// Datos que manda la pantalla para registrar un movimiento. El saldo no
/// viaja: lo calcula el servidor a partir del tipo y la cantidad, asi dos
/// personas moviendo el mismo producto no se pisan el saldo entre si.
/// </summary>
public class MovimientoRequest
{
    public int InventarioId { get; set; }
    public string? Tipo { get; set; }
    public int Cantidad { get; set; }
    public int? UsuarioId { get; set; }
    public string? Motivo { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class MovimientosInventarioController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MovimientosInventarioController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerMovimientos()
    {
        var movimientos = await _context.MovimientoInventario.ToListAsync();
        return Ok(movimientos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerMovimientoPorId(int id)
    {
        var movimiento = await _context.MovimientoInventario.FindAsync(id);

        if (movimiento == null)
            return NotFound(new { mensaje = $"Movimiento con id {id} no encontrado" });

        return Ok(movimiento);
    }

    /// <summary>
    /// Movimientos de un item de inventario, del mas nuevo al mas viejo.
    /// Es el historial que la pantalla de Inventario no tenia.
    /// </summary>
    [HttpGet("inventario/{inventarioId}")]
    public async Task<IActionResult> ObtenerPorInventario(int inventarioId)
    {
        var inventario = await _context.Inventarios
            .Include(i => i.Producto)
            .FirstOrDefaultAsync(i => i.Id == inventarioId);

        if (inventario == null)
            return NotFound(new { mensaje = $"Item de inventario {inventarioId} no encontrado" });

        // Se proyecta plano para no arrastrar las referencias Inventario y
        // Usuario, que al serializarse darian un ciclo.
        var movimientos = await _context.MovimientoInventario
            .Where(m => m.InventarioId == inventarioId)
            .OrderByDescending(m => m.Fecha).ThenByDescending(m => m.Id)
            .Select(m => new
            {
                m.Id,
                m.Tipo,
                m.Cantidad,
                m.Fecha,
                m.Motivo,
                m.UsuarioId,
                usuario = m.Usuario != null ? m.Usuario.Nombre : null
            })
            .ToListAsync();

        return Ok(new
        {
            inventarioId,
            producto = inventario.Producto != null ? inventario.Producto.Nombre : null,
            stockActual = inventario.StockActual,
            movimientos
        });
    }

    [HttpPost]
    public async Task<IActionResult> CrearMovimiento([FromBody] MovimientoRequest datos)
    {
        if (datos == null)
            return BadRequest(new { mensaje = "Los datos del movimiento son requeridos" });

        var tipo = (datos.Tipo ?? string.Empty).Trim().ToUpperInvariant();

        if (tipo != "ENTRADA" && tipo != "SALIDA")
            return BadRequest(new { mensaje = "El tipo debe ser ENTRADA o SALIDA" });

        if (datos.Cantidad <= 0)
            return BadRequest(new { mensaje = "La cantidad debe ser mayor a 0" });

        // El autor es obligatorio: la columna UsuarioId no admite vacio y el
        // sentido de registrar el movimiento es justamente saber quien lo hizo.
        if (datos.UsuarioId == null || datos.UsuarioId <= 0)
            return BadRequest(new { mensaje = "Falta el usuario que registra el movimiento" });

        var existeUsuario = await _context.Usuarios.AnyAsync(u => u.Id == datos.UsuarioId.Value);

        if (!existeUsuario)
            return BadRequest(new { mensaje = $"El usuario {datos.UsuarioId} no existe" });

        var inventario = await _context.Inventarios
            .FirstOrDefaultAsync(i => i.Id == datos.InventarioId);

        if (inventario == null)
            return BadRequest(new { mensaje = $"El item de inventario {datos.InventarioId} no existe" });

        var delta = tipo == "ENTRADA" ? datos.Cantidad : -datos.Cantidad;

        if (inventario.StockActual + delta < 0)
            return BadRequest(new
            {
                mensaje = "La salida dejaria el stock en negativo",
                disponible = inventario.StockActual,
                pedido = datos.Cantidad
            });

        var producto = await _context.Productos
            .FirstOrDefaultAsync(p => p.Id == inventario.ProductoId);

        // El movimiento y los dos saldos se guardan juntos: antes la pantalla
        // pisaba StockActual con un PUT y el movimiento no quedaba en ninguna
        // parte. Ahora este endpoint es la unica via para mover stock.
        await using var transaccion = await _context.Database.BeginTransactionAsync();

        try
        {
            inventario.StockActual += delta;

            // Producto.Stock es el saldo que manda en el sistema (lo leen el
            // catalogo publico y el punto de venta), asi que se mueve igual
            // para que los dos no queden contando cosas distintas.
            if (producto != null)
            {
                var nuevoStock = (producto.Stock ?? 0) + delta;
                producto.Stock = nuevoStock < 0 ? 0 : nuevoStock;
            }

            var movimiento = new MovimientoInventario
            {
                InventarioId = inventario.Id,
                UsuarioId = datos.UsuarioId.Value,
                Tipo = tipo,
                Cantidad = datos.Cantidad,
                Fecha = DateTime.Now,
                Motivo = string.IsNullOrWhiteSpace(datos.Motivo) ? null : datos.Motivo.Trim()
            };

            _context.MovimientoInventario.Add(movimiento);

            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();

            return CreatedAtAction(
                nameof(ObtenerMovimientoPorId),
                new { id = movimiento.Id },
                new
                {
                    id = movimiento.Id,
                    tipo = movimiento.Tipo,
                    cantidad = movimiento.Cantidad,
                    fecha = movimiento.Fecha,
                    stockActual = inventario.StockActual,
                    stockProducto = producto?.Stock
                });
        }
        catch (Exception)
        {
            await transaccion.RollbackAsync();
            return StatusCode(500, new
            {
                mensaje = "No se pudo registrar el movimiento: se deshizo todo y el stock quedo intacto"
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarMovimiento(int id, [FromBody] MovimientoInventario movimientoActualizado)
    {
        var movimiento = await _context.MovimientoInventario.FindAsync(id);

        if (movimiento == null)
            return NotFound(new { mensaje = $"Movimiento con id {id} no encontrado" });

        movimiento.InventarioId = movimientoActualizado.InventarioId;
        movimiento.UsuarioId = movimientoActualizado.UsuarioId;
        movimiento.Tipo = movimientoActualizado.Tipo;
        movimiento.Cantidad = movimientoActualizado.Cantidad;
        movimiento.Fecha = movimientoActualizado.Fecha;
        movimiento.Motivo = movimientoActualizado.Motivo;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Movimiento actualizado correctamente", movimiento });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarMovimiento(int id)
    {
        var movimiento = await _context.MovimientoInventario.FindAsync(id);

        if (movimiento == null)
            return NotFound(new { mensaje = $"Movimiento con id {id} no encontrado" });

        _context.MovimientoInventario.Remove(movimiento);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Movimiento {id} eliminado correctamente" });
    }
}