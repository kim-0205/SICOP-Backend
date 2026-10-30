
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SICOP.Contexts;
using SICOP.DTOs;

namespace SICOP.Controllers
{
    // Controlador para consultar estados de cuenta por proveedor.
    [Route("api/[controller]")]
    [ApiController]

    // Permite consultar estados de cuenta a los dos roles.
    [Authorize(Roles = "Administrador General,Operador")]
    public class EstadosCuentaController : ControllerBase
    {
        // Contexto para consultar SQL Server.
        private readonly ApplicationDbContext _context;

        // Inyección de dependencias.
        public EstadosCuentaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/EstadosCuenta/proveedor/1
        // Consulta el estado de cuenta de un proveedor.
        [HttpGet("proveedor/{proveedorId:int}")]
        public async Task<ActionResult<EstadoCuentaRespuestaDto>>
            GetEstadoCuentaProveedor(int proveedorId)
        {
            // Busca el proveedor en SQL Server.
            var proveedor = await _context.Proveedores
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProveedorId == proveedorId);

            // Verifica que el proveedor exista.
            if (proveedor == null)
            {
                return NotFound(new
                {
                    mensaje = "El proveedor no existe."
                });
            }

            // Obtiene las facturas del proveedor con sus pagos.
            // Incluye facturas y pagos anulados para conservar el historial.
            var facturas = await _context.FacturasCompra
                .AsNoTracking()
                .Where(f => f.ProveedorId == proveedorId)
                .Include(f => f.Pagos)
                .ToListAsync();

            // Crea la lista de movimientos financieros.
            var movimientos = new List<MovimientoEstadoCuentaDto>();

            foreach (var factura in facturas)
            {
                // Las facturas anuladas no generan deuda vigente.
                movimientos.Add(new MovimientoEstadoCuentaDto
                {
                    Fecha = factura.FechaEmision,
                    FacturaCompraId = factura.FacturaCompraId,
                    NumeroFactura = factura.NumeroFactura,
                    TipoMovimiento = "Factura",
                    Descripcion = factura.Anulada
                        ? "Factura anulada"
                        : "Factura de compra",
                    Cargo = factura.Anulada ? 0 : factura.Total,
                    Abono = 0,
                    Anulado = factura.Anulada,
                    MotivoAnulacion = factura.MotivoAnulacion
                });

                // Agrega los pagos asociados a cada factura.
                foreach (var pago in factura.Pagos)
                {
                    // Un pago anulado no disminuye el saldo.
                    // Los pagos de facturas anuladas tampoco afectan la deuda.
                    bool afectaSaldo = !pago.Anulado && !factura.Anulada;

                    movimientos.Add(new MovimientoEstadoCuentaDto
                    {
                        Fecha = pago.FechaPago,
                        FacturaCompraId = factura.FacturaCompraId,
                        PagoId = pago.PagoId,
                        NumeroFactura = factura.NumeroFactura,
                        TipoMovimiento = "Pago",
                        Descripcion = pago.Anulado
                            ? "Pago anulado"
                            : "Pago realizado mediante " + pago.MetodoPago,
                        Cargo = 0,
                        Abono = afectaSaldo ? pago.Monto : 0,
                        Anulado = pago.Anulado || factura.Anulada,
                        MotivoAnulacion = pago.MotivoAnulacion
                    });
                }
            }

            // Ordena cronológicamente los movimientos.
            // En fechas iguales, muestra primero la factura.
            var movimientosOrdenados = movimientos
                .OrderBy(m => m.Fecha)
                .ThenBy(m => m.TipoMovimiento == "Factura" ? 0 : 1)
                .ThenBy(m => m.FacturaCompraId)
                .ThenBy(m => m.PagoId ?? 0)
                .ToList();

            // Calcula el saldo acumulado del proveedor.
            decimal saldoAcumulado = 0;

            foreach (var movimiento in movimientosOrdenados)
            {
                saldoAcumulado += movimiento.Cargo - movimiento.Abono;
                movimiento.SaldoAcumulado = saldoAcumulado;
            }

            // Calcula los totales de las facturas vigentes.
            decimal totalFacturado = facturas
                .Where(f => !f.Anulada)
                .Sum(f => f.Total);

            // Suma únicamente los pagos vigentes.
            decimal totalPagado = facturas
                .Where(f => !f.Anulada)
                .SelectMany(f => f.Pagos)
                .Where(p => !p.Anulado)
                .Sum(p => p.Monto);

            // Prepara la respuesta para el frontend.
            var respuesta = new EstadoCuentaRespuestaDto
            {
                ProveedorId = proveedor.ProveedorId,
                NombreProveedor = proveedor.Nombre,
                NIT = proveedor.NIT,
                FechaConsulta = DateTime.UtcNow,
                TotalFacturas = facturas.Count(f => !f.Anulada),
                TotalFacturado = totalFacturado,
                TotalPagado = totalPagado,
                SaldoPendiente = totalFacturado - totalPagado,
                Movimientos = movimientosOrdenados
            };

            // Devuelve el estado de cuenta completo.
            return Ok(respuesta);
        }
    }
}
