using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SICOP.Models;
using SICOP.Contexts;
using SICOP.DTOs;
using Microsoft.AspNetCore.Identity;

[Route("api/[controller]")]
[ApiController]
public class UsuariosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public UsuariosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Usuarios
    // Consulta todos los usuarios sin mostrar sus contraseñas.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioRespuestaDto>>> GetUsuario()
    {
        // Consulta los usuarios registrados en SQL Server.
        var usuarios = await _context.Usuarios
            .Select(usuario => new UsuarioRespuestaDto
            {
                // Identificador del usuario.
                UsuarioId = usuario.UsuarioId,

                // Nombre completo.
                Nombre = usuario.Nombre,

                // Correo electrónico.
                Correo = usuario.Correo,

                // Rol asignado.
                RolId = usuario.RolId,

                // Estado del usuario.
                Activo = usuario.Activo
            })
            .ToListAsync();

        // Devuelve los usuarios sin incluir PasswordHash.
        return Ok(usuarios);
    }

    // GET: api/Usuarios/1
    // Consulta un usuario por su ID sin mostrar su contraseña.
    [HttpGet("{usuarioid:int}")]
    public async Task<ActionResult<UsuarioRespuestaDto>> GetUsuario(int usuarioid)
    {
        // Busca el usuario en SQL Server.
        var usuario = await _context.Usuarios.FindAsync(usuarioid);

        // Verifica que el usuario exista.
        if (usuario == null)
        {
            return NotFound("El usuario no existe.");
        }

        // Convierte la entidad en un DTO de respuesta.
        var respuesta = new UsuarioRespuestaDto
        {
            UsuarioId = usuario.UsuarioId,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            RolId = usuario.RolId,
            Activo = usuario.Activo
        };

        // Devuelve los datos sin incluir PasswordHash.
        return Ok(respuesta);
    }

    // PUT: api/Usuarios/1
    // Actualiza los datos del usuario utilizando un DTO.
    [HttpPut("{usuarioid:int}")]
    public async Task<IActionResult> PutUsuario(
        int usuarioid, ActualizarUsuarioDto dto)
    {
        // Busca el usuario en SQL Server.
        var usuario = await _context.Usuarios.FindAsync(usuarioid);

        // Verifica que exista.
        if (usuario == null)
        {
            return NotFound("El usuario no existe.");
        }

        // Verifica que otro usuario no tenga el mismo correo.
        bool correoDuplicado = await _context.Usuarios
            .AnyAsync(u => u.Correo == dto.Correo &&
                           u.UsuarioId != usuarioid);

        if (correoDuplicado)
        {
            return Conflict("Ya existe otro usuario con ese correo.");
        }

        // Verifica que el rol seleccionado exista.
        bool rolExiste = await _context.Roles
            .AnyAsync(r => r.RolId == dto.RolId);

        if (!rolExiste)
        {
            return BadRequest("El rol seleccionado no existe.");
        }

        // Actualiza únicamente los campos permitidos.
        usuario.Nombre = dto.Nombre;
        usuario.Correo = dto.Correo;
        usuario.RolId = dto.RolId;

        // Conserva PasswordHash y Activo sin modificaciones.

        try
        {
            // Guarda los cambios en SQL Server.
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx &&
            (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            return Conflict("Ya existe otro usuario con ese correo.");
        }

        // Confirma que la actualización fue exitosa.
        return NoContent();
    }

    // POST: api/Usuarios
    // Registra un usuario con su contraseña protegida.
    [HttpPost]
    public async Task<ActionResult<UsuarioRespuestaDto>> PostUsuario(
        CrearUsuarioDto dto)
    {
        // Verifica que el correo no esté registrado.
        bool correoDuplicado = await _context.Usuarios
            .AnyAsync(u => u.Correo == dto.Correo);

        if (correoDuplicado)
        {
            return Conflict("Ya existe un usuario con ese correo.");
        }

        // Verifica que el rol exista.
        bool rolExiste = await _context.Roles
            .AnyAsync(r => r.RolId == dto.RolId);

        if (!rolExiste)
        {
            return BadRequest("El rol seleccionado no existe.");
        }

        // Crea la entidad Usuario.
        var usuario = new Usuario
        {
            Nombre = dto.Nombre,
            Correo = dto.Correo,
            RolId = dto.RolId,
            Activo = true
        };

        // Convierte la contraseña en un hash seguro.
        var passwordHasher = new PasswordHasher<Usuario>();

        usuario.PasswordHash = passwordHasher.HashPassword(
            usuario, dto.Password);

        // Guarda el usuario en SQL Server.
        _context.Usuarios.Add(usuario);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx &&
            (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            return Conflict("Ya existe un usuario con ese correo.");
        }

        // Prepara la respuesta sin incluir la contraseña.
        var respuesta = new UsuarioRespuestaDto
        {
            UsuarioId = usuario.UsuarioId,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            RolId = usuario.RolId,
            Activo = usuario.Activo
        };

        // Devuelve el usuario registrado.
        return CreatedAtAction(
            nameof(GetUsuario),
            new { usuarioid = usuario.UsuarioId },
            respuesta);
    }

    // DELETE: api/Usuarios/1
    // Impide eliminar usuarios para conservar el historial del sistema.
    [HttpDelete("{usuarioid:int}")]
    public IActionResult DeleteUsuario(int usuarioid)
    {
        // Los usuarios no pueden eliminarse definitivamente.
        // Deben desactivarse mediante el método PATCH.
        return BadRequest(
            "Los usuarios no pueden eliminarse directamente. " +
            "Debe utilizarse el procedimiento de desactivación.");
    }

    private bool UsuarioExists(int? usuarioid)
    {
        return _context.Usuarios.Any(e => e.UsuarioId == usuarioid);
    }

    // PATCH: api/Usuarios/1/desactivar
    // Desactiva un usuario sin eliminarlo de SQL Server.
    [HttpPatch("{usuarioid:int}/desactivar")]
    public async Task<IActionResult> DesactivarUsuario(int usuarioid)
    {
        // Busca el usuario en SQL Server.
        var usuario = await _context.Usuarios.FindAsync(usuarioid);

        // Verifica que exista.
        if (usuario == null)
        {
            return NotFound("El usuario no existe.");
        }

        // Evita desactivar nuevamente un usuario inactivo.
        if (!usuario.Activo)
        {
            return BadRequest("El usuario ya está desactivado.");
        }

        // Cambia su estado a inactivo.
        usuario.Activo = false;

        // Guarda el cambio en SQL Server.
        await _context.SaveChangesAsync();

        // Confirma la desactivación.
        return Ok("Usuario desactivado correctamente.");
    }

    // PATCH: api/Usuarios/1/activar
    // Reactiva un usuario previamente desactivado.
    [HttpPatch("{usuarioid:int}/activar")]
    public async Task<IActionResult> ActivarUsuario(int usuarioid)
    {
        // Busca el usuario en SQL Server.
        var usuario = await _context.Usuarios.FindAsync(usuarioid);

        // Verifica que exista.
        if (usuario == null)
        {
            return NotFound("El usuario no existe.");
        }

        // Evita activar nuevamente un usuario activo.
        if (usuario.Activo)
        {
            return BadRequest("El usuario ya está activo.");
        }

        // Cambia su estado a activo.
        usuario.Activo = true;

        // Guarda los cambios en SQL Server.
        await _context.SaveChangesAsync();

        // Confirma la reactivación.
        return Ok("Usuario activado correctamente.");
    }
}