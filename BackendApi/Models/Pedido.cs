using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("Pedido")]
    public class Pedido
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("VentaId")]
        public int VentaId { get; set; }

        public Venta Venta { get; set; } = null!;

        [Column("Estado")]
        public string Estado { get; set; } = "PENDIENTE";

        [Column("FechaEntrega")]
        public DateTime? FechaEntrega { get; set; }
    }
}