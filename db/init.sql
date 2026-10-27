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
    -- La contraseña NO se escribe aca: se deja un valor imposible de tipear y
    -- el backend pone la real al arrancar, tomandola de la variable de
    -- entorno SUPERADMIN_PASSWORD. Si esa variable esta vacia, el backend
    -- genera una al azar y la escribe en su log.
    INSERT INTO [Usuario] ([Id],[EmprendimientoId],[RolId],[Nombre],[Email],[PasswordHash],[Activo])
        VALUES (1,1,1,'Super Admin','superadmin@kiataque.bo',
                N'SIN-CLAVE-' + CONVERT(nvarchar(50), NEWID()), 1);
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


/* =========================================================================
   Historial de estados del pedido  (HU-05)
   -------------------------------------------------------------------------
   Va como un bloque aparte, con su propio MigrationId, para que se aplique
   sobre una base que ya existe sin volver a crear nada ni tocar los datos
   que ya estan cargados. Si la base es nueva, corre igual despues del
   InitialCreate.
   ========================================================================= */
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261026000000_HistorialEstadoPedido'
)
BEGIN
    /* El estado pasa a ser un texto corto y normalizado: antes era
       nvarchar(max) y admitia cualquier cosa. */
    UPDATE [Pedido] SET [Estado] = UPPER(LTRIM(RTRIM([Estado])));

    /* Cualquier valor que haya quedado fuera de los cuatro validos se lleva
       a PENDIENTE para que el CHECK pueda crearse sin rechazar filas. */
    UPDATE [Pedido]
    SET [Estado] = N'PENDIENTE'
    WHERE [Estado] NOT IN (N'PENDIENTE', N'EN PREPARACION', N'LISTO', N'ENTREGADO');

    ALTER TABLE [Pedido] ALTER COLUMN [Estado] nvarchar(20) NOT NULL;

    ALTER TABLE [Pedido] ADD CONSTRAINT [CK_Pedido_Estado]
        CHECK ([Estado] IN (N'PENDIENTE', N'EN PREPARACION', N'LISTO', N'ENTREGADO'));

    CREATE TABLE [PedidoEstadoHistorial] (
        [Id] int NOT NULL IDENTITY,
        [PedidoId] int NOT NULL,
        [EstadoAnterior] nvarchar(20) NULL,
        [Estado] nvarchar(20) NOT NULL,
        [UsuarioId] int NULL,
        [Fecha] datetime2 NOT NULL,
        [Observacion] nvarchar(200) NULL,
        CONSTRAINT [PK_PedidoEstadoHistorial] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PedidoEstadoHistorial_Pedido_PedidoId]
            FOREIGN KEY ([PedidoId]) REFERENCES [Pedido] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PedidoEstadoHistorial_Usuario_UsuarioId]
            FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id]),
        CONSTRAINT [CK_PedidoEstadoHistorial_Estado]
            CHECK ([Estado] IN (N'PENDIENTE', N'EN PREPARACION', N'LISTO', N'ENTREGADO'))
    );

    CREATE INDEX [IX_PedidoEstadoHistorial_PedidoId]
        ON [PedidoEstadoHistorial] ([PedidoId]);

    /* Los pedidos que ya existian no tienen historia: se les deja una fila
       inicial con su estado actual y sin usuario, porque nadie registro ese
       cambio. La observacion deja claro de donde salio esa fila. */
    INSERT INTO [PedidoEstadoHistorial]
        ([PedidoId],[EstadoAnterior],[Estado],[UsuarioId],[Fecha],[Observacion])
    SELECT [Id], NULL, [Estado], NULL, SYSDATETIME(),
           N'Estado al habilitarse el historial (sin autor registrado)'
    FROM [Pedido];

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261026000000_HistorialEstadoPedido', N'8.0.0');
END;
GO


