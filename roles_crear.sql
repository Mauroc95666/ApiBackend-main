-- ============================================================
--  Crear los roles del sistema  (Panadería Kiataque)
--  Sirve igual para la base LOCAL (Docker) y la de MonsterASP.
--  Se puede correr varias veces: no duplica si ya existen.
-- ============================================================

-- Cajero: solo ve Ventas y Pedidos
IF NOT EXISTS (SELECT 1 FROM [Rol] WHERE [Nombre] = 'Cajero')
    INSERT INTO [Rol] ([Nombre], [Descripcion])
    VALUES ('Cajero', 'Registra ventas y gestiona pedidos.');

-- Inventario: solo ve Inventario (stock) y Productos
IF NOT EXISTS (SELECT 1 FROM [Rol] WHERE [Nombre] = 'Inventario')
    INSERT INTO [Rol] ([Nombre], [Descripcion])
    VALUES ('Inventario', 'Controla el stock y los productos.');

-- Vendedor: ve Ventas, Pedidos y Productos
IF NOT EXISTS (SELECT 1 FROM [Rol] WHERE [Nombre] = 'Vendedor')
    INSERT INTO [Rol] ([Nombre], [Descripcion])
    VALUES ('Vendedor', 'Atiende ventas y consulta productos.');

-- (El rol de Administrador / Superadmin ya existe y ve todo.)

-- Para ver los roles con su Id:
SELECT [Id], [Nombre], [Descripcion] FROM [Rol] ORDER BY [Id];
