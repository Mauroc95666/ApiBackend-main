using Microsoft.AspNetCore.Mvc;
using BackendApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProveedoresController : ControllerBase
{
    private static List<Proveedor> _proveedores = new List<Proveedor>
    {
        new Proveedor
        {
            Id = 1,
            NombreComercial = "Distribuidora Harinas del Sur",
            Rubro = "Materia Prima Básica",
            InsumoPrincipal = "Harina 000 y 0000 (Sacos 50kg)",
            Contacto = "78912345",
            Ciudad = "Santa Cruz"
        },
        new Proveedor
        {
            Id = 2,
            NombreComercial = "Lácteos & Crema Bolivia",
            Rubro = "Insumos Refrigerados",
            InsumoPrincipal = "Mantequilla sin sal y Leche Entera",
            Contacto = "71029384",
            Ciudad = "Santa Cruz"
        },
        new Proveedor
        {
            Id = 3,
            NombreComercial = "Insumos Pasteleros El Molino",
            Rubro = "Repostería y Decoración",
            InsumoPrincipal = "Levadura Seca, Chispas de Chocolate y Esencias",
            Contacto = "76543210",
            Ciudad = "Santa Cruz"
        },
        new Proveedor
        {
            Id = 4,
            NombreComercial = "Empaques & Cajas Dulces",
            Rubro = "Embalaje",
            InsumoPrincipal = "Cajas para Tortas y Bolsas de Papel Kraft",
            Contacto = "72233445",
            Ciudad = "Santa Cruz"
        }
    };

    [HttpGet]
    public IActionResult ObtenerProveedores()
    {
        return Ok(_proveedores);
    }

    [HttpGet("{id}")]
    public IActionResult ObtenerProveedorPorId(int id)
    {
        var proveedor = _proveedores.FirstOrDefault(p => p.Id == id);
        if (proveedor == null)
            return NotFound(new { mensaje = $"No se encontró el proveedor con ID {id}" });

        return Ok(proveedor);
    }

    [HttpPost]
    public IActionResult CrearProveedor([FromBody] Proveedor nuevoProveedor)
    {
        nuevoProveedor.Id = _proveedores.Any() ? _proveedores.Max(p => p.Id) + 1 : 1;
        _proveedores.Add(nuevoProveedor);

        return CreatedAtAction(nameof(ObtenerProveedorPorId), new { id = nuevoProveedor.Id }, nuevoProveedor);
    }

    [HttpPut("{id}")]
    public IActionResult ActualizarProveedor(int id, [FromBody] Proveedor proveedorActualizado)
    {
        var proveedorExistente = _proveedores.FirstOrDefault(p => p.Id == id);
        if (proveedorExistente == null)
            return NotFound(new { mensaje = $"No se encontró el proveedor con ID {id}" });

        proveedorExistente.NombreComercial = proveedorActualizado.NombreComercial;
        proveedorExistente.Rubro = proveedorActualizado.Rubro;
        proveedorExistente.InsumoPrincipal = proveedorActualizado.InsumoPrincipal;
        proveedorExistente.Contacto = proveedorActualizado.Contacto;
        proveedorExistente.Ciudad = proveedorActualizado.Ciudad;

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult EliminarProveedor(int id)
    {
        var proveedor = _proveedores.FirstOrDefault(p => p.Id == id);
        if (proveedor == null)
            return NotFound(new { mensaje = $"No se encontró el proveedor con ID {id}" });

        _proveedores.Remove(proveedor);
        return NoContent();
    }
}
