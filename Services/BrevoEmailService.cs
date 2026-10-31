
using System.Net.Http.Json;
using SICOP.Models;

namespace SICOP.Services
{
    // Servicio encargado de enviar correos mediante Brevo.
    public class BrevoEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        // Recibe las dependencias mediante inyección.
        public BrevoEmailService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        // Envía un correo electrónico al destinatario indicado.
        public async Task EnviarCorreoAsync(
            string destinatario,
            string asunto,
            string mensaje)
        {
            // Obtiene la clave API desde la configuración segura.
            var apiKey = _configuration["Brevo:ApiKey"]
                ?? throw new InvalidOperationException(
                    "No se configuró la clave API de Brevo.");

            // Prepara los datos del correo.
            var correo = new
            {
                sender = new
                {
                    name = "SICOP - Sistema de Cuentas por Pagar",
                    email = "robingarcia0205@gmail.com"
                },
                to = new[]
                {
                    new { email = destinatario }
                },
                subject = asunto,
                textContent = mensaje
            };

            // Prepara la solicitud HTTP a Brevo.
            using var solicitud = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.brevo.com/v3/smtp/email");

            solicitud.Headers.Add("api-key", apiKey);
            solicitud.Content = JsonContent.Create(correo);

            // Envía el correo mediante la API.
            using var respuesta = await _httpClient.SendAsync(solicitud);

            // Comprueba si Brevo aceptó la solicitud.
            respuesta.EnsureSuccessStatusCode();
        }
    }
}
