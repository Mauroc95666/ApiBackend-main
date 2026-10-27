<script setup>
import { ref, computed, onMounted } from 'vue';
import api, { guardarToken } from '@/api/axios';
import { store } from '@/store';
import MapaDireccion from './MapaDireccion.vue';
import { normalizarNombreCliente, normalizarTelefono } from '@/utils/validacionesCliente';

const productos = ref([]);
const categorias = ref([]);
const sucursales = ref([]);
const metodos = ref([]);
const clientes = ref([]);
const clienteRegistrado = ref(null);
const mostrarRegistro = ref(false);
const registrandoCliente = ref(false);
const registroError = ref('');
const registroOk = ref('');
const registro = ref(nuevoRegistro());

const cargando = ref(false);
const error = ref('');
const busqueda = ref('');
const categoriaSel = ref(0);
const carrito = ref([]);
const verCarrito = ref(false);

// Pasos del carrito: 'lista' -> 'datos' -> 'ok'
const paso = ref('lista');
const guardando = ref(false);
const nroPedido = ref(null);

// Datos del cliente necesarios para registrar una compra.
const datos = ref({
  nombre: '',
  telefono: '',
  email: '',
  direccion: '',
  sucursalId: null,
  tipoEntrega: 'RETIRO', // RETIRO o DELIVERY
});

function nuevoRegistro() {
  return { nombre: '', telefono: '', email: '', password: '', direccion: '' };
}

async function cargarTodo() {
  cargando.value = true;
  error.value = '';
  try {
    const [p, c, s, m, cl] = await Promise.all([
      api.get('/productos'),
      api.get('/categorias').catch(() => ({ data: [] })),
      api.get('/sucursales').catch(() => ({ data: [] })),
      api.get('/metodospago').catch(() => ({ data: [] })),
      api.get('/clientes').catch(() => ({ data: [] })),
    ]);
    productos.value = p.data || [];
    categorias.value = c.data || [];
    sucursales.value = s.data || [];
    metodos.value = m.data || [];
    clientes.value = cl.data || [];
    datos.value.sucursalId = sucursales.value[0]?.id ?? null;
    if (store.usuario?.email) {
      clienteRegistrado.value =
        clientes.value.find((cliente) => cliente.email?.toLowerCase() === store.usuario.email.toLowerCase()) ||
        null;
      if (clienteRegistrado.value) cargarDatosCliente(clienteRegistrado.value);
    }
  } catch {
    error.value = 'No se pudieron cargar los productos. Verificá que el backend esté encendido.';
  } finally {
    cargando.value = false;
  }
}

function cargarDatosCliente(cliente) {
  datos.value.nombre = cliente.nombre || '';
  datos.value.telefono = cliente.telefono || '';
  datos.value.email = cliente.email || '';
  datos.value.direccion = cliente.direccion || '';
}

function actualizarNombreRegistro(evento) {
  registro.value.nombre = normalizarNombreCliente(evento.target.value);
}

function actualizarTelefonoRegistro(evento) {
  registro.value.telefono = normalizarTelefono(evento.target.value);
}

async function registrarCliente() {
  registroError.value = '';
  registroOk.value = '';
  if (!registro.value.nombre.trim()) {
    registroError.value = 'Ingresá tu nombre y apellido usando solo letras.';
    return;
  }
  if (registro.value.telefono.length < 6) {
    registroError.value = 'El teléfono debe tener entre 6 y 15 dígitos.';
    return;
  }
  if (!registro.value.direccion) {
    registroError.value = 'Seleccioná tu dirección en el mapa.';
    return;
  }
  if (registro.value.password.length < 8) {
    registroError.value = 'La contraseña debe tener al menos 8 caracteres.';
    return;
  }

  registrandoCliente.value = true;
  try {
    const respuesta = await api.post('/clientes/registrar', {
      nombre: registro.value.nombre,
      telefono: registro.value.telefono,
      email: registro.value.email.trim(),
      password: registro.value.password,
      direccion: registro.value.direccion,
    });
    clienteRegistrado.value = respuesta.data.cliente;
    cargarDatosCliente(clienteRegistrado.value);
    if (!clientes.value.some((cliente) => cliente.id === clienteRegistrado.value.id)) {
      clientes.value.push(clienteRegistrado.value);
    }

    try {
      const login = await api.post('/auth/login', {
        email: registro.value.email.trim(),
        password: registro.value.password,
      });
      guardarToken(login.data.token);
      store.usuario = login.data;
      store.entrarComoCliente();
      registroOk.value = 'Cuenta creada. Ya podés realizar tu pedido.';
    } catch {
      registroOk.value = 'Cuenta creada. Iniciá sesión con tu correo y contraseña para ingresar.';
    }

    registro.value = nuevoRegistro();
    mostrarRegistro.value = false;
  } catch (e) {
    registroError.value = e?.response?.data?.mensaje || 'No se pudo crear la cuenta.';
  } finally {
    registrandoCliente.value = false;
  }
}