/* =========================================================================
   Permisos por modulo  (HU-08)
   -------------------------------------------------------------------------
   Hasta ahora las tablas Permiso y RolPermiso existian vacias y el panel
   decidia que ve cada rol mirando SU NOMBRE. Eso hacia que renombrar un rol
   le cambiara los permisos en silencio, y que un rol nuevo no viera nada.

   Este bloque carga un permiso por cada modulo del panel y se los asigna a
   los roles que ya existen. La asignacion inicial se hace por palabra clave
   del nombre del rol, que es el mismo criterio que usaba el panel: es una
   migracion de una sola vez, no una regla que siga corriendo. De aca en
   adelante los permisos se editan desde la pantalla de Roles.
   ========================================================================= */
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261026010000_PermisosPorModulo'
)
BEGIN
    DECLARE @Modulos TABLE ([Modulo] nvarchar(50), [Nombre] nvarchar(100));
    INSERT INTO @Modulos ([Modulo],[Nombre]) VALUES
        (N'inicio',          N'Ver el panel de control'),
        (N'productos',       N'Gestionar productos'),
        (N'ventas',          N'Registrar ventas'),
        (N'pedidos',         N'Gestionar pedidos'),
        (N'inventario',      N'Gestionar inventario'),
        (N'sucursales',      N'Gestionar sucursales'),
        (N'categorias',      N'Gestionar categorias'),
        (N'clientes',        N'Gestionar clientes'),
        (N'usuarios',        N'Gestionar usuarios'),
        (N'roles',           N'Gestionar roles y permisos'),
        (N'ciudades',        N'Gestionar ciudades'),
        (N'metodospago',     N'Gestionar metodos de pago'),
        (N'proveedores',     N'Gestionar proveedores'),
        (N'emprendimientos', N'Gestionar emprendimientos');

    INSERT INTO [Permiso] ([Nombre],[Modulo])
    SELECT m.[Nombre], m.[Modulo]
    FROM @Modulos m
    WHERE NOT EXISTS (SELECT 1 FROM [Permiso] p WHERE p.[Modulo] = m.[Modulo]);

    /* Administrador, superadmin o gerente: todos los modulos. */
    INSERT INTO [RolPermiso] ([RolId],[PermisoId])
    SELECT r.[Id], p.[Id]
    FROM [Rol] r CROSS JOIN [Permiso] p
    WHERE (LOWER(r.[Nombre]) LIKE N'%admin%'
        OR LOWER(r.[Nombre]) LIKE N'%super%'
        OR LOWER(r.[Nombre]) LIKE N'%gerente%')
      AND NOT EXISTS (
          SELECT 1 FROM [RolPermiso] rp
          WHERE rp.[RolId] = r.[Id] AND rp.[PermisoId] = p.[Id]);

    /* Inventario, almacen, bodega o stock: inicio, inventario y productos. */
    INSERT INTO [RolPermiso] ([RolId],[PermisoId])
    SELECT r.[Id], p.[Id]
    FROM [Rol] r CROSS JOIN [Permiso] p
    WHERE (LOWER(r.[Nombre]) LIKE N'%inventario%'
        OR LOWER(r.[Nombre]) LIKE N'%almac%'
        OR LOWER(r.[Nombre]) LIKE N'%bodega%'
        OR LOWER(r.[Nombre]) LIKE N'%stock%')
      AND p.[Modulo] IN (N'inicio', N'inventario', N'productos')
      AND NOT EXISTS (
          SELECT 1 FROM [RolPermiso] rp
          WHERE rp.[RolId] = r.[Id] AND rp.[PermisoId] = p.[Id]);

    /* Cajero o caja: inicio, ventas y pedidos. */
    INSERT INTO [RolPermiso] ([RolId],[PermisoId])
    SELECT r.[Id], p.[Id]
    FROM [Rol] r CROSS JOIN [Permiso] p
    WHERE (LOWER(r.[Nombre]) LIKE N'%cajero%' OR LOWER(r.[Nombre]) LIKE N'%caja%')
      AND p.[Modulo] IN (N'inicio', N'ventas', N'pedidos')
      AND NOT EXISTS (
          SELECT 1 FROM [RolPermiso] rp
          WHERE rp.[RolId] = r.[Id] AND rp.[PermisoId] = p.[Id]);

    /* Vendedor o ventas: lo del cajero mas ver productos. */
    INSERT INTO [RolPermiso] ([RolId],[PermisoId])
    SELECT r.[Id], p.[Id]
    FROM [Rol] r CROSS JOIN [Permiso] p
    WHERE (LOWER(r.[Nombre]) LIKE N'%vendedor%' OR LOWER(r.[Nombre]) LIKE N'%venta%')
      AND p.[Modulo] IN (N'inicio', N'ventas', N'pedidos', N'productos')
      AND NOT EXISTS (
          SELECT 1 FROM [RolPermiso] rp
          WHERE rp.[RolId] = r.[Id] AND rp.[PermisoId] = p.[Id]);

    /* El rol Cliente no entra al panel: queda sin permisos a proposito. */

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261026010000_PermisosPorModulo', N'8.0.0');
END;
GO


