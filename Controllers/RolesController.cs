using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SICOP.Models;
using SICOP.Contexts;
using SICOP.DTOs;

[Route("api/[controller]")]
[ApiController]

// Exige un token JWT válido para acceder al controlador.
[Authorize]
public class RolesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public RolesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Roles
    // Consulta todos los roles utilizando DTOs de respuesta.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RolRespuestaDto>>> GetRol()
    {
        // Consulta los roles y convierte cada registro en un DTO.
        var roles = await _context.Roles
            .Select(rol => new RolRespuestaDto
            {
                RolId = rol.RolId,
                Nombre = rol.Nombre,
                Descripcion = rol.Descripcion
            })
            .ToListAsync();

        // Devuelve únicamente los datos definidos en el DTO.
        return Ok(roles);
    }

    // GET: api/Roles/3
    // Consulta un rol por su ID utilizando un DTO de respuesta.
    [HttpGet("{rolid:int}")]
    public async Task<ActionResult<RolRespuestaDto>> GetRol(int rolid)
    {
        // Busca el rol en SQL Server.
        var rol = await _context.Roles.FindAsync(rolid);

        // Verifica que el rol exista.
        if (rol == null)
        {
            return NotFound("El rol no existe.");
        }

        // Convierte la entidad en un DTO de respuesta.
        var respuesta = new RolRespuestaDto
        {
            RolId = rol.RolId,
            Nombre = rol.Nombre,
            Descripcion = rol.Descripcion
        };

        // Devuelve únicamente los datos del DTO.
        return Ok(respuesta);
    }
    // PUT: api/Roles/1
    [Authorize(Roles = "Administrador General")]
    // Actualiza los datos de un rol utilizando un DTO.
    [HttpPut("{rolid:int}")]
    public async Task<IActionResult> PutRol(
        int rolid, CrearRolDto dto)
    {
        // Busca el rol en SQL Server.
        var rol = await _context.Roles.FindAsync(rolid);

        // Verifica que el rol exista.
        if (rol == null)
        {
            return NotFound("El rol no existe.");
        }

        // Verifica que otro rol no tenga el mismo nombre.
        bool nombreDuplicado = await _context.Roles
            .AnyAsync(r => r.Nombre == dto.Nombre &&
                           r.RolId != rolid);

        if (nombreDuplicado)
        {
            return Conflict("Ya existe otro rol con ese nombre.");
        }

        // Actualiza únicamente los campos permitidos.
        rol.Nombre = dto.Nombre;
        rol.Descripcion = dto.Descripcion;

        // Guarda los cambios en SQL Server.
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx &&
            (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            return Conflict("Ya existe otro rol con ese nombre.");
        }

        // Confirma que la actualización fue exitosa.
        return NoContent();
    }

    // POST: api/Roles
    // Solo el Administrador General puede registrar roles.
    [Authorize(Roles = "Administrador General")]
    // Registra un nuevo rol utilizando un DTO de entrada.
    [HttpPost]
    public async Task<ActionResult<RolRespuestaDto>> PostRol(
        CrearRolDto dto)
    {
        // Verifica que no exista un rol con el mismo nombre.
        bool rolDuplicado = await _context.Roles
            .AnyAsync(r => r.Nombre == dto.Nombre);

        if (rolDuplicado)
        {
            return Conflict("Ya existe un rol con ese nombre.");
        }

        // Convierte el DTO en una entidad Rol.
        var rol = new Rol
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion
        };

        // Agrega el rol a SQL Server.
        _context.Roles.Add(rol);

        try
        {
            // Guarda el nuevo rol.
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx &&
            (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            // El índice único impide registrar nombres duplicados.
            return Conflict("Ya existe un rol con ese nombre.");
        }

        // Convierte la entidad guardada en un DTO de respuesta.
        var respuesta = new RolRespuestaDto
        {
            RolId = rol.RolId,
            Nombre = rol.Nombre,
            Descripcion = rol.Descripcion
        };

        // Devuelve el rol registrado.
        return CreatedAtAction(
            nameof(GetRol),
            new { rolid = rol.RolId },
            respuesta);
    }

    // DELETE: api/Roles/1
    [Authorize(Roles = "Administrador General")]
    // Impide eliminar roles para proteger las relaciones con usuarios.
    [HttpDelete("{rolid:int}")]
    public IActionResult DeleteRol(int rolid)
    {
        // Los roles registrados no pueden eliminarse directamente.
        return BadRequest(
            "Los roles no pueden eliminarse directamente " +
            "porque forman parte de la seguridad del sistema.");
    }
}