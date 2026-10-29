using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SICOP.Models;
using SICOP.Contexts;

[Route("api/[controller]")]
[ApiController]
public class ProveedoresController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public ProveedoresController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Proveedor
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Proveedor>>> GetProveedor()
    {
        return await _context.Proveedores.ToListAsync();
    }

    // GET: api/Proveedor/5
    [HttpGet("{proveedorid}")]
    public async Task<ActionResult<Proveedor>> GetProveedor(int proveedorid)
    {
        var proveedor = await _context.Proveedores.FindAsync(proveedorid);

        if (proveedor == null)
        {
            return NotFound();
        }

        return proveedor;
    }

    // PUT: api/Proveedor/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{proveedorid}")]
    public async Task<IActionResult> PutProveedor(int? proveedorid, Proveedor proveedor)
    {
        if (proveedorid != proveedor.ProveedorId)
        {
            return BadRequest();
        }

        _context.Entry(proveedor).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProveedorExists(proveedorid))
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

    // POST: api/Proveedor
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Proveedor>> PostProveedor(Proveedor proveedor)
    {
        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetProveedor", new { proveedorid = proveedor.ProveedorId }, proveedor);
    }

    // DELETE: api/Proveedor/5
    [HttpDelete("{proveedorid}")]
    public async Task<IActionResult> DeleteProveedor(int? proveedorid)
    {
        var proveedor = await _context.Proveedores.FindAsync(proveedorid);
        if (proveedor == null)
        {
            return NotFound();
        }

        _context.Proveedores.Remove(proveedor);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ProveedorExists(int? proveedorid)
    {
        return _context.Proveedores.Any(e => e.ProveedorId == proveedorid);
    }
}