/* =========================================================================
   Permiso de Reportes  (HU-09)
   -------------------------------------------------------------------------
   La pantalla de reportes es nueva, asi que necesita su propio permiso. Se
   le da a los mismos roles que ya podian ver el panel completo.
   ========================================================================= */
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261026020000_PermisoReportes'
)
BEGIN
    INSERT INTO [Permiso] ([Nombre],[Modulo])
    SELECT N'Ver reportes', N'reportes'
    WHERE NOT EXISTS (SELECT 1 FROM [Permiso] WHERE [Modulo] = N'reportes');

    INSERT INTO [RolPermiso] ([RolId],[PermisoId])
    SELECT r.[Id], p.[Id]
    FROM [Rol] r CROSS JOIN [Permiso] p
    WHERE p.[Modulo] = N'reportes'
      AND (LOWER(r.[Nombre]) LIKE N'%admin%'
        OR LOWER(r.[Nombre]) LIKE N'%super%'
        OR LOWER(r.[Nombre]) LIKE N'%gerente%')
      AND NOT EXISTS (
          SELECT 1 FROM [RolPermiso] rp
          WHERE rp.[RolId] = r.[Id] AND rp.[PermisoId] = p.[Id]);

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261026020000_PermisoReportes', N'8.0.0');
END;
GO


/* =========================================================================
   Super-usuario y usuario de sistema  (HU-11)
   -------------------------------------------------------------------------
   Agrega dos marcas al usuario:
   - EsSistema: cuentas internas que no se listan ni se pueden borrar. Son el
     super-usuario de arranque y el usuario con el que la tienda publica
     registra sus ventas.
   - DebeCambiarPassword: obliga a cambiar la clave en el proximo ingreso.
     Se marca solo a quienes todavia tienen la contraseña sin hashear, que son
     los que nunca entraron desde que se activo el hasheo.
   ========================================================================= */
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261026030000_SuperUsuarioYUsuarioDeSistema'
)
BEGIN
    IF COL_LENGTH('Usuario', 'EsSistema') IS NULL
        ALTER TABLE [Usuario] ADD [EsSistema] bit NOT NULL DEFAULT 0;

    IF COL_LENGTH('Usuario', 'DebeCambiarPassword') IS NULL
        ALTER TABLE [Usuario] ADD [DebeCambiarPassword] bit NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261026030000_SuperUsuarioYUsuarioDeSistema'
)
BEGIN
    /* El usuario sembrado es el super-usuario de arranque. */
    UPDATE [Usuario] SET [EsSistema] = 1 WHERE [Id] = 1;

    /* Quien todavia tiene la clave en texto plano nunca entro desde que se
       activo el hasheo: a ese se le pide cambiarla. A quien ya entro no se le
       molesta, porque su clave ya quedo hasheada. */
    UPDATE [Usuario]
    SET [DebeCambiarPassword] = 1
    WHERE [PasswordHash] NOT LIKE N'PBKDF2$%';

    /* Usuario con el que la tienda publica registra sus ventas. Antes esas
       ventas se le atribuian al super-usuario, asi que parecia que las habia
       hecho el administrador desde el mostrador. No puede iniciar sesion:
       su contraseña queda en un valor que ningun formulario produce. */
    IF NOT EXISTS (SELECT 1 FROM [Usuario] WHERE [Email] = N'tienda@kiataque.bo')
    BEGIN
        INSERT INTO [Usuario] ([EmprendimientoId],[RolId],[Nombre],[Email],[PasswordHash],[Activo],[EsSistema],[DebeCambiarPassword])
        SELECT 1,
               (SELECT TOP 1 [Id] FROM [Rol] WHERE LOWER([Nombre]) LIKE N'%cliente%' ORDER BY [Id]),
               N'Tienda en línea',
               N'tienda@kiataque.bo',
               N'SIN-INGRESO-' + CONVERT(nvarchar(50), NEWID()),
               1, 1, 0;
    END;

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261026030000_SuperUsuarioYUsuarioDeSistema', N'8.0.0');
END;
GO


