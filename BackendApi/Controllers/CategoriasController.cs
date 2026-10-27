using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CategoriasController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerCategorias()
    {
        var categorias = await _context.Categorias.ToListAsync();
        return Ok(categorias);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerCategoriaPorId(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);

        if (categoria == null)
            return NotFound(new { mensaje = $"Categoría con id {id} no encontrada" });

        return Ok(categoria);
    }

    [HttpPost]
    public async Task<IActionResult> CrearCategoria([FromBody] Categoria nuevaCategoria)
    {
        if (nuevaCategoria == null)
            return BadRequest(new { mensaje = "Los datos de la categoría son requeridos" });

        if (string.IsNullOrWhiteSpace(nuevaCategoria.Nombre))
            return BadRequest(new { mensaje = "El nombre de la categoría es requerido" });

        nuevaCategoria.Nombre = nuevaCategoria.Nombre.Trim();

        // Nombre repetido: dos categorias con el mismo nombre confunden al
        // cargar un producto y ensucian el filtro del catalogo.
        var yaExiste = await _context.Categorias
            .AnyAsync(c => c.Nombre.ToLower() == nuevaCategoria.Nombre.ToLower());

        if (yaExiste)
            return BadRequest(new { mensaje = $"Ya existe una categoría llamada '{nuevaCategoria.Nombre}'" });

        _context.Categorias.Add(nuevaCategoria);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerCategoriaPorId), new { id = nuevaCategoria.Id }, nuevaCategoria);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarCategoria(int id, [FromBody] Categoria categoriaActualizada)
    {
        var categoria = await _context.Categorias.FindAsync(id);

        if (categoria == null)
            return NotFound(new { mensaje = $"Categoría con id {id} no encontrada" });

        if (string.IsNullOrWhiteSpace(categoriaActualizada.Nombre))
            return BadRequest(new { mensaje = "El nombre de la categoría es requerido" });

        var nombre = categoriaActualizada.Nombre.Trim();

        var repetida = await _context.Categorias
            .AnyAsync(c => c.Id != id && c.Nombre.ToLower() == nombre.ToLower());

        if (repetida)
            return BadRequest(new { mensaje = $"Ya existe una categoría llamada '{nombre}'" });

        categoria.Nombre = nombre;
        categoria.Descripcion = categoriaActualizada.Descripcion;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Categoría actualizada correctamente", categoria });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarCategoria(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);

        if (categoria == null)
            return NotFound(new { mensaje = $"Categoría con id {id} no encontrada" });

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Categoría {id} eliminada correctamente" });
    }
}