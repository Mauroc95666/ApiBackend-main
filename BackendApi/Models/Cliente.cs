using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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

        // Se fija el nombre con que viaja en el JSON. Sin esto, .NET lo
        // convertia a "cI_NIT" y el frontend quedaba atado a esa rareza.
        [Column("CI_NIT")]
        [JsonPropertyName("ciNit")]
        public string? CI_NIT { get; set; }

        [Column("Telefono")]
        public string? Telefono { get; set; }

        [Column("Email")]
        public string? Email { get; set; }

        // Dirección del cliente (para el delivery / registro de la compra).
        [Column("Direccion")]
        public string? Direccion { get; set; }
    }
}