<script setup>
import { ref, computed, onMounted } from 'vue';
import api from '@/api/axios';
import { store, puedeVerModulo } from '@/store';

import Dashboard from './admin/Dashboard.vue';
import ProductosAdmin from './admin/ProductosAdmin.vue';
import VentasAdmin from './admin/VentasAdmin.vue';
import PedidosAdmin from './admin/PedidosAdmin.vue';
import InventarioAdmin from './admin/InventarioAdmin.vue';
import ReportesAdmin from './admin/ReportesAdmin.vue';
import RespaldosAdmin from './admin/RespaldosAdmin.vue';
import TablaCrud from './admin/TablaCrud.vue';

// Modulo que se esta viendo
const modulo = ref('inicio');

// Datos de referencia para los <select> del CRUD generico
const roles = ref([]);
const ciudades = ref([]);
const emprendimientos = ref([]);

async function cargarReferencias() {
  const [r, c, e] = await Promise.all([
    api.get('/roles').catch(() => ({ data: [] })),
    api.get('/ciudades').catch(() => ({ data: [] })),
    api.get('/emprendimientos').catch(() => ({ data: [] })),
  ]);
  roles.value = r.data || [];
  ciudades.value = c.data || [];
  emprendimientos.value = e.data || [];
}

const opRolesUsuarios = computed(() =>
  roles.value
    .filter((rol) => !rol.nombre?.trim().toLowerCase().includes('cliente'))
    .map((x) => ({ valor: x.id, texto: x.nombre }))
);
const opCiudades = computed(() => ciudades.value.map((x) => ({ valor: x.id, texto: x.nombre })));
const opEmpren = computed(() => emprendimientos.value.map((x) => ({ valor: x.id, texto: x.nombre })));
const empDefecto = computed(() => emprendimientos.value[0]?.id ?? 1);

// Que modulos puede ver este usuario segun su rol.
// '*' significa todos (administrador). Si no, es una lista de claves.
// true si el usuario puede ver ese modulo del menu. Lo decide la lista de
// permisos que trajo el login desde RolPermiso.
function puedeVer(clave) {
  return puedeVerModulo(clave);
}

// Menu lateral organizado por secciones
const menuCompleto = [
  {
    seccion: 'Principal',
    items: [
      { clave: 'inicio', texto: 'Inicio', icono: '🏠' },
      { clave: 'productos', texto: 'Productos', icono: '🥖' },
      { clave: 'ventas', texto: 'Ventas', icono: '🧾' },
      { clave: 'pedidos', texto: 'Pedidos', icono: '📦' },
      { clave: 'inventario', texto: 'Inventario', icono: '📊' },
      { clave: 'reportes', texto: 'Reportes', icono: '📈' },
    ],
  },
  {
    seccion: 'Administración',
    items: [
      { clave: 'sucursales', texto: 'Sucursales', icono: '🏬' },
      { clave: 'categorias', texto: 'Categorías', icono: '🏷️' },
      { clave: 'clientes', texto: 'Clientes', icono: '👥' },
      { clave: 'usuarios', texto: 'Usuarios', icono: '🔑' },
      { clave: 'roles', texto: 'Roles', icono: '🛡️' },
    ],
  },
  {
    seccion: 'Configuración',
    items: [
      { clave: 'ciudades', texto: 'Ciudades', icono: '🌆' },
      { clave: 'metodospago', texto: 'Métodos de pago', icono: '💳' },
      { clave: 'proveedores', texto: 'Proveedores', icono: '🚚' },
      { clave: 'emprendimientos', texto: 'Emprendimientos', icono: '🏢' },
      { clave: 'respaldos', texto: 'Respaldos', icono: '💾' },
    ],
  },
];

// --- Permisos por rol -------------------------------------------------
// La pantalla de Roles pasa a poder editar que modulos abre cada rol. Los
// permisos viven en la tabla RolPermiso, no en el nombre del rol.
const rolDePermisos = ref(null);
const permisosDisponibles = ref([]);
const permisosTildados = ref([]);
const cargandoPermisos = ref(false);
const guardandoPermisos = ref(false);
const errorPermisos = ref('');

