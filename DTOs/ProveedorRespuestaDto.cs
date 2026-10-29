namespace SICOP.DTOs
{
    // Define los datos que la API devolverá
    // al registrar o consultar un proveedor.
    public class ProveedorRespuestaDto
    {
        // Identificador del proveedor.
        public int ProveedorId { get; set; }

        // Nombre del proveedor.
        public string Nombre { get; set; } = string.Empty;

        // Número de identificación tributaria.
        public string NIT { get; set; } = string.Empty;

        // Teléfono del proveedor.
        public string? Telefono { get; set; }

        // Correo electrónico.
        public string? Correo { get; set; }

        // Dirección del proveedor.
        public string? Direccion { get; set; }

        // Indica si el proveedor está activo.
        public bool Activo { get; set; }
    }
}