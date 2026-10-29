using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SICOP.Contexts;
using SICOP.DTOs;
using SICOP.Models;

namespace SICOP.Controllers
{
    // Controlador encargado de la autenticación de usuarios.
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // Permite consultar los usuarios registrados.
        private readonly ApplicationDbContext _context;

        // Permite acceder a la configuración JWT.
        private readonly IConfiguration _configuration;

        // Constructor con inyección de dependencias.
        public AuthController(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

    // POST: api/Auth/login
// Verifica las credenciales del usuario.
[HttpPost("login")]
        public async Task<ActionResult<LoginRespuestaDto>> Login(
    [FromBody] LoginDto dto)
        {
            // Busca al usuario mediante su correo electrónico.
            // Busca al usuario y carga su rol desde SQL Server.
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == dto.Correo);

            // Verifica que el usuario exista.
            if (usuario == null)
            {
                return Unauthorized(new
                {
                    mensaje = "Correo electrónico o contraseña incorrectos."
                });
            }

            // Verifica que el usuario esté activo.
            if (!usuario.Activo)
            {
                return Unauthorized(new
                {
                    mensaje = "El usuario se encuentra desactivado."
                });
            }

            // Prepara el verificador de contraseñas.
            var passwordHasher = new PasswordHasher<Usuario>();

            // Compara la contraseña recibida con el hash almacenado.
            var resultado = passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.PasswordHash,
                dto.Password
            );

            // Rechaza las credenciales incorrectas.
            if (resultado == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    mensaje = "Correo electrónico o contraseña incorrectos."
                });
            }

            // Verifica que el usuario tenga un rol válido.
            if (usuario.Rol == null ||
                string.IsNullOrWhiteSpace(usuario.Rol.Nombre))
            {
                return Unauthorized(new
                {
                    mensaje = "El usuario no tiene un rol válido asignado."
                });
            }

            // Obtiene la configuración necesaria para generar el JWT.
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Falta la clave JWT.");

            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            // Obtiene la duración del token desde appsettings.json.
            int minutosExpiracion = _configuration
                .GetValue<int>("Jwt:ExpirationMinutes");

            // Establece la fecha de vencimiento en UTC.
            var expiracion = DateTime.UtcNow.AddMinutes(minutosExpiracion);

            // Define los datos que estarán dentro del token JWT.
            var claims = new List<Claim>
{
    // Identificador del usuario.
    new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),

    // Nombre del usuario.
    new Claim(ClaimTypes.Name, usuario.Nombre),

    // Correo electrónico.
    new Claim(ClaimTypes.Email, usuario.Correo),

    // Rol actual del usuario.
    new Claim(ClaimTypes.Role, usuario.Rol.Nombre),

    // Versión de seguridad del usuario.
    // Permite invalidar los tokens antiguos cuando cambie la versión.
    new Claim("VersionToken", usuario.VersionToken.ToString())
};

            // Convierte la clave secreta en bytes.
            var clave = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            );

            // Configura la firma digital del token.
            var credenciales = new SigningCredentials(
                clave,
                SecurityAlgorithms.HmacSha256
            );

            // Genera el token JWT.
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expiracion,
                signingCredentials: credenciales
            );

            // Convierte el token en una cadena de texto.
            string tokenGenerado = new JwtSecurityTokenHandler()
                .WriteToken(token);

            // Prepara la respuesta sin incluir la contraseña.
            var respuesta = new LoginRespuestaDto
            {
                Token = tokenGenerado,
                Expiracion = expiracion,
                UsuarioId = usuario.UsuarioId,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                RolId = usuario.RolId
            };

            // Devuelve HTTP 200 con el token y los datos del usuario.
            return Ok(respuesta);
        }
    }
}