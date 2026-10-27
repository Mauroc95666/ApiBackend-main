using BackendApi.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace BackendApi.Controllers;

/// <summary>
/// Respaldo y restauracion de la base para el responsable del sistema.
/// El control de acceso lo hace el middleware: este grupo pide el permiso
/// "respaldos" incluso para leer, porque la lista de respaldos ya es
/// informacion sensible.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RespaldosController : ControllerBase
{
    private readonly Respaldos _respaldos;
    private readonly ILogger<RespaldosController> _registro;

    public RespaldosController(Respaldos respaldos, ILogger<RespaldosController> registro)
    {
        _respaldos = respaldos;
        _registro = registro;
    }

    public class RestaurarRequest
    {
        public string Archivo { get; set; } = string.Empty;
        public string Confirmacion { get; set; } = string.Empty;
    }

    /// <summary>Respaldos guardados, del mas nuevo al mas viejo.</summary>
    [HttpGet]
    public ActionResult Listar()
    {
        var archivos = _respaldos.Listar();

        return Ok(new
        {
            carpeta = _respaldos.Carpeta,
            cadaHoras = _respaldos.CadaHoras,
            conservar = _respaldos.Conservar,
            cantidad = archivos.Count,
            ultimo = archivos.FirstOrDefault()?.Fecha,
            archivos
        });
    }

    /// <summary>Genera un respaldo en el momento.</summary>
    [HttpPost]
    public async Task<ActionResult> Crear()
    {
        try
        {
            var archivo = await _respaldos.Crear("manual");
            return Ok(new { mensaje = "Respaldo generado correctamente", archivo });
        }
        catch (Exception ex)
        {
            _registro.LogError(ex, "No se pudo generar el respaldo.");
            return StatusCode(500, new { mensaje = "No se pudo generar el respaldo: " + ex.Message });
        }
    }

    /// <summary>
    /// Restaura la base desde un respaldo. Pide escribir RESTAURAR como
    /// confirmacion, porque esta operacion reemplaza TODOS los datos actuales
    /// y no se puede deshacer.
    /// </summary>
    [HttpPost("restaurar")]
    public async Task<ActionResult> Restaurar([FromBody] RestaurarRequest datos)
    {
        if (datos == null || string.IsNullOrWhiteSpace(datos.Archivo))
            return BadRequest(new { mensaje = "Falta indicar cual respaldo restaurar" });

        if (!string.Equals(datos.Confirmacion?.Trim(), "RESTAURAR", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { mensaje = "Para restaurar hay que escribir RESTAURAR como confirmación" });

        try
        {
            // Antes de pisar los datos actuales se saca un respaldo de como
            // estan ahora. Si la restauracion era un error, hay vuelta atras.
            var previo = await _respaldos.Crear("manual-antes-de-restaurar");

            await _respaldos.Restaurar(datos.Archivo);

            return Ok(new
            {
                mensaje = "Base restaurada desde " + datos.Archivo,
                respaldoPrevio = previo.Archivo
            });
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _registro.LogError(ex, "Fallo la restauracion desde {Archivo}", datos.Archivo);
            return StatusCode(500, new { mensaje = "No se pudo restaurar: " + ex.Message });
        }
    }

    /// <summary>Descarga un respaldo para guardarlo fuera del servidor.</summary>
    [HttpGet("descargar/{archivo}")]
    public ActionResult Descargar(string archivo)
    {
        try
        {
            var ruta = _respaldos.RutaParaDescargar(archivo);
            var flujo = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(flujo, "application/octet-stream", Path.GetFileName(ruta));
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
