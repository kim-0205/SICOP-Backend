using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SICOP.Models;
using SICOP.Contexts;
using SICOP.Services;
using SICOP.DTOs;
using System.Data;

[Route("api/[controller]")]
[ApiController]
public class PagosController : ControllerBase
{
    // Contexto para acceder a SQL Server.
    private readonly ApplicationDbContext _context;

    // Servicio para consultar el saldo de las facturas.
    private readonly FacturaService _facturaService;

    // Recibe las dependencias automáticamente.
    public PagosController(
        ApplicationDbContext context,
        FacturaService facturaService)
    {
        _context = context;
        _facturaService = facturaService;
    }

    // GET: api/Pagos
    // Consulta todos los pagos utilizando DTOs de respuesta.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PagoRespuestaDto>>> GetPago()
    {
        // Consulta los pagos y convierte cada registro en un DTO.
        var pagos = await _context.Pagos
            .Select(pago => new PagoRespuestaDto
            {
                PagoId = pago.PagoId,
                FacturaCompraId = pago.FacturaCompraId,
                FechaPago = pago.FechaPago,
                Monto = pago.Monto,
                MetodoPago = pago.MetodoPago,
                Referencia = pago.Referencia,
                Observaciones = pago.Observaciones,
                Anulado = pago.Anulado,
                FechaAnulacion = pago.FechaAnulacion,
                MotivoAnulacion = pago.MotivoAnulacion
            })
            .ToListAsync();

        // Devuelve únicamente los datos definidos en el DTO.
        return Ok(pagos);
    }

    // GET: api/Pagos/1
    // Consulta un pago por su ID utilizando un DTO de respuesta.
    [HttpGet("{pagoid:int}")]
    public async Task<ActionResult<PagoRespuestaDto>> GetPago(int pagoid)
    {
        // Busca el pago en SQL Server.
        var pago = await _context.Pagos.FindAsync(pagoid);

        // Verifica que exista.
        if (pago == null)
        {
            return NotFound("El pago no existe.");
        }

        // Convierte la entidad en un DTO de respuesta.
        var respuesta = new PagoRespuestaDto
        {
            PagoId = pago.PagoId,
            FacturaCompraId = pago.FacturaCompraId,
            FechaPago = pago.FechaPago,
            Monto = pago.Monto,
            MetodoPago = pago.MetodoPago,
            Referencia = pago.Referencia,
            Observaciones = pago.Observaciones,
            Anulado = pago.Anulado,
            FechaAnulacion = pago.FechaAnulacion,
            MotivoAnulacion = pago.MotivoAnulacion
        };

        // Devuelve únicamente los datos del DTO.
        return Ok(respuesta);
    }


    // PUT: api/Pagos/1
    // Impide modificar pagos registrados para conservar el historial.
    [HttpPut("{pagoid:int}")]
    public IActionResult PutPago(int pagoid)
    {
        // Los pagos no pueden modificarse directamente.
        // Si existe un error, se debe anular el pago
        // y registrar uno nuevo.
        return BadRequest(
            "Los pagos registrados no pueden modificarse. " +
            "Debe anular el pago y registrar uno nuevo.");
    }


    // POST: api/Pagos
    // Registra un pago parcial o completo utilizando un DTO.
    [HttpPost]
    public async Task<ActionResult<PagoRespuestaDto>> PostPago(CrearPagoDto dto)
    {
        // Inicia una transacción para proteger el registro del pago.
        await using var transaccion = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);


