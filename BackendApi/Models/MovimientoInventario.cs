using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("MovimientoInventario")]
    public class MovimientoInventario
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("InventarioId")]
        public int InventarioId { get; set; }

        public Inventario Inventario { get; set; } = null!;

        [Column("UsuarioId")]
        public int UsuarioId { get; set; }

        public Usuario Usuario { get; set; } = null!;

        [Column("Tipo")]
        public string Tipo { get; set; } = string.Empty;

        [Column("Cantidad")]
        public int Cantidad { get; set; }

        [Column("Fecha")]
        public DateTime Fecha { get; set; }

        [Column("Motivo")]
        public string? Motivo { get; set; }
    }
}