Aquí tienes la documentación formateada con las rutas de imágenes exactas de la carpeta portillo y los datos de las capturas de pantalla de Productos (ID 8, "pan frances de chocolate", "frances con vainilla", etc.):

Markdown
## 🧪 Pruebas de Funcionamiento API Backend (CRUD Productos)

El backend en **.NET 7 Web API** (`BackendApi`) expone operaciones RESTful documentadas mediante **Swagger UI**. A continuación, se presentan las evidencias del ciclo de vida completo (CRUD) probado sobre el controlador de Productos:

### 📸 Evidencias de Operaciones HTTP

<table>
  <tr>
    <td align="center" width="50%"><b>01. POST — Registrar Nuevo Producto</b></td>
    <td align="center" width="50%"><b>02. GET por ID — Consultar Registro Creado</b></td>
  </tr>
  <tr>
    <td><img src="BackendApi/img/portillo/WhatsApp%20Image%202026-07-22%20at%2010.15.08.jpeg" alt="POST Crear Producto" width="100%"/></td>
    <td><img src="BackendApi/img/portillo/WhatsApp%20Image%202026-07-22%20at%2010.15.08%20(1).jpeg" alt="GET Producto por ID" width="100%"/></td>
  </tr>
  <tr>
    <td>Creación exitosa <code>200 OK</code> agregando el producto "pan frances de chocolate" (ID 8).</td>
    <td>Consulta directa <code>200 OK</code> especificando el identificador único (ID 8).</td>
  </tr>
  <tr>
    <td align="center" width="50%"><b>03. PUT — Actualizar Datos de Producto</b></td>
    <td align="center" width="50%"><b>04. DELETE — Eliminar Registro</b></td>
  </tr>
  <tr>
    <td><img src="BackendApi/img/portillo/WhatsApp%20Image%202026-07-22%20at%2010.15.08%20(2).jpeg" alt="PUT Actualizar Producto" width="100%"/></td>
    <td><img src="BackendApi/img/portillo/WhatsApp%20Image%202026-07-22%20at%2010.15.08%20(3).jpeg" alt="DELETE Producto" width="100%"/></td>
  </tr>
  <tr>
    <td>Modificación exitosa <code>200 OK</code> actualizando a "frances con vainilla".</td>
    <td>Eliminación exitosa <code>200 OK</code> confirmando "Producto 8 eliminado correctamente".</td>
  </tr>
</table>

#### 🔍 Verificación de Eliminación (Validación de Error 404)

<table>
  <tr>
    <td align="center" width="100%"><b>05. GET por ID — Confirmación de Registro Inexistente</b></td>
  </tr>
  <tr>
    <td><img src="BackendApi/img/portillo/WhatsApp%20Image%202026-07-22%20at%2010.15.08%20(4).jpeg" alt="GET 404 Not Found" width="100%"/></td>
  </tr>
  <tr>
    <td align="center">Respuesta controlada <code>404 Not Found</code> confirmando que el producto 8 ya no existe en el sistema.</td>
  </tr>
</table>

---

## 🧪 Pruebas de Funcionamiento API Backend (CRUD Pedidos)

El backend en **.NET 7 Web API** (`BackendApi`) expone operaciones RESTful documentadas mediante **Swagger UI**. A continuación, se presentan las evidencias del ciclo de vida completo (CRUD) probado sobre el controlador de Pedidos:

### 📸 Evidencias de Operaciones HTTP

<table>
  <tr>
    <td align="center" width="50%"><b>01. GET — Listar Todos los Pedidos</b></td>
    <td align="center" width="50%"><b>02. POST — Registrar Nuevo Pedido</b></td>
  </tr>
  <tr>
    <td><img src="BackendApi/img/kiataque/Captura%20de%20pantalla%202026-07-22%20101548.png" alt="GET Pedidos Listado" width="100%"/></td>
    <td><img src="BackendApi/img/kiataque/Captura%20de%20pantalla%202026-07-22%20101800.png" alt="POST Crear Pedido" width="100%"/></td>
  </tr>
  <tr>
    <td>Respuesta exitosa <code>200 OK</code> retornando el listado general en formato JSON.</td>
    <td>Creación exitosa <code>201 Created</code> agregando el pedido ID 5.</td>
  </tr>
  <tr>
    <td align="center" width="50%"><b>03. GET por ID — Consultar Registro Creado</b></td>
    <td align="center" width="50%"><b>04. PUT — Actualizar Datos de Pedido</b></td>
  </tr>
  <tr>
    <td><img src="BackendApi/img/kiataque/Captura%20de%20pantalla%202026-07-22%20101944.png" alt="GET Pedido por ID" width="100%"/></td>
    <td><img src="BackendApi/img/kiataque/Captura%20de%20pantalla%202026-07-22%20102101.png" alt="PUT Actualizar Pedido" width="100%"/></td>
  </tr>
  <tr>
    <td>Consulta directa <code>200 OK</code> especificando el identificador único (ID 5).</td>
    <td>Modificación <code>204 No Content</code> actualizando estado y detalle del pedido.</td>
  </tr>
  <tr>
    <td align="center" width="50%"><b>05. GET por ID — Validar Edición</b></td>
    <td align="center" width="50%"><b>06. DELETE — Eliminar Registro</b></td>
  </tr>
  <tr>
    <td><img src="BackendApi/img/kiataque/Captura%20de%20pantalla%202026-07-22%20102121.png" alt="GET Validar Edicion" width="100%"/></td>
    <td><img src="BackendApi/img/kiataque/Captura%20de%20pantalla%202026-07-22%20102146.png" alt="DELETE Pedido" width="100%"/></td>
  </tr>
  <tr>
    <td>Verificación <code>200 OK</code> confirmando los cambios realizados en el payload.</td>
    <td>Eliminación exitosa <code>204 No Content</code> del pedido ID 5.</td>
  </tr>
