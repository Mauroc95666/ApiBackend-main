import { reactive } from 'vue';

// Store bien simple hecho con reactive() para no depender de vuex/pinia.
// Guarda la sesion del usuario y en que pantalla estamos.
// La sesion se guarda tambien en memoria mientras la pagina este abierta.

export const store = reactive({
  // pantalla puede ser: 'login', 'tienda' o 'panel'
  pantalla: 'login',
  usuario: null, // objeto usuario que devuelve la API
  esAdmin: false,

  entrarComoAdmin(usuario) {
    this.usuario = usuario;
    this.esAdmin = true;
    this.pantalla = 'panel';
  },

  entrarComoCliente() {
    this.esAdmin = false;
    this.pantalla = 'tienda';
  },

  cerrarSesion() {
    this.usuario = null;
    this.esAdmin = false;
    this.pantalla = 'login';
  },
});

// Devuelve true si el rol parece de administrador / superadmin.
// Como el backend no tiene endpoint de login, deducimos el rol por el nombre.
export function rolEsAdmin(nombreRol) {
  if (!nombreRol) return false;
  const n = nombreRol.toLowerCase();
  return n.includes('admin') || n.includes('super') || n.includes('gerente');
}
