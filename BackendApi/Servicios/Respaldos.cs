using Microsoft.Data.SqlClient;

namespace BackendApi.Servicios;

/// <summary>
/// Respaldo y restauracion de la base de datos.
///
/// Antes el proyecto solo tenia PERSISTENCIA: el volumen de Docker guarda los
/// datos fuera del contenedor, asi que sobreviven a un reinicio. Eso no es un
/// respaldo: si alguien borra una tabla, el volumen se corrompe o se elimina,
/// no hay de donde recuperar nada.
///
/// Este servicio ejecuta BACKUP DATABASE y RESTORE DATABASE contra el motor y
/// deja los archivos .bak en una carpeta montada desde el host, para que los
/// respaldos tambien sobrevivan a que se borre el volumen.
/// </summary>
public class Respaldos
{
    private readonly IConfiguration _config;
    private readonly ILogger<Respaldos> _registro;

    public Respaldos(IConfiguration config, ILogger<Respaldos> registro)
    {
        _config = config;
        _registro = registro;
    }

    /// <summary>Carpeta donde quedan los .bak. La ven el motor y el backend.</summary>
    public string Carpeta => _config["Respaldos:Carpeta"] ?? "/respaldos";

    /// <summary>Cuantos respaldos automaticos se conservan antes de borrar los viejos.</summary>
    public int Conservar =>
        int.TryParse(_config["Respaldos:Conservar"], out var cantidad) && cantidad > 0 ? cantidad : 10;

    /// <summary>Cada cuantas horas corre el respaldo automatico. 0 lo desactiva.</summary>
    public int CadaHoras =>
        int.TryParse(_config["Respaldos:CadaHoras"], out var horas) && horas >= 0 ? horas : 24;

    public record ArchivoDeRespaldo(string Archivo, DateTime Fecha, long Bytes, string Tamano, string Origen);

    /// <summary>Lista los respaldos existentes, del mas nuevo al mas viejo.</summary>
    public List<ArchivoDeRespaldo> Listar()
    {
        if (!Directory.Exists(Carpeta)) return new List<ArchivoDeRespaldo>();

        return new DirectoryInfo(Carpeta)
            .GetFiles("*.bak")
            .OrderByDescending(a => a.LastWriteTime)
            .Select(a => new ArchivoDeRespaldo(
                a.Name,
                a.LastWriteTime,
                a.Length,
                Legible(a.Length),
                a.Name.Contains("-automatico", StringComparison.OrdinalIgnoreCase) ? "automatico" : "manual"))
            .ToList();
    }

    /// <summary>Genera un respaldo completo y devuelve el archivo creado.</summary>
    public async Task<ArchivoDeRespaldo> Crear(string origen, CancellationToken ct = default)
    {
        Directory.CreateDirectory(Carpeta);

        var baseDatos = NombreDeLaBase();
        var nombre = $"{baseDatos}-{DateTime.Now:yyyyMMdd-HHmmss}-{Etiqueta(origen)}.bak";
        var rutaParaElMotor = $"{Carpeta.TrimEnd('/')}/{nombre}";

        await using var conexion = new SqlConnection(CadenaAlSistema());
        await conexion.OpenAsync(ct);

        await using var comando = conexion.CreateCommand();
        comando.CommandTimeout = 900;
        comando.CommandText =
            $"BACKUP DATABASE [{Escapar(baseDatos)}] TO DISK = @ruta " +
            "WITH INIT, FORMAT, NAME = @nombre, DESCRIPTION = 'Respaldo del sistema Kiataque'";
        comando.Parameters.AddWithValue("@ruta", rutaParaElMotor);
        comando.Parameters.AddWithValue("@nombre", $"Kiataque {origen}");

        await comando.ExecuteNonQueryAsync(ct);

        _registro.LogInformation("Respaldo generado: {Archivo} ({Origen})", nombre, origen);

        var archivo = new FileInfo(Path.Combine(Carpeta, nombre));
        return new ArchivoDeRespaldo(
            nombre,
            archivo.Exists ? archivo.LastWriteTime : DateTime.Now,
            archivo.Exists ? archivo.Length : 0,
            archivo.Exists ? Legible(archivo.Length) : "-",
            origen);
    }

