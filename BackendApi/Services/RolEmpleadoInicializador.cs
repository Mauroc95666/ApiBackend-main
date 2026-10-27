using BackendApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Services;

public static class RolEmpleadoInicializador
{
    private static readonly (string Modulo, string Nombre)[] PermisosEmpleado =
    [
        ("inicio", "Ver el panel de control"),
        ("inventario", "Gestionar inventario"),
        ("ventas", "Registrar ventas"),
        ("pedidos", "Gestionar pedidos"),
        ("clientes", "Gestionar clientes"),
        ("reportes", "Ver reportes")
    ];

    public static async Task AsegurarAsync(ApplicationDbContext db)
    {
        var rol = await db.Roles.FirstOrDefaultAsync(r => r.Nombre.ToLower() == "empleado");
        if (rol == null)
        {
            rol = new Rol
            {
                Nombre = "Empleado",
                Descripcion = "Gestiona inventario, clientes, ventas, pedidos y reportes sin acceso a precios."
            };
            db.Roles.Add(rol);
            await db.SaveChangesAsync();
        }
        else if (string.IsNullOrWhiteSpace(rol.Descripcion))
        {
            rol.Descripcion = "Gestiona inventario, clientes, ventas, pedidos y reportes sin acceso a precios.";
        }

        foreach (var (modulo, nombre) in PermisosEmpleado)
        {
            var permiso = await db.Permisos.FirstOrDefaultAsync(p => p.Modulo == modulo);
            if (permiso == null)
            {
                permiso = new Permiso { Modulo = modulo, Nombre = nombre };
                db.Permisos.Add(permiso);
                await db.SaveChangesAsync();
            }

            var asignado = await db.RolPermisos
                .AnyAsync(rp => rp.RolId == rol.Id && rp.PermisoId == permiso.Id);
            if (!asignado)
                db.RolPermisos.Add(new RolPermiso { RolId = rol.Id, PermisoId = permiso.Id });
        }

        var permisosPermitidos = await db.Permisos
            .Where(p => PermisosEmpleado.Select(x => x.Modulo).Contains(p.Modulo))
            .Select(p => p.Id)
            .ToListAsync();
        var asignacionesNoPermitidas = await db.RolPermisos
            .Where(rp => rp.RolId == rol.Id && !permisosPermitidos.Contains(rp.PermisoId))
            .ToListAsync();
        db.RolPermisos.RemoveRange(asignacionesNoPermitidas);

        await db.SaveChangesAsync();
    }
}
