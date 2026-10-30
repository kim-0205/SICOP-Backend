using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SICOP.Contexts;
using SICOP.DTOs;
using SICOP.Models;
using System.Security.Claims;

// Define la ruta del controlador.
[Route("api/[controller]")]
[ApiController]

// Permite el acceso únicamente al Administrador General.
[Authorize(Roles = "Administrador General")]
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
                UsuarioId = usuario.UsuarioId,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                RolId = usuario.RolId,
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

        // Verifica si el usuario es Administrador General.
        bool esAdministrador = await _context.Roles
            .AnyAsync(r => r.RolId == usuario.RolId &&
                           r.Nombre == "Administrador General");

        // Verifica si el nuevo rol también es Administrador General.
        bool nuevoRolEsAdministrador = await _context.Roles
            .AnyAsync(r => r.RolId == dto.RolId &&
                           r.Nombre == "Administrador General");

        // Evita que el último administrador activo pierda su rol.
        if (usuario.Activo &&
            esAdministrador &&
            !nuevoRolEsAdministrador)
        {
            int administradoresActivos = await _context.Usuarios
                .CountAsync(u => u.Activo &&
                    u.Rol.Nombre == "Administrador General");

            if (administradoresActivos <= 1)
            {
                return BadRequest(
                    "No puedes cambiar el rol del último Administrador General activo.");
            }
        }


        // Comprueba si el administrador está cambiando el rol.
        bool cambioRol = usuario.RolId != dto.RolId;

        // Actualiza únicamente los campos permitidos.
        usuario.Nombre = dto.Nombre;
        usuario.Correo = dto.Correo;
        usuario.RolId = dto.RolId;

        // Si cambia el rol, invalida los tokens anteriores.
        if (cambioRol)
        {
            usuario.VersionToken++;
        }


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

    // PATCH: api/Usuarios/1/cambiar-password
    // Permite al Administrador General cambiar la contraseña de un usuario.
    [HttpPatch("{usuarioid:int}/cambiar-password")]
    [Authorize(Roles = "Administrador General")]
    public async Task<IActionResult> CambiarPassword(
        int usuarioid,
        [FromBody] CambiarPasswordUsuarioDto dto)
    {
        // Busca el usuario en SQL Server.
        var usuario = await _context.Usuarios.FindAsync(usuarioid);

        // Verifica que el usuario exista.
        if (usuario == null)
        {
            return NotFound("El usuario no existe.");
        }

        // Genera un hash seguro para la nueva contraseña.
        var passwordHasher = new PasswordHasher<Usuario>();

        usuario.PasswordHash = passwordHasher.HashPassword(
            usuario, dto.NuevaPassword);

        // Invalida los tokens anteriores del usuario.
        usuario.VersionToken++;

        // Guarda los cambios en SQL Server.
        await _context.SaveChangesAsync();

        // Confirma que la contraseña fue actualizada.
        return Ok("Contraseña actualizada correctamente.");
    }

    // DELETE: api/Usuarios/1
    // Impide eliminar usuarios para conservar el historial.
    [HttpDelete("{usuarioid:int}")]
    public IActionResult DeleteUsuario(int usuarioid)
    {
        // Los usuarios no pueden eliminarse definitivamente.
        // Deben desactivarse mediante el método PATCH.
        return BadRequest(
            "Los usuarios no pueden eliminarse directamente. " +
            "Debe utilizarse el procedimiento de desactivación.");
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

        // Obtiene el ID del administrador autenticado mediante JWT.
        var usuarioActualId = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        // Impide que el administrador desactive su propia cuenta.
        if (usuario.UsuarioId.ToString() == usuarioActualId)
        {
            return BadRequest(
                "No puedes desactivar tu propia cuenta.");
        }

        // Comprueba si el usuario tiene el rol Administrador General.
        bool esAdministrador = await _context.Roles
            .AnyAsync(r => r.RolId == usuario.RolId &&
                           r.Nombre == "Administrador General");

        if (esAdministrador)
        {
            // Cuenta los administradores que permanecen activos.
            int administradoresActivos = await _context.Usuarios
                .CountAsync(u => u.Activo &&
                    u.Rol.Nombre == "Administrador General");

            // Evita desactivar al último administrador activo.
            if (administradoresActivos <= 1)
            {
                return BadRequest(
                    "No puedes desactivar al último Administrador General activo.");
            }
        }

        // Desactiva al usuario.
        usuario.Activo = false;

        // Incrementa la versión de seguridad.
        // Invalida permanentemente los JWT emitidos anteriormente.
        usuario.VersionToken++;

        // Guarda ambos cambios en SQL Server.
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