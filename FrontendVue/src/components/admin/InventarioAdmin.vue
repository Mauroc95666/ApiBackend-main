<script setup>
import { ref, computed, onMounted } from 'vue';
import api from '@/api/axios';

const inventarios = ref([]);
const productos = ref([]);
const sucursales = ref([]);
const cargando = ref(false);
const error = ref('');
const ok = ref('');

// modal alta/edicion de fila de inventario
const modalAbierto = ref(false);
const editandoId = ref(null);
const guardando = ref(false);
const form = ref(nuevo());

// modal de movimiento (entrada / salida)
const modalMov = ref(false);
const mov = ref({ inventarioId: null, tipo: 'ENTRADA', cantidad: 1 });

function nuevo() {
  return { productoId: null, sucursalId: null, stockActual: 0, stockMinimo: 0 };
}

async function cargar() {
  cargando.value = true;
  error.value = '';
  try {
    const [i, p, s] = await Promise.all([
      api.get('/inventarios'),
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

function abrirNuevo() {
  editandoId.value = null;
  form.value = nuevo();
  modalAbierto.value = true;
}
function abrirEditar(i) {
  editandoId.value = i.id;
  form.value = { ...i };
  modalAbierto.value = true;
}

async function guardar() {
  error.value = '';
  const stockActual = Number(form.value.stockActual);
  const stockMinimo = Number(form.value.stockMinimo);
  if (!form.value.productoId || !form.value.sucursalId) {
    error.value = 'Elegí el producto y la sucursal.';
    return;
  }
  if (isNaN(stockActual) || stockActual < 0 || isNaN(stockMinimo) || stockMinimo < 0) {
    error.value = 'El stock actual y el mínimo tienen que ser números mayores o iguales a 0.';
    return;
  }
  guardando.value = true;
  try {
    const cuerpo = {
      ...form.value,
      stockActual,
      stockMinimo,
    };
    if (editandoId.value) {
      await api.put('/inventarios/' + editandoId.value, cuerpo);
      ok.value = 'Inventario actualizado.';
    } else {
      await api.post('/inventarios', cuerpo);
      ok.value = 'Ítem agregado al inventario.';
    }
    modalAbierto.value = false;
    await cargar();
  } catch (e) {
    error.value = e?.response?.data?.mensaje || 'No se pudo guardar.';
  } finally {
    guardando.value = false;
  }
}

async function eliminar(i) {
  if (!confirm('¿Eliminar este ítem del inventario?')) return;
  try {
    await api.delete('/inventarios/' + i.id);
    ok.value = 'Ítem eliminado.';
    await cargar();
  } catch (e) {
    error.value = e?.response?.data?.mensaje || 'No se pudo eliminar.';
  }
}

function abrirMovimiento(tipo) {
  mov.value = { inventarioId: inventarios.value[0]?.id ?? null, tipo, cantidad: 1 };
  modalMov.value = true;
}

// La entrada suma stock y la salida lo resta (se actualiza el StockActual).
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
  const nuevoStock =
    mov.value.tipo === 'ENTRADA' ? inv.stockActual + cant : inv.stockActual - cant;
  if (nuevoStock < 0) {
    error.value = 'No hay suficiente stock para esa salida.';
    return;
  }
  try {
    await api.put('/inventarios/' + inv.id, { ...inv, stockActual: nuevoStock });
    ok.value = 'Movimiento registrado (' + mov.value.tipo.toLowerCase() + ').';
    modalMov.value = false;
    await cargar();
  } catch (e) {
    error.value = e?.response?.data?.mensaje || 'No se pudo registrar el movimiento.';
  }
}

onMounted(cargar);
</script>

<template>
  <div>
    <h2>Inventario</h2>
    <p class="gris">Control de stock e insumos</p>

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
        <button class="btn btn-linea" @click="abrirNuevo">Agregar ítem</button>
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
              <button class="btn btn-linea btn-mini" @click="abrirEditar(i)">Editar</button>
              <button class="btn btn-peligro btn-mini" @click="eliminar(i)">Eliminar</button>
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
        <h3>{{ editandoId ? 'Editar' : 'Nuevo' }} ítem de inventario</h3>
        <form @submit.prevent="guardar">
          <div class="campo-form">
            <label>Producto</label>
            <select v-model="form.productoId" required>
              <option :value="null" disabled>Seleccioná...</option>
              <option v-for="p in productos" :key="p.id" :value="p.id">{{ p.nombre }}</option>
            </select>
          </div>
          <div class="campo-form">
            <label>Sucursal</label>
            <select v-model="form.sucursalId" required>
              <option :value="null" disabled>Seleccioná...</option>
              <option v-for="s in sucursales" :key="s.id" :value="s.id">{{ s.nombre }}</option>
            </select>
          </div>
          <div class="fila">
            <div class="campo-form" style="flex:1"><label>Stock actual</label><input v-model="form.stockActual" type="number" min="0" /></div>
            <div class="campo-form" style="flex:1"><label>Stock mínimo</label><input v-model="form.stockMinimo" type="number" min="0" /></div>
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
              <option v-for="i in inventarios" :key="i.id" :value="i.id">
                {{ nombreProducto(i.productoId) }} (actual: {{ i.stockActual }})
              </option>
            </select>
          </div>
          <div class="campo-form">
            <label>Cantidad</label>
            <input v-model="mov.cantidad" type="number" min="1" required />
          </div>
          <div class="fila entre acciones-modal">
            <button type="button" class="btn btn-linea" @click="modalMov = false">Cancelar</button>
            <button class="btn btn-principal">Registrar</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
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
