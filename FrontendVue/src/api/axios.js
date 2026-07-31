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

export default api;
