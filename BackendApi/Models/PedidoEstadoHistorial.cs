using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    /// <summary>
    /// Una fila por cada cambio de estado de un pedido. Antes el pedido solo
    /// guardaba su estado actual y el anterior se perdia, asi que no habia
    /// forma de saber como llego hasta ahi ni cuanto tardo cada paso.
    /// Esta tabla es la que permite medir el tiempo de servicio (HU-12).
    /// </summary>
    [Table("PedidoEstadoHistorial")]
    public class PedidoEstadoHistorial
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("PedidoId")]
        public int PedidoId { get; set; }

        public Pedido Pedido { get; set; } = null!;

        /// <summary>Estado del que venia. Va vacio en la primera fila.</summary>
        [Column("EstadoAnterior")]
        public string? EstadoAnterior { get; set; }

        [Column("Estado")]
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// Quien hizo el cambio. Admite vacio porque los pedidos que ya
        /// existian antes de esta tabla no tienen autor registrado.
        /// </summary>
        [Column("UsuarioId")]
        public int? UsuarioId { get; set; }

        public Usuario? Usuario { get; set; }

        [Column("Fecha")]
        public DateTime Fecha { get; set; }

        [Column("Observacion")]
        public string? Observacion { get; set; }
    }
}
