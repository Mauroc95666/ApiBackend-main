-- ============================================================
--  Agregar columnas nuevas  (Panadería Kiataque)
--  - Cliente.Direccion  -> dirección del cliente
--  - Producto.ImagenUrl -> URL de la foto del producto
--
--  Sirve igual para la base LOCAL (Docker) y la de MonsterASP.
--  Es seguro: NO borra datos y NO falla si ya existen las columnas.
--  Se puede correr varias veces sin problema.
-- ============================================================

-- Dirección del cliente
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'Direccion' AND Object_ID = Object_ID(N'[Cliente]')
)
BEGIN
    ALTER TABLE [Cliente] ADD [Direccion] nvarchar(max) NULL;
END;
GO

-- URL de la imagen del producto
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'ImagenUrl' AND Object_ID = Object_ID(N'[Producto]')
)
BEGIN
    ALTER TABLE [Producto] ADD [ImagenUrl] nvarchar(max) NULL;
END;
GO

-- Para comprobar que quedaron:
SELECT 'Cliente' AS Tabla, name AS Columna FROM sys.columns WHERE Object_ID = Object_ID(N'[Cliente]') AND name = 'Direccion'
UNION ALL
SELECT 'Producto', name FROM sys.columns WHERE Object_ID = Object_ID(N'[Producto]') AND name = 'ImagenUrl';