function nombreCategoria(id) {
  const c = categorias.value.find((x) => x.id === id);
  return c ? c.nombre : 'General';
}

const productosFiltrados = computed(() => {
  return productos.value.filter((p) => {
    const okCat = categoriaSel.value === 0 || p.categoriaId === categoriaSel.value;
    const okTexto =
      !busqueda.value || (p.nombre || '').toLowerCase().includes(busqueda.value.toLowerCase());
    return okCat && okTexto;
  });
});

function agregar(prod) {
  const stock = Number(prod.stock ?? 0);
  if (stock <= 0) {
    error.value = 'Este producto está agotado.';
    return;
  }
  const item = carrito.value.find((i) => i.id === prod.id);
  if (item) {
    if (item.cantidad >= stock) {
      error.value = 'Solo hay ' + stock + ' unidades de ' + prod.nombre + ' en stock.';
      return;
    }
    item.cantidad++;
  } else {
    carrito.value.push({
      id: prod.id,
      nombre: prod.nombre,
      precio: Number(prod.precio),
      cantidad: 1,
      stock,
    });
  }
  error.value = '';
  verCarrito.value = true;
}

function quitar(id) {
  carrito.value = carrito.value.filter((i) => i.id !== id);
}

function cambiarCantidad(item, delta) {
  const cantidad = item.cantidad + delta;
  if (cantidad > item.stock) {
    error.value = 'Solo hay ' + item.stock + ' unidades de ' + item.nombre + ' en stock.';
    return;
  }
  item.cantidad = cantidad;
  if (item.cantidad <= 0) quitar(item.id);
}

function normalizarCantidad(item) {
  const cantidad = Number(item.cantidad);
  if (!Number.isInteger(cantidad) || cantidad < 1) {
    item.cantidad = 1;
    return;
  }
  if (cantidad > item.stock) {
    item.cantidad = item.stock;
    error.value = 'Solo hay ' + item.stock + ' unidades de ' + item.nombre + ' en stock.';
    return;
  }
  error.value = '';
}

const total = computed(() => carrito.value.reduce((s, i) => s + i.precio * i.cantidad, 0));
const cantidadTotal = computed(() => carrito.value.reduce((s, i) => s + i.cantidad, 0));

function irADatos() {
  if (carrito.value.length === 0) return;
  error.value = '';
  // Si el cliente está logueado, precargamos su nombre.
  if (store.usuario && !datos.value.nombre) datos.value.nombre = store.usuario.nombre || '';
  paso.value = 'datos';
}

function nombreSucursal(id) {
  const s = sucursales.value.find((x) => x.id === id);
  return s ? s.nombre : '';
}

