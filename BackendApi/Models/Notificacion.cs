using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("Notificacion")]
    public class Notificacion
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("UsuarioId")]
        public int UsuarioId { get; set; }

        public Usuario Usuario { get; set; } = null!;

        [Column("Mensaje")]
        public string Mensaje { get; set; } = string.Empty;

        [Column("Leido")]
        public bool? Leido { get; set; }

        [Column("Fecha")]
        public DateTime? Fecha { get; set; }
    }
}