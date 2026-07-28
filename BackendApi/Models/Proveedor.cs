using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // <-- Asegúrate de incluir este namespace

namespace BackendApi.Models
{
    [Table("Proveedor")] // <-- Esto le dice a EF que la tabla se llama Proveedor en SQL
    public class Proveedor
    {
        public int Id { get; set; }
        public int EmprendimientoId { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Contacto { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
    }
}