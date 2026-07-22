# 🛠️ Desarrollo del Backend

El backend fue desarrollado con **ASP.NET Core Web API**, implementando una arquitectura basada en **Models** y **Controllers** para gestionar la información de la Panadería Kiataque.

## 📂 Estructura del Proyecto

```
BackendApi/
│
├── Controllers/
│   ├── ProductoController.cs
│   ├── CategoriaController.cs
│   ├── ClienteController.cs
│   ├── VentaController.cs
│   └── ...
│
├── Models/
│   ├── Producto.cs
│   ├── Categoria.cs
│   ├── Cliente.cs
│   ├── Venta.cs
│   └── ...
```

---

# 📦 Models Implementados

Se crearon los modelos necesarios para representar las entidades principales de la panadería.

| Modelo | Descripción |
|---------|-------------|
| Producto | Información de los productos de la panadería. |
| Categoria | Clasificación de los productos. |
| Cliente | Datos de los clientes. |
| Venta | Registro de ventas realizadas. |
| Pedido | Gestión de pedidos. |

### Capturas

### Modelo Producto

![Modelo Producto](docs/models/modelo-producto.png)

### Modelo Cliente

![Modelo Cliente](docs/models/modelo-cliente.png)

---

# 🎮 Controllers Implementados

Cada entidad posee su controlador correspondiente encargado de exponer los servicios REST.

| Controller | Función |
|------------|---------|
| ProductoController | Administración de productos |
| ClienteController | Administración de clientes |
| CategoriaController | Administración de categorías |
| VentaController | Administración de ventas |

### Capturas

### ProductoController

![ProductoController](docs/controllers/producto-controller.png)

### ClienteController

![ClienteController](docs/controllers/cliente-controller.png)

---

# 🔗 Endpoints CRUD

Se implementaron las operaciones CRUD para las entidades principales.

## GET

Obtiene la lista de registros.

```
GET /api/Producto
```

### Captura

![GET](docs/endpoints/get-producto.png)

---

## GET por ID

Obtiene un registro específico.

```
GET /api/Producto/{id}
```

### Captura

![GETID](docs/endpoints/get-id-producto.png)

---

## POST

Permite registrar un nuevo producto.

```
POST /api/Producto
```

Ejemplo JSON

```json
{
    "nombre":"Pan Francés",
    "precio":1.50,
    "stock":120
}
```

### Captura

![POST](docs/endpoints/post-producto.png)

---

## PUT

Actualiza la información de un producto existente.

```
PUT /api/Producto/{id}
```

Ejemplo JSON

```json
{
    "id":1,
    "nombre":"Pan Integral",
    "precio":2.00,
    "stock":90
}
```

### Captura

![PUT](docs/endpoints/put-producto.png)

---

## DELETE

Elimina un registro.

```
DELETE /api/Producto/{id}
```

### Captura

![DELETE](docs/endpoints/delete-producto.png)

---

# ✔ Resultados

Se verificó el correcto funcionamiento de todas las operaciones CRUD mediante pruebas realizadas en Swagger/Postman.

- ✔ Consulta de registros (GET)
- ✔ Consulta por ID (GET)
- ✔ Registro de datos (POST)
- ✔ Actualización (PUT)
- ✔ Eliminación (DELETE)

Las pruebas evidencian que la API responde correctamente y mantiene la información almacenada de manera consistente.
