
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SICOP.Contexts;
using SICOP.DTOs;

namespace SICOP.Controllers
{
    // Controlador encargado de generar reportes de cuentas por pagar.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador General,Operador")]
    public class ReportesController : ControllerBase
    {
        // Acceso a SQL Server mediante Entity Framework Core.
        private readonly ApplicationDbContext _context;

        // Constructor con inyección de dependencias.
        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Método privado que prepara la información de las facturas.
        // No modifica registros de la base de datos.
        private async Task<List<ReporteFacturaDto>> ObtenerFacturasReporte()
        {
            // Utilizamos la fecha actual para identificar vencimientos.
            DateTime hoy = DateTime.Today;

            // Consultamos las facturas vigentes, sus proveedores
            // y la suma de los pagos que no están anulados.
            var datos = await _context.FacturasCompra
                .AsNoTracking()
                .Where(f => !f.Anulada)
                .Select(f => new
                {
                    f.FacturaCompraId,
                    f.NumeroFactura,
                    f.ProveedorId,
                    NombreProveedor = f.Proveedor != null
                        ? f.Proveedor.Nombre
                        : "",
                    f.FechaEmision,
                    f.FechaVencimiento,
                    f.Total,

                    TotalPagado = f.Pagos
                        .Where(p => !p.Anulado)
                        .Sum(p => (decimal?)p.Monto) ?? 0m
                })
                .ToListAsync();

            // Transformamos los datos en DTOs para el frontend.
            var reporte = datos.Select(f =>
            {
                decimal saldo = f.Total - f.TotalPagado;

                // Los días vencidos solo se calculan
                // cuando existe saldo pendiente.
                int diasVencidos = saldo > 0
                    && f.FechaVencimiento.Date < hoy
                    ? (hoy - f.FechaVencimiento.Date).Days
                    : 0;

                // Determinamos el estado financiero.
                string estado;

                if (saldo <= 0)
                {
                    estado = "Pagada";
                }
                else if (diasVencidos > 0)
                {
                    estado = "Vencida";
                }
                else if (f.TotalPagado > 0)
                {
                    estado = "Parcial";
                }
                else
                {
                    estado = "Pendiente";
                }

                return new ReporteFacturaDto
                {
                    FacturaCompraId = f.FacturaCompraId,
                    NumeroFactura = f.NumeroFactura,
                    ProveedorId = f.ProveedorId,
                    NombreProveedor = f.NombreProveedor,
                    FechaEmision = f.FechaEmision,
                    FechaVencimiento = f.FechaVencimiento,
                    Total = f.Total,
                    TotalPagado = f.TotalPagado,
                    SaldoPendiente = saldo,
                    Estado = estado,
                    DiasVencidos = diasVencidos
                };
            }).ToList();

            return reporte;
        }

        // GET: api/Reportes/facturas-pendientes
        // Incluye facturas con saldo pendiente,
        // aunque su fecha de vencimiento ya haya pasado.
        [HttpGet("facturas-pendientes")]
        public async Task<ActionResult<List<ReporteFacturaDto>>>
            GetFacturasPendientes()
        {
            var facturas = await ObtenerFacturasReporte();

            return Ok(facturas
                .Where(f => f.SaldoPendiente > 0)
                .OrderBy(f => f.FechaVencimiento)
                .ToList());
        }

        // GET: api/Reportes/facturas-pagadas
        // Muestra únicamente facturas totalmente pagadas.
        [HttpGet("facturas-pagadas")]
        public async Task<ActionResult<List<ReporteFacturaDto>>>
            GetFacturasPagadas()
        {
            var facturas = await ObtenerFacturasReporte();

            return Ok(facturas
                .Where(f => f.SaldoPendiente <= 0)
                .OrderByDescending(f => f.FechaEmision)
                .ToList());
        }

        // GET: api/Reportes/facturas-vencidas
        // Muestra facturas con deuda y fecha vencida.
        [HttpGet("facturas-vencidas")]
        public async Task<ActionResult<List<ReporteFacturaDto>>>
            GetFacturasVencidas()
        {
            var facturas = await ObtenerFacturasReporte();

            return Ok(facturas
                .Where(f => f.DiasVencidos > 0)
                .OrderByDescending(f => f.DiasVencidos)
                .ToList());
        }


        // GET: api/Reportes/historial-pagos
        // Consulta todos los pagos registrados en SICOP.
        // Incluye los pagos válidos y anulados.
        [HttpGet("historial-pagos")]
        public async Task<ActionResult<List<ReportePagoDto>>>
            GetHistorialPagos()
        {
            // Consultamos los pagos registrados en SQL Server.
            // AsNoTracking mejora las consultas de solo lectura.
            var pagos = await _context.Pagos
                .AsNoTracking()

                // Ordenamos del pago más reciente al más antiguo.
                .OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.PagoId)

                // Convertimos los datos al DTO del reporte.
                .Select(p => new ReportePagoDto
                {
                    PagoId = p.PagoId,

                    FacturaCompraId = p.FacturaCompraId,

                    // Obtenemos el número de la factura.
                    NumeroFactura = p.FacturaCompra.NumeroFactura,

                    // Obtenemos el proveedor de la factura.
                    ProveedorId = p.FacturaCompra.ProveedorId,

                    NombreProveedor =
                        p.FacturaCompra.Proveedor != null
                            ? p.FacturaCompra.Proveedor.Nombre
                            : "",

                    FechaPago = p.FechaPago,

                    // Conservamos el monto original,
                    // incluso cuando el pago fue anulado.
                    Monto = p.Monto,

                    MetodoPago = p.MetodoPago,

                    Referencia = p.Referencia,

                    Observaciones = p.Observaciones,

                    // Información de anulación.
                    Anulado = p.Anulado,

                    FechaAnulacion = p.FechaAnulacion,

                    MotivoAnulacion = p.MotivoAnulacion
                })
                .ToListAsync();

            // Devolvemos el historial completo al frontend.
            return Ok(pagos);
        }

    }

}
