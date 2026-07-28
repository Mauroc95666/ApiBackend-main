using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("Emprendimiento")]
    public class Emprendimiento
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("NIT_RUC")]
        public string NIT_RUC { get; set; } = string.Empty;

        [Column("Direccion")]
        public string? Direccion { get; set; }

        [Column("Telefono")]
        public string? Telefono { get; set; }

        [Column("Email")]
        public string? Email { get; set; }
    }
}