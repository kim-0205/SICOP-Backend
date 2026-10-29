using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SICOP.Models;
using SICOP.Contexts;

[Route("api/[controller]")]
[ApiController]
public class ParametrosSistemasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public ParametrosSistemasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/ParametroSistema
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ParametroSistema>>> GetParametroSistema()
    {
        return await _context.ParametrosSistema.ToListAsync();
    }

    // GET: api/ParametroSistema/5
    [HttpGet("{parametrosistemaid}")]
    public async Task<ActionResult<ParametroSistema>> GetParametroSistema(int parametrosistemaid)
    {
        var parametrosistema = await _context.ParametrosSistema.FindAsync(parametrosistemaid);

        if (parametrosistema == null)
        {
            return NotFound();
        }

        return parametrosistema;
    }

    // PUT: api/ParametroSistema/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{parametrosistemaid}")]
    public async Task<IActionResult> PutParametroSistema(int? parametrosistemaid, ParametroSistema parametrosistema)
    {
        if (parametrosistemaid != parametrosistema.ParametroSistemaId)
        {
            return BadRequest();
        }

        _context.Entry(parametrosistema).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ParametroSistemaExists(parametrosistemaid))
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

    // POST: api/ParametroSistema
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<ParametroSistema>> PostParametroSistema(ParametroSistema parametrosistema)
    {
        _context.ParametrosSistema.Add(parametrosistema);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetParametroSistema", new { parametrosistemaid = parametrosistema.ParametroSistemaId }, parametrosistema);
    }

    // DELETE: api/ParametroSistema/5
    [HttpDelete("{parametrosistemaid}")]
    public async Task<IActionResult> DeleteParametroSistema(int? parametrosistemaid)
    {
        var parametrosistema = await _context.ParametrosSistema.FindAsync(parametrosistemaid);
        if (parametrosistema == null)
        {
            return NotFound();
        }

        _context.ParametrosSistema.Remove(parametrosistema);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ParametroSistemaExists(int? parametrosistemaid)
    {
        return _context.ParametrosSistema.Any(e => e.ParametroSistemaId == parametrosistemaid);
    }
}
