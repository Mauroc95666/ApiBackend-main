using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("Categoria")]
    public class Categoria
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("EmprendimientoId")]
        public int EmprendimientoId { get; set; }

        public Emprendimiento Emprendimiento { get; set; } = null!;

        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("Descripcion")]
        public string? Descripcion { get; set; }
    }
}