<script setup>
import { ref, computed, onMounted } from 'vue';
import api from '@/api/axios';
import { store } from '@/store';

const pedidos = ref([]);
const ventas = ref([]);
const clientes = ref([]);
const cargando = ref(false);
const error = ref('');
const ok = ref('');
const tab = ref('PENDIENTE');

// Hoy, para que la fecha de entrega no admita dias que ya pasaron.
const hoyStr = new Date().toISOString().slice(0, 10);

// Historial de estados del pedido que se esta mirando.
const modalHistorial = ref(false);
const historial = ref([]);
const minutosHistorial = ref(null);
const pedidoHistorial = ref(null);
const cargandoHistorial = ref(false);

// Confirmacion de borrado con el modal del sistema, no con el del navegador.
const pedidoABorrar = ref(null);

const modalAbierto = ref(false);
const editandoId = ref(null);
const guardando = ref(false);
const form = ref(nuevo());

// Estados posibles del pedido (en el orden en que avanzan).
const estados = ['PENDIENTE', 'EN PREPARACION', 'LISTO', 'ENTREGADO'];

function nuevo() {
  return { ventaId: null, estado: 'PENDIENTE', fechaEntrega: null };
}

async function cargar() {
  cargando.value = true;
  error.value = '';
  try {
    const [pe, v, c] = await Promise.all([
      api.get('/pedidos'),
      api.get('/ventas').catch(() => ({ data: [] })),
      api.get('/clientes').catch(() => ({ data: [] })),
    ]);
    pedidos.value = pe.data || [];
    ventas.value = v.data || [];
    clientes.value = c.data || [];
  } catch {
    error.value = 'No se pudieron cargar los pedidos.';
  } finally {
    cargando.value = false;
  }
}

function venta(id) {
  return ventas.value.find((x) => x.id === id);
}
function clienteDeVenta(ventaId) {
  const v = venta(ventaId);
  if (!v) return '—';
  const c = clientes.value.find((x) => x.id === v.clienteId);
  return c ? c.nombre : 'Cliente #' + v.clienteId;
}
function totalDeVenta(ventaId) {
  const v = venta(ventaId);
  return v ? Number(v.total || 0) : 0;
}

const filtrados = computed(() =>
  pedidos.value.filter((p) => (p.estado || '').toUpperCase() === tab.value)
);

function contar(est) {
  return pedidos.value.filter((p) => (p.estado || '').toUpperCase() === est).length;
}

function claseEstado(est) {
  const e = (est || '').toUpperCase();
  if (e === 'PENDIENTE') return 'et-nuevo';
  if (e === 'EN PREPARACION') return 'et-prep';
  if (e === 'LISTO') return 'et-listo';
  if (e === 'ENTREGADO') return 'et-ok';
  return 'et-confirmado';
}

// Avanza el pedido al siguiente estado.
async function avanzar(p) {
  const idx = estados.indexOf((p.estado || '').toUpperCase());
  if (idx === -1 || idx >= estados.length - 1) return;
  const siguiente = estados[idx + 1];
  try {
    // Se manda quien ejecuta el cambio: el backend lo guarda en el
    // historial, que antes no existia.
    await api.put('/pedidos/' + p.id, {
      ...p,
      estado: siguiente,
      usuarioId: store.usuario?.id ?? null,
    });
    ok.value = 'Pedido #' + p.id + ' → ' + siguiente;
    await cargar();
  } catch (e) {
    error.value = e?.response?.data?.mensaje || 'No se pudo cambiar el estado.';
  }
}

// En el modal solo se ofrece el estado actual y el que sigue. Antes estaban
// los cuatro, asi que se podia retroceder o saltearse pasos.
const estadosDisponibles = computed(() => {
  if (!editandoId.value) return [estados[0]];
  const idx = estados.indexOf((form.value.estado || '').toUpperCase());
  if (idx === -1) return estados.slice();
  return estados.slice(idx, idx + 2);
});

async function verHistorial(p) {
  pedidoHistorial.value = p;
  modalHistorial.value = true;
  cargandoHistorial.value = true;
  historial.value = [];
  minutosHistorial.value = null;
  try {
    const r = await api.get('/pedidos/' + p.id + '/historial');
    historial.value = r.data.historial || [];
    minutosHistorial.value = r.data.minutosTranscurridos;
  } catch {
    error.value = 'No se pudo cargar el historial del pedido.';
    modalHistorial.value = false;
  } finally {
    cargandoHistorial.value = false;
  }
}

