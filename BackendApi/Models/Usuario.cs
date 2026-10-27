using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BackendApi.Models
{
    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("EmprendimientoId")]
        public int EmprendimientoId { get; set; }

        public Emprendimiento Emprendimiento { get; set; } = null!;

        [Column("RolId")]
        public int RolId { get; set; }

        public Rol Rol { get; set; } = null!;

        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// La contraseña hasheada. JsonIgnore la deja fuera de cualquier
        /// respuesta: si alguna entidad arrastra su Usuario al JSON, el hash
        /// no viaja igual.
        /// </summary>
        [Column("PasswordHash")]
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("Activo")]
        public bool Activo { get; set; }

        /// <summary>
        /// Cuenta interna del sistema: el super-usuario de arranque y el
        /// usuario con el que la tienda publica registra sus ventas. No se
        /// listan en la pantalla de Usuarios ni se pueden eliminar.
        /// </summary>
        [Column("EsSistema")]
        public bool EsSistema { get; set; }

        /// <summary>
        /// Si esta en true, el proximo ingreso obliga a cambiar la clave
        /// antes de poder usar el sistema.
        /// </summary>
        [Column("DebeCambiarPassword")]
        public bool DebeCambiarPassword { get; set; }
    }
}