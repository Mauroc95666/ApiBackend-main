<script setup>
import { ref, computed, onMounted } from 'vue';
import api from '@/api/axios';

const pedidos = ref([]);
const ventas = ref([]);
const clientes = ref([]);
const cargando = ref(false);
const error = ref('');
const ok = ref('');
const tab = ref('PENDIENTE');

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
    await api.put('/pedidos/' + p.id, { ...p, estado: siguiente });
    ok.value = 'Pedido #' + p.id + ' → ' + siguiente;
    await cargar();
  } catch (e) {
    error.value = e?.response?.data?.mensaje || 'No se pudo cambiar el estado.';
  }
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
    const cuerpo = { ...form.value, ventaId: Number(form.value.ventaId) };
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

async function eliminar(p) {
  if (!confirm('¿Eliminar el pedido #' + p.id + '?')) return;
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
              <button class="btn btn-peligro btn-mini" @click="eliminar(p)">✕</button>
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
              <option v-for="e in estados" :key="e" :value="e">{{ e }}</option>
            </select>
          </div>
          <div class="campo-form">
            <label>Fecha de entrega</label>
            <input v-model="form.fechaEntrega" type="date" />
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
