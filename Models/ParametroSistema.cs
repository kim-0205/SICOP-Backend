
using System.ComponentModel.DataAnnotations;

namespace SICOP.Models
{
    // Representa las configuraciones generales del sistema.
    public class ParametroSistema
    {
        // Llave primaria del parámetro.
        [Key]
        public int ParametroSistemaId { get; set; }

        // Identificador único del parámetro.
        [Required(ErrorMessage = "La clave del parámetro es obligatoria.")]
        [StringLength(100)]
        public string Clave { get; set; } = string.Empty;

        // Valor almacenado para el parámetro.
        [Required(ErrorMessage = "El valor del parámetro es obligatorio.")]
        [StringLength(500)]
        public string Valor { get; set; } = string.Empty;

        // Descripción opcional del parámetro.
        [StringLength(250)]
        public string? Descripcion { get; set; }
    }
}
