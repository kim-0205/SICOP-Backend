namespace SICOP.DTOs
{
    // Define los datos que devolverá la API al consultar un parámetro.
    public class ParametroSistemaRespuestaDto
    {
        // Identificador único del parámetro.
        public int ParametroSistemaId { get; set; }

        // Clave del parámetro.
        public string Clave { get; set; } = string.Empty;

        // Valor almacenado.
        public string Valor { get; set; } = string.Empty;

        // Descripción opcional.
        public string? Descripcion { get; set; }
    }
}