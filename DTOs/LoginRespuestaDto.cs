namespace SICOP.DTOs
{
    // Define los datos que devuelve la API al iniciar sesión.
    public class LoginRespuestaDto
    {
        // Token JWT que utilizará el usuario para autenticarse.
        public string Token { get; set; } = string.Empty;

        // Fecha y hora de vencimiento del token en UTC.
        public DateTime Expiracion { get; set; }

        // Identificador del usuario autenticado.
        public int UsuarioId { get; set; }

        // Nombre del usuario.
        public string Nombre { get; set; } = string.Empty;

        // Correo electrónico del usuario.
        public string Correo { get; set; } = string.Empty;

        // Rol asignado al usuario.
        public int RolId { get; set; }
    }
}