// Guarda la compra en la base: crea (o reutiliza) el cliente, la venta y el pedido.
async function confirmarPedido() {
  error.value = '';
  if (carrito.value.length === 0) {
    error.value = 'Tu carrito está vacío.';
    return;
  }
  // Para registrar una compra se necesitan los datos de contacto y entrega.
  if (!(datos.value.nombre || '').trim()) {
    error.value = 'Escribí tu nombre.';
    return;
  }
  if (!/^[\p{L}]+(?: [\p{L}]+)*$/u.test(datos.value.nombre.trim())) {
    error.value = 'El nombre solo puede contener letras y espacios.';
    return;
  }
  if (!/^\d{6,15}$/.test(datos.value.telefono)) {
    error.value = 'El teléfono es obligatorio y debe tener entre 6 y 15 dígitos.';
    return;
  }
  if (datos.value.tipoEntrega === 'DELIVERY' && !(datos.value.direccion || '').trim()) {
    error.value = 'Seleccioná la dirección de entrega en el mapa.';
    return;
  }
  if (!datos.value.sucursalId) {
    error.value = 'Elegí una sucursal.';
    return;
  }
  // La venta necesita un usuario y un método de pago que existan en la base.
  // Si el visitante no inició sesión, la venta se registra a nombre del
  // usuario de la tienda, que es una cuenta del sistema. Antes se tomaba el
  // primero de la lista de usuarios, que solía ser el administrador, así que
  // las compras de la web parecían hechas por él desde el mostrador.
  let usuarioId = store.usuario?.id;

  if (!usuarioId) {
    try {
      const r = await api.get('/auth/usuario-tienda');
      usuarioId = r.data.id;
    } catch {
      usuarioId = null;
    }
  }
  const metodoPagoId = metodos.value[0]?.id;
  if (!usuarioId) {
    error.value = 'No hay usuarios registrados en el sistema. Pedile al administrador que cree al menos uno.';
    return;
  }
  if (!metodoPagoId) {
    error.value = 'No hay métodos de pago cargados. Pedile al administrador que cree al menos uno (ej: Efectivo).';
    return;
  }

  guardando.value = true;
  try {
    // 1) Cliente: creamos uno con los datos ingresados (así queda guardada la info).
    let clienteId = clienteRegistrado.value?.id ?? null;
    const nombreCliente = (datos.value.nombre || '').trim() || 'Consumidor final';
    if (!clienteId && datos.value.email) {
      clienteId = clientes.value.find(
        (cliente) => cliente.email?.toLowerCase() === datos.value.email.trim().toLowerCase()
      )?.id ?? null;
    }
    if (!clienteId) {
      const rc = await api.post('/clientes', {
        emprendimientoId: 1,
        nombre: nombreCliente,
        ciNit: null,
        telefono: datos.value.telefono || null,
        email: (datos.value.email || '').trim() || null,
        direccion: (datos.value.direccion || '').trim() || null,
      });
      clienteId = rc.data?.id;
    }
    if (!clienteId) {
      throw new Error('No se pudo registrar al cliente.');
    }

    // 2) Venta, con su detalle por producto. El detalle viaja junto con la
    // venta igual que en el punto de venta: sin el, la compra queda sin
    // saberse qué se vendió y el stock no se puede descontar.
    const rv = await api.post('/ventas', {
      sucursalId: Number(datos.value.sucursalId),
      clienteId: Number(clienteId),
      usuarioId: Number(usuarioId),
      metodoPagoId: Number(metodoPagoId),
      fecha: new Date().toISOString(),
      total: total.value,
      detallesVenta: carrito.value.map((i) => ({
        productoId: i.id,
        cantidad: i.cantidad,
        precioUnitario: i.precio,
      })),
    });
    const ventaId = rv.data?.id;

    // 3) Pedido (para que aparezca en la gestión de pedidos)
    await api.post('/pedidos', {
      ventaId: Number(ventaId),
      estado: 'PENDIENTE',
      fechaEntrega: null,
      tipoEntrega: datos.value.tipoEntrega, // se ignora si la base aún no tiene la columna
    });

    nroPedido.value = ventaId;
    paso.value = 'ok';
  } catch (e) {
    error.value = e?.response?.data?.mensaje || 'No se pudo registrar el pedido. Revisá que el backend esté encendido.';
  } finally {
    guardando.value = false;
  }
}

function nuevoPedido() {
  carrito.value = [];
  if (clienteRegistrado.value) cargarDatosCliente(clienteRegistrado.value);
  else {
    datos.value.nombre = '';
    datos.value.telefono = '';
    datos.value.email = '';
    datos.value.direccion = '';
  }
  datos.value.tipoEntrega = 'RETIRO';
  paso.value = 'lista';
  nroPedido.value = null;
  verCarrito.value = false;
}

onMounted(cargarTodo);
</script>

