using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SICOP.Models;
using SICOP.Contexts;
using SICOP.Services;
using SICOP.DTOs;

[Route("api/[controller]")]
[ApiController]

// Exige autenticación JWT para acceder al controlador.
[Authorize(Roles = "Administrador General,Operador")]
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
                Observaciones = factura.Observaciones,
                Anulada = factura.Anulada,
                FechaAnulacion = factura.FechaAnulacion,
                MotivoAnulacion = factura.MotivoAnulacion,
                AnuladaPorUsuarioId = factura.AnuladaPorUsuarioId

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
            Observaciones = factura.Observaciones,
            Anulada = factura.Anulada,
            FechaAnulacion = factura.FechaAnulacion,
            MotivoAnulacion = factura.MotivoAnulacion,
            AnuladaPorUsuarioId = factura.AnuladaPorUsuarioId

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

        // Impedir registrar facturas con proveedores inactivos.
        if (!proveedor.Activo)
        {
            return BadRequest(
                "No se puede registrar una factura con un proveedor inactivo."
            );
        }

        // Verifica que no exista otra factura con el mismo número
        // en todo el sistema, independientemente del proveedor.
        bool facturaDuplicada = await _context.FacturasCompra
            .AnyAsync(f => f.NumeroFactura == dto.NumeroFactura);

        // Impide registrar números de factura repetidos.
        if (facturaDuplicada)
        {
            return Conflict(
                "Ya existe una factura registrada con ese número. " +
                "No se permiten números de factura duplicados.");
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
                        "Ya existe una factura registrada con ese número. " +
                        "No se permiten números de factura duplicados.");
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

            Observaciones = factura.Observaciones,
            Anulada = factura.Anulada,
            FechaAnulacion = factura.FechaAnulacion,
            MotivoAnulacion = factura.MotivoAnulacion,
            AnuladaPorUsuarioId = factura.AnuladaPorUsuarioId

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


    // PUT: api/FacturasCompras/1/anular
    // Permite anular una factura sin eliminarla de SQL Server.
    // Solo el Administrador General puede realizar esta operación.
    [HttpPut("{id:int}/anular")]
    [Authorize(Roles = "Administrador General")]
    public async Task<IActionResult> AnularFactura(
        int id,
        [FromBody] string motivo)
    {
        // Inicia una transacción para proteger la operación.
        await using var transaccion = await _context.Database
            .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        // Bloquea la factura durante la anulación.
        var factura = await _context.FacturasCompra
            .FromSqlInterpolated($@"
                SELECT *
                FROM FacturasCompra WITH (UPDLOCK, HOLDLOCK)
                WHERE FacturaCompraId = {id}")
            .FirstOrDefaultAsync();

        // Verifica que la factura exista.
        if (factura == null)
        {
            return NotFound("La factura no existe.");
        }

        // Impide anular dos veces la misma factura.
        if (factura.Anulada)
        {
            return BadRequest("La factura ya está anulada.");
        }

        // Verifica que se proporcione un motivo válido.
        if (string.IsNullOrWhiteSpace(motivo) ||
            motivo.Length > 500)
        {
            return BadRequest(
                "Ingrese un motivo de anulación de máximo 500 caracteres.");
        }

        // Comprueba si existen pagos vigentes asociados.
        bool tienePagosVigentes = await _context.Pagos
            .AnyAsync(p => p.FacturaCompraId == id && !p.Anulado);

        // Impide anular facturas con pagos vigentes.
        if (tienePagosVigentes)
        {
            return BadRequest(
                "No se puede anular una factura con pagos vigentes.");
        }

        // Obtiene el identificador del administrador autenticado.
        var usuarioIdTexto = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(usuarioIdTexto, out int usuarioId))
        {
            return Unauthorized("No se pudo identificar al administrador.");
        }

        // Registra la información de la anulación.
        factura.Anulada = true;
        factura.FechaAnulacion = DateTime.UtcNow;
        factura.MotivoAnulacion = motivo.Trim();
        factura.AnuladaPorUsuarioId = usuarioId;

        // Guarda los cambios en SQL Server.
        await _context.SaveChangesAsync();

        // Confirma la transacción.
        await transaccion.CommitAsync();

        // Devuelve el resultado de la operación.
        return Ok(new
        {
            Mensaje = "Factura anulada correctamente.",
            factura.FacturaCompraId,
            factura.Anulada,
            factura.FechaAnulacion,
            factura.MotivoAnulacion,
            factura.AnuladaPorUsuarioId
        });
    }


}