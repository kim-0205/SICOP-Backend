
namespace SICOP.DTOs
{
    // Representa las estadísticas generales
    // que se mostrarán en el Dashboard de SICOP.
    public class DashboardResumenDto
    {
        // Cantidad de proveedores activos.
        public int TotalProveedores { get; set; }

        // Cantidad de facturas vigentes registradas.
        public int TotalFacturas { get; set; }

        // Suma de los importes de las facturas vigentes.
        public decimal MontoFacturado { get; set; }

        // Total de pagos válidos realizados.
        public decimal TotalPagado { get; set; }

        // Cantidad total que todavía se debe.
        public decimal SaldoPendiente { get; set; }

        // Cantidad de facturas completamente pagadas.
        public int FacturasPagadas { get; set; }

        // Cantidad de facturas con saldo pendiente.
        public int FacturasPendientes { get; set; }

        // Cantidad de facturas vencidas con deuda.
        public int FacturasVencidas { get; set; }

        // Suma de los saldos de facturas vencidas.
        public decimal MontoVencido { get; set; }

        // Fecha en que se generaron las estadísticas.
        public DateTime FechaConsulta { get; set; }
    }
}
