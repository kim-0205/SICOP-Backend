namespace SICOP.DTOs
{
    // Representa el estado de cuenta completo de un proveedor.
    public class EstadoCuentaRespuestaDto
    {
        // Identificador del proveedor.
        public int ProveedorId { get; set; }

        // Nombre del proveedor.
        public string NombreProveedor { get; set; } = string.Empty;

        // NIT del proveedor.
        public string NIT { get; set; } = string.Empty;

        // Fecha en que se genera la consulta.
        public DateTime FechaConsulta { get; set; }

        // Cantidad de facturas vigentes del proveedor.
        public int TotalFacturas { get; set; }

        // Suma de los importes de las facturas vigentes.
        public decimal TotalFacturado { get; set; }

        // Suma de los pagos válidos realizados.
        public decimal TotalPagado { get; set; }

        // Cantidad que todavía se debe al proveedor.
        public decimal SaldoPendiente { get; set; }

        // Historial de facturas y pagos del proveedor.
        public List<MovimientoEstadoCuentaDto> Movimientos { get; set; }
            = new List<MovimientoEstadoCuentaDto>();
    }
}