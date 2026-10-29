using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SICOP.Models;
using SICOP.Contexts;
using SICOP.DTOs;

[Route("api/[controller]")]
[ApiController]
public class ProveedoresController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public ProveedoresController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Proveedores
    // Consulta todos los proveedores utilizando DTOs de respuesta.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProveedorRespuestaDto>>> GetProveedor()
    {
        // Consulta los proveedores y convierte cada registro en un DTO.
        var proveedores = await _context.Proveedores
            .Select(proveedor => new ProveedorRespuestaDto
            {
                ProveedorId = proveedor.ProveedorId,
                Nombre = proveedor.Nombre,
                NIT = proveedor.NIT,
                Telefono = proveedor.Telefono,
                Correo = proveedor.Correo,
                Direccion = proveedor.Direccion,
                Activo = proveedor.Activo
            })
            .ToListAsync();

        // Devuelve únicamente los datos definidos en el DTO.
        return Ok(proveedores);
    }

    // GET: api/Proveedores/3
    // Consulta un proveedor por su ID utilizando un DTO de respuesta.
    [HttpGet("{proveedorid}")]
    public async Task<ActionResult<ProveedorRespuestaDto>> GetProveedor(int proveedorid)
    {
        // Busca el proveedor en SQL Server.
        var proveedor = await _context.Proveedores.FindAsync(proveedorid);

        // Verifica que el proveedor exista.
        if (proveedor == null)
        {
            return NotFound("El proveedor no existe.");
        }

        // Convierte la entidad en un DTO de respuesta.
        var respuesta = new ProveedorRespuestaDto
        {
            ProveedorId = proveedor.ProveedorId,
            Nombre = proveedor.Nombre,
            NIT = proveedor.NIT,
            Telefono = proveedor.Telefono,
            Correo = proveedor.Correo,
            Direccion = proveedor.Direccion,
            Activo = proveedor.Activo
        };

        // Devuelve únicamente los datos del DTO.
        return Ok(respuesta);
    }

    // PUT: api/Proveedores/3
    // Actualiza los datos del proveedor utilizando un DTO.
    [HttpPut("{proveedorid:int}")]
    public async Task<IActionResult> PutProveedor(
        int proveedorid, ActualizarProveedorDto dto)
    {
        // Busca el proveedor en SQL Server.
        var proveedor = await _context.Proveedores.FindAsync(proveedorid);

        // Verifica que exista.
        if (proveedor == null)
        {
            return NotFound("El proveedor no existe.");
        }

        // Verifica que otro proveedor no tenga el mismo NIT.
        bool nitDuplicado = await _context.Proveedores
            .AnyAsync(p => p.NIT == dto.NIT &&
                           p.ProveedorId != proveedorid);

        if (nitDuplicado)
        {
            return Conflict("Ya existe otro proveedor con ese NIT.");
        }

        // Actualiza únicamente los campos permitidos.
        proveedor.Nombre = dto.Nombre;
        proveedor.NIT = dto.NIT;
        proveedor.Telefono = dto.Telefono;
        proveedor.Correo = dto.Correo;
        proveedor.Direccion = dto.Direccion;

        // Guarda los cambios en SQL Server.
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx &&
            (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            return Conflict("Ya existe otro proveedor con ese NIT.");
        }

        // Confirma que la actualización fue exitosa.
        return NoContent();
    }

    // POST: api/Proveedores
    // Registra un proveedor utilizando un DTO de entrada.
    [HttpPost]
    public async Task<ActionResult<ProveedorRespuestaDto>> PostProveedor(
        CrearProveedorDto dto)
    {
        // Verifica que el NIT no esté registrado.
        bool nitDuplicado = await _context.Proveedores
            .AnyAsync(p => p.NIT == dto.NIT);

        if (nitDuplicado)
        {
            return Conflict("Ya existe un proveedor con ese NIT.");
        }

        // Convierte el DTO en una entidad Proveedor.
        var proveedor = new Proveedor
        {
            Nombre = dto.Nombre,
            NIT = dto.NIT,
            Telefono = dto.Telefono,
            Correo = dto.Correo,
            Direccion = dto.Direccion,
            Activo = true
        };

        // Guarda el proveedor en SQL Server.
        _context.Proveedores.Add(proveedor);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx &&
            (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            // Evita errores 500 cuando SQL Server detecta un NIT duplicado.
            return Conflict("Ya existe un proveedor con ese NIT.");
        }

        // Convierte la entidad guardada en un DTO de respuesta.
        var respuesta = new ProveedorRespuestaDto
        {
            ProveedorId = proveedor.ProveedorId,
            Nombre = proveedor.Nombre,
            NIT = proveedor.NIT,
            Telefono = proveedor.Telefono,
            Correo = proveedor.Correo,
            Direccion = proveedor.Direccion,
            Activo = proveedor.Activo
        };

        // Devuelve únicamente los datos del DTO.
        return CreatedAtAction(
            nameof(GetProveedor),
            new { proveedorid = proveedor.ProveedorId },
            respuesta);
    }

    // DELETE: api/Proveedores/3
    // Impide eliminar proveedores para proteger el historial financiero.
    [HttpDelete("{proveedorid:int}")]
    public IActionResult DeleteProveedor(int proveedorid)
    {
        // No se permite eliminar proveedores registrados.
        return BadRequest(
            "Los proveedores no pueden eliminarse directamente. " +
            "Debe utilizarse un procedimiento de desactivación.");
    }

    // PATCH: api/Proveedores/3/desactivar
    // Desactiva un proveedor sin eliminarlo de SQL Server.
    [HttpPatch("{proveedorid:int}/desactivar")]
    public async Task<IActionResult> DesactivarProveedor(int proveedorid)
    {
        // Busca el proveedor por su ID.
        var proveedor = await _context.Proveedores.FindAsync(proveedorid);

        // Verifica que exista.
        if (proveedor == null)
        {
            return NotFound("El proveedor no existe.");
        }

        // Evita desactivar nuevamente un proveedor inactivo.
        if (!proveedor.Activo)
        {
            return BadRequest("El proveedor ya está desactivado.");
        }

        // Cambia su estado a inactivo.
        proveedor.Activo = false;

        // Guarda el cambio en SQL Server.
        await _context.SaveChangesAsync();

        // Confirma la desactivación.
        return Ok("Proveedor desactivado correctamente.");
    }

    // PATCH: api/Proveedores/3/activar
    // Reactiva un proveedor que estaba desactivado.
    [HttpPatch("{proveedorid:int}/activar")]
    public async Task<IActionResult> ActivarProveedor(int proveedorid)
    {
        // Busca el proveedor en SQL Server.
        var proveedor = await _context.Proveedores.FindAsync(proveedorid);

        // Verifica que exista.
        if (proveedor == null)
        {
            return NotFound("El proveedor no existe.");
        }

        // Evita activar nuevamente un proveedor activo.
        if (proveedor.Activo)
        {
            return BadRequest("El proveedor ya está activo.");
        }

        // Cambia su estado a activo.
        proveedor.Activo = true;

        // Guarda el cambio en SQL Server.
        await _context.SaveChangesAsync();

        // Confirma la reactivación.
        return Ok("Proveedor activado correctamente.");
    }
}