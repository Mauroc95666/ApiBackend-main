using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("DetalleVenta")]
    public class DetalleVenta
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("VentaId")]
        public int VentaId { get; set; }

        public Venta Venta { get; set; } = null!;

        [Column("ProductoId")]
        public int ProductoId { get; set; }

        public Producto Producto { get; set; } = null!;

        [Column("Cantidad")]
        public int Cantidad { get; set; }

        [Column("PrecioUnitario")]
        public decimal PrecioUnitario { get; set; }

        [Column("Subtotal")]
        public decimal? Subtotal { get; set; }
    }
}