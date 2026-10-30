using Microsoft.EntityFrameworkCore;
using SICOP.Models;

namespace SICOP.Contexts
{
    // Contexto principal de la base de datos SICOP.
    public class ApplicationDbContext : DbContext
    {
        // Recibe la configuración mediante inyección de dependencias.
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Representan las tablas de nuestra base de datos.
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<FacturaCompra> FacturasCompra { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<ParametroSistema> ParametrosSistema { get; set; }

        // Configura las relaciones y restricciones de las entidades.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Un proveedor tiene muchas facturas.
            modelBuilder.Entity<FacturaCompra>()
                .HasOne(f => f.Proveedor)
                .WithMany(p => p.Facturas)
                .HasForeignKey(f => f.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Una factura puede recibir muchos pagos.
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.FacturaCompra)
                .WithMany(f => f.Pagos)
                .HasForeignKey(p => p.FacturaCompraId)
                .OnDelete(DeleteBehavior.Restrict);

            // Un rol puede tener varios usuarios.
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.RolId)
                .OnDelete(DeleteBehavior.Restrict);

            // El NIT no puede repetirse.
            modelBuilder.Entity<Proveedor>()
                .HasIndex(p => p.NIT)
                .IsUnique();

            // El número de factura es único por proveedor.
            modelBuilder.Entity<FacturaCompra>()
                .HasIndex(f => new { f.ProveedorId, f.NumeroFactura })
                .IsUnique();

            // El correo de cada usuario debe ser único.
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Correo)
                .IsUnique();

            // El nombre de cada rol debe ser único.
            modelBuilder.Entity<Rol>()
                .HasIndex(r => r.Nombre)
                .IsUnique();

            // La clave de cada parámetro debe ser única.
            modelBuilder.Entity<ParametroSistema>()
                .HasIndex(p => p.Clave)
                .IsUnique();

            // Los importes deben ser mayores que cero.
            modelBuilder.Entity<FacturaCompra>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_FacturaCompra_Total",
                    "[Total] > 0"));

            modelBuilder.Entity<Pago>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Pago_Monto",
                    "[Monto] > 0"));

            // El vencimiento no puede ser anterior a la emisión.
            modelBuilder.Entity<FacturaCompra>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_FacturaCompra_Fechas",
                    "[FechaVencimiento] >= [FechaEmision]"));
        }
    }
}
