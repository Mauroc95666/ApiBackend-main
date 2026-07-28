using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("Resena")]
    public class Resena
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("ProductoId")]
        public int ProductoId { get; set; }

        public Producto Producto { get; set; } = null!;

        [Column("ClienteId")]
        public int ClienteId { get; set; }

        public Cliente Cliente { get; set; } = null!;

        [Column("Calificacion")]
        public int? Calificacion { get; set; }

        [Column("Comentario")]
        public string? Comentario { get; set; }

        [Column("Fecha")]
        public DateTime Fecha { get; set; }
    }
}