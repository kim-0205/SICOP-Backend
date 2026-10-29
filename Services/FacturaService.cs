
using Microsoft.EntityFrameworkCore;
using SICOP.Contexts;

namespace SICOP.Services
{
    // Servicio encargado de calcular los saldos de las facturas.
    public class FacturaService
    {
        // Acceso a nuestra base de datos.
        private readonly ApplicationDbContext _context;

        // Constructor: recibe el contexto mediante inyección de dependencias.
        public FacturaService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Calcula cuánto se ha pagado de una factura.
        // Calcula únicamente los pagos válidos de una factura.
        public async Task<decimal> ObtenerTotalPagado(int facturaId)
        {
            // Suma los pagos que no han sido anulados.
            return await _context.Pagos
                .Where(p => p.FacturaCompraId == facturaId
                         && !p.Anulado)
                .SumAsync(p => p.Monto);
        }

        // Calcula cuánto dinero falta por pagar.
        public async Task<decimal> ObtenerSaldoPendiente(int facturaId)
        {
            // Busca la factura en la base de datos.
            var factura = await _context.FacturasCompra
                .FindAsync(facturaId);

            // Si no existe, devuelve un error.
            if (factura == null)
            {
                throw new KeyNotFoundException(
                    "La factura no existe.");
            }

            // Obtiene la suma de todos los abonos.
            decimal totalPagado = await ObtenerTotalPagado(facturaId);

            // Calcula el saldo pendiente.
            return factura.Total - totalPagado;
        }

        // Determina el estado actual de una factura.
        public async Task<string> ObtenerEstadoFactura(int facturaId)
        {
            // Busca la factura.
            var factura = await _context.FacturasCompra
                .FindAsync(facturaId);

            if (factura == null)
            {
                throw new KeyNotFoundException(
                    "La factura no existe.");
            }


            // Si la factura está anulada, devuelve su estado.
            if (factura.Anulada)
            {
                return "Anulada";
            }


            // Calcula el total pagado.
            decimal totalPagado = await ObtenerTotalPagado(facturaId);

            // Determina el estado según los abonos.
            if (totalPagado <= 0)
                return "Pendiente";

            if (totalPagado >= factura.Total)
                return "Pagada";

            return "Parcialmente pagada";
        }
    }
}