function fechaLegible(f) {
  if (!f) return '';
  const d = new Date(f);
  return d.toLocaleDateString() + ' ' + d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

function abrirNuevo() {
  editandoId.value = null;
  form.value = nuevo();
  modalAbierto.value = true;
}
function abrirEditar(p) {
  editandoId.value = p.id;
  form.value = { ...p };
  modalAbierto.value = true;
}

async function guardar() {
  guardando.value = true;
  error.value = '';
  try {
    const cuerpo = {
      ...form.value,
      ventaId: Number(form.value.ventaId),
      usuarioId: store.usuario?.id ?? null,
    };
    if (editandoId.value) {
      await api.put('/pedidos/' + editandoId.value, cuerpo);
      ok.value = 'Pedido actualizado.';
    } else {
      await api.post('/pedidos', cuerpo);
      ok.value = 'Pedido creado.';
    }
    modalAbierto.value = false;
    await cargar();
  } catch (e) {
    error.value = e?.response?.data?.mensaje || 'No se pudo guardar el pedido.';
  } finally {
    guardando.value = false;
  }
}

function pedirBorrado(p) {
  pedidoABorrar.value = p;
}

async function eliminar() {
  const p = pedidoABorrar.value;
  if (!p) return;
  pedidoABorrar.value = null;
  try {
    await api.delete('/pedidos/' + p.id);
    ok.value = 'Pedido eliminado.';
    await cargar();
  } catch (e) {
    error.value = e?.response?.data?.mensaje || 'No se pudo eliminar.';
  }
}

onMounted(cargar);
</script>

<template>
  <div>
    <div class="fila entre encabezado">
      <div>
        <h2>Pedidos</h2>
        <p class="gris">Gestión de pedidos · {{ pedidos.length }} en total</p>
      </div>
      <button class="btn btn-principal" @click="abrirNuevo">+ Nuevo pedido</button>
    </div>

    <p v-if="ok" class="mensaje-ok">{{ ok }}</p>
    <p v-if="error" class="mensaje-error">{{ error }}</p>

    <!-- Pestañas -->
    <div class="tabs">
      <button
        v-for="e in estados"
        :key="e"
        class="tab"
        :class="{ activo: tab === e }"
        @click="tab = e"
      >
        {{ e === 'EN PREPARACION' ? 'En preparación' : e.charAt(0) + e.slice(1).toLowerCase() }}
        ({{ contar(e) }})
      </button>
    </div>

    <div class="tarjeta">
      <table class="tabla">
        <thead>
          <tr>
            <th>N.°</th>
            <th>Cliente</th>
            <th>Venta</th>
            <th>Total</th>
            <th>Entrega</th>
            <th>Estado</th>
            <th style="width:200px">Acción</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="p in filtrados" :key="p.id">
            <td><strong>#{{ p.id }}</strong></td>
            <td>{{ clienteDeVenta(p.ventaId) }}</td>
            <td class="gris">Venta #{{ p.ventaId }}</td>
            <td><strong>Bs {{ totalDeVenta(p.ventaId).toFixed(2) }}</strong></td>
            <td class="gris">{{ p.fechaEntrega ? p.fechaEntrega.slice(0, 10) : 'Sin fecha' }}</td>
            <td><span class="etiqueta" :class="claseEstado(p.estado)">{{ p.estado }}</span></td>
            <td>
              <button
                v-if="tab !== 'ENTREGADO'"
                class="btn btn-principal btn-mini"
                @click="avanzar(p)"
              >
                Avanzar →
              </button>
              <button class="btn btn-linea btn-mini" @click="abrirEditar(p)">Editar</button>
              <button class="btn btn-linea btn-mini" @click="verHistorial(p)">Historial</button>
              <button class="btn btn-peligro btn-mini" @click="pedirBorrado(p)">✕</button>
            </td>
          </tr>
          <tr v-if="filtrados.length === 0">
            <td colspan="7" class="gris" style="text-align:center;padding:24px">
              No hay pedidos en este estado.
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal -->
    <div v-if="modalAbierto" class="fondo-modal" @click.self="modalAbierto = false">
      <div class="caja-modal">
        <h3>{{ editandoId ? 'Editar' : 'Nuevo' }} pedido</h3>
        <form @submit.prevent="guardar">
          <div class="campo-form">
            <label>Venta asociada</label>
            <select v-model="form.ventaId" required>
              <option :value="null" disabled>Seleccioná una venta...</option>
              <option v-for="v in ventas" :key="v.id" :value="v.id">
                Venta #{{ v.id }} — Bs {{ Number(v.total || 0).toFixed(2) }}
              </option>
            </select>
          </div>
          <div class="campo-form">
            <label>Estado</label>
            <select v-model="form.estado">
              <option v-for="e in estadosDisponibles" :key="e" :value="e">{{ e }}</option>
            </select>
          </div>
          <div class="campo-form">
            <label>Fecha de entrega</label>
            <input v-model="form.fechaEntrega" type="date" :min="hoyStr" />
          </div>
          <div class="fila entre acciones-modal">
            <button type="button" class="btn btn-linea" @click="modalAbierto = false">Cancelar</button>
            <button class="btn btn-principal" :disabled="guardando">
              {{ guardando ? 'Guardando...' : 'Guardar' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Modal: linea de tiempo del pedido -->
    <div v-if="modalHistorial" class="fondo-modal" @click.self="modalHistorial = false">
      <div class="caja-modal">
        <h3>Historial del pedido #{{ pedidoHistorial?.id }}</h3>
        <p v-if="cargandoHistorial" class="gris">Cargando...</p>
        <p v-else-if="historial.length === 0" class="gris">Este pedido todavía no tiene movimientos.</p>
        <table v-else class="tabla">
          <thead>
            <tr><th>Estado</th><th>Cuándo</th><th>Quién</th></tr>
          </thead>
          <tbody>
            <tr v-for="h in historial" :key="h.id">
              <td>
                <span v-if="h.estadoAnterior" class="gris chico">{{ h.estadoAnterior }} → </span>
                <span class="etiqueta" :class="claseEstado(h.estado)">{{ h.estado }}</span>
              </td>
              <td class="gris">{{ fechaLegible(h.fecha) }}</td>
              <td class="gris">{{ h.usuario || 'Sin autor registrado' }}</td>
            </tr>
          </tbody>
        </table>
        <p v-if="minutosHistorial != null" class="gris" style="margin-top:12px">
          Tiempo transcurrido desde el primer movimiento: <strong>{{ minutosHistorial }} min</strong>
        </p>
        <div class="fila entre acciones-modal">
          <span></span>
          <button class="btn btn-principal" @click="modalHistorial = false">Cerrar</button>
        </div>
      </div>
    </div>

    <!-- Modal: confirmar el borrado, en vez del confirm() del navegador -->
    <div v-if="pedidoABorrar" class="fondo-modal" @click.self="pedidoABorrar = null">
      <div class="caja-modal">
        <h3>Eliminar pedido</h3>
        <p class="gris">
          Se va a eliminar el pedido #{{ pedidoABorrar.id }} junto con su historial de estados.
          Esta acción no se puede deshacer.
        </p>
        <div class="fila entre acciones-modal">
          <button class="btn btn-linea" @click="pedidoABorrar = null">Cancelar</button>
          <button class="btn btn-peligro" @click="eliminar">Eliminar</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.encabezado { margin-bottom: 16px; }

.tabs {
  display: flex;
  gap: 4px;
  border-bottom: 2px solid var(--linea);
  margin-bottom: 18px;
  flex-wrap: wrap;
}

.tab {
  background: none;
  border: none;
  padding: 12px 18px;
  cursor: pointer;
  font-size: 14px;
  color: var(--texto-suave);
  border-bottom: 3px solid transparent;
  margin-bottom: -2px;
}

.tab.activo {
  color: var(--caramelo);
  border-bottom-color: var(--caramelo);
  font-weight: 700;
}

.btn-mini { margin-right: 6px; }
.campo-form { margin-bottom: 14px; }
.acciones-modal { margin-top: 20px; }
</style>
