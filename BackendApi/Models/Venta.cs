using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("Venta")]
    public class Venta
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("SucursalId")]
        public int SucursalId { get; set; }

        public Sucursal Sucursal { get; set; } = null!;

        [Column("ClienteId")]
        public int ClienteId { get; set; }

        public Cliente Cliente { get; set; } = null!;

        [Column("UsuarioId")]
        public int UsuarioId { get; set; }

        public Usuario Usuario { get; set; } = null!;

        [Column("MetodoPagoId")]
        public int MetodoPagoId { get; set; }

        public MetodoPago MetodoPago { get; set; } = null!;

        [Column("Fecha")]
        public DateTime Fecha { get; set; }

        [Column("Total")]
        public decimal Total { get; set; }

        public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();

        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}