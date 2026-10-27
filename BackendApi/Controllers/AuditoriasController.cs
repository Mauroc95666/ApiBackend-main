using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditoriasController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AuditoriasController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerAuditorias()
    {
        var auditorias = await _context.Auditorias.ToListAsync();
        return Ok(auditorias);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerAuditoriaPorId(int id)
    {
        var auditoria = await _context.Auditorias.FindAsync(id);

        if (auditoria == null)
            return NotFound(new { mensaje = $"Auditoría con id {id} no encontrada" });

        return Ok(auditoria);
    }

    [HttpPost]
    public async Task<IActionResult> CrearAuditoria([FromBody] Auditoria nuevaAuditoria)
    {
        if (nuevaAuditoria == null)
            return BadRequest(new { mensaje = "Los datos de la auditoría son requeridos" });

        _context.Auditorias.Add(nuevaAuditoria);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerAuditoriaPorId), new { id = nuevaAuditoria.Id }, nuevaAuditoria);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarAuditoria(int id, [FromBody] Auditoria auditoriaActualizada)
    {
        var auditoria = await _context.Auditorias.FindAsync(id);

        if (auditoria == null)
            return NotFound(new { mensaje = $"Auditoría con id {id} no encontrada" });

        auditoria.UsuarioId = auditoriaActualizada.UsuarioId;
        auditoria.Accion = auditoriaActualizada.Accion;
        auditoria.TablaAfectada = auditoriaActualizada.TablaAfectada;
        auditoria.Fecha = auditoriaActualizada.Fecha;
        auditoria.Detalles = auditoriaActualizada.Detalles;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Auditoría actualizada correctamente", auditoria });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarAuditoria(int id)
    {
        var auditoria = await _context.Auditorias.FindAsync(id);

        if (auditoria == null)
            return NotFound(new { mensaje = $"Auditoría con id {id} no encontrada" });

        _context.Auditorias.Remove(auditoria);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Auditoría {id} eliminada correctamente" });
    }
}