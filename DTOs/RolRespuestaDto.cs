namespace SICOP.DTOs
{
    // Define los datos que devolverá la API al consultar un rol.
    public class RolRespuestaDto
    {
        // Identificador del rol.
        public int RolId { get; set; }

        // Nombre del rol.
        public string Nombre { get; set; } = string.Empty;

        // Descripción opcional del rol.
        public string? Descripcion { get; set; }
    }
}