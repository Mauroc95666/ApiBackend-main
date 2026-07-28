using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("Promocion")]
    public class Promocion
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("EmprendimientoId")]
        public int EmprendimientoId { get; set; }

        public Emprendimiento Emprendimiento { get; set; } = null!;

        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("Descuento")]
        public decimal Descuento { get; set; }

        [Column("FechaInicio")]
        public DateTime FechaInicio { get; set; }

        [Column("FechaFin")]
        public DateTime FechaFin { get; set; }
    }
}