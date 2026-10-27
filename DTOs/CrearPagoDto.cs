
using System.ComponentModel.DataAnnotations;

namespace SICOP.DTOs
{
    // Datos necesarios para registrar un pago.
    public class CrearPagoDto
    {
        // Identifica la factura que recibirá el pago.
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una factura válida.")]
        public int FacturaCompraId { get; set; }

        // Cantidad que se pagará.
        [Range(typeof(decimal), "0.01", "9999999999999999.99",
            ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Monto { get; set; }

        // Método utilizado para realizar el pago.
        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        public string MetodoPago { get; set; } = string.Empty;

        // Número de transferencia, cheque u otra referencia.
        public string? Referencia { get; set; }

        // Información adicional del pago.
        public string? Observaciones { get; set; }
    }
}
