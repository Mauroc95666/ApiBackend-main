using System.Security.Cryptography;

namespace BackendApi.Servicios;

/// <summary>
/// Guarda y verifica contraseñas. Antes se guardaban tal cual se tipeaban, asi
/// que cualquiera con acceso a la tabla Usuario las leia todas.
///
/// Se usa PBKDF2 con SHA-256, que viene en .NET y no necesita instalar nada.
/// El formato guardado es PBKDF2$iteraciones$sal$hash, con la sal y el hash en
/// base64. La sal es distinta para cada usuario, asi dos personas con la misma
/// contraseña no quedan con el mismo hash.
/// </summary>
public static class Passwords
{
    private const string Prefijo = "PBKDF2";
    private const int Iteraciones = 100_000;
    private const int TamanoSal = 16;
    private const int TamanoHash = 32;

    public static string Hashear(string contrasena)
    {
        var sal = RandomNumberGenerator.GetBytes(TamanoSal);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            contrasena, sal, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);

        return $"{Prefijo}${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Dice si el valor guardado ya tiene formato de hash. Lo que no lo tiene
    /// es una contraseña vieja, de cuando se guardaban en texto plano.
    /// </summary>
    public static bool EstaHasheada(string? guardado) =>
        !string.IsNullOrEmpty(guardado) && guardado.StartsWith(Prefijo + "$", StringComparison.Ordinal);

    /// <summary>
    /// Verifica la contraseña contra lo que hay guardado. Si lo guardado
    /// todavia es texto plano se compara directo, para que nadie quede afuera
    /// del sistema al activarse el hasheo; quien llama se encarga de
    /// reemplazarlo por un hash despues de un ingreso correcto.
    /// </summary>
    public static bool Verificar(string contrasena, string? guardado)
    {
        if (string.IsNullOrEmpty(guardado))
            return false;

        if (!EstaHasheada(guardado))
            return guardado == contrasena;

        var partes = guardado.Split('$');

        if (partes.Length != 4 || !int.TryParse(partes[1], out var iteraciones))
            return false;

        try
        {
            var sal = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);

            var calculado = Rfc2898DeriveBytes.Pbkdf2(
                contrasena, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);

            // Comparacion de tiempo fijo: no corta en la primera diferencia.
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
