using System.ComponentModel.DataAnnotations;

namespace SICOP.DTOs
{
    // Define los datos que pueden actualizarse de un parámetro.
    public class ActualizarParametroSistemaDto
    {
        // Clave única del parámetro.
        [Required(ErrorMessage = "La clave es obligatoria.")]
        [StringLength(100)]
        public string Clave { get; set; } = string.Empty;

        // Valor del parámetro.
        [Required(ErrorMessage = "El valor es obligatorio.")]
        [StringLength(500)]
        public string Valor { get; set; } = string.Empty;

        // Descripción opcional.
        [StringLength(250)]
        public string? Descripcion { get; set; }
    }
}