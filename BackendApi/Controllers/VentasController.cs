using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;
using BackendApi.Services;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public VentasController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerVentas()
    {
        var ventas = await _context.Ventas.ToListAsync();
        return Ok(ventas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerVentaPorId(int id)
    {
        // Proyectamos a un objeto plano en vez de devolver la entidad: asi se
        // puede ver el detalle de la venta sin que System.Text.Json entre en
        // bucle por la referencia DetalleVenta -> Venta -> DetallesVenta.
        var venta = await _context.Ventas
            .Where(v => v.Id == id)
            .Select(v => new
            {
                v.Id,
                v.SucursalId,
                v.ClienteId,
                v.UsuarioId,
                v.MetodoPagoId,
                v.Fecha,
                v.Total,
                DetallesVenta = v.DetallesVenta.Select(d => new
                {
                    d.Id,
                    d.ProductoId,
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.Subtotal
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (venta == null)
            return NotFound(new { mensaje = $"Venta con id {id} no encontrada" });

        return Ok(venta);
    }

    [HttpPost]
    public async Task<IActionResult> CrearVenta([FromBody] Venta nuevaVenta)
    {
        if (nuevaVenta == null)
            return BadRequest(new { mensaje = "Los datos de la venta son requeridos" });

        var lineas = nuevaVenta.DetallesVenta?.ToList() ?? new List<DetalleVenta>();

        if (lineas.Count == 0)
            return BadRequest(new { mensaje = "La venta debe tener al menos un producto" });

        if (lineas.Any(d => d.Cantidad <= 0))
            return BadRequest(new { mensaje = "La cantidad de cada producto debe ser mayor a 0" });

        await InventarioInicializador.AsegurarFilasAsync(_context);

        // El mismo producto puede venir en mas de una linea: se suman las
        // cantidades para controlar el stock una sola vez por producto.
        var pedidoPorProducto = lineas
            .GroupBy(d => d.ProductoId)
            .ToDictionary(g => g.Key, g => g.Sum(d => d.Cantidad));

        var idsPedidos = pedidoPorProducto.Keys.ToList();

        var productos = await _context.Productos
            .Where(p => idsPedidos.Contains(p.Id))
            .ToListAsync();
        var inventariosSucursal = await _context.Inventarios
            .Where(i => idsPedidos.Contains(i.ProductoId) && i.SucursalId == nuevaVenta.SucursalId)
            .ToDictionaryAsync(i => i.ProductoId);

        // Se controlan los saldos global y de la sucursal antes de guardar;
        // si falta stock en cualquiera de ellos, no se registra la venta.
        var faltantes = new List<object>();

        foreach (var pedido in pedidoPorProducto)
        {
            var producto = productos.FirstOrDefault(p => p.Id == pedido.Key);

            if (producto == null)
                return BadRequest(new { mensaje = $"El producto {pedido.Key} no existe" });

            var disponible = producto.Stock ?? 0;
            var inventario = inventariosSucursal.GetValueOrDefault(pedido.Key);
            var disponibleSucursal = inventario?.StockActual ?? 0;

            if (pedido.Value > disponible || pedido.Value > disponibleSucursal)
                faltantes.Add(new
                {
                    producto = producto.Nombre,
                    pedido = pedido.Value,
                    disponible,
                    disponibleSucursal,
                    sucursalId = nuevaVenta.SucursalId
                });
        }

        if (faltantes.Count > 0)
            return BadRequest(new
            {
                mensaje = "No hay stock suficiente para completar la venta",
                faltantes
            });

        // La venta y el descuento de stock van juntos en una transaccion: asi
        // no puede quedar la venta sin su descuento ni el descuento sin venta.
        await using var transaccion = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Ventas.Add(nuevaVenta);
            // El detalle que venga en DetallesVenta se inserta en cascada junto
            // con la venta: no hace falta guardarlo aparte.
            await _context.SaveChangesAsync();

            foreach (var pedido in pedidoPorProducto)
            {
                var producto = productos.First(p => p.Id == pedido.Key);
                producto.Stock = (producto.Stock ?? 0) - pedido.Value;

                // Si la sucursal lleva este producto en su inventario, se baja
                // tambien ese saldo y queda asentado el movimiento de salida.
                var inventario = inventariosSucursal[pedido.Key];
                inventario.StockActual -= pedido.Value;

                _context.MovimientoInventario.Add(new MovimientoInventario
                {
                    InventarioId = inventario.Id,
                    UsuarioId = nuevaVenta.UsuarioId,
                    Tipo = "SALIDA",
                    Cantidad = pedido.Value,
                    Fecha = DateTime.Now,
                    Motivo = $"Venta {nuevaVenta.Id}"
                });
            }

            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();
        }
        catch (Exception)
        {
            await transaccion.RollbackAsync();
            return StatusCode(500, new
            {
                mensaje = "No se pudo registrar la venta: se deshizo todo y el stock quedo intacto"
            });
        }

        // Devolvemos un resumen plano, no la entidad: al guardar, EF completa
        // la referencia inversa DetalleVenta.Venta y serializarla da un ciclo.
        return CreatedAtAction(
            nameof(ObtenerVentaPorId),
            new { id = nuevaVenta.Id },
            new
            {
                id = nuevaVenta.Id,
                total = nuevaVenta.Total,
                lineas = lineas.Count,
                stockDescontado = pedidoPorProducto.Select(x => new
                {
                    productoId = x.Key,
                    descontado = x.Value,
                    stockRestante = productos.First(p => p.Id == x.Key).Stock
                })
            });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarVenta(int id, [FromBody] Venta ventaActualizada)
    {
        var venta = await _context.Ventas.FindAsync(id);

        if (venta == null)
            return NotFound(new { mensaje = $"Venta con id {id} no encontrada" });

        venta.SucursalId = ventaActualizada.SucursalId;
        venta.ClienteId = ventaActualizada.ClienteId;
        venta.UsuarioId = ventaActualizada.UsuarioId;
        venta.MetodoPagoId = ventaActualizada.MetodoPagoId;
        venta.Fecha = ventaActualizada.Fecha;
        venta.Total = ventaActualizada.Total;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Venta actualizada correctamente", venta });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarVenta(int id)
    {
        var venta = await _context.Ventas.FindAsync(id);

        if (venta == null)
            return NotFound(new { mensaje = $"Venta con id {id} no encontrada" });

        _context.Ventas.Remove(venta);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Venta {id} eliminada correctamente" });
    }
}