/* =========================================================================
   Tiempos de servicio del pedido  (HU-12)
   -------------------------------------------------------------------------
   El pedido no guardaba cuando nacio ni cuando se entrego de verdad: solo
   tenia FechaEntrega, que es la fecha planificada y se carga a mano.
   Se agregan dos marcas de tiempo reales y se completan hacia atras con lo
   que ya quedo en el historial de estados.
   ========================================================================= */
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261026040000_TiemposDePedido'
)
BEGIN
    IF COL_LENGTH('Pedido', 'FechaCreacion') IS NULL
        ALTER TABLE [Pedido] ADD [FechaCreacion] datetime2 NULL;

    IF COL_LENGTH('Pedido', 'FechaEntregaReal') IS NULL
        ALTER TABLE [Pedido] ADD [FechaEntregaReal] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261026040000_TiemposDePedido'
)
BEGIN
    /* Nacimiento: la primera marca del historial. Si el pedido es anterior al
       historial, se usa la fecha de su venta. */
    UPDATE p
    SET [FechaCreacion] = COALESCE(
        (SELECT MIN(h.[Fecha]) FROM [PedidoEstadoHistorial] h WHERE h.[PedidoId] = p.[Id]),
        (SELECT v.[Fecha] FROM [Venta] v WHERE v.[Id] = p.[VentaId]))
    FROM [Pedido] p
    WHERE p.[FechaCreacion] IS NULL;

    /* Entrega real: la marca en que paso a ENTREGADO. Los que todavia no
       llegaron a ese estado quedan sin fecha, que es lo correcto. */
    UPDATE p
    SET [FechaEntregaReal] =
        (SELECT MAX(h.[Fecha]) FROM [PedidoEstadoHistorial] h
         WHERE h.[PedidoId] = p.[Id] AND h.[Estado] = N'ENTREGADO')
    FROM [Pedido] p
    WHERE p.[FechaEntregaReal] IS NULL AND p.[Estado] = N'ENTREGADO';

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261026040000_TiemposDePedido', N'8.0.0');
END;
GO


/* =========================================================================
   Permiso de Respaldos  (HU-13)
   -------------------------------------------------------------------------
   Respaldar y sobre todo RESTAURAR la base no es una tarea de cualquiera: una
   restauracion reemplaza todos los datos actuales. Por eso el modulo tiene su
   propio permiso y no se lo hereda nadie: se les da solo a los roles de
   administracion, igual que se hizo con reportes.
   ========================================================================= */
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261026050000_PermisoRespaldos'
)
BEGIN
    INSERT INTO [Permiso] ([Nombre],[Modulo])
    SELECT N'Respaldar y restaurar la base', N'respaldos'
    WHERE NOT EXISTS (SELECT 1 FROM [Permiso] WHERE [Modulo] = N'respaldos');

    INSERT INTO [RolPermiso] ([RolId],[PermisoId])
    SELECT r.[Id], p.[Id]
    FROM [Rol] r CROSS JOIN [Permiso] p
    WHERE p.[Modulo] = N'respaldos'
      AND (LOWER(r.[Nombre]) LIKE N'%admin%'
        OR LOWER(r.[Nombre]) LIKE N'%super%')
      AND NOT EXISTS (
          SELECT 1 FROM [RolPermiso] rp
          WHERE rp.[RolId] = r.[Id] AND rp.[PermisoId] = p.[Id]);

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261026050000_PermisoRespaldos', N'8.0.0');
END;
GO
