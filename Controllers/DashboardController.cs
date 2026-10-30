
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SICOP.Contexts;
using SICOP.DTOs;

namespace SICOP.Controllers
{
    // Controlador que proporciona las estadísticas
    // generales para el Dashboard de SICOP.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador General,Operador")]
    public class DashboardController : ControllerBase
    {
        // Acceso a la base de datos SQL Server.
        private readonly ApplicationDbContext _context;

        // Inyección de dependencias.
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Dashboard/resumen
        // Obtiene los indicadores financieros generales.
        [HttpGet("resumen")]
        public async Task<ActionResult<DashboardResumenDto>>
            GetResumen()
        {
            // Fecha actual para identificar facturas vencidas.
            DateTime hoy = DateTime.Today;

            // Cuenta únicamente los proveedores activos.
            int totalProveedores = await _context.Proveedores
                .AsNoTracking()
                .CountAsync(p => p.Activo);

            // Consulta las facturas vigentes y sus pagos válidos.
            // Las facturas anuladas no participan en los totales.
            var facturas = await _context.FacturasCompra
                .AsNoTracking()
                .Where(f => !f.Anulada)
                .Select(f => new
                {
                    f.Total,
                    f.FechaVencimiento,

                    // Suma únicamente los pagos no anulados.
                    TotalPagado = f.Pagos
                        .Where(p => !p.Anulado)
                        .Sum(p => (decimal?)p.Monto) ?? 0m
                })
                .ToListAsync();

            // Calcula el importe total facturado.
            decimal montoFacturado = facturas.Sum(f => f.Total);

            // Calcula los pagos válidos realizados.
            decimal totalPagado = facturas.Sum(f => f.TotalPagado);

            // Calcula la deuda pendiente de cada factura.
            var saldos = facturas.Select(f => new
            {
                f.FechaVencimiento,
                Saldo = f.Total - f.TotalPagado
            }).ToList();

            // Suma toda la deuda pendiente.
            decimal saldoPendiente = saldos.Sum(f => f.Saldo);

            // Cuenta las facturas completamente pagadas.
            int facturasPagadas = saldos.Count(f => f.Saldo <= 0);

            // Cuenta las facturas con deuda.
            int facturasPendientes = saldos.Count(f => f.Saldo > 0);

            // Identifica las facturas vencidas que aún tienen deuda.
            var vencidas = saldos
                .Where(f => f.Saldo > 0
                    && f.FechaVencimiento.Date < hoy)
                .ToList();

            // Construye la respuesta del Dashboard.
            var respuesta = new DashboardResumenDto
            {
                TotalProveedores = totalProveedores,
                TotalFacturas = facturas.Count,
                MontoFacturado = montoFacturado,
                TotalPagado = totalPagado,
                SaldoPendiente = saldoPendiente,
                FacturasPagadas = facturasPagadas,
                FacturasPendientes = facturasPendientes,
                FacturasVencidas = vencidas.Count,
                MontoVencido = vencidas.Sum(f => f.Saldo),
                FechaConsulta = DateTime.UtcNow
            };

            // Devuelve las estadísticas al frontend.
            return Ok(respuesta);
        }


        // GET: api/Dashboard/mensual
        // Obtiene la facturación y los pagos agrupados por mes.
        [HttpGet("mensual")]
        public async Task<ActionResult<List<DashboardMensualDto>>>
            GetResumenMensual([FromQuery] int? anio)
        {
            // Si no se proporciona un año, utilizamos el actual.
            int anioConsulta = anio ?? DateTime.Today.Year;

            // Validamos el año solicitado.
            if (anioConsulta < 2000 || anioConsulta > 9998)
            {
                return BadRequest(new
                {
                    mensaje = "El año consultado no es válido."
                });
            }

            // Establecemos el período de consulta.
            DateTime inicio = new DateTime(anioConsulta, 1, 1);
            DateTime fin = inicio.AddYears(1);

            // Consultamos las facturas vigentes emitidas en el año.
            // Las facturas anuladas no participan en las estadísticas.
            var facturas = await _context.FacturasCompra
                .AsNoTracking()
                .Where(f => !f.Anulada
                    && f.FechaEmision >= inicio
                    && f.FechaEmision < fin)
                .GroupBy(f => f.FechaEmision.Month)
                .Select(g => new
                {
                    Mes = g.Key,
                    Total = g.Sum(f => f.Total),
                    Cantidad = g.Count()
                })
                .ToListAsync();

            // Consultamos los pagos válidos realizados en el año.
            // También excluimos pagos de facturas anuladas.
            var pagos = await _context.Pagos
                .AsNoTracking()
                .Where(p => !p.Anulado
                    && !p.FacturaCompra.Anulada
                    && p.FechaPago >= inicio
                    && p.FechaPago < fin)
                .GroupBy(p => p.FechaPago.Month)
                .Select(g => new
                {
                    Mes = g.Key,
                    Total = g.Sum(p => p.Monto),
                    Cantidad = g.Count()
                })
                .ToListAsync();

            // Nombres de los meses para las gráficas de Vue.
            string[] nombresMeses =
            {
                "Enero", "Febrero", "Marzo", "Abril",
                "Mayo", "Junio", "Julio", "Agosto",
                "Septiembre", "Octubre", "Noviembre", "Diciembre"
            };

            // Generamos los doce meses, incluso cuando
            // no existen movimientos registrados.
            var resultado = Enumerable.Range(1, 12)
                .Select(mes =>
                {
                    var datosFactura = facturas
                        .FirstOrDefault(f => f.Mes == mes);

                    var datosPago = pagos
                        .FirstOrDefault(p => p.Mes == mes);

                    return new DashboardMensualDto
                    {
                        Anio = anioConsulta,
                        Mes = mes,
                        NombreMes = nombresMeses[mes - 1],

                        TotalFacturado = datosFactura?.Total ?? 0m,
                        TotalPagado = datosPago?.Total ?? 0m,

                        CantidadFacturas = datosFactura?.Cantidad ?? 0,
                        CantidadPagos = datosPago?.Cantidad ?? 0
                    };
                })
                .ToList();

            // Devolvemos las estadísticas mensuales.
            return Ok(resultado);
        }

    }
}
