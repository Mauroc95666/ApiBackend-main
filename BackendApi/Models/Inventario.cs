using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("Inventario")]
    public class Inventario
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("ProductoId")]
        public int ProductoId { get; set; }

        public Producto Producto { get; set; } = null!;

        [Column("SucursalId")]
        public int SucursalId { get; set; }

        public Sucursal Sucursal { get; set; } = null!;

        [Column("StockActual")]
        public int StockActual { get; set; }

        [Column("StockMinimo")]
        public int? StockMinimo { get; set; }
    }
}