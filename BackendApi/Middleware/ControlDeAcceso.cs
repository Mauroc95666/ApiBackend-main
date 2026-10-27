using Microsoft.EntityFrameworkCore;
using BackendApi.Models;
using BackendApi.Servicios;

namespace BackendApi.Middleware;

/// <summary>
/// Control de acceso del lado del servidor. Antes el unico control vivia en el
/// frontend: el menu escondia las opciones, pero la API respondia a cualquiera
/// que escribiera la URL a mano.
///
/// La regla es:
///  - La tienda publica sigue funcionando sin sesion. Puede leer el catalogo y
///    puede comprar, porque usa los mismos endpoints que el panel.
///  - Cualquier otra lectura pide una sesion valida.
///  - Cualquier escritura administrativa pide el permiso del modulo.
/// </summary>
public class ControlDeAcceso
{
    private readonly RequestDelegate _siguiente;

    public ControlDeAcceso(RequestDelegate siguiente)
    {
        _siguiente = siguiente;
    }

    /// <summary>Que permiso corresponde a cada grupo de endpoints.</summary>
    private static readonly Dictionary<string, string> ModuloDe = new(StringComparer.OrdinalIgnoreCase)
    {
        ["productos"] = "productos",
        ["categorias"] = "categorias",
        ["ventas"] = "ventas",
        ["detalleventa"] = "ventas",
        ["pedidos"] = "pedidos",
        ["inventarios"] = "inventario",
        ["movimientosinventario"] = "inventario",
        ["sucursales"] = "sucursales",
        ["clientes"] = "clientes",
        ["usuarios"] = "usuarios",
        ["roles"] = "roles",
        ["permisos"] = "roles",
        ["ciudades"] = "ciudades",
        ["metodospago"] = "metodospago",
        ["proveedores"] = "proveedores",
        ["emprendimientos"] = "emprendimientos",
        ["respaldos"] = "respaldos",
        ["reportes"] = "reportes",
    };

    /// <summary>
    /// Grupos donde hasta la lectura pide el permiso del modulo. La lista de
    /// respaldos ya dice que se respalda y cuando, asi que no la ve cualquiera
    /// que tenga sesion.
    /// </summary>
    private static readonly HashSet<string> PermisoTambienParaLeer = new(StringComparer.OrdinalIgnoreCase)
    {
        "respaldos", "reportes"
    };

    /// <summary>Lecturas que la tienda publica necesita para mostrar el catalogo.</summary>
    private static readonly HashSet<string> LecturaPublica = new(StringComparer.OrdinalIgnoreCase)
    {
        "productos", "categorias", "sucursales", "metodospago", "clientes"
    };

    /// <summary>
    /// Altas que hace el visitante al comprar en la tienda. Quedan abiertas
    /// porque la tienda usa los mismos endpoints que el panel; separarlas en
    /// una API propia de la tienda es lo que queda pendiente.
    /// </summary>
    private static readonly HashSet<string> AltaPublica = new(StringComparer.OrdinalIgnoreCase)
    {
        "clientes", "ventas", "pedidos"
    };

    public async Task InvokeAsync(HttpContext contexto, Tokens tokens, ApplicationDbContext db)
    {
        var ruta = contexto.Request.Path.Value ?? string.Empty;

        // Fuera de /api no se controla nada: ahi estan el frontend y Swagger.
        if (!ruta.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            await _siguiente(contexto);
            return;
        }

        var partes = ruta.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var grupo = partes.Length >= 2 ? partes[1] : string.Empty;
        var metodo = contexto.Request.Method;

        // El navegador pregunta con OPTIONS antes de un pedido con
        // encabezados: esa consulta la contesta CORS, no este control.
        if (HttpMethods.IsOptions(metodo))
        {
            await _siguiente(contexto);
            return;
        }

        // El login tiene que ser alcanzable sin sesion, obviamente.
        if (grupo.Equals("auth", StringComparison.OrdinalIgnoreCase))
        {
            await _siguiente(contexto);
            return;
        }

        var esLectura = HttpMethods.IsGet(metodo) || HttpMethods.IsHead(metodo);

        if (esLectura && LecturaPublica.Contains(grupo))
        {
            await _siguiente(contexto);
            return;
        }

        if (HttpMethods.IsPost(metodo) && AltaPublica.Contains(grupo))
        {
            await _siguiente(contexto);
            return;
        }

        // De aca en adelante hace falta una sesion.
        var encabezado = contexto.Request.Headers.Authorization.ToString();
        var token = encabezado.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? encabezado["Bearer ".Length..].Trim()
            : null;

        var sesion = tokens.Leer(token);

        if (sesion == null)
        {
            await Responder(contexto, 401, "Necesitás iniciar sesión para acceder a esto");
            return;
        }

        // AsNoTracking a proposito: si el usuario queda en seguimiento, EF
        // completa las referencias Usuario de otras entidades en la misma
        // consulta y terminan apareciendo en respuestas donde no van.
        var usuario = await db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == sesion.Value.UsuarioId);

        if (usuario == null || !usuario.Activo)
        {
            await Responder(contexto, 401, "La sesión ya no es válida");
            return;
        }

        if (!esLectura
            && grupo.Equals("productos", StringComparison.OrdinalIgnoreCase)
            && await db.Roles.AnyAsync(r => r.Id == usuario.RolId && r.Nombre.ToLower() == "empleado"))
        {
            await Responder(contexto, 403, "El rol Empleado no puede modificar productos ni sus precios");
            return;
        }

        // Leer estando adentro alcanza. Para modificar hace falta el permiso
        // del modulo que se esta tocando.
        var pideSiemprePermiso = PermisoTambienParaLeer.Contains(grupo);

        if ((!esLectura || pideSiemprePermiso) && ModuloDe.TryGetValue(grupo, out var modulo))
        {
            var tiene = await db.RolPermisos
                .Include(rp => rp.Permiso)
                .AnyAsync(rp => rp.RolId == usuario.RolId && rp.Permiso.Modulo == modulo);

            if (!tiene)
            {
                var accion = esLectura ? "ver" : "modificar";
                await Responder(contexto, 403, $"Tu rol no tiene permiso para {accion} {modulo}");
                return;
            }
        }

        await _siguiente(contexto);
    }

    private static async Task Responder(HttpContext contexto, int codigo, string mensaje)
    {
        contexto.Response.StatusCode = codigo;
        contexto.Response.ContentType = "application/json; charset=utf-8";
        await contexto.Response.WriteAsJsonAsync(new { mensaje });
    }
}
