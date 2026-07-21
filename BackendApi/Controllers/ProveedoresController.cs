using Microsoft.AspNetCore.Mvc;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProveedoresController : ControllerBase
{
    [HttpGet]
    public IActionResult ObtenerProveedores()
    {
        var proveedores = new[]
        {
            new
            {
                Id = 1,
                NombreComercial = "Distribuidora Harinas del Sur",
                Rubro = "Materia Prima Básica",
                InsumoPrincipal = "Harina 000 y 0000 (Sacos 50kg)",
                Contacto = "78912345",
                Ciudad = "Santa Cruz"
            },
            new
            {
                Id = 2,
                NombreComercial = "Lácteos & Crema Bolivia",
                Rubro = "Insumos Refrigerados",
                InsumoPrincipal = "Mantequilla sin sal y Leche Entera",
                Contacto = "71029384",
                Ciudad = "Santa Cruz"
            },
            new
            {
                Id = 3,
                NombreComercial = "Insumos Pasteleros El Molino",
                Rubro = "Repostería y Decoración",
                InsumoPrincipal = "Levadura Seca, Chispas de Chocolate y Esencias",
                Contacto = "76543210",
                Ciudad = "Santa Cruz"
            },
            new
            {
                Id = 4,
                NombreComercial = "Empaques & Cajas Dulces",
                Rubro = "Embalaje",
                InsumoPrincipal = "Cajas para Tortas y Bolsas de Papel Kraft",
                Contacto = "72233445",
                Ciudad = "Santa Cruz"
            }
        };
        return Ok(proveedores);
    }

    [HttpGet("{id}")]
    public IActionResult ObtenerProveedorPorId(int id)
    {
        var proveedor = new
        {
            Id = id,
            NombreComercial = "Proveedor de Panadería Ejemplo",
            Rubro = "Insumos Varios",
            InsumoPrincipal = "Materia Prima para Panadería",
            Contacto = "70000000",
            Ciudad = "Santa Cruz"
        };
        return Ok(proveedor);
    }
}