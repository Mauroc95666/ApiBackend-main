using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("EmprendimientoId")]
        public int EmprendimientoId { get; set; }

        public Emprendimiento Emprendimiento { get; set; } = null!;

        [Column("RolId")]
        public int RolId { get; set; }

        public Rol Rol { get; set; } = null!;

        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Column("PasswordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("Activo")]
        public bool Activo { get; set; }
    }
}