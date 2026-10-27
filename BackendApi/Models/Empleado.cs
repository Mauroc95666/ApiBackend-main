using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("Empleado")]
    public class Empleado
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("UsuarioId")]
        public int UsuarioId { get; set; }

        public Usuario Usuario { get; set; } = null!;

        [Column("SucursalId")]
        public int SucursalId { get; set; }

        public Sucursal Sucursal { get; set; } = null!;

        [Column("Cargo")]
        public string Cargo { get; set; } = string.Empty;

        [Column("Salario")]
        public decimal Salario { get; set; }
    }
}