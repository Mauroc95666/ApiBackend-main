using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendApi.Models
{
    [Table("Permiso")]
    public class Permiso
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("Modulo")]
        public string Modulo { get; set; } = string.Empty;
    }
}