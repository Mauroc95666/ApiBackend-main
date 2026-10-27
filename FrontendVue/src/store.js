import { reactive } from 'vue';
import { borrarToken } from '@/api/axios';

// Store bien simple hecho con reactive() para no depender de vuex/pinia.
// Guarda la sesion del usuario y en que pantalla estamos.
// La sesion se guarda tambien en memoria mientras la pagina este abierta.

export const store = reactive({
  // pantalla puede ser: 'login', 'tienda' o 'panel'
  pantalla: 'login',
  usuario: null, // objeto usuario que devuelve la API
  esAdmin: false,
  rolNombre: '', // nombre del rol, solo para mostrarlo en pantalla
  // Modulos que este usuario tiene permitidos. Vienen de la tabla RolPermiso
  // a traves de la respuesta del login: NO se deducen del nombre del rol.
  permisos: [],

  entrarAlPanel(usuario, rolNombre, permisos) {
    this.usuario = usuario;
    this.rolNombre = rolNombre || '';
    this.permisos = Array.isArray(permisos) ? permisos : [];
    // Administrador es quien puede administrar usuarios y roles. Se decide
    // por lo que puede hacer, no por como se llama su rol.
    this.esAdmin = this.permisos.includes('usuarios') && this.permisos.includes('roles');
    this.pantalla = 'panel';
  },

  entrarComoCliente() {
    this.esAdmin = false;
    this.pantalla = 'tienda';
  },

  cerrarSesion() {
    // Sin token, la API deja de responderle a esta pestaña.
    borrarToken();
    this.usuario = null;
    this.rolNombre = '';
    this.permisos = [];
    this.esAdmin = false;
    this.pantalla = 'login';
  },
});

// Unico lugar donde se decide si el usuario puede ver un modulo.
export function puedeVerModulo(clave) {
  return store.permisos.includes(clave);
}

// Antes aca vivian rolEsAdmin(), rolEsStaff() y accesoDeRol(), que decidian
// los permisos leyendo el NOMBRE del rol por palabra clave. Se eliminaron: los
// permisos ahora salen de la tabla RolPermiso y llegan en la respuesta del
// login. Renombrar un rol ya no le cambia lo que puede hacer, y un rol nuevo
// recibe exactamente los permisos que se le tilden en la pantalla de Roles.
