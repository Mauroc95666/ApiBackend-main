import axios from 'axios';

// Instancia de axios que apunta a la API .NET.
// - En desarrollo local (npm run dev) usa http://localhost:5208/api.
// - En Docker se compila con VITE_API_URL=/api y nginx redirige /api al backend.
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5208/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Token de la sesion abierta. Lo guarda el login y lo borra el cierre de
// sesion. Vive solo en memoria: al recargar la pagina hay que volver a entrar.
let token = null;

export function guardarToken(nuevo) {
  token = nuevo || null;
}

export function borrarToken() {
  token = null;
}

// Cada pedido sale con el token, que es lo que el servidor usa para saber
// quien pregunta y que permisos tiene.
api.interceptors.request.use((config) => {
  if (token) {
    config.headers.Authorization = 'Bearer ' + token;
  }
  return config;
});

export default api;
