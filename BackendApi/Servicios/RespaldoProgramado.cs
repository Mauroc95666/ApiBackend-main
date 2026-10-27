namespace BackendApi.Servicios;

/// <summary>
/// Tarea programada de respaldo. Corre dentro del backend, asi que no hace
/// falta configurar cron ni el Agente de SQL Server (la imagen de Linux no lo
/// trae en la edicion Developer).
///
/// Al arrancar espera un minuto para no pelear con el arranque del motor, saca
/// un respaldo y despues repite cada Respaldos:CadaHoras. Si esa configuracion
/// es 0, la tarea no corre: el respaldo queda solo manual.
/// </summary>
public class RespaldoProgramado : BackgroundService
{
    private readonly Respaldos _respaldos;
    private readonly ILogger<RespaldoProgramado> _registro;

    public RespaldoProgramado(Respaldos respaldos, ILogger<RespaldoProgramado> registro)
    {
        _respaldos = respaldos;
        _registro = registro;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var horas = _respaldos.CadaHoras;

        if (horas <= 0)
        {
            _registro.LogInformation("Respaldo automatico desactivado (Respaldos:CadaHoras = 0).");
            return;
        }

        _registro.LogInformation("Respaldo automatico activo: cada {Horas} h, se conservan {Cuantos}.",
            horas, _respaldos.Conservar);

        try
        {
            await Task.Delay(TimeSpan.FromMinutes(1), ct);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var archivo = await _respaldos.Crear("automatico", ct);
                var borrados = _respaldos.LimpiarViejos();

                _registro.LogInformation(
                    "Respaldo automatico listo: {Archivo} ({Tamano}). Se borraron {Borrados} viejos.",
                    archivo.Archivo, archivo.Tamano, borrados);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                // Que falle un respaldo no puede tumbar el backend: se registra
                // y se vuelve a intentar en el siguiente turno.
                _registro.LogError(ex, "Fallo el respaldo automatico.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(horas), ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
