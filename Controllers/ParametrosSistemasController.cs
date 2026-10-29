using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SICOP.Models;
using SICOP.Contexts;
using SICOP.DTOs;

[Route("api/[controller]")]
[ApiController]

// Permite acceder únicamente al Administrador General.
[Authorize(Roles = "Administrador General")]
public class ParametrosSistemasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public ParametrosSistemasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/ParametrosSistemas
    // Consulta todos los parámetros utilizando DTOs de respuesta.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ParametroSistemaRespuestaDto>>> GetParametroSistema()
    {
        // Consulta los parámetros registrados en SQL Server.
        var parametros = await _context.ParametrosSistema
            .Select(parametro => new ParametroSistemaRespuestaDto
            {
                // Identificador del parámetro.
                ParametroSistemaId = parametro.ParametroSistemaId,

                // Clave única del parámetro.
                Clave = parametro.Clave,

                // Valor almacenado.
                Valor = parametro.Valor,

                // Descripción opcional.
                Descripcion = parametro.Descripcion
            })
            .ToListAsync();

        // Devuelve únicamente los datos definidos en el DTO.
        return Ok(parametros);
    }

    // GET: api/ParametrosSistemas/5
    // Consulta un parámetro específico mediante su identificador.
    [HttpGet("{parametrosistemaid}")]
    public async Task<ActionResult<ParametroSistemaRespuestaDto>> GetParametroSistema(int parametrosistemaid)
    {
        // Busca el parámetro en la base de datos.
        var parametro = await _context.ParametrosSistema
            .Where(p => p.ParametroSistemaId == parametrosistemaid)
            .Select(p => new ParametroSistemaRespuestaDto
            {
                // Identificador del parámetro.
                ParametroSistemaId = p.ParametroSistemaId,

                // Clave del parámetro.
                Clave = p.Clave,

                // Valor almacenado.
                Valor = p.Valor,

                // Descripción opcional.
                Descripcion = p.Descripcion
            })
            .FirstOrDefaultAsync();

        // Verifica si el parámetro existe.
        if (parametro == null)
        {
            return NotFound(new
            {
                mensaje = "El parámetro solicitado no existe."
            });
        }

        // Devuelve el parámetro utilizando el DTO.
        return Ok(parametro);
    }

    // PUT: api/ParametrosSistemas/1
    // Actualiza un parámetro existente utilizando un DTO.
    [HttpPut("{parametrosistemaid}")]
    public async Task<IActionResult> PutParametroSistema(
        int parametrosistemaid,
        [FromBody] ActualizarParametroSistemaDto dto)
    {
        // Busca el parámetro que se desea actualizar.
        var parametro = await _context.ParametrosSistema
            .FindAsync(parametrosistemaid);

        // Verifica si existe.
        if (parametro == null)
        {
            return NotFound(new
            {
                mensaje = "El parámetro solicitado no existe."
            });
        }

        // Verifica que la clave y el valor contengan información.
        if (string.IsNullOrWhiteSpace(dto.Clave) ||
            string.IsNullOrWhiteSpace(dto.Valor))
        {
            return BadRequest(new
            {
                mensaje = "La clave y el valor son obligatorios."
            });
        }

        // Elimina espacios innecesarios.
        string clave = dto.Clave.Trim();

        // Verifica que otro parámetro no tenga la misma clave.
        bool existe = await _context.ParametrosSistema
            .AnyAsync(p => p.Clave == clave &&
                           p.ParametroSistemaId != parametrosistemaid);

        if (existe)
        {
            return Conflict(new
            {
                mensaje = "Ya existe otro parámetro registrado con esta clave."
            });
        }

        // Actualiza los campos permitidos.
        parametro.Clave = clave;
        parametro.Valor = dto.Valor.Trim();
        parametro.Descripcion = dto.Descripcion?.Trim();

        // Guarda los cambios en SQL Server.
        await _context.SaveChangesAsync();

        // Devuelve HTTP 204 cuando la actualización es exitosa.
        return NoContent();
    }

    // POST: api/ParametrosSistemas
    // Registra un nuevo parámetro del sistema.
    [HttpPost]
    public async Task<ActionResult<ParametroSistemaRespuestaDto>> PostParametroSistema(
        [FromBody] CrearParametroSistemaDto dto)
    {
        // Verifica que la clave no esté vacía.
        if (string.IsNullOrWhiteSpace(dto.Clave))
        {
            return BadRequest(new
            {
                mensaje = "La clave del parámetro es obligatoria."
            });
        }

        // Verifica que el valor no esté vacío.
        if (string.IsNullOrWhiteSpace(dto.Valor))
        {
            return BadRequest(new
            {
                mensaje = "El valor del parámetro es obligatorio."
            });
        }

        // Elimina espacios innecesarios de la clave.
        string clave = dto.Clave.Trim();

        // Verifica si ya existe un parámetro con la misma clave.
        bool existe = await _context.ParametrosSistema
            .AnyAsync(p => p.Clave == clave);

        if (existe)
        {
            return Conflict(new
            {
                mensaje = "Ya existe un parámetro registrado con esta clave."
            });
        }

        // Crea la entidad que se almacenará en SQL Server.
        var parametro = new ParametroSistema
        {
            Clave = clave,
            Valor = dto.Valor.Trim(),
            Descripcion = dto.Descripcion?.Trim()
        };

        // Agrega el parámetro a la base de datos.
        _context.ParametrosSistema.Add(parametro);

        // Guarda los cambios en SQL Server.
        await _context.SaveChangesAsync();

        // Prepara la respuesta utilizando el DTO.
        var respuesta = new ParametroSistemaRespuestaDto
        {
            ParametroSistemaId = parametro.ParametroSistemaId,
            Clave = parametro.Clave,
            Valor = parametro.Valor,
            Descripcion = parametro.Descripcion
        };

        // Devuelve HTTP 201 cuando el registro es exitoso.
        return CreatedAtAction(
            nameof(GetParametroSistema),
            new { parametrosistemaid = parametro.ParametroSistemaId },
            respuesta
        );
    }

    // DELETE: api/ParametrosSistemas/1
    // Impide eliminar directamente los parámetros del sistema.
    [HttpDelete("{parametrosistemaid}")]
    public IActionResult DeleteParametroSistema(int parametrosistemaid)
    {
        // Bloquea la eliminación para proteger las configuraciones.
        return BadRequest(new
        {
            mensaje = "Los parámetros del sistema no pueden eliminarse directamente."
        });
    }

    private bool ParametroSistemaExists(int? parametrosistemaid)
    {
        return _context.ParametrosSistema.Any(e => e.ParametroSistemaId == parametrosistemaid);
    }
}