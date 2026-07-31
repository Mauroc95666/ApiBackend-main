/* ============================================================
   init.sql  -  Panadería Kiataque
   Crea la base DB_Panaderia2, todas las tablas y datos semilla.
   Lo ejecuta el contenedor db-init una sola vez al levantar.
   ============================================================ */
IF DB_ID('DB_Panaderia2') IS NULL
    CREATE DATABASE DB_Panaderia2;
GO
USE DB_Panaderia2;
GO
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Ciudad] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(max) NOT NULL,
        [Departamento] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Ciudad] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Emprendimiento] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(max) NOT NULL,
        [NIT_RUC] nvarchar(max) NOT NULL,
        [Direccion] nvarchar(max) NULL,
        [Telefono] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        CONSTRAINT [PK_Emprendimiento] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [MetodoPago] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(max) NOT NULL,
        [Descripcion] nvarchar(max) NULL,
        CONSTRAINT [PK_MetodoPago] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Permiso] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(max) NOT NULL,
        [Modulo] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Permiso] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Producto] (
        [Id] int NOT NULL IDENTITY,
        [EmprendimientoId] int NULL,
        [CategoriaId] int NULL,
        [ProveedorId] int NULL,
        [Nombre] nvarchar(max) NOT NULL,
        [Precio] decimal(10,2) NOT NULL,
        [Stock] int NULL,
        [ImagenUrl] nvarchar(max) NULL,
        CONSTRAINT [PK_Producto] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Proveedor] (
        [Id] int NOT NULL IDENTITY,
        [EmprendimientoId] int NOT NULL,
        [Nombre] nvarchar(max) NOT NULL,
        [Contacto] nvarchar(max) NULL,
        [Telefono] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        CONSTRAINT [PK_Proveedor] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Rol] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(max) NOT NULL,
        [Descripcion] nvarchar(max) NULL,
        CONSTRAINT [PK_Rol] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Categoria] (
        [Id] int NOT NULL IDENTITY,
        [EmprendimientoId] int NOT NULL,
        [Nombre] nvarchar(max) NOT NULL,
        [Descripcion] nvarchar(max) NULL,
        CONSTRAINT [PK_Categoria] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Categoria_Emprendimiento_EmprendimientoId] FOREIGN KEY ([EmprendimientoId]) REFERENCES [Emprendimiento] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Cliente] (
        [Id] int NOT NULL IDENTITY,
        [EmprendimientoId] int NOT NULL,
        [Nombre] nvarchar(max) NOT NULL,
        [CI_NIT] nvarchar(max) NULL,
        [Telefono] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [Direccion] nvarchar(max) NULL,
        CONSTRAINT [PK_Cliente] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Cliente_Emprendimiento_EmprendimientoId] FOREIGN KEY ([EmprendimientoId]) REFERENCES [Emprendimiento] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Configuracion] (
        [Id] int NOT NULL IDENTITY,
        [EmprendimientoId] int NOT NULL,
        [Clave] nvarchar(max) NOT NULL,
        [Valor] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Configuracion] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Configuracion_Emprendimiento_EmprendimientoId] FOREIGN KEY ([EmprendimientoId]) REFERENCES [Emprendimiento] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Dashboard] (
        [Id] int NOT NULL IDENTITY,
        [EmprendimientoId] int NOT NULL,
        [MetricasJson] nvarchar(max) NULL,
        [UltimaActualizacion] datetime2 NULL,
        CONSTRAINT [PK_Dashboard] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Dashboard_Emprendimiento_EmprendimientoId] FOREIGN KEY ([EmprendimientoId]) REFERENCES [Emprendimiento] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Promocion] (
        [Id] int NOT NULL IDENTITY,
        [EmprendimientoId] int NOT NULL,
        [Nombre] nvarchar(max) NOT NULL,
        [Descuento] decimal(18,2) NOT NULL,
        [FechaInicio] datetime2 NOT NULL,
        [FechaFin] datetime2 NOT NULL,
        CONSTRAINT [PK_Promocion] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Promocion_Emprendimiento_EmprendimientoId] FOREIGN KEY ([EmprendimientoId]) REFERENCES [Emprendimiento] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Reporte] (
        [Id] int NOT NULL IDENTITY,
        [EmprendimientoId] int NOT NULL,
        [Tipo] nvarchar(max) NOT NULL,
        [FechaGeneracion] datetime2 NULL,
        [Datos] nvarchar(max) NULL,
        CONSTRAINT [PK_Reporte] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Reporte_Emprendimiento_EmprendimientoId] FOREIGN KEY ([EmprendimientoId]) REFERENCES [Emprendimiento] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Sucursal] (
        [Id] int NOT NULL IDENTITY,
        [EmprendimientoId] int NOT NULL,
        [CiudadId] int NOT NULL,
        [Nombre] nvarchar(max) NOT NULL,
        [Direccion] nvarchar(max) NOT NULL,
        [Telefono] nvarchar(max) NULL,
        CONSTRAINT [PK_Sucursal] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Sucursal_Ciudad_CiudadId] FOREIGN KEY ([CiudadId]) REFERENCES [Ciudad] ([Id]),
        CONSTRAINT [FK_Sucursal_Emprendimiento_EmprendimientoId] FOREIGN KEY ([EmprendimientoId]) REFERENCES [Emprendimiento] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [RolPermiso] (
        [RolId] int NOT NULL,
        [PermisoId] int NOT NULL,
        CONSTRAINT [PK_RolPermiso] PRIMARY KEY ([RolId], [PermisoId]),
        CONSTRAINT [FK_RolPermiso_Permiso_PermisoId] FOREIGN KEY ([PermisoId]) REFERENCES [Permiso] ([Id]),
        CONSTRAINT [FK_RolPermiso_Rol_RolId] FOREIGN KEY ([RolId]) REFERENCES [Rol] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Usuario] (
        [Id] int NOT NULL IDENTITY,
        [EmprendimientoId] int NOT NULL,
        [RolId] int NOT NULL,
        [Nombre] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Usuario] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Usuario_Emprendimiento_EmprendimientoId] FOREIGN KEY ([EmprendimientoId]) REFERENCES [Emprendimiento] ([Id]),
        CONSTRAINT [FK_Usuario_Rol_RolId] FOREIGN KEY ([RolId]) REFERENCES [Rol] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Resena] (
        [Id] int NOT NULL IDENTITY,
        [ProductoId] int NOT NULL,
        [ClienteId] int NOT NULL,
        [Calificacion] int NULL,
        [Comentario] nvarchar(max) NULL,
        [Fecha] datetime2 NOT NULL,
        CONSTRAINT [PK_Resena] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Resena_Cliente_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Cliente] ([Id]),
        CONSTRAINT [FK_Resena_Producto_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Producto] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Inventario] (
        [Id] int NOT NULL IDENTITY,
        [ProductoId] int NOT NULL,
        [SucursalId] int NOT NULL,
        [StockActual] int NOT NULL,
        [StockMinimo] int NULL,
        CONSTRAINT [PK_Inventario] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Inventario_Producto_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Producto] ([Id]),
        CONSTRAINT [FK_Inventario_Sucursal_SucursalId] FOREIGN KEY ([SucursalId]) REFERENCES [Sucursal] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Auditoria] (
        [Id] int NOT NULL IDENTITY,
        [UsuarioId] int NULL,
        [Accion] nvarchar(max) NOT NULL,
        [TablaAfectada] nvarchar(max) NOT NULL,
        [Fecha] datetime2 NULL,
        [Detalles] nvarchar(max) NULL,
        CONSTRAINT [PK_Auditoria] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Auditoria_Usuario_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Empleado] (
        [Id] int NOT NULL IDENTITY,
        [UsuarioId] int NOT NULL,
        [SucursalId] int NOT NULL,
        [Cargo] nvarchar(max) NOT NULL,
        [Salario] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Empleado] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Empleado_Sucursal_SucursalId] FOREIGN KEY ([SucursalId]) REFERENCES [Sucursal] ([Id]),
        CONSTRAINT [FK_Empleado_Usuario_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Notificacion] (
        [Id] int NOT NULL IDENTITY,
        [UsuarioId] int NOT NULL,
        [Mensaje] nvarchar(max) NOT NULL,
        [Leido] bit NULL,
        [Fecha] datetime2 NULL,
        CONSTRAINT [PK_Notificacion] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notificacion_Usuario_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Venta] (
        [Id] int NOT NULL IDENTITY,
        [SucursalId] int NOT NULL,
        [ClienteId] int NOT NULL,
        [UsuarioId] int NOT NULL,
        [MetodoPagoId] int NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Venta] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Venta_Cliente_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Cliente] ([Id]),
        CONSTRAINT [FK_Venta_MetodoPago_MetodoPagoId] FOREIGN KEY ([MetodoPagoId]) REFERENCES [MetodoPago] ([Id]),
        CONSTRAINT [FK_Venta_Sucursal_SucursalId] FOREIGN KEY ([SucursalId]) REFERENCES [Sucursal] ([Id]),
        CONSTRAINT [FK_Venta_Usuario_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [MovimientoInventario] (
        [Id] int NOT NULL IDENTITY,
        [InventarioId] int NOT NULL,
        [UsuarioId] int NOT NULL,
        [Tipo] nvarchar(max) NOT NULL,
        [Cantidad] int NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [Motivo] nvarchar(max) NULL,
        CONSTRAINT [PK_MovimientoInventario] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MovimientoInventario_Inventario_InventarioId] FOREIGN KEY ([InventarioId]) REFERENCES [Inventario] ([Id]),
        CONSTRAINT [FK_MovimientoInventario_Usuario_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [DetalleVenta] (
        [Id] int NOT NULL IDENTITY,
        [VentaId] int NOT NULL,
        [ProductoId] int NOT NULL,
        [Cantidad] int NOT NULL,
        [PrecioUnitario] decimal(18,2) NOT NULL,
        [Subtotal] AS Cantidad * PrecioUnitario,
        CONSTRAINT [PK_DetalleVenta] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DetalleVenta_Producto_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Producto] ([Id]),
        CONSTRAINT [FK_DetalleVenta_Venta_VentaId] FOREIGN KEY ([VentaId]) REFERENCES [Venta] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE TABLE [Pedido] (
        [Id] int NOT NULL IDENTITY,
        [VentaId] int NOT NULL,
        [Estado] nvarchar(max) NOT NULL,
        [FechaEntrega] datetime2 NULL,
        CONSTRAINT [PK_Pedido] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Pedido_Venta_VentaId] FOREIGN KEY ([VentaId]) REFERENCES [Venta] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Auditoria_UsuarioId] ON [Auditoria] ([UsuarioId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Categoria_EmprendimientoId] ON [Categoria] ([EmprendimientoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Cliente_EmprendimientoId] ON [Cliente] ([EmprendimientoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Configuracion_EmprendimientoId] ON [Configuracion] ([EmprendimientoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Dashboard_EmprendimientoId] ON [Dashboard] ([EmprendimientoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DetalleVenta_ProductoId] ON [DetalleVenta] ([ProductoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DetalleVenta_VentaId] ON [DetalleVenta] ([VentaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Empleado_SucursalId] ON [Empleado] ([SucursalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Empleado_UsuarioId] ON [Empleado] ([UsuarioId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Inventario_ProductoId] ON [Inventario] ([ProductoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Inventario_SucursalId] ON [Inventario] ([SucursalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MovimientoInventario_InventarioId] ON [MovimientoInventario] ([InventarioId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MovimientoInventario_UsuarioId] ON [MovimientoInventario] ([UsuarioId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notificacion_UsuarioId] ON [Notificacion] ([UsuarioId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Pedido_VentaId] ON [Pedido] ([VentaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Promocion_EmprendimientoId] ON [Promocion] ([EmprendimientoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reporte_EmprendimientoId] ON [Reporte] ([EmprendimientoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Resena_ClienteId] ON [Resena] ([ClienteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Resena_ProductoId] ON [Resena] ([ProductoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RolPermiso_PermisoId] ON [RolPermiso] ([PermisoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sucursal_CiudadId] ON [Sucursal] ([CiudadId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sucursal_EmprendimientoId] ON [Sucursal] ([EmprendimientoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Usuario_EmprendimientoId] ON [Usuario] ([EmprendimientoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Usuario_RolId] ON [Usuario] ([RolId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Venta_ClienteId] ON [Venta] ([ClienteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Venta_MetodoPagoId] ON [Venta] ([MetodoPagoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Venta_SucursalId] ON [Venta] ([SucursalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Venta_UsuarioId] ON [Venta] ([UsuarioId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021547_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260730021547_InitialCreate', N'8.0.0');
END;
GO

COMMIT;
GO


/* ---------- Datos iniciales ---------- */
IF NOT EXISTS (SELECT 1 FROM [Emprendimiento])
BEGIN
    SET IDENTITY_INSERT [Emprendimiento] ON;
    INSERT INTO [Emprendimiento] ([Id],[Nombre],[NIT_RUC],[Direccion],[Telefono],[Email])
        VALUES (1,'Panadería Kiataque','1023456789','Av. Principal #123, Santa Cruz','700-00000','contacto@kiataque.bo');
    SET IDENTITY_INSERT [Emprendimiento] OFF;

    SET IDENTITY_INSERT [Ciudad] ON;
    INSERT INTO [Ciudad] ([Id],[Nombre],[Departamento]) VALUES (1,'Santa Cruz de la Sierra','Santa Cruz');
    SET IDENTITY_INSERT [Ciudad] OFF;

    SET IDENTITY_INSERT [Rol] ON;
    INSERT INTO [Rol] ([Id],[Nombre],[Descripcion]) VALUES
        (1,'Superadmin','Acceso total al sistema'),
        (2,'Cliente','Cliente de la tienda');
    SET IDENTITY_INSERT [Rol] OFF;

    SET IDENTITY_INSERT [MetodoPago] ON;
    INSERT INTO [MetodoPago] ([Id],[Nombre],[Descripcion]) VALUES
        (1,'Efectivo',NULL),(2,'QR',NULL),(3,'Tarjeta',NULL);
    SET IDENTITY_INSERT [MetodoPago] OFF;

    SET IDENTITY_INSERT [Categoria] ON;
    INSERT INTO [Categoria] ([Id],[EmprendimientoId],[Nombre],[Descripcion]) VALUES
        (1,1,'Panes',NULL),(2,1,'Salados',NULL),(3,1,'Dulces',NULL),(4,1,'Bebidas',NULL);
    SET IDENTITY_INSERT [Categoria] OFF;

    SET IDENTITY_INSERT [Usuario] ON;
    INSERT INTO [Usuario] ([Id],[EmprendimientoId],[RolId],[Nombre],[Email],[PasswordHash],[Activo])
        VALUES (1,1,1,'Super Admin','superadmin@kiataque.bo','Admin123',1);
    SET IDENTITY_INSERT [Usuario] OFF;

    SET IDENTITY_INSERT [Sucursal] ON;
    INSERT INTO [Sucursal] ([Id],[EmprendimientoId],[CiudadId],[Nombre],[Direccion],[Telefono])
        VALUES (1,1,1,'Sucursal Centro','Av. Principal #123','700-00000');
    SET IDENTITY_INSERT [Sucursal] OFF;

    SET IDENTITY_INSERT [Cliente] ON;
    INSERT INTO [Cliente] ([Id],[EmprendimientoId],[Nombre],[CI_NIT],[Telefono],[Email])
        VALUES (1,1,'Consumidor final',NULL,NULL,NULL);
    SET IDENTITY_INSERT [Cliente] OFF;

    SET IDENTITY_INSERT [Producto] ON;
    INSERT INTO [Producto] ([Id],[EmprendimientoId],[CategoriaId],[ProveedorId],[Nombre],[Precio],[Stock]) VALUES
        (1,1,2,NULL,'Cuñapé',3.50,128),
        (2,1,2,NULL,'Empanada de queso',5.00,94),
        (3,1,1,NULL,'Pan de batalla',0.50,340),
        (4,1,3,NULL,'Masaco dulce',8.00,22),
        (5,1,3,NULL,'Torta de arroz',6.50,9),
        (6,1,2,NULL,'Sonso de yuca',7.00,45),
        (7,1,1,NULL,'Pan integral',1.20,76),
        (8,1,4,NULL,'Api con pastel',10.00,30);
    SET IDENTITY_INSERT [Producto] OFF;
END;
GO