<template>
  <div class="tienda">
    <header class="cabecera">
      <div class="marca">
        <div class="logo-mini">K</div>
        <div>
          <strong>Panadería Kiataque</strong>
          <div class="gris chico">Pan fresco todos los días</div>
        </div>
      </div>
      <div class="acciones-cab">
        <button
          v-if="!store.usuario"
          class="btn btn-linea btn-mini"
          @click="mostrarRegistro = true"
        >Registrarse</button>
        <button class="btn btn-linea btn-mini" @click="verCarrito = !verCarrito">
          🛒 Mi pedido ({{ cantidadTotal }})
        </button>
        <button class="btn btn-principal btn-mini" @click="store.cerrarSesion()">
          {{ store.usuario ? 'Cerrar sesión' : 'Iniciar sesión' }}
        </button>
      </div>
    </header>

    <div v-if="registroOk" class="mensaje-ok registro-aviso">{{ registroOk }}</div>
    <div v-if="mostrarRegistro" class="fondo-modal" @click.self="mostrarRegistro = false">
      <div class="caja-modal">
        <h3>Crear cuenta de cliente</h3>
        <p v-if="registroError" class="mensaje-error">{{ registroError }}</p>
        <form @submit.prevent="registrarCliente">
          <div class="campo-t">
            <label>Nombre y apellido *</label>
            <input
              :value="registro.nombre"
              required
              autocomplete="name"
              @input="actualizarNombreRegistro"
            />
          </div>
          <div class="campo-t">
            <label>Teléfono *</label>
            <input
              :value="registro.telefono"
              type="tel"
              inputmode="numeric"
              pattern="[0-9]{6,15}"
              maxlength="15"
              autocomplete="tel"
              required
              @input="actualizarTelefonoRegistro"
            />
          </div>
          <div class="campo-t">
            <label>Correo *</label>
            <input v-model="registro.email" type="email" autocomplete="email" required />
          </div>
          <div class="campo-t">
            <label>Contraseña *</label>
            <input
              v-model="registro.password"
              type="password"
              minlength="8"
              autocomplete="new-password"
              required
            />
          </div>
          <div class="campo-t">
            <label>Seleccioná tu dirección *</label>
            <MapaDireccion v-model="registro.direccion" />
          </div>
          <div class="fila entre acciones-modal">
            <button type="button" class="btn btn-linea" @click="mostrarRegistro = false">Cancelar</button>
            <button class="btn btn-principal" :disabled="registrandoCliente || !registro.direccion">
              {{ registrandoCliente ? 'Creando...' : 'Crear cuenta' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <div class="hero">
      <h1>Nuestros productos</h1>
      <p>Elegí lo que quieras y armá tu pedido. Cuñapés, empanadas, panes y más.</p>
    </div>

    <div class="cuerpo">
      <div class="catalogo">
        <div class="barra-filtros">
          <input v-model="busqueda" class="buscador" placeholder="Buscar producto..." />
          <div class="chips">
            <button class="chip" :class="{ activo: categoriaSel === 0 }" @click="categoriaSel = 0">Todos</button>
            <button
              v-for="c in categorias"
              :key="c.id"
              class="chip"
              :class="{ activo: categoriaSel === c.id }"
              @click="categoriaSel = c.id"
            >
              {{ c.nombre }}
            </button>
          </div>
        </div>

        <p v-if="cargando" class="cargando">Cargando productos...</p>
        <p v-if="error && paso === 'lista'" class="mensaje-error">{{ error }}</p>

        <div v-if="!cargando && productosFiltrados.length === 0 && !error" class="vacio">
          No hay productos para mostrar.
        </div>

        <div class="grilla">
          <div v-for="p in productosFiltrados" :key="p.id" class="prod">
            <div class="prod-foto">
              <img v-if="p.imagenUrl" :src="p.imagenUrl" :alt="p.nombre" class="img-prod" />
              <span v-else>Foto del producto</span>
            </div>
            <div class="prod-info">
              <h3>{{ p.nombre }}</h3>
              <div class="gris chico">{{ nombreCategoria(p.categoriaId) }}</div>
              <div class="prod-precio">Bs {{ Number(p.precio).toFixed(2) }}</div>
              <div class="chico" :class="(p.stock ?? 0) > 0 ? 'hay' : 'agotado'">
                {{ (p.stock ?? 0) > 0 ? `Stock disponible: ${p.stock} unidades` : 'Agotado · 0 unidades' }}
              </div>
              <button
                class="btn btn-principal btn-mini agregar"
                :disabled="(p.stock ?? 0) <= 0"
                @click="agregar(p)"
              >
                Agregar
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Panel del pedido -->
      <aside v-if="verCarrito" class="carrito">
        <h3>Mi pedido</h3>

        <!-- Paso 1: lista del carrito -->
        <template v-if="paso === 'lista'">
          <p v-if="carrito.length === 0" class="gris">Todavía no agregaste nada.</p>
          <div v-for="i in carrito" :key="i.id" class="linea-carrito">
            <div class="info-carrito">
              <div>{{ i.nombre }}</div>
              <div class="gris chico">Precio unitario: Bs {{ i.precio.toFixed(2) }}</div>
              <div class="gris chico">Subtotal: Bs {{ (i.precio * i.cantidad).toFixed(2) }}</div>
            </div>
            <div class="cant">
              <button @click="cambiarCantidad(i, -1)">−</button>
              <input
                v-model.number="i.cantidad"
                class="cantidad-input"
                type="number"
                min="1"
                :max="i.stock"
                :aria-label="'Cantidad de ' + i.nombre"
                @change="normalizarCantidad(i)"
              />
              <button @click="cambiarCantidad(i, 1)">+</button>
            </div>
          </div>
          <div v-if="carrito.length" class="total-carrito">
            <span>Total</span><strong>Bs {{ total.toFixed(2) }}</strong>
          </div>
          <button class="btn btn-principal hacer-pedido" :disabled="carrito.length === 0" @click="irADatos">
            Continuar
          </button>
        </template>

        <!-- Paso 2: datos del cliente y entrega -->
        <template v-else-if="paso === 'datos'">
          <p v-if="error" class="mensaje-error">{{ error }}</p>

          <p class="gris chico" style="margin-bottom:10px">
            Completá tus datos de contacto para confirmar el pedido.
          </p>
          <div class="campo-t">
            <label>Tu nombre *</label>
            <input
              :value="datos.nombre"
              required
              :readonly="Boolean(clienteRegistrado)"
              autocomplete="name"
              @input="datos.nombre = normalizarNombreCliente($event.target.value)"
            />
          </div>
          <div class="campo-t">
            <label>Teléfono *</label>
            <input
              :value="datos.telefono"
              type="tel"
              inputmode="numeric"
              pattern="[0-9]{6,15}"
              maxlength="15"
              autocomplete="tel"
              required
              :readonly="Boolean(clienteRegistrado)"
              @input="datos.telefono = normalizarTelefono($event.target.value)"
            />
          </div>
          <div class="campo-t" v-if="datos.tipoEntrega === 'DELIVERY'">
            <label>Ubicación de entrega *</label>
            <MapaDireccion v-model="datos.direccion" />
          </div>
          <div class="campo-t">
            <label>Correo (opcional)</label>
            <input
              v-model="datos.email"
              type="email"
              autocomplete="email"
              placeholder="tucorreo@mail.com"
              :readonly="Boolean(clienteRegistrado)"
            />
          </div>
          <div class="campo-t">
            <label>Sucursal</label>
            <select v-model="datos.sucursalId">
              <option v-for="s in sucursales" :key="s.id" :value="s.id">{{ s.nombre }}</option>
              <option v-if="sucursales.length === 0" :value="null">No hay sucursales</option>
            </select>
          </div>
          <div class="campo-t">
            <label>¿Cómo lo querés?</label>
            <div class="entrega">
              <button
                type="button"
                class="op-entrega"
                :class="{ activo: datos.tipoEntrega === 'RETIRO' }"
                @click="datos.tipoEntrega = 'RETIRO'"
              >
                🏬 Retiro en local
              </button>
              <button
                type="button"
                class="op-entrega"
                :class="{ activo: datos.tipoEntrega === 'DELIVERY' }"
                @click="datos.tipoEntrega = 'DELIVERY'"
              >
                🛵 Delivery
              </button>
            </div>
          </div>

          <div class="total-carrito">
            <span>Total</span><strong>Bs {{ total.toFixed(2) }}</strong>
          </div>

          <button
            class="btn btn-principal hacer-pedido"
            :disabled="guardando || (datos.tipoEntrega === 'DELIVERY' && !datos.direccion)"
            @click="confirmarPedido"
          >
            {{ guardando ? 'Guardando...' : 'Confirmar pedido' }}
          </button>
          <button class="btn btn-linea volver" @click="paso = 'lista'">Volver</button>
        </template>

        <!-- Paso 3: confirmación -->
        <div v-else class="pedido-ok">
          <div class="check-grande">✓</div>
          <h4>¡Pedido registrado!</h4>
          <p class="gris">Pedido <strong>#{{ nroPedido }}</strong> guardado.</p>
          <p class="gris resumen">
            {{ datos.tipoEntrega === 'RETIRO' ? 'Retiro en' : 'Delivery desde' }}
            <strong>{{ nombreSucursal(datos.sucursalId) }}</strong><br />
            Total: <strong>Bs {{ total.toFixed(2) }}</strong>
          </p>
          <p class="gris chico">La panadería se va a contactar para coordinar la entrega.</p>
          <button class="btn btn-linea" @click="nuevoPedido">Hacer otro pedido</button>
        </div>
      </aside>
    </div>

    <footer class="pie">Panadería Kiataque · Santa Cruz, Bolivia · Tel. 700-00000</footer>
  </div>
</template>

<style scoped>
.tienda { min-height: 100vh; display: flex; flex-direction: column; }

.cabecera {
  background: var(--cafe-oscuro);
  color: #f4ead9;
  padding: 14px 28px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.marca { display: flex; align-items: center; gap: 12px; }
.logo-mini {
  width: 42px; height: 42px; border-radius: 50%;
  background: var(--caramelo); color: #fff; font-weight: 800;
  display: flex; align-items: center; justify-content: center; font-size: 20px;
}
.marca strong { color: #fff; }
.acciones-cab { display: flex; gap: 10px; }
.chico { font-size: 12px; }

.hero { background: var(--crema-2); padding: 34px 28px; border-bottom: 1px solid var(--linea); }
.hero h1 { font-size: 28px; }

.cuerpo { flex: 1; display: flex; gap: 20px; padding: 24px 28px; align-items: flex-start; }
.catalogo { flex: 1; }
.barra-filtros { margin-bottom: 18px; }
.buscador { max-width: 320px; margin-bottom: 12px; }
.chips { display: flex; gap: 8px; flex-wrap: wrap; }
.chip {
  border: 1px solid var(--linea); background: #fff; border-radius: 20px;
  padding: 7px 16px; font-size: 13px; cursor: pointer; color: var(--texto);
}
.chip.activo { background: var(--caramelo); color: #fff; border-color: var(--caramelo); }

.grilla { display: grid; grid-template-columns: repeat(auto-fill, minmax(210px, 1fr)); gap: 18px; }
.prod { background: #fff; border: 1px solid var(--linea); border-radius: 10px; overflow: hidden; box-shadow: var(--sombra); }
.prod-foto { height: 150px; background: var(--crema-2); display: flex; align-items: center; justify-content: center; color: #c2a888; font-size: 13px; overflow: hidden; }
.img-prod { width: 100%; height: 100%; object-fit: cover; }
.prod-info { padding: 14px; }
.prod-info h3 { font-size: 16px; margin-bottom: 2px; }
.prod-precio { color: var(--caramelo); font-size: 20px; font-weight: 700; margin: 6px 0 2px 0; }
.hay { color: var(--verde); }
.agotado { color: var(--rojo); }
.agregar { width: 100%; margin-top: 10px; }

.carrito {
  width: 320px; background: #fff; border: 1px solid var(--linea);
  border-radius: 10px; padding: 18px; position: sticky; top: 24px; box-shadow: var(--sombra);
}
.linea-carrito { display: flex; justify-content: space-between; align-items: center; gap: 8px; padding: 10px 0; border-bottom: 1px solid var(--linea); }
.info-carrito { min-width: 0; }
.cant { display: flex; align-items: center; gap: 6px; flex-shrink: 0; }
.cant button { width: 26px; height: 26px; border: 1px solid var(--linea); background: var(--crema); border-radius: 5px; cursor: pointer; padding: 0; }
.cantidad-input { width: 58px; padding: 4px; text-align: center; }
.total-carrito { display: flex; justify-content: space-between; margin: 16px 0; font-size: 18px; }
.hacer-pedido { width: 100%; }
.volver { width: 100%; margin-top: 8px; }

.campo-t { margin-bottom: 12px; }
.campo-t label { display: block; font-size: 12px; font-weight: 700; text-transform: uppercase; color: var(--texto-suave); margin-bottom: 5px; }
.entrega { display: flex; gap: 8px; }
.op-entrega {
  flex: 1; border: 1px solid var(--linea); background: #fff; border-radius: 8px;
  padding: 10px 6px; cursor: pointer; font-size: 13px; color: var(--texto);
}
.op-entrega.activo { background: var(--caramelo); color: #fff; border-color: var(--caramelo); }

.pedido-ok { text-align: center; padding: 10px 0; }
.check-grande {
  width: 60px; height: 60px; border-radius: 50%; background: var(--verde);
  color: #fff; font-size: 30px; display: flex; align-items: center; justify-content: center; margin: 0 auto 14px auto;
}
.resumen { margin: 10px 0; }
.vacio { padding: 30px; color: var(--texto-suave); }
.pie { background: var(--cafe-oscuro); color: #c9b193; text-align: center; padding: 16px; font-size: 13px; }

@media (max-width: 820px) {
  .cuerpo { flex-direction: column; }
  .carrito { width: 100%; position: static; }
}
</style>
