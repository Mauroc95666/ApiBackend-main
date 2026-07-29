using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("Dashboard")]
    public class Dashboard
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("EmprendimientoId")]
        public int EmprendimientoId { get; set; }

        public Emprendimiento Emprendimiento { get; set; } = null!;

        [Column("MetricasJson")]
        public string? MetricasJson { get; set; }

        [Column("UltimaActualizacion")]
        public DateTime? UltimaActualizacion { get; set; }
    }
}