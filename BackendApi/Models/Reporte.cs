using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("Reporte")]
    public class Reporte
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("EmprendimientoId")]
        public int EmprendimientoId { get; set; }

        public Emprendimiento Emprendimiento { get; set; } = null!;

        [Column("Tipo")]
        public string Tipo { get; set; } = string.Empty;

        [Column("FechaGeneracion")]
        public DateTime? FechaGeneracion { get; set; }

        [Column("Datos")]
        public string? Datos { get; set; }
    }
}