    /// <summary>
    /// Restaura la base desde un archivo de la carpeta de respaldos.
    ///
    /// Para restaurar hay que sacar a todos los conectados: se pasa la base a
    /// SINGLE_USER con ROLLBACK IMMEDIATE, se restaura y se la devuelve a
    /// MULTI_USER. El devolverla va en el finally para que la base no quede
    /// bloqueada si la restauracion falla a mitad de camino.
    /// </summary>
    public async Task Restaurar(string archivo, CancellationToken ct = default)
    {
        var nombre = ValidarNombre(archivo);
        var ruta = Path.Combine(Carpeta, nombre);

        if (!File.Exists(ruta))
            throw new FileNotFoundException($"No existe el respaldo {nombre}");

        var baseDatos = NombreDeLaBase();
        var rutaParaElMotor = $"{Carpeta.TrimEnd('/')}/{nombre}";

        await using var conexion = new SqlConnection(CadenaAlSistema());
        await conexion.OpenAsync(ct);

        try
        {
            await Ejecutar(conexion,
                $"ALTER DATABASE [{Escapar(baseDatos)}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", ct);

            await Ejecutar(conexion,
                $"RESTORE DATABASE [{Escapar(baseDatos)}] FROM DISK = @ruta WITH REPLACE, RECOVERY",
                ct, ("@ruta", rutaParaElMotor));

            _registro.LogWarning("Base restaurada desde el respaldo {Archivo}", nombre);
        }
        finally
        {
            try
            {
                await Ejecutar(conexion, $"ALTER DATABASE [{Escapar(baseDatos)}] SET MULTI_USER", ct);
            }
            catch (Exception ex)
            {
                _registro.LogError(ex, "La base quedo en SINGLE_USER y hay que devolverla a mano.");
            }

            // Las conexiones que el backend tenia abiertas murieron con el
            // ROLLBACK IMMEDIATE. Si no se limpia el pool, la primera consulta
            // despues de restaurar falla con una conexion ya invalida.
            SqlConnection.ClearAllPools();
        }
    }

    /// <summary>Borra los respaldos automaticos mas viejos y deja los ultimos N.</summary>
    public int LimpiarViejos()
    {
        var automaticos = Listar().Where(a => a.Origen == "automatico").Skip(Conservar).ToList();
        var borrados = 0;

        foreach (var viejo in automaticos)
        {
            try
            {
                File.Delete(Path.Combine(Carpeta, viejo.Archivo));
                borrados++;
            }
            catch (Exception ex)
            {
                _registro.LogWarning(ex, "No se pudo borrar el respaldo viejo {Archivo}", viejo.Archivo);
            }
        }

        return borrados;
    }

    /// <summary>Ruta fisica de un respaldo, ya validada, para poder descargarlo.</summary>
    public string RutaParaDescargar(string archivo)
    {
        var nombre = ValidarNombre(archivo);
        var ruta = Path.Combine(Carpeta, nombre);

        if (!File.Exists(ruta))
            throw new FileNotFoundException($"No existe el respaldo {nombre}");

        return ruta;
    }

    // --- Auxiliares -------------------------------------------------------

    private async Task Ejecutar(SqlConnection conexion, string sql, CancellationToken ct,
        params (string Nombre, object Valor)[] parametros)
    {
        await using var comando = conexion.CreateCommand();
        comando.CommandTimeout = 900;
        comando.CommandText = sql;

        foreach (var (clave, valor) in parametros)
            comando.Parameters.AddWithValue(clave, valor);

        await comando.ExecuteNonQueryAsync(ct);
    }

    /// <summary>
    /// Conexion a master. BACKUP y RESTORE no se pueden ejecutar estando
    /// conectado a la base que se esta tocando.
    /// </summary>
    private string CadenaAlSistema()
    {
        var cadena = _config.GetConnectionString("ConexionDB") ?? string.Empty;
        var constructor = new SqlConnectionStringBuilder(cadena)
        {
            InitialCatalog = "master",
            ConnectTimeout = 30,
        };
        return constructor.ConnectionString;
    }

    private string NombreDeLaBase()
    {
        var cadena = _config.GetConnectionString("ConexionDB") ?? string.Empty;
        var nombre = new SqlConnectionStringBuilder(cadena).InitialCatalog;

        if (string.IsNullOrWhiteSpace(nombre))
            throw new InvalidOperationException("La cadena de conexion no indica la base de datos.");

        return nombre;
    }

    /// <summary>
    /// El nombre llega por la URL, asi que no se confia: solo se acepta un
    /// archivo .bak sin carpetas, para que nadie pida leer o restaurar algo de
    /// otro lugar del disco.
    /// </summary>
    private static string ValidarNombre(string archivo)
    {
        if (string.IsNullOrWhiteSpace(archivo))
            throw new ArgumentException("Falta indicar el archivo de respaldo");

        var nombre = Path.GetFileName(archivo.Trim());

        if (nombre != archivo.Trim() || !nombre.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("El archivo de respaldo no es valido");

        return nombre;
    }

    private static string Escapar(string identificador) => identificador.Replace("]", "]]");

    private static string Etiqueta(string origen) =>
        origen.Equals("automatico", StringComparison.OrdinalIgnoreCase) ? "automatico" : "manual";

    private static string Legible(long bytes)
    {
        if (bytes >= 1024L * 1024 * 1024) return $"{bytes / 1024d / 1024 / 1024:0.0} GB";
        if (bytes >= 1024L * 1024) return $"{bytes / 1024d / 1024:0.0} MB";
        if (bytes >= 1024) return $"{bytes / 1024d:0.0} KB";
        return $"{bytes} B";
    }
}
