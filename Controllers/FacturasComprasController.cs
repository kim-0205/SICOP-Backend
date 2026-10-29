using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SICOP.Models;
using SICOP.Contexts;
using SICOP.Services;
using SICOP.DTOs;

[Route("api/[controller]")]
[ApiController]
public class FacturasComprasController : ControllerBase
{
    // Contexto para acceder a SQL Server.
    private readonly ApplicationDbContext _context;

    // Servicio que calcula saldos y estados.
    private readonly FacturaService _facturaService;

    // Recibe las dependencias automáticamente.
    public FacturasComprasController(
        ApplicationDbContext context,
        FacturaService facturaService)
    {
        _context = context;
        _facturaService = facturaService;
    }

    // GET: api/FacturasCompras
    // Consulta todas las facturas utilizando DTOs de respuesta.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FacturaCompraRespuestaDto>>> GetFacturaCompra()
    {
        // Consulta las facturas y convierte cada registro en un DTO.
        var facturas = await _context.FacturasCompra
            .Select(factura => new FacturaCompraRespuestaDto
            {
                FacturaCompraId = factura.FacturaCompraId,
                ProveedorId = factura.ProveedorId,
                NumeroFactura = factura.NumeroFactura,
                FechaEmision = factura.FechaEmision,
                FechaVencimiento = factura.FechaVencimiento,
                Total = factura.Total,
                Observaciones = factura.Observaciones
            })
            .ToListAsync();

        // Devuelve la lista sin incluir relaciones de navegación.
        return Ok(facturas);
    }

    // GET: api/FacturasCompras/3
    // Consulta una factura utilizando un DTO de respuesta.
    [HttpGet("{facturacompraid}")]
    public async Task<ActionResult<FacturaCompraRespuestaDto>> GetFacturaCompra(
        int facturacompraid)
    {
        // Busca la factura en SQL Server.
        var factura = await _context.FacturasCompra
            .FindAsync(facturacompraid);

        // Verifica que exista.
        if (factura == null)
        {
            return NotFound("La factura no existe.");
        }

        // Convierte la entidad en un DTO de respuesta.
        var respuesta = new FacturaCompraRespuestaDto
        {
            FacturaCompraId = factura.FacturaCompraId,
            ProveedorId = factura.ProveedorId,
            NumeroFactura = factura.NumeroFactura,
            FechaEmision = factura.FechaEmision,
            FechaVencimiento = factura.FechaVencimiento,
            Total = factura.Total,
            Observaciones = factura.Observaciones
        };

        // Devuelve únicamente los datos necesarios.
        return Ok(respuesta);
    }

    // PUT: api/FacturasCompras/1
    // Impide modificar facturas registradas para proteger
    // los montos y el historial financiero.
    [HttpPut("{facturacompraid:int}")]
    public IActionResult PutFacturaCompra(int facturacompraid)
    {
        // No se permite modificar directamente una factura,
        // porque podría alterar los saldos y pagos registrados.
        return BadRequest(
            "Las facturas registradas no pueden modificarse directamente. " +
            "Debe utilizarse un procedimiento de corrección autorizado.");
    }


    // POST: api/FacturasCompras
    // Registra una factura utilizando un DTO de entrada.
    [HttpPost]
    public async Task<ActionResult<FacturaCompraRespuestaDto>> PostFacturaCompra(
    CrearFacturaCompraDto dto)
    {
        // Verifica que el proveedor exista.
        var proveedor = await _context.Proveedores
            .FindAsync(dto.ProveedorId);

        if (proveedor == null)
        {
            return BadRequest("El proveedor no existe.");
        }

        // Verifica que el proveedor no tenga registrada
        // otra factura con el mismo número.
        bool facturaDuplicada = await _context.FacturasCompra
            .AnyAsync(f =>
                f.ProveedorId == dto.ProveedorId &&
                f.NumeroFactura == dto.NumeroFactura);

        // Si ya existe, impide registrar la factura.
        if (facturaDuplicada)
        {
            return Conflict(
                "Ya existe una factura con ese número para el proveedor seleccionado.");
        }

        // Verifica que la fecha de vencimiento no sea
        // anterior a la fecha de emisión.
        if (dto.FechaVencimiento < dto.FechaEmision)
        {
            return BadRequest(
                "La fecha de vencimiento no puede ser anterior a la fecha de emisión.");
        }

        // Convierte el DTO en una entidad FacturaCompra.
        var factura = new FacturaCompra
        {
            ProveedorId = dto.ProveedorId,
            NumeroFactura = dto.NumeroFactura,
            FechaEmision = dto.FechaEmision!.Value,
            FechaVencimiento = dto.FechaVencimiento!.Value,
            Total = dto.Total,
            Observaciones = dto.Observaciones
        };

        // Guarda la factura en SQL Server.
        _context.FacturasCompra.Add(factura);

        try
        {
            // Intenta guardar la factura.
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx &&
            (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            // SQL Server detectó un registro duplicado.
            // El índice único protege la base de datos.
            return Conflict(
                "Ya existe una factura con ese número para el proveedor seleccionado.");
        }

        // Devuelve la factura registrada.
        // Convierte la factura guardada en un DTO de respuesta.
        var respuesta = new FacturaCompraRespuestaDto
        {
            FacturaCompraId = factura.FacturaCompraId,
            ProveedorId = factura.ProveedorId,
            NumeroFactura = factura.NumeroFactura,
            FechaEmision = factura.FechaEmision,
            FechaVencimiento = factura.FechaVencimiento,
            Total = factura.Total,
            Observaciones = factura.Observaciones
        };

        // Devuelve únicamente los datos del DTO.
        return CreatedAtAction(
            nameof(GetFacturaCompra),
            new { facturacompraid = factura.FacturaCompraId },
            respuesta);
    }

    // DELETE: api/FacturasCompras/1
    // Impide eliminar facturas para conservar el historial financiero.
    [HttpDelete("{facturacompraid:int}")]
    public IActionResult DeleteFacturaCompra(int facturacompraid)
    {
        // Las facturas no deben eliminarse directamente,
        // porque forman parte del historial de cuentas por pagar.
        return BadRequest(
            "Las facturas registradas no pueden eliminarse. " +
            "Debe utilizarse un procedimiento de anulación autorizado.");
    }

    // GET: api/FacturasCompras/1/saldo
    // Consulta el total pagado y el saldo pendiente.
    [HttpGet("{id:int}/saldo")]
    public async Task<IActionResult> GetSaldo(int id)
    {
        // Verifica que la factura exista.
        var factura = await _context.FacturasCompra.FindAsync(id);

        if (factura == null)
            return NotFound("La factura no existe.");

        // Obtiene los cálculos desde el servicio.
        decimal totalPagado =
            await _facturaService.ObtenerTotalPagado(id);

        decimal saldoPendiente =
            await _facturaService.ObtenerSaldoPendiente(id);

        return Ok(new
        {
            factura.FacturaCompraId,
            factura.Total,
            TotalPagado = totalPagado,
            SaldoPendiente = saldoPendiente
        });
    }

    // GET: api/FacturasCompras/1/estado
    // Consulta si la factura está pendiente o pagada.
    [HttpGet("{id:int}/estado")]
    public async Task<IActionResult> GetEstado(int id)
    {
        // Verifica que la factura exista.
        var factura = await _context.FacturasCompra.FindAsync(id);

        if (factura == null)
            return NotFound("La factura no existe.");

        // Solicita al servicio el estado de la factura.
        string estado =
            await _facturaService.ObtenerEstadoFactura(id);

        return Ok(new
        {
            factura.FacturaCompraId,
            Estado = estado
        });
    }

}