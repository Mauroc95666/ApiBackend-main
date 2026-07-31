import { reactive } from 'vue';

// Store bien simple hecho con reactive() para no depender de vuex/pinia.
// Guarda la sesion del usuario y en que pantalla estamos.
// La sesion se guarda tambien en memoria mientras la pagina este abierta.

export const store = reactive({
  // pantalla puede ser: 'login', 'tienda' o 'panel'
  pantalla: 'login',
  usuario: null, // objeto usuario que devuelve la API
  esAdmin: false,
  rolNombre: '', // nombre del rol del usuario logueado (para filtrar el menu)

  entrarComoAdmin(usuario, rolNombre) {
    this.usuario = usuario;
    this.rolNombre = rolNombre || '';
    this.esAdmin = true;
    this.pantalla = 'panel';
  },

  // Entra al panel pero con los modulos limitados segun su rol.
  entrarComoStaff(usuario, rolNombre) {
    this.usuario = usuario;
    this.rolNombre = rolNombre || '';
    this.esAdmin = false;
    this.pantalla = 'panel';
  },

  entrarComoCliente() {
    this.esAdmin = false;
    this.pantalla = 'tienda';
  },

  cerrarSesion() {
    this.usuario = null;
    this.rolNombre = '';
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

// Devuelve true si el rol es de personal interno (staff) que igual entra al
// panel pero viendo solo algunos modulos (cajero, inventario, vendedor...).
export function rolEsStaff(nombreRol) {
  if (!nombreRol) return false;
  if (rolEsAdmin(nombreRol)) return false;
  return accesoDeRol(nombreRol).length > 0;
}

// Segun el nombre del rol, decide que modulos del panel puede ver.
// Devolvemos '*' para acceso total, o un arreglo con las claves permitidas.
// Los nombres de rol se comparan por palabra clave para que aguante variantes
// ("Cajero", "Caja Principal", "Encargado de Inventario", etc.).
export function accesoDeRol(nombreRol) {
  if (!nombreRol) return [];
  const n = nombreRol.toLowerCase();

  // Administrador / superadmin / gerente -> ve todo
  if (n.includes('admin') || n.includes('super') || n.includes('gerente')) {
    return '*';
  }
  // Inventario / almacen / bodega / stock -> solo inventario y productos
  if (
    n.includes('inventario') ||
    n.includes('almac') || // almacen / almacén
    n.includes('bodega') ||
    n.includes('stock')
  ) {
    return ['inicio', 'inventario', 'productos'];
  }
  // Cajero / caja -> solo ventas y pedidos
  if (n.includes('cajero') || n.includes('caja')) {
    return ['inicio', 'ventas', 'pedidos'];
  }
  // Vendedor / ventas -> ventas, pedidos y ver productos
  if (n.includes('vendedor') || n.includes('venta')) {
    return ['inicio', 'ventas', 'pedidos', 'productos'];
  }
  // Cualquier otro rol conocido de mostrador -> lo minimo (inicio)
  return [];
}
