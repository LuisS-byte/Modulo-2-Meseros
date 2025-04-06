using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Modulo_2_Meseros.Models;

namespace Modulo_2_Meseros.Context;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categorias> Categorias { get; set; }
    

    public virtual DbSet<Combo> Combos { get; set; }

    public virtual DbSet<DetallePedido> DetallePedidos { get; set; }

    public virtual DbSet<Detallefactura> Detallefacturas { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<EstadoPedido> EstadoPedidos { get; set; }

    public virtual DbSet<Factura> Facturas { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<MenuItems> MenuItems { get; set; }

    public virtual DbSet<Mesa> Mesas { get; set; }

    public virtual DbSet<MetodosPago> MetodosPagos { get; set; }

    public virtual DbSet<Pedido> Pedidos { get; set; }

    public virtual DbSet<Platos> Platos { get; set; }

    public virtual DbSet<Promocione> Promociones { get; set; }

    public virtual DbSet<PromocionesItem> PromocionesItems { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Tipopago> Tipopagos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categorias>(entity =>
        {
            entity.HasKey(e => e.CategoriaId).HasName("PK__Categori__F353C1C543495682");

            entity.HasIndex(e => e.Nombre, "UQ__Categori__75E3EFCF9DF01757").IsUnique();

            entity.Property(e => e.CategoriaId).HasColumnName("CategoriaID");
            entity.Property(e => e.Descripcion).HasMaxLength(255);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Combo>(entity =>
        {
            entity.HasKey(e => e.ComboId).HasName("PK__Combos__DD42580EA909DB39");

            entity.Property(e => e.ComboId).HasColumnName("ComboID");
            entity.Property(e => e.Descripcion).HasMaxLength(255);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImagenUrl).HasColumnName("ImagenURL");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");

            entity.HasMany(d => d.Platos).WithMany(p => p.Combos)
                .UsingEntity<Dictionary<string, object>>(
                    "PlatosCombo",
                    r => r.HasOne<Platos>().WithMany()
                        .HasForeignKey("PlatoId")
                        .HasConstraintName("FK__PlatosCom__Plato__4222D4EF"),
                    l => l.HasOne<Combo>().WithMany()
                        .HasForeignKey("ComboId")
                        .HasConstraintName("FK__PlatosCom__Combo__412EB0B6"),
                    j =>
                    {
                        j.HasKey("ComboId", "PlatoId").HasName("PK__PlatosCo__49E9B9A4A50CA975");
                        j.ToTable("PlatosCombos");
                        j.IndexerProperty<int>("ComboId").HasColumnName("ComboID");
                        j.IndexerProperty<int>("PlatoId").HasColumnName("PlatoID");
                    });
        });

        modelBuilder.Entity<DetallePedido>(entity =>
        {
            entity.HasKey(e => new { e.IdPedido, e.IdMenu }).HasName("PK__DETALLE___B42EA0EC7F265A46");

            entity.ToTable("DETALLE_PEDIDO");

            entity.Property(e => e.IdPedido).HasColumnName("ID_PEDIDO");
            entity.Property(e => e.IdMenu).HasColumnName("ID_MENU");
            entity.Property(e => e.DetCantidad).HasColumnName("DET_CANTIDAD");
            entity.Property(e => e.DetComentarios)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("DET_COMENTARIOS");
            entity.Property(e => e.DetPrecio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("DET_PRECIO");
            entity.Property(e => e.DetSubtotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("DET_SUBTOTAL");
            entity.Property(e => e.IdEstadopedido).HasColumnName("ID_ESTADOPEDIDO");

            entity.HasOne(d => d.IdEstadopedidoNavigation).WithMany(p => p.DetallePedidos)
                .HasForeignKey(d => d.IdEstadopedido)
                .HasConstraintName("FK__DETALLE_P__ID_ES__6A30C649");

            entity.HasOne(d => d.IdMenuNavigation).WithMany(p => p.DetallePedidos)
                .HasForeignKey(d => d.IdMenu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DETALLE_P__ID_ME__693CA210");

            entity.HasOne(d => d.IdPedidoNavigation).WithMany(p => p.DetallePedidos)
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DETALLE_P__ID_PE__68487DD7");
        });

        modelBuilder.Entity<Detallefactura>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__detallef__3213E83FE6B93176");

            entity.ToTable("detallefactura");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ComboId).HasColumnName("combo_id");
            entity.Property(e => e.FacturaId).HasColumnName("factura_id");
            entity.Property(e => e.PlatoId).HasColumnName("plato_id");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("subtotal");

            entity.HasOne(d => d.Factura).WithMany(p => p.Detallefacturas)
                .HasForeignKey(d => d.FacturaId)
                .HasConstraintName("FK__detallefa__subto__74AE54BC");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.EmpleadoId).HasName("PK__Empleado__958BE6F05152FC5A");

            entity.HasIndex(e => e.Email, "UQ__Empleado__A9D10534CEC706A2").IsUnique();

            entity.Property(e => e.EmpleadoId).HasColumnName("EmpleadoID");
            entity.Property(e => e.Apellido).HasMaxLength(100);
            entity.Property(e => e.Contrasena).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FechaIngreso).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.RolId).HasColumnName("RolID");
            entity.Property(e => e.Telefono).HasMaxLength(15);

            entity.HasOne(d => d.Rol).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Empleados__RolID__5812160E");
        });

        modelBuilder.Entity<EstadoPedido>(entity =>
        {
            entity.HasKey(e => e.IdEstadopedido).HasName("PK__ESTADO_P__DD52A8BDD0C65382");

            entity.ToTable("ESTADO_PEDIDO");

            entity.Property(e => e.IdEstadopedido)
                .ValueGeneratedNever()
                .HasColumnName("ID_ESTADOPEDIDO");
            entity.Property(e => e.EstadoNombre)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("ESTADO_NOMBRE");
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__factura__3213E83F8CBDB95B");

            entity.ToTable("factura");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha");
            entity.Property(e => e.PedidoId).HasColumnName("pedido_id");
            entity.Property(e => e.TipopagoId).HasColumnName("tipopago_id");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.Empleado).WithMany(p => p.Facturas)
                .HasForeignKey(d => d.EmpleadoId)
                .HasConstraintName("FK__factura__emplead__71D1E811");

            entity.HasOne(d => d.Pedido).WithMany(p => p.Facturas)
                .HasForeignKey(d => d.PedidoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__factura__pedido___6FE99F9F");

            entity.HasOne(d => d.Tipopago).WithMany(p => p.Facturas)
                .HasForeignKey(d => d.TipopagoId)
                .HasConstraintName("FK__factura__tipopag__70DDC3D8");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.MenuId).HasName("PK__Menu__C99ED2505ACC802A");

            entity.ToTable("Menu");

            entity.Property(e => e.MenuId).HasColumnName("MenuID");
            entity.Property(e => e.Descripcion).HasMaxLength(255);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<MenuItems>(entity =>
        {
            entity.HasKey(e => e.MenuItemId).HasName("PK__Menu_Ite__8943F7226E967256");

            entity.ToTable("Menu_Items");

            entity.Property(e => e.ComboId).HasColumnName("ComboID");
            entity.Property(e => e.MenuId).HasColumnName("MenuID");
            entity.Property(e => e.PlatoId).HasColumnName("PlatoID");
            entity.Property(e => e.PromocionId).HasColumnName("PromocionID");

            entity.HasOne(d => d.Combo).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.ComboId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Menu_Item__Combo__5070F446");

            entity.HasOne(d => d.Menu).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.MenuId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Menu_Item__MenuI__4E88ABD4");

            entity.HasOne(d => d.Platos).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.PlatoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Menu_Item__Plato__4F7CD00D");

            entity.HasOne(d => d.Promocion).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.PromocionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Menu_Item__Promo__5165187F");
        });

        modelBuilder.Entity<Mesa>(entity =>
        {
            entity.HasKey(e => e.MesaId).HasName("PK__Mesas__6A4196C807ABBB95");

            entity.HasIndex(e => e.NumeroMesa, "UQ__Mesas__A5588DD2D1C15B4A").IsUnique();

            entity.Property(e => e.MesaId).HasColumnName("MesaID");
            entity.Property(e => e.Estado).HasDefaultValue(false);
        });

        modelBuilder.Entity<MetodosPago>(entity =>
        {
            entity.HasKey(e => e.MetodoId).HasName("PK__MetodosP__5C1E3E31799DF35F");

            entity.ToTable("MetodosPago");

            entity.HasIndex(e => e.Nombre, "UQ__MetodosP__75E3EFCF467043D5").IsUnique();

            entity.Property(e => e.MetodoId).HasColumnName("MetodoID");
            entity.Property(e => e.Descripcion).HasMaxLength(255);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.IdPedido).HasName("PK__PEDIDO__A05C2F2A06632762");

            entity.ToTable("PEDIDO");

            entity.Property(e => e.IdPedido)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID_PEDIDO");
            entity.Property(e => e.IdEstadopedido).HasColumnName("ID_ESTADOPEDIDO");
            entity.Property(e => e.IdMesa).HasColumnName("ID_MESA");
            entity.Property(e => e.IdMesero).HasColumnName("ID_MESERO");

            entity.HasOne(d => d.IdEstadopedidoNavigation).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.IdEstadopedido)
                .HasConstraintName("FK__PEDIDO__ID_ESTAD__656C112C");

            entity.HasOne(d => d.IdMesaNavigation).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.IdMesa)
                .HasConstraintName("FK__PEDIDO__ID_MESA__6383C8BA");

            entity.HasOne(d => d.IdMeseroNavigation).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.IdMesero)
                .HasConstraintName("FK__PEDIDO__ID_MESER__6477ECF3");
        });

        modelBuilder.Entity<Platos>(entity =>
        {
            entity.HasKey(e => e.PlatoId).HasName("PK__Platos__4ABE1AA84BF767D0");

            entity.Property(e => e.PlatoId).HasColumnName("PlatoID");
            entity.Property(e => e.CategoriaId).HasColumnName("CategoriaID");
            entity.Property(e => e.Descripcion).HasMaxLength(255);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImagenUrl).HasColumnName("ImagenURL");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Platos)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Platos__Categori__3B75D760");
        });

        modelBuilder.Entity<Promocione>(entity =>
        {
            entity.HasKey(e => e.PromocionId).HasName("PK__Promocio__2DA61DBD71406A5F");

            entity.Property(e => e.PromocionId).HasColumnName("PromocionID");
            entity.Property(e => e.Descripcion).HasMaxLength(255);
            entity.Property(e => e.Descuento).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ImagenUrl).HasColumnName("ImagenURL");
        });

        modelBuilder.Entity<PromocionesItem>(entity =>
        {
            entity.HasKey(e => new { e.PromocionId, e.PlatoId, e.ComboId }).HasName("PK__Promocio__B6D0BE4F8EB41B6A");

            entity.ToTable("Promociones_Items");

            entity.Property(e => e.PromocionId).HasColumnName("PromocionID");
            entity.Property(e => e.PlatoId).HasColumnName("PlatoID");
            entity.Property(e => e.ComboId).HasColumnName("ComboID");

            entity.HasOne(d => d.Combo).WithMany(p => p.PromocionesItems)
                .HasForeignKey(d => d.ComboId)
                .HasConstraintName("FK__Promocion__Combo__48CFD27E");

            entity.HasOne(d => d.Platos).WithMany(p => p.PromocionesItems)
                .HasForeignKey(d => d.PlatoId)
                .HasConstraintName("FK__Promocion__Plato__47DBAE45");

            entity.HasOne(d => d.Promocion).WithMany(p => p.PromocionesItems)
                .HasForeignKey(d => d.PromocionId)
                .HasConstraintName("FK__Promocion__Promo__46E78A0C");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RolId).HasName("PK__Roles__F92302D1E3083BB9");

            entity.Property(e => e.RolId).HasColumnName("RolID");
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<Tipopago>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tipopago__3213E83F73A988CD");

            entity.ToTable("tipopago");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}