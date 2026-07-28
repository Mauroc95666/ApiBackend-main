using Microsoft.EntityFrameworkCore;

namespace BackendApi.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Ciudad> Ciudades { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<RolPermiso> RolPermisos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<DetalleVenta> DetalleVenta { get; set; }
        public DbSet<Sucursal> Sucursales { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<MovimientoInventario> MovimientoInventario { get; set; }
        public DbSet<Promocion> Promociones { get; set; }
        public DbSet<Resena> Resenas { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }
        public DbSet<Emprendimiento> Emprendimientos { get; set; }
        public DbSet<Configuracion> Configuraciones { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<Reporte> Reportes { get; set; }
        public DbSet<Dashboard> Dashboards { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Proveedor>().ToTable("Proveedor");
            modelBuilder.Entity<Producto>().ToTable("Producto");
            modelBuilder.Entity<Pedido>().ToTable("Pedido");
            modelBuilder.Entity<Ciudad>().ToTable("Ciudad");
            modelBuilder.Entity<Rol>().ToTable("Rol");
            modelBuilder.Entity<Permiso>().ToTable("Permiso");
            modelBuilder.Entity<RolPermiso>().ToTable("RolPermiso");
            modelBuilder.Entity<Cliente>().ToTable("Cliente");
            modelBuilder.Entity<Venta>().ToTable("Venta");
            modelBuilder.Entity<Inventario>().ToTable("Inventario");
            modelBuilder.Entity<DetalleVenta>().ToTable("DetalleVenta");
            modelBuilder.Entity<Sucursal>().ToTable("Sucursal");
            modelBuilder.Entity<Empleado>().ToTable("Empleado");
            modelBuilder.Entity<Categoria>().ToTable("Categoria");
            modelBuilder.Entity<MovimientoInventario>().ToTable("MovimientoInventario");
            modelBuilder.Entity<Promocion>().ToTable("Promocion");
            modelBuilder.Entity<Resena>().ToTable("Resena");
            modelBuilder.Entity<MetodoPago>().ToTable("MetodoPago");
            modelBuilder.Entity<Emprendimiento>().ToTable("Emprendimiento");
            modelBuilder.Entity<Configuracion>().ToTable("Configuracion");
            modelBuilder.Entity<Auditoria>().ToTable("Auditoria");
            modelBuilder.Entity<Notificacion>().ToTable("Notificacion");
            modelBuilder.Entity<Reporte>().ToTable("Reporte");
            modelBuilder.Entity<Dashboard>().ToTable("Dashboard");
            modelBuilder.Entity<Usuario>().ToTable("Usuario");

            modelBuilder.Entity<Producto>()
                .Property(p => p.Precio)
                .HasPrecision(10, 2);

            modelBuilder.Entity<RolPermiso>()
                .HasKey(rp => new { rp.RolId, rp.PermisoId });

            modelBuilder.Entity<DetalleVenta>()
                .Property(d => d.Subtotal)
                .HasComputedColumnSql("Cantidad * PrecioUnitario");

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Emprendimiento)
                .WithMany()
                .HasForeignKey(c => c.EmprendimientoId);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Sucursal)
                .WithMany()
                .HasForeignKey(v => v.SucursalId);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Cliente)
                .WithMany()
                .HasForeignKey(v => v.ClienteId);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Usuario)
                .WithMany()
                .HasForeignKey(v => v.UsuarioId);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.MetodoPago)
                .WithMany()
                .HasForeignKey(v => v.MetodoPagoId);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Emprendimiento)
                .WithMany()
                .HasForeignKey(u => u.EmprendimientoId);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany()
                .HasForeignKey(u => u.RolId);

            modelBuilder.Entity<Empleado>()
                .HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId);

            modelBuilder.Entity<Empleado>()
                .HasOne(e => e.Sucursal)
                .WithMany()
                .HasForeignKey(e => e.SucursalId);

            modelBuilder.Entity<Sucursal>()
                .HasOne(s => s.Emprendimiento)
                .WithMany()
                .HasForeignKey(s => s.EmprendimientoId);

            modelBuilder.Entity<Sucursal>()
                .HasOne(s => s.Ciudad)
                .WithMany()
                .HasForeignKey(s => s.CiudadId);

            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(m => m.Inventario)
                .WithMany()
                .HasForeignKey(m => m.InventarioId);

            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(m => m.Usuario)
                .WithMany()
                .HasForeignKey(m => m.UsuarioId);

            modelBuilder.Entity<Inventario>()
                .HasOne(i => i.Producto)
                .WithMany()
                .HasForeignKey(i => i.ProductoId);

            modelBuilder.Entity<Inventario>()
                .HasOne(i => i.Sucursal)
                .WithMany()
                .HasForeignKey(i => i.SucursalId);

            modelBuilder.Entity<Promocion>()
                .HasOne(p => p.Emprendimiento)
                .WithMany()
                .HasForeignKey(p => p.EmprendimientoId);

            modelBuilder.Entity<Resena>()
                .HasOne(r => r.Producto)
                .WithMany()
                .HasForeignKey(r => r.ProductoId);

            modelBuilder.Entity<Resena>()
                .HasOne(r => r.Cliente)
                .WithMany()
                .HasForeignKey(r => r.ClienteId);

            modelBuilder.Entity<Categoria>()
                .HasOne(c => c.Emprendimiento)
                .WithMany()
                .HasForeignKey(c => c.EmprendimientoId);

            modelBuilder.Entity<Permiso>()
                .Property(p => p.Modulo)
                .HasMaxLength(100);

            modelBuilder.Entity<Auditoria>()
                .HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId);

            modelBuilder.Entity<Configuracion>()
                .HasOne(c => c.Emprendimiento)
                .WithMany()
                .HasForeignKey(c => c.EmprendimientoId);

            modelBuilder.Entity<Notificacion>()
                .HasOne(n => n.Usuario)
                .WithMany()
                .HasForeignKey(n => n.UsuarioId);

            modelBuilder.Entity<Reporte>()
                .HasOne(r => r.Emprendimiento)
                .WithMany()
                .HasForeignKey(r => r.EmprendimientoId);

            modelBuilder.Entity<Dashboard>()
                .HasOne(d => d.Emprendimiento)
                .WithMany()
                .HasForeignKey(d => d.EmprendimientoId);

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Venta)
                .WithMany(v => v.DetallesVenta)
                .HasForeignKey(d => d.VentaId);

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ProductoId);

modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Venta)
                .WithMany(v => v.Pedidos)
                .HasForeignKey(p => p.VentaId);
        }
    }
}