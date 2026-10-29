namespace SICOP.DTOs
{
    // Define los datos que devolverá la API al consultar un usuario.
    public class UsuarioRespuestaDto
    {
        // Identificador del usuario.
        public int UsuarioId { get; set; }

        // Nombre completo.
        public string Nombre { get; set; } = string.Empty;

        // Correo electrónico.
        public string Correo { get; set; } = string.Empty;

        // Identificador del rol asignado.
        public int RolId { get; set; }

        // Indica si el usuario está activo.
        public bool Activo { get; set; }
    }
}