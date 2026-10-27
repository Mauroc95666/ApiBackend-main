using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("Auditoria")]
    public class Auditoria
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("UsuarioId")]
        public int? UsuarioId { get; set; }

        public Usuario? Usuario { get; set; }

        [Column("Accion")]
        public string Accion { get; set; } = string.Empty;

        [Column("TablaAfectada")]
        public string TablaAfectada { get; set; } = string.Empty;

        [Column("Fecha")]
        public DateTime? Fecha { get; set; }

        [Column("Detalles")]
        public string? Detalles { get; set; }
    }
}