        // Consulta y bloquea la factura durante la transacción.
        // UPDLOCK impide que otro pago tome el mismo bloqueo.
        // HOLDLOCK mantiene el bloqueo hasta finalizar la transacción.
        var factura = await _context.FacturasCompra
            .FromSqlInterpolated($@"
                SELECT *
                FROM FacturasCompra WITH (UPDLOCK, HOLDLOCK)
                WHERE FacturaCompraId = {dto.FacturaCompraId}")
            .FirstOrDefaultAsync();


        if (factura == null)
        {
            return NotFound("La factura no existe.");
        }

        // Verifica que el monto sea mayor que cero.
        if (dto.Monto <= 0)
        {
            return BadRequest(
                "El monto del pago debe ser mayor que cero.");
        }

        // Consulta el saldo pendiente de la factura.
        decimal saldoPendiente =
            await _facturaService.ObtenerSaldoPendiente(
                dto.FacturaCompraId);

        // Impide pagar una factura completamente pagada.
        if (saldoPendiente <= 0)
        {
            return BadRequest(
                "La factura ya está completamente pagada.");
        }

        // Impide registrar un pago mayor al saldo.
        if (dto.Monto > saldoPendiente)
        {
            return BadRequest(
                $"El pago excede el saldo pendiente de Q {saldoPendiente:N2}.");
        }

        // Convierte el DTO en una entidad Pago.
        var pago = new Pago
        {
            FacturaCompraId = dto.FacturaCompraId,
            Monto = dto.Monto,
            FechaPago = DateTime.UtcNow,
            MetodoPago = dto.MetodoPago,
            Referencia = dto.Referencia,
            Observaciones = dto.Observaciones,

            // Estos campos los controla el servidor.
            Anulado = false,
            FechaAnulacion = null,
            MotivoAnulacion = null
        };

        // Guarda el pago en SQL Server.
        _context.Pagos.Add(pago);
        await _context.SaveChangesAsync();

        // Confirma la transacción después de guardar el pago.
        // Libera los bloqueos y hace permanentes los cambios.
        await transaccion.CommitAsync();


        // Convierte la entidad en un DTO de respuesta.
        var respuesta = new PagoRespuestaDto
        {
            PagoId = pago.PagoId,
            FacturaCompraId = pago.FacturaCompraId,
            FechaPago = pago.FechaPago,
            Monto = pago.Monto,
            MetodoPago = pago.MetodoPago,
            Referencia = pago.Referencia,
            Observaciones = pago.Observaciones,
            Anulado = pago.Anulado,
            FechaAnulacion = pago.FechaAnulacion,
            MotivoAnulacion = pago.MotivoAnulacion
        };

        // Devuelve el pago sin incluir relaciones circulares.
        return CreatedAtAction(
            nameof(GetPago),
            new { pagoid = pago.PagoId },
            respuesta);

    }

    // PUT: api/Pagos/1/anular
    // Anula un pago sin eliminarlo de la base de datos.
    [HttpPut("{pagoid:int}/anular")]
    public async Task<IActionResult> AnularPago(
        int pagoid,
        [FromBody] string motivo)
    {
        // Busca el pago en SQL Server.
        var pago = await _context.Pagos.FindAsync(pagoid);

        // Verifica que el pago exista.
        if (pago == null)
        {
            return NotFound("El pago no existe.");
        }

        // Impide anular un pago dos veces.
        if (pago.Anulado)
        {
            return BadRequest("El pago ya está anulado.");
        }

        // Verifica que se proporcione un motivo válido.
        if (string.IsNullOrWhiteSpace(motivo) ||
            motivo.Length > 500)
        {
            return BadRequest(
                "Ingrese un motivo de anulación de máximo 500 caracteres.");
        }

        // Marca el pago como anulado.
        pago.Anulado = true;

        // Registra la fecha de anulación en UTC.
        pago.FechaAnulacion = DateTime.UtcNow;

        // Guarda el motivo de la anulación.
        pago.MotivoAnulacion = motivo;

        // Actualiza el registro sin eliminarlo.
        await _context.SaveChangesAsync();

        // Confirma que el pago fue anulado.
        return Ok(new
        {
            Mensaje = "Pago anulado correctamente.",
            pago.PagoId,
            pago.Anulado,
            pago.FechaAnulacion,
            pago.MotivoAnulacion
        });
    }
}