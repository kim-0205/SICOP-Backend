using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SICOP.Models
{
    // Representa a los usuarios que acceden al sistema.
    public class Usuario
    {
        // Llave primaria del usuario.
        [Key]
        public int UsuarioId { get; set; }

        // Nombre completo del usuario.
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        // Correo utilizado para iniciar sesión.
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [StringLength(150)]
        public string Correo { get; set; } = string.Empty;

        // Hash seguro de la contraseña.
        // Nunca almacenaremos contraseñas en texto plano.
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // Llave foránea del rol asignado.
        [Required]
        public int RolId { get; set; }

        // Indica si el usuario puede acceder al sistema.
        public bool Activo { get; set; } = true;

        // Relación: cada usuario pertenece a un rol.
        [ForeignKey(nameof(RolId))]
        public Rol Rol { get; set; } = null!;
    }
}