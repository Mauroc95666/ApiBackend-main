using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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

        // Mismo caso que CI_NIT: sin esto viajaba como "nIT_RUC".
        [Column("NIT_RUC")]
        [JsonPropertyName("nitRuc")]
        public string NIT_RUC { get; set; } = string.Empty;

        [Column("Direccion")]
        public string? Direccion { get; set; }

        [Column("Telefono")]
        public string? Telefono { get; set; }

        [Column("Email")]
        public string? Email { get; set; }
    }
}