async function abrirPermisos(rol) {
  rolDePermisos.value = rol;
  errorPermisos.value = '';
  cargandoPermisos.value = true;
  permisosTildados.value = [];
  try {
    const [todos, delRol] = await Promise.all([
      api.get('/permisos'),
      api.get('/roles/' + rol.id + '/permisos'),
    ]);
    permisosDisponibles.value = todos.data || [];
    permisosTildados.value = (delRol.data || []).map((p) => p.id);
  } catch {
    errorPermisos.value = 'No se pudieron cargar los permisos.';
  } finally {
    cargandoPermisos.value = false;
  }
}

async function guardarPermisos() {
  guardandoPermisos.value = true;
  errorPermisos.value = '';
  try {
    await api.put('/roles/' + rolDePermisos.value.id + '/permisos', {
      permisoIds: permisosTildados.value,
    });
    // Si uno se cambio los permisos de su propio rol, la sesion abierta queda
    // con los de antes hasta que vuelva a entrar.
    if (store.usuario?.rolId === rolDePermisos.value.id) {
      errorPermisos.value = 'Guardado. Cerrá sesión y volvé a entrar para que te apliquen a vos.';
      guardandoPermisos.value = false;
      return;
    }
    rolDePermisos.value = null;
  } catch (e) {
    errorPermisos.value = e?.response?.data?.mensaje || 'No se pudieron guardar los permisos.';
  } finally {
    guardandoPermisos.value = false;
  }
}

// Menu ya filtrado: solo las secciones e items que el rol puede ver.
const menu = computed(() =>
  menuCompleto
    .map((s) => ({ ...s, items: s.items.filter((it) => puedeVer(it.clave)) }))
    .filter((s) => s.items.length > 0)
);

