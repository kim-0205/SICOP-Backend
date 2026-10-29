using System.ComponentModel.DataAnnotations;

namespace SICOP.DTOs
{
    // Define los datos necesarios para registrar un parámetro.
    public class CrearParametroSistemaDto
    {
        // Clave única del parámetro.
        [Required(ErrorMessage = "La clave es obligatoria.")]
        [StringLength(100)]
        public string Clave { get; set; } = string.Empty;

        // Valor que tendrá el parámetro.
        [Required(ErrorMessage = "El valor es obligatorio.")]
        [StringLength(500)]
        public string Valor { get; set; } = string.Empty;

        // Descripción opcional del parámetro.
        [StringLength(250)]
        public string? Descripcion { get; set; }
    }
}