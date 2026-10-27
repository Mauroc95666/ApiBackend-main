<script setup>
import { ref, computed, onMounted } from 'vue';
import api from '@/api/axios';
import { store } from '@/store';

const inventarios = ref([]);
const productos = ref([]);
const sucursales = ref([]);
// 0 significa todas las sucursales. El filtrado lo hace el servidor.
const sucursalFiltro = ref(0);
const cargando = ref(false);
const error = ref('');
const ok = ref('');

// Modal para editar el stock mínimo. El stock actual cambia solo con movimientos.
const modalAbierto = ref(false);
const editandoId = ref(null);
const guardando = ref(false);
const form = ref({ stockMinimo: 0 });

// modal de movimiento (entrada / salida)
const modalMov = ref(false);
const mov = ref({ inventarioId: null, tipo: 'ENTRADA', cantidad: 1, motivo: '' });
const registrandoMov = ref(false);

// modal de historial de movimientos de un item
const modalHistorial = ref(false);
const movimientos = ref([]);
const itemHistorial = ref(null);
const cargandoHistorial = ref(false);

async function cargar() {
  cargando.value = true;
  error.value = '';
  try {
    const [i, p, s] = await Promise.all([
      api.get('/inventarios', {
        params: { sucursalId: sucursalFiltro.value || undefined },
      }),
      api.get('/productos').catch(() => ({ data: [] })),
      api.get('/sucursales').catch(() => ({ data: [] })),
    ]);
    inventarios.value = i.data || [];
    productos.value = p.data || [];
    sucursales.value = s.data || [];
  } catch {
    error.value = 'No se pudo cargar el inventario.';
  } finally {
    cargando.value = false;
  }
}

function nombreProducto(id) {
  const p = productos.value.find((x) => x.id === id);
  return p ? p.nombre : 'Producto #' + id;
}
function precioProducto(id) {
  const p = productos.value.find((x) => x.id === id);
  return p ? Number(p.precio) : 0;
}
function nombreSucursal(id) {
  const s = sucursales.value.find((x) => x.id === id);
  return s ? s.nombre : '—';
}

const bajoMinimo = computed(
  () => inventarios.value.filter((i) => i.stockMinimo != null && i.stockActual <= i.stockMinimo).length
);
const sinStock = computed(() => inventarios.value.filter((i) => i.stockActual <= 0).length);
const valorStock = computed(() =>
  inventarios.value.reduce((s, i) => s + i.stockActual * precioProducto(i.productoId), 0)
);

function estado(i) {
  if (i.stockActual <= 0) return { txt: 'SIN STOCK', clase: 'et-critico' };
  if (i.stockMinimo != null && i.stockActual < i.stockMinimo) return { txt: 'CRÍTICO', clase: 'et-critico' };
  if (i.stockMinimo != null && i.stockActual === i.stockMinimo) return { txt: 'AL LÍMITE', clase: 'et-bajo' };
  return { txt: 'OK', clase: 'et-ok' };
}

function nivel(i) {
  const min = i.stockMinimo || 1;
  const pct = Math.min(100, Math.round((i.stockActual / (min * 2)) * 100));
  return pct;
}
function colorNivel(i) {
  const e = estado(i).clase;
  if (e === 'et-critico') return 'var(--rojo)';
  if (e === 'et-bajo') return 'var(--ambar)';
  return 'var(--verde)';
}

function abrirEditar(i) {
  editandoId.value = i.id;
  form.value = { stockMinimo: i.stockMinimo ?? 0 };
  modalAbierto.value = true;
}

async function guardar() {
  error.value = '';
  const stockMinimo = Number(form.value.stockMinimo);
  if (!Number.isInteger(stockMinimo) || stockMinimo < 0) {
    error.value = 'El stock mínimo tiene que ser un número entero mayor o igual a 0.';
    return;
  }
  guardando.value = true;
  try {
    await api.put('/inventarios/' + editandoId.value, { stockMinimo });
    ok.value = 'Stock mínimo actualizado.';
    modalAbierto.value = false;
    await cargar();
  } catch (e) {
    error.value = e?.response?.data?.mensaje || 'No se pudo guardar.';
  } finally {
    guardando.value = false;
  }
}

// Arranca sin item elegido a proposito: antes venia preseleccionado el
// primero de la lista y era facil cargarle el movimiento al producto
// equivocado sin darse cuenta.
function abrirMovimiento(tipo, inv = null) {
  error.value = '';
  mov.value = { inventarioId: inv?.id ?? null, tipo, cantidad: 1, motivo: '' };
  modalMov.value = true;
}