// Configuracion del CRUD generico por modulo.
// Cada campo: { campo, etiqueta, tipo, opciones, requerido, ocultarEnTabla }
const configs = computed(() => ({
  sucursales: {
    titulo: 'Sucursales',
    subtitulo: 'Locales de la panadería',
    endpoint: '/sucursales',
    nuevoModelo: () => ({ nombre: '', direccion: '', telefono: '', ciudadId: null, emprendimientoId: empDefecto.value }),
    campos: [
      { campo: 'nombre', etiqueta: 'Nombre', tipo: 'text', requerido: true },
      { campo: 'direccion', etiqueta: 'Dirección', tipo: 'text' },
      { campo: 'telefono', etiqueta: 'Teléfono', tipo: 'text' },
      { campo: 'ciudadId', etiqueta: 'Ciudad', tipo: 'select', opciones: opCiudades.value },
      { campo: 'emprendimientoId', etiqueta: 'Emprendimiento', tipo: 'select', opciones: opEmpren.value },
    ],
  },
  categorias: {
    titulo: 'Categorías',
    subtitulo: 'Categorías de productos',
    endpoint: '/categorias',
    nuevoModelo: () => ({ nombre: '', descripcion: '', emprendimientoId: empDefecto.value }),
    campos: [
      { campo: 'nombre', etiqueta: 'Nombre', tipo: 'text', requerido: true },
      { campo: 'descripcion', etiqueta: 'Descripción', tipo: 'text' },
      { campo: 'emprendimientoId', etiqueta: 'Emprendimiento', tipo: 'select', opciones: opEmpren.value },
    ],
  },
  clientes: {
    titulo: 'Clientes',
    subtitulo: 'Clientes registrados',
    endpoint: '/clientes',
    nuevoModelo: () => ({ nombre: '', ciNit: '', telefono: '', email: '', direccion: '', emprendimientoId: empDefecto.value }),
    campos: [
      { campo: 'nombre', etiqueta: 'Nombre', tipo: 'text', requerido: true, formato: 'nombre' },
      { campo: 'ciNit', etiqueta: 'CI / NIT', tipo: 'text' },
      { campo: 'telefono', etiqueta: 'Teléfono', tipo: 'text', requerido: true, formato: 'telefono', maxLength: 15 },
      { campo: 'email', etiqueta: 'Email', tipo: 'text' },
      { campo: 'direccion', etiqueta: 'Dirección', tipo: 'mapa', requerido: true, mostrarMapa: true },
      { campo: 'emprendimientoId', etiqueta: 'Emprendimiento', tipo: 'select', opciones: opEmpren.value },
    ],
  },
  usuarios: {
    titulo: 'Usuarios',
    subtitulo: 'Usuarios del sistema',
    endpoint: '/usuarios',
    nuevoModelo: () => ({ nombre: '', email: '', password: '', rolId: null, activo: true, emprendimientoId: empDefecto.value }),
    campos: [
      { campo: 'nombre', etiqueta: 'Nombre', tipo: 'text', requerido: true },
      { campo: 'email', etiqueta: 'Email', tipo: 'text', requerido: true },
      // Tipo password y fuera de la tabla: antes era un input de texto comun y
      // la contraseña se veia como una columna mas del listado de usuarios.
      // Al editar llega vacio, y vacio significa "no la cambies".
      { campo: 'password', etiqueta: 'Contraseña', tipo: 'password', ocultarEnTabla: true, ayuda: 'Al editar, dejala vacía para no cambiarla' },
      { campo: 'rolId', etiqueta: 'Rol', tipo: 'select', opciones: opRolesUsuarios.value },
      { campo: 'activo', etiqueta: 'Activo', tipo: 'checkbox' },
      { campo: 'emprendimientoId', etiqueta: 'Emprendimiento', tipo: 'select', opciones: opEmpren.value },
    ],
  },
  roles: {
    titulo: 'Roles',
    subtitulo: 'Roles y perfiles',
    endpoint: '/roles',
    nuevoModelo: () => ({ nombre: '', descripcion: '' }),
    campos: [
      { campo: 'nombre', etiqueta: 'Nombre', tipo: 'text', requerido: true },
      { campo: 'descripcion', etiqueta: 'Descripción', tipo: 'text' },
    ],
  },
  ciudades: {
    titulo: 'Ciudades',
    subtitulo: 'Ciudades y departamentos',
    endpoint: '/ciudades',
    nuevoModelo: () => ({ nombre: '', departamento: '' }),
    campos: [
      { campo: 'nombre', etiqueta: 'Nombre', tipo: 'text', requerido: true },
      { campo: 'departamento', etiqueta: 'Departamento', tipo: 'text' },
    ],
  },
  metodospago: {
    titulo: 'Métodos de pago',
    subtitulo: 'Formas de pago aceptadas',
    endpoint: '/metodospago',
    nuevoModelo: () => ({ nombre: '', descripcion: '' }),
    campos: [
      { campo: 'nombre', etiqueta: 'Nombre', tipo: 'text', requerido: true },
      { campo: 'descripcion', etiqueta: 'Descripción', tipo: 'text' },
    ],
  },
  proveedores: {
    titulo: 'Proveedores',
    subtitulo: 'Proveedores de insumos',
    endpoint: '/proveedores',
    nuevoModelo: () => ({ nombre: '', contacto: '', telefono: '', email: '', emprendimientoId: empDefecto.value }),
    campos: [
      { campo: 'nombre', etiqueta: 'Nombre', tipo: 'text', requerido: true },
      { campo: 'contacto', etiqueta: 'Contacto', tipo: 'text' },
      { campo: 'telefono', etiqueta: 'Teléfono', tipo: 'text' },
      { campo: 'email', etiqueta: 'Email', tipo: 'text' },
      { campo: 'emprendimientoId', etiqueta: 'Emprendimiento', tipo: 'select', opciones: opEmpren.value },
    ],
  },
  emprendimientos: {
    titulo: 'Emprendimientos',
    subtitulo: 'Empresa / negocio',
    endpoint: '/emprendimientos',
    nuevoModelo: () => ({ nombre: '', nitRuc: '', direccion: '', telefono: '', email: '' }),
    campos: [
      { campo: 'nombre', etiqueta: 'Nombre', tipo: 'text', requerido: true },
      { campo: 'nitRuc', etiqueta: 'NIT / RUC', tipo: 'text' },
      { campo: 'direccion', etiqueta: 'Dirección', tipo: 'text' },
      { campo: 'telefono', etiqueta: 'Teléfono', tipo: 'text' },
      { campo: 'email', etiqueta: 'Email', tipo: 'text' },
    ],
  },
}));

