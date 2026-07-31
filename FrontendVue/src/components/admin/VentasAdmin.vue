<script setup>
import { ref, computed, onMounted } from 'vue';
import api from '@/api/axios';
import { store } from '@/store';

const productos = ref([]);
const clientes = ref([]);
const metodos = ref([]);
const sucursales = ref([]);
const ventas = ref([]);
const cargando = ref(false);
const error = ref('');
const ok = ref('');
const busqueda = ref('');

// Datos de la venta que se esta armando
const carrito = ref([]);
const clienteId = ref(null);
const metodoId = ref(null);
const sucursalId = ref(null);
const confirmando = ref(false);

// Fecha de la venta. Por defecto es hoy, pero se puede registrar una venta
// de dias anteriores (ej: cargar ventas atrasadas). Nunca se permite una
// fecha futura porque ensuciaria los reportes y el panel.
const hoyStr = new Date().toISOString().slice(0, 10);
const fechaVenta = ref(hoyStr);

async function cargar() {
  cargando.value = true;
  error.value = '';
  try {
    const [p, c, m, s, v] = await Promise.all([
      api.get('/productos'),
      api.get('/clientes').catch(() => ({ data: [] })),
      api.get('/metodospago').catch(() => ({ data: [] })),
      api.get('/sucursales').catch(() => ({ data: [] })),
      api.get('/ventas').catch(() => ({ data: [] })),
    ]);
    productos.value = p.data || [];
    clientes.value = c.data || [];
    metodos.value = m.data || [];
    sucursales.value = s.data || [];
    ventas.value = v.data || [];
    // valores por defecto
    clienteId.value = clientes.value[0]?.id ?? null;
    metodoId.value = metodos.value[0]?.id ?? null;
    sucursalId.value = sucursales.value[0]?.id ?? null;
  } catch {
    error.value = 'No se pudieron cargar los datos para la venta.';
  } finally {
    cargando.value = false;
  }
}

const productosFiltrados = computed(() =>
  productos.value.filter(
    (p) => !busqueda.value || (p.nombre || '').toLowerCase().includes(busqueda.value.toLowerCase())
  )
);

function agregar(p) {
  // Precio seguro: si el producto no tiene precio valido, lo tomamos como 0
  // en vez de dejar que se cuele un NaN al total.
  const precio = Number(p.precio);
  const item = carrito.value.find((i) => i.productoId === p.id);
  if (item) item.cantidad++;
  else carrito.value.push({ productoId: p.id, nombre: p.nombre, precio: isNaN(precio) ? 0 : precio, cantidad: 1 });
}
function cambiar(item, delta) {
  item.cantidad += delta;
  // Tope de seguridad para no cargar cantidades absurdas por error.
  if (item.cantidad > 9999) item.cantidad = 9999;
  if (item.cantidad <= 0) carrito.value = carrito.value.filter((i) => i !== item);
}

const total = computed(() => carrito.value.reduce((s, i) => s + i.precio * i.cantidad, 0));

async function confirmar() {
  error.value = '';
  ok.value = '';

  if (carrito.value.length === 0) {
    error.value = 'Agregá al menos un producto.';
    return;
  }
  // Validamos que existan los datos obligatorios en vez de mandar un id "1"
  // inventado que podria no existir y romper la venta con un error del servidor.
  const usuarioId = store.usuario?.id;
  if (!usuarioId) {
    error.value = 'No hay un usuario en sesión para registrar la venta.';
    return;
  }
  if (!sucursalId.value) {
    error.value = 'Elegí una sucursal. Si no hay ninguna, creá una primero.';
    return;
  }
  if (!clienteId.value) {
    error.value = 'Elegí un cliente. Si no hay ninguno, creá uno primero.';
    return;
  }
  if (!metodoId.value) {
    error.value = 'Elegí un método de pago. Si no hay ninguno, creá uno (ej: Efectivo).';
    return;
  }
  // La fecha no puede ser futura (sí puede ser de días anteriores).
  if (fechaVenta.value > hoyStr) {
    error.value = 'La fecha de la venta no puede ser posterior a hoy.';
    return;
  }
  if (total.value <= 0) {
    error.value = 'El total de la venta debe ser mayor a 0.';
    return;
  }

  confirmando.value = true;
  try {
    const cuerpo = {
      sucursalId: Number(sucursalId.value),
      clienteId: Number(clienteId.value),
      usuarioId: Number(usuarioId),
      metodoPagoId: Number(metodoId.value),
      // Usamos las 12:00 del día elegido para que la zona horaria no lo corra al día anterior.
      fecha: new Date(fechaVenta.value + 'T12:00:00').toISOString(),
      total: total.value,
    };
    await api.post('/ventas', cuerpo);
    ok.value = 'Venta registrada por Bs ' + total.value.toFixed(2) + '.';
    carrito.value = [];
    fechaVenta.value = hoyStr;
    await cargar();
  } catch (e) {
    error.value = e?.response?.data?.mensaje || 'No se pudo registrar la venta.';
  } finally {
    confirmando.value = false;
  }
}

function nombreCliente(id) {
  const c = clientes.value.find((x) => x.id === id);
  return c ? c.nombre : 'Consumidor final';
}

onMounted(cargar);
</script>