async function verHistorial(i) {
  itemHistorial.value = i;
  modalHistorial.value = true;
  cargandoHistorial.value = true;
  movimientos.value = [];
  try {
    const r = await api.get('/movimientosinventario/inventario/' + i.id);
    movimientos.value = r.data.movimientos || [];
  } catch {
    error.value = 'No se pudo cargar el historial de movimientos.';
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

// Los movimientos manuales dejan historial y actualizan ambos saldos en una
// sola transacción; las ventas también sincronizan ambos desde el backend.
async function registrarMovimiento() {
  error.value = '';
  const inv = inventarios.value.find((x) => x.id === mov.value.inventarioId);
  if (!inv) {
    error.value = 'Elegí un ítem del inventario.';
    return;
  }
  const cant = Number(mov.value.cantidad);
  // La cantidad tiene que ser un número entero mayor a 0 (nada de vacío ni negativos).
  if (isNaN(cant) || cant <= 0) {
    error.value = 'La cantidad tiene que ser un número mayor a 0.';
    return;
  }
  // Chequeo en pantalla para avisar antes de ir al servidor; el servidor lo
  // vuelve a verificar porque es el que decide.
  if (mov.value.tipo === 'SALIDA' && cant > inv.stockActual) {
    error.value = 'No hay suficiente stock para esa salida: hay ' + inv.stockActual + '.';
    return;
  }
  registrandoMov.value = true;
  try {
    const r = await api.post('/movimientosinventario', {
      inventarioId: inv.id,
      tipo: mov.value.tipo,
      cantidad: cant,
      usuarioId: store.usuario?.id ?? null,
      motivo: mov.value.motivo || null,
    });
    ok.value =
      mov.value.tipo === 'ENTRADA'
        ? 'Entrada registrada. Stock: ' + r.data.stockActual + '.'
        : 'Salida registrada. Stock: ' + r.data.stockActual + '.';
    modalMov.value = false;
    await cargar();
  } catch (e) {
    error.value = e?.response?.data?.mensaje || 'No se pudo registrar el movimiento.';
  } finally {
    registrandoMov.value = false;
  }
}

onMounted(cargar);
</script>

<template>
  <div>
    <div class="fila entre encabezado-inv">
      <div>
        <h2>Inventario</h2>
        <p class="gris">Control de stock e insumos</p>
      </div>
      <div class="campo-form selector-sucursal">
        <label>Sucursal</label>
        <select v-model="sucursalFiltro" @change="cargar">
          <option :value="0">Todas las sucursales</option>
          <option v-for="s in sucursales" :key="s.id" :value="s.id">{{ s.nombre }}</option>
        </select>
      </div>
    </div>

    <p v-if="ok" class="mensaje-ok">{{ ok }}</p>
    <p v-if="error" class="mensaje-error">{{ error }}</p>

    <!-- KPIs -->
    <div class="tarjetas-kpi">
      <div class="kpi"><div class="kpi-titulo">Total de ítems</div><div class="kpi-numero">{{ inventarios.length }}</div></div>
      <div class="kpi"><div class="kpi-titulo">Bajo mínimo</div><div class="kpi-numero">{{ bajoMinimo }}</div></div>
      <div class="kpi"><div class="kpi-titulo">Sin stock</div><div class="kpi-numero">{{ sinStock }}</div></div>
      <div class="kpi"><div class="kpi-titulo">Valor del stock</div><div class="kpi-numero">Bs {{ valorStock.toFixed(0) }}</div></div>
    </div>

    <!-- Botones de accion (no centrados, alineados a la izquierda) -->
    <div class="fila entre barra-acciones">
      <div class="fila">
        <button class="btn btn-principal" @click="abrirMovimiento('ENTRADA')">+ Registrar entrada</button>
        <button class="btn btn-linea" @click="abrirMovimiento('SALIDA')">− Registrar salida</button>
      </div>
    </div>

    <div class="tarjeta">
      <table class="tabla">
        <thead>
          <tr>
            <th>Producto / Insumo</th>
            <th>Sucursal</th>
            <th>Actual</th>
            <th>Mínimo</th>
            <th>Nivel</th>
            <th>Estado</th>
            <th style="width:150px">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="i in inventarios" :key="i.id">
            <td><strong>{{ nombreProducto(i.productoId) }}</strong></td>
            <td class="gris">{{ nombreSucursal(i.sucursalId) }}</td>
            <td><strong>{{ i.stockActual }}</strong></td>
            <td class="gris">{{ i.stockMinimo }}</td>
            <td>
              <div class="barra-nivel">
                <div class="relleno" :style="{ width: nivel(i) + '%', background: colorNivel(i) }"></div>
              </div>
            </td>
            <td><span class="etiqueta" :class="estado(i).clase">{{ estado(i).txt }}</span></td>
            <td>
              <button class="btn btn-linea btn-mini" @click="abrirEditar(i)">Editar mínimo</button>
              <button class="btn btn-linea btn-mini" @click="verHistorial(i)">Historial</button>
            </td>
          </tr>
          <tr v-if="!cargando && inventarios.length === 0">
            <td colspan="7" class="gris" style="text-align:center;padding:24px">Sin ítems en inventario.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal inventario -->
    <div v-if="modalAbierto" class="fondo-modal" @click.self="modalAbierto = false">
      <div class="caja-modal">
        <h3>Editar stock mínimo</h3>
        <form @submit.prevent="guardar">
          <div class="campo-form">
            <label>Stock mínimo</label>
            <input v-model="form.stockMinimo" type="number" min="0" step="1" />
          </div>
          <div class="fila entre acciones-modal">
            <button type="button" class="btn btn-linea" @click="modalAbierto = false">Cancelar</button>
            <button class="btn btn-principal" :disabled="guardando">{{ guardando ? 'Guardando...' : 'Guardar' }}</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Modal movimiento -->
    <div v-if="modalMov" class="fondo-modal" @click.self="modalMov = false">
      <div class="caja-modal">
        <h3>Registrar {{ mov.tipo === 'ENTRADA' ? 'entrada' : 'salida' }}</h3>
        <form @submit.prevent="registrarMovimiento">
          <div class="campo-form">
            <label>Ítem</label>
            <select v-model="mov.inventarioId" required>
              <option :value="null" disabled>Elegí el ítem...</option>
              <option v-for="i in inventarios" :key="i.id" :value="i.id">
                {{ nombreProducto(i.productoId) }} (actual: {{ i.stockActual }})
              </option>
            </select>
          </div>
          <div class="campo-form">
            <label>Cantidad</label>
            <input v-model="mov.cantidad" type="number" min="1" required />
          </div>
          <div class="campo-form">
            <label>Motivo</label>
            <input
              v-model="mov.motivo"
              type="text"
              maxlength="200"
              :placeholder="mov.tipo === 'ENTRADA' ? 'Ej: compra al proveedor' : 'Ej: merma por vencimiento'"
            />
          </div>
          <div class="fila entre acciones-modal">
            <button type="button" class="btn btn-linea" @click="modalMov = false">Cancelar</button>
            <button class="btn btn-principal" :disabled="registrandoMov">
              {{ registrandoMov ? 'Registrando...' : 'Registrar' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Modal: historial de movimientos del item -->
    <div v-if="modalHistorial" class="fondo-modal" @click.self="modalHistorial = false">
      <div class="caja-modal">
        <h3>Movimientos de {{ nombreProducto(itemHistorial?.productoId) }}</h3>
        <p v-if="cargandoHistorial" class="gris">Cargando...</p>
        <p v-else-if="movimientos.length === 0" class="gris">
          Este ítem todavía no tiene movimientos registrados.
        </p>
        <table v-else class="tabla">
          <thead>
            <tr><th>Tipo</th><th>Cantidad</th><th>Cuándo</th><th>Quién</th><th>Motivo</th></tr>
          </thead>
          <tbody>
            <tr v-for="m in movimientos" :key="m.id">
              <td>
                <span class="etiqueta" :class="m.tipo === 'ENTRADA' ? 'et-ok' : 'et-prep'">
                  {{ m.tipo }}
                </span>
              </td>
              <td><strong>{{ m.tipo === 'ENTRADA' ? '+' : '−' }}{{ m.cantidad }}</strong></td>
              <td class="gris">{{ fechaLegible(m.fecha) }}</td>
              <td class="gris">{{ m.usuario || 'Sin autor registrado' }}</td>
              <td class="gris">{{ m.motivo || '—' }}</td>
            </tr>
          </tbody>
        </table>
        <div class="fila entre acciones-modal">
          <span></span>
          <button class="btn btn-principal" @click="modalHistorial = false">Cerrar</button>
        </div>
      </div>
    </div>

  </div>
</template>

<style scoped>
.encabezado-inv { align-items: flex-end; gap: 16px; flex-wrap: wrap; margin-bottom: 8px; }
.selector-sucursal { margin-bottom: 0; min-width: 220px; }

@media (max-width: 620px) {
  .selector-sucursal { width: 100%; }
}

.tarjetas-kpi {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 16px;
  margin: 18px 0;
}
.kpi { background:#fff; border:1px solid var(--linea); border-radius:10px; padding:18px; box-shadow: var(--sombra); }
.kpi-titulo { color: var(--texto-suave); font-size:13px; margin-bottom:8px; }
.kpi-numero { font-size:28px; font-weight:800; color: var(--cafe-oscuro); }

.barra-acciones { margin-bottom: 16px; }

.barra-nivel {
  width: 140px;
  height: 8px;
  background: var(--crema-2);
  border-radius: 6px;
  overflow: hidden;
}
.relleno { height: 100%; border-radius: 6px; }

.btn-mini { margin-right: 6px; }
.campo-form { margin-bottom: 14px; }
.acciones-modal { margin-top: 20px; }
</style>
