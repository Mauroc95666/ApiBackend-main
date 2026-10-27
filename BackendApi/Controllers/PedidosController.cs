using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

/// <summary>
/// Datos que llegan desde la pantalla. No se usa la entidad Pedido directo
/// porque ademas de sus campos hace falta saber QUIEN hace el cambio, y eso
/// no vive en la tabla Pedido sino en el historial.
/// </summary>
public class PedidoRequest
{
    public int VentaId { get; set; }
    public string? Estado { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public int? UsuarioId { get; set; }
    public string? Observacion { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuracion;

    /// <summary>Los cuatro estados, en el orden en que avanzan.</summary>
    private static readonly string[] Estados =
        { "PENDIENTE", "EN PREPARACION", "LISTO", "ENTREGADO" };

    public PedidosController(ApplicationDbContext context, IConfiguration configuracion)
    {
        _context = context;
        _configuracion = configuracion;
    }

    private static string Normalizar(string? estado) =>
        (estado ?? string.Empty).Trim().ToUpperInvariant();

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

    /// <summary>
    /// Linea de tiempo del pedido: cada estado por el que paso, cuando y
    /// quien lo movio. Es la base para medir el tiempo de servicio (HU-12).
    /// </summary>
    [HttpGet("{id}/historial")]
    public async Task<IActionResult> ObtenerHistorial(int id)
    {
        var existe = await _context.Pedidos.AnyAsync(p => p.Id == id);

        if (!existe)
            return NotFound(new { mensaje = $"Pedido con id {id} no encontrado" });

        // Se proyecta a un objeto plano para no arrastrar las referencias
        // Pedido y Usuario, que al serializarse darian un ciclo.
        var historial = await _context.PedidoEstadoHistorial
            .Where(h => h.PedidoId == id)
            .OrderBy(h => h.Fecha).ThenBy(h => h.Id)
            .Select(h => new
            {
                h.Id,
                h.EstadoAnterior,
                h.Estado,
                h.UsuarioId,
                usuario = h.Usuario != null ? h.Usuario.Nombre : null,
                h.Fecha,
                h.Observacion
            })
            .ToListAsync();

        // Minutos entre la primera y la ultima marca: cuanto tardo el pedido
        // desde que se registro hasta su ultimo movimiento.
        double? minutos = historial.Count >= 2
            ? Math.Round((historial[^1].Fecha - historial[0].Fecha).TotalMinutes, 1)
            : null;

        return Ok(new { pedidoId = id, minutosTranscurridos = minutos, historial });
    }

    /// <summary>
    /// Tiempos de servicio: cuanto tardo cada pedido entregado, cuanto duro
    /// cada etapa y el promedio del periodo. Es el requisito RNF-10, que
    /// figuraba como completo sin estar implementado.
    ///
    /// La meta en minutos sale de la configuracion Pedidos:MetaMinutos. El
    /// valor por defecto es 30 y queda pendiente que el Product Owner lo
    /// confirme.
    /// </summary>
    [HttpGet("tiempos")]
    public async Task<IActionResult> TiemposDeServicio(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
    {
        var hoy = DateTime.Today;
        var inicio = (desde ?? hoy.AddDays(-30)).Date;
        var fin = (hasta ?? hoy).Date.AddDays(1);

        if (inicio >= fin)
            return BadRequest(new { mensaje = "La fecha desde tiene que ser anterior a la fecha hasta" });

        var meta = _configuracion.GetValue<int?>("Pedidos:MetaMinutos") ?? 30;

        var entregados = await _context.Pedidos
            .Where(p => p.Estado == "ENTREGADO"
                     && p.FechaCreacion != null
                     && p.FechaEntregaReal != null
                     && p.FechaEntregaReal >= inicio
                     && p.FechaEntregaReal < fin)
            .Select(p => new { p.Id, p.VentaId, p.FechaCreacion, p.FechaEntregaReal })
            .ToListAsync();

        var ids = entregados.Select(p => p.Id).ToList();

        var historial = await _context.PedidoEstadoHistorial
            .Where(h => ids.Contains(h.PedidoId))
            .OrderBy(h => h.PedidoId).ThenBy(h => h.Fecha).ThenBy(h => h.Id)
            .Select(h => new { h.PedidoId, h.Estado, h.Fecha })
            .ToListAsync();

        var pedidos = entregados.Select(p =>
        {
            var minutos = Math.Round(
                (p.FechaEntregaReal!.Value - p.FechaCreacion!.Value).TotalMinutes, 1);

            // Duracion de cada etapa: desde que entro a un estado hasta que
            // entro al siguiente.
            var marcas = historial.Where(h => h.PedidoId == p.Id).ToList();

            var etapas = marcas.Select((m, i) => new
            {
                estado = m.Estado,
                desde = m.Fecha,
                hasta = i + 1 < marcas.Count ? marcas[i + 1].Fecha : p.FechaEntregaReal!.Value,
                minutos = Math.Round(
                    ((i + 1 < marcas.Count ? marcas[i + 1].Fecha : p.FechaEntregaReal!.Value) - m.Fecha)
                    .TotalMinutes, 1)
            })
            .Where(e => e.estado != "ENTREGADO")
            .ToList();

            return new
            {
                p.Id,
                p.VentaId,
                creado = p.FechaCreacion,
                entregado = p.FechaEntregaReal,
                minutos,
                superaLaMeta = minutos > meta,
                etapas
            };
        })
        .OrderByDescending(p => p.entregado)
        .ToList();

        var promedio = pedidos.Count == 0
            ? 0
            : Math.Round(pedidos.Average(p => p.minutos), 1);

        // Promedio por etapa, juntando todos los pedidos del periodo.
        var porEtapa = pedidos
            .SelectMany(p => p.etapas)
            .GroupBy(e => e.estado)
            .Select(g => new
            {
                estado = g.Key,
                promedioMinutos = Math.Round(g.Average(e => e.minutos), 1),
                vecesMedido = g.Count()
            })
            .ToList();

        return Ok(new
        {
            desde = inicio,
            hasta = fin.AddDays(-1),
            metaMinutos = meta,
            cantidadEntregados = pedidos.Count,
            promedioMinutos = promedio,
            fueraDeMeta = pedidos.Count(p => p.superaLaMeta),
            porEtapa,
            pedidos
        });
    }

    [HttpPost]
    public async Task<IActionResult> CrearPedido([FromBody] PedidoRequest datos)
    {
        if (datos == null)
            return BadRequest(new { mensaje = "Los datos del pedido son requeridos" });

        if (datos.VentaId <= 0)
            return BadRequest(new { mensaje = "El VentaId es obligatorio" });

        var estado = Normalizar(datos.Estado);

        if (string.IsNullOrEmpty(estado))
            estado = Estados[0];

        if (!Estados.Contains(estado))
            return BadRequest(new
            {
                mensaje = $"Estado no valido: '{datos.Estado}'",
                estadosValidos = Estados
            });

        var ahora = DateTime.Now;

        var nuevoPedido = new Pedido
        {
            VentaId = datos.VentaId,
            Estado = estado,
            FechaEntrega = datos.FechaEntrega,
            // La pone el sistema: es el punto cero para medir cuanto tarda.
            FechaCreacion = ahora,
            // Si el pedido naciera ya entregado, su entrega real es ahora.
            FechaEntregaReal = estado == "ENTREGADO" ? ahora : null
        };

        _context.Pedidos.Add(nuevoPedido);
        await _context.SaveChangesAsync();

        // Primera marca de la linea de tiempo: de donde arranca el pedido.
        _context.PedidoEstadoHistorial.Add(new PedidoEstadoHistorial
        {
            PedidoId = nuevoPedido.Id,
            EstadoAnterior = null,
            Estado = estado,
            UsuarioId = datos.UsuarioId,
            Fecha = DateTime.Now,
            Observacion = datos.Observacion ?? "Pedido creado"
        });

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPedidoPorId), new { id = nuevoPedido.Id }, nuevoPedido);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarPedido(int id, [FromBody] PedidoRequest datos)
    {
        var pedido = await _context.Pedidos.FindAsync(id);

        if (pedido == null)
            return NotFound(new { mensaje = $"Pedido con id {id} no encontrado" });

        pedido.VentaId = datos.VentaId > 0 ? datos.VentaId : pedido.VentaId;
        pedido.FechaEntrega = datos.FechaEntrega ?? pedido.FechaEntrega;

        var estadoAnterior = Normalizar(pedido.Estado);
        var estadoNuevo = string.IsNullOrWhiteSpace(datos.Estado)
            ? estadoAnterior
            : Normalizar(datos.Estado);

        if (estadoNuevo != estadoAnterior)
        {
            if (!Estados.Contains(estadoNuevo))
                return BadRequest(new
                {
                    mensaje = $"Estado no valido: '{datos.Estado}'",
                    estadosValidos = Estados
                });

            var posActual = Array.IndexOf(Estados, estadoAnterior);
            var posNueva = Array.IndexOf(Estados, estadoNuevo);

            // El pedido avanza de a un paso y no vuelve atras: eso es lo que
            // mantiene la linea de tiempo coherente. Antes el modal de edicion
            // permitia retroceder o saltearse estados.
            if (posNueva < posActual)
                return BadRequest(new
                {
                    mensaje = $"El pedido no puede volver de {estadoAnterior} a {estadoNuevo}"
                });

            if (posNueva > posActual + 1)
                return BadRequest(new
                {
                    mensaje = $"No se puede saltear estados: despues de {estadoAnterior} sigue {Estados[posActual + 1]}"
                });

            pedido.Estado = estadoNuevo;

            // La hora real de entrega la pone el sistema cuando el pedido
            // llega a ENTREGADO. FechaEntrega sigue siendo la planificada.
            if (estadoNuevo == "ENTREGADO" && pedido.FechaEntregaReal == null)
                pedido.FechaEntregaReal = DateTime.Now;

            _context.PedidoEstadoHistorial.Add(new PedidoEstadoHistorial
            {
                PedidoId = pedido.Id,
                EstadoAnterior = estadoAnterior,
                Estado = estadoNuevo,
                UsuarioId = datos.UsuarioId,
                Fecha = DateTime.Now,
                Observacion = datos.Observacion
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Pedido actualizado correctamente", pedido });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarPedido(int id)
    {
        var pedido = await _context.Pedidos.FindAsync(id);

        if (pedido == null)
            return NotFound(new { mensaje = $"Pedido con id {id} no encontrado" });

        // El historial se borra solo: la clave foranea esta declarada con
        // ON DELETE CASCADE en db/init.sql.
        _context.Pedidos.Remove(pedido);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Pedido {id} eliminado correctamente" });
    }
}
