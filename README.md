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
