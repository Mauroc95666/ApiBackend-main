---

## 🧪 Pruebas de Funcionamiento de la API Backend

El proyecto integra un backend desarrollado en **.NET 7 Web API** (`BackendApi`) documentado dinámicamente mediante **Swagger UI**. A continuación, se detallan las evidencias del correcto funcionamiento de las peticiones HTTP y la consola del servidor:

### 📸 Evidencias de Ejecución y Endpoints

<table>
  <tr>
    <td align="center" width="50%"><b>01. Proveedores (GET / POST)</b></td>
    <td align="center" width="50%"><b>02. Módulo de Pedidos</b></td>
  </tr>
  <tr>
    <td><img src="docs/backend/01_Prueba_Proveedores_GET_POST.png" alt="Respuesta JSON Proveedores" width="100%"/></td>
    <td><img src="docs/backend/02_Endpoint_Pedidos_Respuesta.png" alt="Listado de Pedidos" width="100%"/></td>
  </tr>
  <tr>
    <td>Respuesta estructurada en formato JSON para la gestión de proveedores.</td>
    <td>Consulta y retorno de datos desde el controlador de Pedidos.</td>
  </tr>
  <tr>
    <td align="center" width="50%"><b>03. Detalle de Productos</b></td>
    <td align="center" width="50%"><b>04. Módulo de Clientes</b></td>
  </tr>
  <tr>
    <td><img src="docs/backend/03_Prueba_Productos_Detalle.png" alt="Respuesta JSON Productos" width="100%"/></td>
    <td><img src="docs/backend/04_Modulo_Clientes_Consultas.png" alt="Interfaz Clientes" width="100%"/></td>
  </tr>
  <tr>
    <td>Verificación de endpoint para obtención de stock y catálogo de productos.</td>
    <td>Registros cargados y validación del módulo de clientes.</td>
  </tr>
  <tr>
    <td align="center" width="50%"><b>05. Confirmación HTTP Status 200</b></td>
    <td align="center" width="50%"><b>06. Consola del Servidor Backend</b></td>
  </tr>
  <tr>
    <td><img src="docs/backend/05_Peticion_HTTP_Exitosa.png" alt="Petición HTTP Exitosa" width="100%"/></td>
    <td><img src="docs/backend/06_Consola_Servidor_Ejecucion.png" alt="Estado del Servidor" width="100%"/></td>
  </tr>
  <tr>
    <td>Confirmación de peticiones exitosas (Status Code 200 OK).</td>
    <td>Servidor local en ejecución escuchando peticiones en tiempo real.</td>
  </tr>
</table>

#### 🌐 Vista General de Documentación Swagger

<table>
  <tr>
    <td align="center" width="100%"><b>07. Swagger UI — Endpoints Expuestos</b></td>
  </tr>
  <tr>
    <td><img src="docs/backend/07_Listado_General_Swagger_UI.png" alt="Swagger UI General" width="100%"/></td>
  </tr>
  <tr>
    <td align="center">Panel interactivo de Open API / Swagger documentando los controladores RESTful del sistema.</td>
  </tr>
</table>

---

## ⚙️ Arquitectura del Backend

El backend se encuentra estructurado bajo la plataforma .NET utilizando el patrón Controller-Service:

* **Controladores Implementados:**
  * `ClientesController`: Operaciones para el registro y consulta de clientes.
  * `PedidosController`: Gestión de solicitudes y deliveries.
  * `ProductoController`: Control de inventario y catálogo.
  * `ProveedoresController`: Registro y administración de suministros.
* **Middleware y Documentación:**
  * Implementación de **Swashbuckle.AspNetCore** para la generación interactiva de Swagger UI.
