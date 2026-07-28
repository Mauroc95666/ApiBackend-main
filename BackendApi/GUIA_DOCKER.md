# Guía: subir tu API a Docker Hub (paso a paso)

Tu proyecto es una **API en .NET 8 (ASP.NET Core)** con Swagger. Ya te dejé
en la carpeta `BackendApi` tres archivos nuevos/actualizados:

- `Dockerfile` — receta para construir la imagen.
- `.dockerignore` — evita copiar basura (bin, obj, imágenes) a la imagen.
- `appsettings.json` — actualizado con tu servidor SQL `Macrosboy\SQL_DEV`.

> Ya probé la imagen aquí: **compila y arranca**, y Swagger responde
> correctamente mostrando todos tus endpoints.

---

## 0) Requisito previo

Instala **Docker Desktop** para Windows y ábrelo (debe quedar corriendo, el
ícono de la ballena encendido): https://www.docker.com/products/docker-desktop/

Y crea tu cuenta gratis en Docker Hub si aún no la tienes:
https://hub.docker.com/  → tu usuario es **mauroc95666**.

---

## 1) Abrir la terminal EN LA CARPETA CORRECTA

Todos los comandos se ejecutan **dentro de la carpeta `BackendApi`** (ahí está
el `Dockerfile`). Abre **PowerShell** y pega:

```powershell
cd "C:\Users\luisg\Downloads\ApiBackend-main-main (2) (2)\ApiBackend-main-main\ApiBackend-main-main\BackendApi"
```

Para confirmar que estás en el lugar correcto, este comando debe listar el `Dockerfile`:

```powershell
dir Dockerfile
```

---

## 2) Iniciar sesión en Docker Hub

```powershell
docker login
```

Te pedirá tu **usuario** (mauroc95666) y tu **contraseña** de Docker Hub.
Debe decir `Login Succeeded`.

---

## 3) Construir la imagen

```powershell
docker build -t mauroc95666/backendapi:latest .
```

> No olvides el punto `.` del final (significa "usa esta carpeta").
> La primera vez descarga el SDK de .NET y tarda unos minutos.

---

## 4) (Opcional) Probarla en tu máquina antes de subir

```powershell
docker run -d -p 8080:8080 --name backendapi mauroc95666/backendapi:latest
```

Luego abre en el navegador: **http://localhost:8080/swagger**

Para detenerla y borrarla después de probar:

```powershell
docker rm -f backendapi
```

---

## 5) Subir (push) la imagen a Docker Hub

```powershell
docker push mauroc95666/backendapi:latest
```

Cuando termine, tu imagen queda pública en Docker Hub.

---

## 6) El enlace que debes entregar

Tu duda era si el enlace es de Git o de Docker. Para una tarea de **Docker /
Docker Hub**, el enlace es el de la imagen en Docker Hub:

**https://hub.docker.com/r/mauroc95666/backendapi**

Cualquiera podría descargar y correr tu API con:

```
docker pull mauroc95666/backendapi:latest
docker run -p 8080:8080 mauroc95666/backendapi:latest
```

> Si tu profe además pidió el **código fuente**, ese enlace sí sería de
> **GitHub** (el repositorio del proyecto). Son cosas distintas: Docker Hub
> guarda la imagen lista para ejecutar; GitHub guarda el código.

---

## Nota sobre la base de datos

La cadena de conexión ahora apunta a `Macrosboy\SQL_DEV` (autenticación de
Windows). Eso funciona cuando ejecutas la API **directamente en tu PC** con
`dotnet run`.

Ten en cuenta que, **desde dentro de un contenedor Docker (Linux)**, conectarse
a un SQL Server local con autenticación de Windows no funciona directamente
(el contenedor está aislado y no usa tu usuario de Windows). Para la tarea esto
normalmente no importa: lo que se evalúa es que la **imagen se construya, se
suba a Docker Hub y arranque con Swagger**, y todo eso ya funciona.

Si más adelante quieres que la API dentro del contenedor sí llegue a una base
de datos, lo más limpio es usar un `docker-compose` con un SQL Server también
en contenedor y usuario/contraseña. Avísame y te lo preparo.
