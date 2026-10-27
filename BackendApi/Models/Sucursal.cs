using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("Sucursal")]
    public class Sucursal
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("EmprendimientoId")]
        public int EmprendimientoId { get; set; }

        public Emprendimiento Emprendimiento { get; set; } = null!;

        [Column("CiudadId")]
        public int CiudadId { get; set; }

        public Ciudad Ciudad { get; set; } = null!;

        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("Direccion")]
        public string Direccion { get; set; } = string.Empty;

        [Column("Telefono")]
        public string? Telefono { get; set; }
    }
}