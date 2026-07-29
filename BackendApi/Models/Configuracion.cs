using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("Configuracion")]
    public class Configuracion
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("EmprendimientoId")]
        public int EmprendimientoId { get; set; }

        public Emprendimiento Emprendimiento { get; set; } = null!;

        [Column("Clave")]
        public string Clave { get; set; } = string.Empty;

        [Column("Valor")]
        public string Valor { get; set; } = string.Empty;
    }
}