using BackendApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Services;

public static class InventarioInicializador
{
    public static async Task AsegurarFilasAsync(ApplicationDbContext context)
    {
        var sucursales = await context.Sucursales
            .OrderBy(s => s.Id)
            .ToListAsync();

        if (sucursales.Count == 0)
            return;

        var productos = await context.Productos
            .OrderBy(p => p.Id)
            .ToListAsync();
        var inventarios = await context.Inventarios.ToListAsync();
        var clavesExistentes = inventarios
            .Select(i => (i.ProductoId, i.SucursalId))
            .ToHashSet();
        var nuevos = new List<Inventario>();

        foreach (var producto in productos)
        {
            var yaTieneInventario = inventarios.Any(i => i.ProductoId == producto.Id);

            foreach (var sucursal in sucursales)
            {
                if (!clavesExistentes.Add((producto.Id, sucursal.Id)))
                    continue;

                nuevos.Add(new Inventario
                {
                    ProductoId = producto.Id,
                    SucursalId = sucursal.Id,
                    StockActual = !yaTieneInventario && sucursal.Id == sucursales[0].Id
                        ? producto.Stock ?? 0
                        : 0,
                    StockMinimo = 0
                });
            }
        }

        if (nuevos.Count == 0)
            return;

        context.Inventarios.AddRange(nuevos);
        await context.SaveChangesAsync();
    }
}