</table>

#### 🔍 Verificación de Eliminación (Validación de Error 404)

<table>
  <tr>
    <td align="center" width="100%"><b>07. GET por ID — Confirmación de Registro Inexistente</b></td>
  </tr>
  <tr>
    <td><img src="BackendApi/img/kiataque/Captura%20de%20pantalla%202026-07-22%20102202.png" alt="GET 404 Not Found" width="100%"/></td>
  </tr>
  <tr>
    <td align="center">Respuesta controlada <code>404 Not Found</code> confirmando que el recurso ya no existe en el sistema.</td>
  </tr>
</table>

## 🧪 Pruebas de Funcionamiento API Backend (CRUD Proveedores)

El backend en **.NET 7 Web API** (`BackendApi`) expone operaciones RESTful documentadas mediante **Swagger UI**. A continuación, se presentan las evidencias del ciclo de vida completo (CRUD) probado sobre el controlador de Proveedores:

### 📸 Evidencias de Operaciones HTTP

<table>
  <tr>
    <td align="center" width="50%"><b>01. GET — Listar Todos los Proveedores</b></td>
    <td align="center" width="50%"><b>02. POST — Registrar Nuevo Proveedor</b></td>
  </tr>
  <tr>
    <td><img src="BackendApi/img/kiataque/Captura%20de%20pantalla%202026-07-22%20101548.png" alt="GET Proveedores Listado" width="100%"/></td>
    <td><img src="BackendApi/img/kiataque/Captura%20de%20pantalla%202026-07-22%20101800.png" alt="POST Crear Proveedor" width="100%"/></td>
  </tr>
  <tr>
    <td>Respuesta exitosa <code>200 OK</code> retornando el listado general en formato JSON.</td>
    <td>Creación exitosa <code>201 Created</code> agregando al proveedor ID 5.</td>
  </tr>
  <tr>
    <td align="center" width="50%"><b>03. GET por ID — Consultar Registro Creado</b></td>
    <td align="center" width="50%"><b>04. PUT — Actualizar Datos de Proveedor</b></td>
  </tr>
  <tr>
    <td><img src="BackendApi/img/kiataque/Captura%20de%20pantalla%202026-07-22%20101944.png" alt="GET Proveedor por ID" width="100%"/></td>
    <td><img src="BackendApi/img/kiataque/Captura%20de%20pantalla%202026-07-22%20102101.png" alt="PUT Actualizar Proveedor" width="100%"/></td>
  </tr>
  <tr>
    <td>Consulta directa <code>200 OK</code> especificando el identificador único (ID 5).</td>
    <td>Modificación <code>204 No Content</code> actualizando nombre y rubro comercial.</td>
  </tr>
  <tr>
    <td align="center" width="50%"><b>05. GET por ID — Validar Edición</b></td>
    <td align="center" width="50%"><b>06. DELETE — Eliminar Registro</b></td>
  </tr>
  <tr>
    <td><img src="BackendApi/img/kiataque/Captura%20de%20pantalla%202026-07-22%20102121.png" alt="GET Validar Edicion" width="100%"/></td>
    <td><img src="BackendApi/img/kiataque/Captura%20de%20pantalla%202026-07-22%20102146.png" alt="DELETE Proveedor" width="100%"/></td>
  </tr>
  <tr>
    <td>Verificación <code>200 OK</code> confirmando los cambios realizados en el payload.</td>
    <td>Eliminación exitosa <code>204 No Content</code> del proveedor ID 5.</td>
  </tr>
</table>

#### 🔍 Verificación de Eliminación (Validación de Error 404)

<table>
  <tr>
    <td align="center" width="100%"><b>07. GET por ID — Confirmación de Registro Inexistente</b></td>
  </tr>
  <tr>
    <td><img src="BackendApi/img/kiataque/Captura%20de%20pantalla%202026-07-22%20102202.png" alt="GET 404 Not Found" width="100%"/></td>
  </tr>
  <tr>
    <td align="center">Respuesta controlada <code>404 Not Found</code> confirmando que el recurso ya no existe en el sistema.</td>
  </tr>
</table>

---

## ⚙️ Arquitectura del Backend

El backend está desarrollado bajo la plataforma .NET utilizando arquitectura en capas y controladores RESTful:

* **Controladores e Integración:**
  * `ProveedoresController`: Gestión de proveedores (`GET`, `POST`, `PUT`, `DELETE`).
  * `ClientesController`, `PedidosController`, `ProductoController`: Gestión operativa integral.
* **Middleware y Documentación:**
  * **Swagger / OpenAPI** mediante `Swashbuckle.AspNetCore` para consumo dinámico de endpoints.