const configActual = computed(() => configs.value[modulo.value]);

const esModuloCustom = computed(() =>
  ['inicio', 'productos', 'ventas', 'pedidos', 'inventario'].includes(modulo.value)
);

const inicialUsuario = computed(() => (store.usuario?.nombre || 'A').charAt(0).toUpperCase());

// Etiqueta del rol que se muestra abajo y en el avatar.
const etiquetaRol = computed(() => store.rolNombre || 'Administrador');

onMounted(() => {
  cargarReferencias();
  // Si por algun motivo el modulo actual no esta permitido, caemos al primero
  // que si lo este (el primer item del menu ya filtrado).
  if (!puedeVer(modulo.value)) {
    const primero = menu.value[0]?.items[0]?.clave;
    if (primero) modulo.value = primero;
  }
});
</script>

<template>
  <div class="panel">
    <!-- Sidebar -->
    <aside class="sidebar">
      <div class="marca">
        <div class="logo">K</div>
        <div>
          <strong>Kiataque</strong>
          <div class="gris chico">Panadería</div>
        </div>
      </div>

      <nav class="nav">
        <div v-for="s in menu" :key="s.seccion" class="grupo">
          <div class="grupo-titulo">{{ s.seccion }}</div>
          <button
            v-for="it in s.items"
            :key="it.clave"
            class="item"
            :class="{ activo: modulo === it.clave }"
            @click="modulo = it.clave"
          >
            <span class="icono">{{ it.icono }}</span>
            <span class="etiqueta-item">{{ it.texto }}</span>
          </button>
        </div>
      </nav>

      <div class="usuario-pie">
        <div class="avatar">{{ inicialUsuario }}</div>
        <div>
          <strong class="chico">{{ store.usuario?.nombre || 'Administrador' }}</strong>
          <div class="gris chico">{{ etiquetaRol }}</div>
        </div>
      </div>
    </aside>

    <!-- Contenido -->
    <div class="contenido">
      <header class="topbar">
        <div class="titulo-top">{{ configActual ? configActual.titulo : 'Panadería Kiataque' }}</div>
        <div class="fila derecha-top">
          <button v-if="puedeVer('ventas')" class="btn btn-linea btn-mini" @click="modulo = 'ventas'">+ Venta</button>
          <button class="btn btn-linea btn-mini" @click="store.cerrarSesion()">Cerrar sesión</button>
          <div class="avatar chico-avatar">{{ inicialUsuario }}</div>
        </div>
      </header>

      <main class="area">
        <Dashboard v-if="modulo === 'inicio'" />
        <ProductosAdmin v-else-if="modulo === 'productos'" />
        <VentasAdmin v-else-if="modulo === 'ventas'" />
        <PedidosAdmin v-else-if="modulo === 'pedidos'" />
        <InventarioAdmin v-else-if="modulo === 'inventario'" />
        <ReportesAdmin v-else-if="modulo === 'reportes'" />
        <RespaldosAdmin v-else-if="modulo === 'respaldos'" />

        <TablaCrud
          v-else-if="configActual"
          :key="modulo"
          :titulo="configActual.titulo"
          :subtitulo="configActual.subtitulo"
          :endpoint="configActual.endpoint"
          :campos="configActual.campos"
          :nuevoModelo="configActual.nuevoModelo"
          :accionExtra="modulo === 'roles' ? { texto: 'Permisos', accion: abrirPermisos } : null"
        />

        <!-- Modal: permisos del rol -->
        <div v-if="rolDePermisos" class="fondo-modal" @click.self="rolDePermisos = null">
          <div class="caja-modal">
            <h3>Permisos de {{ rolDePermisos.nombre }}</h3>
            <p class="gris">
              Lo que se tilde acá es lo que ese rol va a poder abrir. Antes los
              permisos se deducían del nombre del rol.
            </p>
            <p v-if="errorPermisos" class="mensaje-error">{{ errorPermisos }}</p>
            <p v-if="cargandoPermisos" class="gris">Cargando...</p>
            <div v-else class="lista-permisos">
              <label v-for="p in permisosDisponibles" :key="p.id" class="check-form">
                <input type="checkbox" :value="p.id" v-model="permisosTildados" />
                {{ p.nombre }}
              </label>
            </div>
            <div class="fila entre acciones-modal">
              <button class="btn btn-linea" @click="rolDePermisos = null">Cancelar</button>
              <button class="btn btn-principal" :disabled="guardandoPermisos" @click="guardarPermisos">
                {{ guardandoPermisos ? 'Guardando...' : 'Guardar permisos' }}
              </button>
            </div>
          </div>
        </div>
      </main>
    </div>
  </div>
