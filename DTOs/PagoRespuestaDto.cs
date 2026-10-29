
namespace SICOP.DTOs
{
    // Datos que la API devolverá al consultar o registrar un pago.
    public class PagoRespuestaDto
    {
        public int PagoId { get; set; }

        public int FacturaCompraId { get; set; }

        public DateTime FechaPago { get; set; }

        public decimal Monto { get; set; }

        public string MetodoPago { get; set; } = string.Empty;

        public string? Referencia { get; set; }

        public string? Observaciones { get; set; }

        public bool Anulado { get; set; }

        public DateTime? FechaAnulacion { get; set; }

        public string? MotivoAnulacion { get; set; }
    }
}
