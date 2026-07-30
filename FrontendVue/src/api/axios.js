import axios from 'axios';

// Instancia de axios que apunta a la API .NET.
// Si el backend corre en otro puerto, se cambia solamente esta baseURL.
const api = axios.create({
  baseURL: 'http://localhost:5208/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

export default api;
