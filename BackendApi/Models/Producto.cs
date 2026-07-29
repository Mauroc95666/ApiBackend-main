using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("Producto")]
    public class Producto
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("EmprendimientoId")]
        public int? EmprendimientoId { get; set; }

        [Column("CategoriaId")]
        public int? CategoriaId { get; set; }

        [Column("ProveedorId")]
        public int? ProveedorId { get; set; }

        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("Precio")]
        public decimal Precio { get; set; }

        [Column("Stock")]
        public int? Stock { get; set; }
    }
}