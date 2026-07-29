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

        categoria.Nombre = categoriaActualizada.Nombre;
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