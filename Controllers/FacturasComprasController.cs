using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SICOP.Models;
using SICOP.Contexts;
using SICOP.Services;

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

    // GET: api/FacturaCompra
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FacturaCompra>>> GetFacturaCompra()
    {
        return await _context.FacturasCompra.ToListAsync();
    }

    // GET: api/FacturaCompra/5
    [HttpGet("{facturacompraid}")]
    public async Task<ActionResult<FacturaCompra>> GetFacturaCompra(int facturacompraid)
    {
        var facturacompra = await _context.FacturasCompra.FindAsync(facturacompraid);

        if (facturacompra == null)
        {
            return NotFound();
        }

        return facturacompra;
    }

    // PUT: api/FacturaCompra/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{facturacompraid}")]
    public async Task<IActionResult> PutFacturaCompra(int? facturacompraid, FacturaCompra facturacompra)
    {
        if (facturacompraid != facturacompra.FacturaCompraId)
        {
            return BadRequest();
        }

        _context.Entry(facturacompra).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!FacturaCompraExists(facturacompraid))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/FacturaCompra
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<FacturaCompra>> PostFacturaCompra(FacturaCompra facturacompra)
    {
        _context.FacturasCompra.Add(facturacompra);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetFacturaCompra", new { facturacompraid = facturacompra.FacturaCompraId }, facturacompra);
    }

    // DELETE: api/FacturaCompra/5
    [HttpDelete("{facturacompraid}")]
    public async Task<IActionResult> DeleteFacturaCompra(int? facturacompraid)
    {
        var facturacompra = await _context.FacturasCompra.FindAsync(facturacompraid);
        if (facturacompra == null)
        {
            return NotFound();
        }

        _context.FacturasCompra.Remove(facturacompra);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool FacturaCompraExists(int? facturacompraid)
    {
        return _context.FacturasCompra.Any(e => e.FacturaCompraId == facturacompraid);
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