<template>
  <div>
    <h2>Registrar Venta</h2>
    <p class="gris">Punto de venta</p>

    <p v-if="ok" class="mensaje-ok">{{ ok }}</p>
    <p v-if="error" class="mensaje-error">{{ error }}</p>

    <div class="pos">
      <!-- Productos -->
      <div class="pos-productos">
        <input v-model="busqueda" class="buscador" placeholder="Buscar producto o escanear código..." />
        <div class="grilla-pos">
          <button
            v-for="p in productosFiltrados"
            :key="p.id"
            class="prod-pos"
            @click="agregar(p)"
          >
            <div class="foto-pos"></div>
            <div class="nom">{{ p.nombre }}</div>
            <div class="precio-pos">Bs {{ Number(p.precio).toFixed(2) }}</div>
          </button>
        </div>
      </div>

      <!-- Detalle -->
      <aside class="pos-detalle">
        <h3>Detalle de la venta</h3>

        <label>Fecha de la venta</label>
        <input type="date" v-model="fechaVenta" :max="hoyStr" />

        <label>Cliente</label>
        <select v-model="clienteId">
          <option v-for="c in clientes" :key="c.id" :value="c.id">{{ c.nombre }}</option>
          <option v-if="clientes.length === 0" :value="null">Consumidor final</option>
        </select>

        <div class="detalle-lista">
          <div v-if="carrito.length === 0" class="gris chico" style="padding:10px 0">
            Tocá un producto para agregarlo.
          </div>
          <div v-for="i in carrito" :key="i.productoId" class="linea">
            <div>
              <div>{{ i.nombre }}</div>
              <div class="gris chico">{{ i.cantidad }} x Bs {{ i.precio.toFixed(2) }}</div>
            </div>
            <div class="cant">
              <button @click="cambiar(i, -1)">−</button>
              <span>Bs {{ (i.precio * i.cantidad).toFixed(2) }}</span>
              <button @click="cambiar(i, 1)">+</button>
            </div>
          </div>
        </div>

        <div class="total-linea">
          <span>TOTAL</span>
          <strong>Bs {{ total.toFixed(2) }}</strong>
        </div>

        <label>Método de pago</label>
        <div class="metodos">
          <button
            v-for="m in metodos"
            :key="m.id"
            class="metodo"
            :class="{ activo: metodoId === m.id }"
            @click="metodoId = m.id"
          >
            {{ m.nombre }}
          </button>
          <span v-if="metodos.length === 0" class="gris chico">Sin métodos cargados</span>
        </div>

        <button class="btn btn-principal confirmar" :disabled="confirmando" @click="confirmar">
          {{ confirmando ? 'Registrando...' : 'CONFIRMAR VENTA' }}
        </button>
        <p class="nota gris chico">
          * El detalle por producto se muestra aquí; en la base se guarda el encabezado de la venta.
        </p>
      </aside>
    </div>

    <!-- Historial simple -->
    <div class="tarjeta" style="margin-top:24px">
      <h3>Últimas ventas</h3>
      <table class="tabla">
        <thead>
          <tr><th>#</th><th>Cliente</th><th>Fecha</th><th>Total</th></tr>
        </thead>
        <tbody>
          <tr v-for="v in ventas.slice().reverse().slice(0, 8)" :key="v.id">
            <td>#{{ v.id }}</td>
            <td>{{ nombreCliente(v.clienteId) }}</td>
            <td class="gris">{{ (v.fecha || '').slice(0, 10) }}</td>
            <td><strong>Bs {{ Number(v.total || 0).toFixed(2) }}</strong></td>
          </tr>
          <tr v-if="ventas.length === 0">
            <td colspan="4" class="gris" style="text-align:center;padding:20px">Sin ventas registradas.</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.chico { font-size: 12px; }

.pos {
  display: flex;
  gap: 20px;
  align-items: flex-start;
}

.pos-productos { flex: 1; }

.buscador { margin-bottom: 16px; }

.grilla-pos {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(130px, 1fr));
  gap: 12px;
}

.prod-pos {
  background: #fff;
  border: 1px solid var(--linea);
  border-radius: 10px;
  padding: 10px;
  cursor: pointer;
  text-align: center;
  font-family: inherit;
  transition: border-color 0.15s;
}
.prod-pos:hover { border-color: var(--caramelo); }

.foto-pos {
  height: 60px;
  background: var(--crema-2);
  border-radius: 6px;
  margin-bottom: 8px;
}
.nom { font-weight: 700; font-size: 14px; }
.precio-pos { color: var(--caramelo); font-weight: 700; margin-top: 2px; }

.pos-detalle {
  width: 320px;
  background: #fff;
  border: 1px solid var(--linea);
  border-radius: 12px;
  padding: 18px;
  box-shadow: var(--sombra);
  position: sticky;
  top: 20px;
}

.detalle-lista {
  margin: 10px 0;
  border-top: 1px solid var(--linea);
}

.linea {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0;
  border-bottom: 1px solid var(--linea);
}

.cant { display: flex; align-items: center; gap: 8px; }
.cant button {
  width: 24px; height: 24px;
  border: 1px solid var(--linea);
  background: var(--crema);
  border-radius: 5px; cursor: pointer; padding: 0;
}

.total-linea {
  display: flex;
  justify-content: space-between;
  font-size: 20px;
  margin: 14px 0;
  color: var(--caramelo);
}

.metodos { display: flex; gap: 8px; margin: 8px 0 16px 0; flex-wrap: wrap; }
.metodo {
  border: 1px solid var(--linea);
  background: #fff;
  border-radius: 6px;
  padding: 8px 14px;
  cursor: pointer;
  font-size: 13px;
  color: var(--texto);
}
.metodo.activo { background: var(--caramelo); color: #fff; border-color: var(--caramelo); }

.confirmar { width: 100%; padding: 13px; }
.nota { margin-top: 10px; }

@media (max-width: 860px) {
  .pos { flex-direction: column; }
  .pos-detalle { width: 100%; position: static; }
}
</style>
