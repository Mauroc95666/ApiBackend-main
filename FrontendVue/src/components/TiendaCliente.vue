<script setup>
import { ref, computed, onMounted } from 'vue';
import api from '@/api/axios';
import { store } from '@/store';

const productos = ref([]);
const categorias = ref([]);
const sucursales = ref([]);
const usuarios = ref([]);
const metodos = ref([]);
const clientes = ref([]);

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

// Datos de la "cuenta" que el cliente crea para poder comprar.
// Aunque entre como anónimo, para confirmar la compra tiene que dejar
// su nombre y su dirección (y opcionalmente teléfono y correo).
const datos = ref({
  nombre: '',
  telefono: '',
  email: '',
  direccion: '',
  sucursalId: null,
  tipoEntrega: 'RETIRO', // RETIRO o DELIVERY
});

async function cargarTodo() {
  cargando.value = true;
  error.value = '';
  try {
    const [p, c, s, u, m, cl] = await Promise.all([
      api.get('/productos'),
      api.get('/categorias').catch(() => ({ data: [] })),
      api.get('/sucursales').catch(() => ({ data: [] })),
      api.get('/usuarios').catch(() => ({ data: [] })),
      api.get('/metodospago').catch(() => ({ data: [] })),
      api.get('/clientes').catch(() => ({ data: [] })),
    ]);
    productos.value = p.data || [];
    categorias.value = c.data || [];
    sucursales.value = s.data || [];
    usuarios.value = u.data || [];
    metodos.value = m.data || [];
    clientes.value = cl.data || [];
    datos.value.sucursalId = sucursales.value[0]?.id ?? null;
  } catch {
    error.value = 'No se pudieron cargar los productos. Verificá que el backend esté encendido.';
  } finally {
    cargando.value = false;
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
  const item = carrito.value.find((i) => i.id === prod.id);
  if (item) item.cantidad++;
  else carrito.value.push({ id: prod.id, nombre: prod.nombre, precio: Number(prod.precio), cantidad: 1 });
  verCarrito.value = true;
}

function quitar(id) {
  carrito.value = carrito.value.filter((i) => i.id !== id);
}

function cambiarCantidad(item, delta) {
  item.cantidad += delta;
  if (item.cantidad <= 0) quitar(item.id);
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
  // Para comprar hay que "crear la cuenta": nombre y dirección son obligatorios.
  if (!(datos.value.nombre || '').trim()) {
    error.value = 'Escribí tu nombre para crear tu cuenta.';
    return;
  }
  if (!(datos.value.direccion || '').trim()) {
    error.value = 'Escribí tu dirección para poder registrar la compra.';
    return;
  }
  if (!datos.value.sucursalId) {
    error.value = 'Elegí una sucursal.';
    return;
  }
  // La venta necesita un usuario y un método de pago que existan en la base.
  const usuarioId = store.usuario?.id || usuarios.value[0]?.id;
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
    let clienteId = null;
    const nombreCliente = (datos.value.nombre || '').trim() || 'Consumidor final';
    try {
      const rc = await api.post('/clientes', {
        emprendimientoId: 1,
        nombre: nombreCliente,
        cI_NIT: null,
        telefono: datos.value.telefono || null,
        email: (datos.value.email || '').trim() || null,
        direccion: (datos.value.direccion || '').trim() || null,
      });
      clienteId = rc.data?.id;
    } catch {
      // Si falla, reutilizamos el primer cliente que exista.
      clienteId = clientes.value[0]?.id;
    }
    if (!clienteId) {
      error.value = 'No se pudo registrar el cliente. Pedile al administrador que cree un cliente inicial.';
      guardando.value = false;
      return;
    }

    // 2) Venta (encabezado de la compra)
    const rv = await api.post('/ventas', {
      sucursalId: Number(datos.value.sucursalId),
      clienteId: Number(clienteId),
      usuarioId: Number(usuarioId),
      metodoPagoId: Number(metodoPagoId),
      fecha: new Date().toISOString(),
      total: total.value,
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
  datos.value.nombre = '';
  datos.value.telefono = '';
  datos.value.email = '';
  datos.value.direccion = '';
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
        <button class="btn btn-linea btn-mini" @click="verCarrito = !verCarrito">
          🛒 Mi pedido ({{ cantidadTotal }})
        </button>
        <button class="btn btn-principal btn-mini" @click="store.cerrarSesion()">Iniciar sesión</button>
      </div>
    </header>

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
                {{ (p.stock ?? 0) > 0 ? 'Disponible' : 'Agotado' }}
              </div>
              <button class="btn btn-principal btn-mini agregar" @click="agregar(p)">Agregar</button>
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
            <div>
              <div>{{ i.nombre }}</div>
              <div class="gris chico">Bs {{ i.precio.toFixed(2) }} c/u</div>
            </div>
            <div class="cant">
              <button @click="cambiarCantidad(i, -1)">−</button>
              <span>{{ i.cantidad }}</span>
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
            Para confirmar tu pedido creá tu cuenta con tus datos.
          </p>
          <div class="campo-t">
            <label>Tu nombre *</label>
            <input v-model="datos.nombre" placeholder="Nombre y apellido" />
          </div>
          <div class="campo-t">
            <label>Dirección *</label>
            <input v-model="datos.direccion" placeholder="Ej: Av. Siempre Viva #123, zona Norte" />
          </div>
          <div class="campo-t">
            <label>Teléfono (opcional)</label>
            <input v-model="datos.telefono" placeholder="700-00000" />
          </div>
          <div class="campo-t">
            <label>Correo (opcional)</label>
            <input v-model="datos.email" placeholder="tucorreo@mail.com" />
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
                class="op-entrega"
                :class="{ activo: datos.tipoEntrega === 'RETIRO' }"
                @click="datos.tipoEntrega = 'RETIRO'"
              >
                🏬 Retiro en local
              </button>
              <button
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

          <button class="btn btn-principal hacer-pedido" :disabled="guardando" @click="confirmarPedido">
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
.linea-carrito { display: flex; justify-content: space-between; align-items: center; padding: 10px 0; border-bottom: 1px solid var(--linea); }
.cant { display: flex; align-items: center; gap: 8px; }
.cant button { width: 26px; height: 26px; border: 1px solid var(--linea); background: var(--crema); border-radius: 5px; cursor: pointer; padding: 0; }
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
