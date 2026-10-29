using System.ComponentModel.DataAnnotations;

namespace SICOP.Models
{
    // Representa los roles de seguridad del sistema.
    public class Rol
    {
        // Llave primaria del rol.
        [Key]
        public int RolId { get; set; }

        // Nombre obligatorio del rol.
        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        // Descripción opcional del rol.
        [StringLength(200)]
        public string? Descripcion { get; set; }

        // Relación: un rol puede tener varios usuarios.
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}