</template>

<style scoped>
.panel {
  display: flex;
  min-height: 100vh;
}

.chico { font-size: 12px; }

/* Sidebar */
.sidebar {
  width: 230px;
  background: var(--cafe-oscuro);
  color: #e9d9c4;
  display: flex;
  flex-direction: column;
  padding: 18px 0;
  flex-shrink: 0;
}

.marca {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 0 18px 18px 18px;
  border-bottom: 1px solid #4d3323;
}

.marca strong { color: #fff; }

.logo {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: var(--caramelo);
  color: #fff;
  font-weight: 800;
  display: flex;
  align-items: center;
  justify-content: center;
}

.nav {
  flex: 1;
  overflow-y: auto;
  padding: 12px 10px;
}

.grupo { margin-bottom: 14px; }

.grupo-titulo {
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 0.6px;
  color: #9c8064;
  padding: 6px 10px;
}

.item {
  display: block;
  width: 100%;
  text-align: left;
  background: none;
  border: none;
  color: #e9d9c4;
  padding: 10px 12px;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  font-family: inherit;
}

.item:hover { background: #4a3123; }

.item.activo {
  background: var(--caramelo);
  color: #fff;
  font-weight: 600;
}

.icono { margin-right: 8px; }

.usuario-pie {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 14px 18px 0 18px;
  border-top: 1px solid #4d3323;
}

.avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--caramelo);
  color: #fff;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Contenido */
.contenido {
  flex: 1;
  display: flex;
  flex-direction: column;
  background: var(--crema);
  min-width: 0;
}

.topbar {
  background: #fff;
  border-bottom: 1px solid var(--linea);
  padding: 12px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.titulo-top { font-weight: 700; color: var(--cafe-oscuro); font-size: 16px; }

.derecha-top {
  align-items: center;
  gap: 10px;
}

.chico-avatar {
  width: 34px;
  height: 34px;
  font-size: 14px;
}

.area {
  padding: 26px;
  overflow-y: auto;
}

@media (max-width: 760px) {
  .sidebar { width: 64px; }
  .sidebar .marca strong,
  .sidebar .marca .chico,
  .grupo-titulo,
  .etiqueta-item,
  .usuario-pie div:last-child { display: none; }
  .item { text-align: center; }
  .icono { margin: 0; }
}
</style>

<style scoped>
/* Lista de permisos del rol: dos columnas para que entren sin scroll largo. */
.lista-permisos {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 6px 16px;
  margin: 12px 0;
}

@media (max-width: 520px) {
  .lista-permisos { grid-template-columns: 1fr; }
}
</style>
