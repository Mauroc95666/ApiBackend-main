using System.Security.Cryptography;
using System.Text;

namespace BackendApi.Servicios;

/// <summary>
/// Token de sesion firmado. Se arma con el id del usuario, el de su rol y un
/// vencimiento, y se le pega una firma HMAC-SHA256. Si alguien cambia
/// cualquier dato del token, la firma deja de coincidir y el token se rechaza.
///
/// Se hizo a mano con lo que trae .NET en vez de sumar un paquete de JWT:
/// menos dependencias y el mismo efecto para lo que necesita el sistema.
/// </summary>
public class Tokens
{
    private readonly byte[] _clave;

    public Tokens(IConfiguration configuracion)
    {
        var texto = configuracion["Seguridad:ClaveToken"];

        // Sin clave configurada se genera una al azar al arrancar. Eso cierra
        // todas las sesiones cuando el servidor se reinicia, que es preferible
        // a dejar una clave fija escrita en el repositorio.
        _clave = string.IsNullOrWhiteSpace(texto)
            ? RandomNumberGenerator.GetBytes(32)
            : Encoding.UTF8.GetBytes(texto);
    }

    public string Emitir(int usuarioId, int rolId, TimeSpan duracion)
    {
        var vence = DateTimeOffset.UtcNow.Add(duracion).ToUnixTimeSeconds();
        var cuerpo = $"{usuarioId}.{rolId}.{vence}";
        return $"{cuerpo}.{Firmar(cuerpo)}";
    }

    /// <summary>
    /// Devuelve el usuario y el rol del token, o null si esta vencido, mal
    /// formado o la firma no coincide.
    /// </summary>
    public (int UsuarioId, int RolId)? Leer(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var partes = token.Split('.');

        if (partes.Length != 4)
            return null;

        var cuerpo = $"{partes[0]}.{partes[1]}.{partes[2]}";

        if (!FirmaCoincide(cuerpo, partes[3]))
            return null;

        if (!int.TryParse(partes[0], out var usuarioId) ||
            !int.TryParse(partes[1], out var rolId) ||
            !long.TryParse(partes[2], out var vence))
            return null;

        if (DateTimeOffset.FromUnixTimeSeconds(vence) < DateTimeOffset.UtcNow)
            return null;

        return (usuarioId, rolId);
    }

    private string Firmar(string cuerpo)
    {
        using var hmac = new HMACSHA256(_clave);
        var firma = hmac.ComputeHash(Encoding.UTF8.GetBytes(cuerpo));
        return Convert.ToBase64String(firma).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private bool FirmaCoincide(string cuerpo, string firmaRecibida)
    {
        var esperada = Encoding.UTF8.GetBytes(Firmar(cuerpo));
        var recibida = Encoding.UTF8.GetBytes(firmaRecibida);

        return esperada.Length == recibida.Length &&
               CryptographicOperations.FixedTimeEquals(esperada, recibida);
    }
}
