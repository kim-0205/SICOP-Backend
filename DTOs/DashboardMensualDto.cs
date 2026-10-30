
namespace SICOP.DTOs
{
    // Representa las estadísticas financieras
    // correspondientes a un mes del Dashboard.
    public class DashboardMensualDto
    {
        // Año al que pertenecen los datos.
        public int Anio { get; set; }

        // Número del mes (1 al 12).
        public int Mes { get; set; }

        // Nombre del mes para mostrar en las gráficas.
        public string NombreMes { get; set; } = string.Empty;

        // Importe de las facturas emitidas durante el mes.
        public decimal TotalFacturado { get; set; }

        // Importe de los pagos válidos realizados durante el mes.
        public decimal TotalPagado { get; set; }

        // Cantidad de facturas emitidas durante el mes.
        public int CantidadFacturas { get; set; }

        // Cantidad de pagos válidos realizados durante el mes.
        public int CantidadPagos { get; set; }
    }
}
