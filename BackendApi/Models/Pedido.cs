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

        /// <summary>Fecha planificada de entrega. La carga una persona.</summary>
        [Column("FechaEntrega")]
        public DateTime? FechaEntrega { get; set; }

        /// <summary>
        /// Cuando nacio el pedido. La pone el sistema, no se edita.
        /// </summary>
        [Column("FechaCreacion")]
        public DateTime? FechaCreacion { get; set; }

        /// <summary>
        /// Cuando el pedido paso de verdad a ENTREGADO. Junto con
        /// FechaCreacion es lo que permite medir cuanto tardo.
        /// </summary>
        [Column("FechaEntregaReal")]
        public DateTime? FechaEntregaReal { get; set; }
    }
}