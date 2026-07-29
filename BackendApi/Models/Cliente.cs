using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("Cliente")]
    public class Cliente
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("EmprendimientoId")]
        public int EmprendimientoId { get; set; }

        public Emprendimiento Emprendimiento { get; set; } = null!;

        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("CI_NIT")]
        public string? CI_NIT { get; set; }

        [Column("Telefono")]
        public string? Telefono { get; set; }

        [Column("Email")]
        public string? Email { get; set; }
    }
}