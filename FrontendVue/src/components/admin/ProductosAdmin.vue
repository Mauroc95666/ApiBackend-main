<script setup>
import { ref, computed, onMounted } from 'vue';
import api from '@/api/axios';

const productos = ref([]);
const categorias = ref([]);
const cargando = ref(false);
const error = ref('');
const ok = ref('');
const categoriaSel = ref(0);

const modalAbierto = ref(false);
const editandoId = ref(null);
const guardando = ref(false);
const form = ref(nuevo());

function nuevo() {
  return {
    nombre: '',
    precio: 0,
    stock: 0,
    categoriaId: null,
    proveedorId: null,
    emprendimientoId: 1,
  };
}

async function cargar() {
  cargando.value = true;
  error.value = '';
  try {
    const [p, c] = await Promise.all([
      api.get('/productos'),
      api.get('/categorias').catch(() => ({ data: [] })),
    ]);
    productos.value = p.data || [];
    categorias.value = c.data || [];
  } catch {
    error.value = 'No se pudieron cargar los productos.';
  } finally {
    cargando.value = false;
  }
}

function nombreCategoria(id) {
  const c = categorias.value.find((x) => x.id === id);
  return c ? c.nombre : 'Sin categoría';
}

const productosFiltrados = computed(() =>
  categoriaSel.value === 0
    ? productos.value
    : productos.value.filter((p) => p.categoriaId === categoriaSel.value)
);

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
    // aseguramos que los numeros vayan como numeros
    const cuerpo = {
      ...form.value,
      precio: Number(form.value.precio),
      stock: Number(form.value.stock),
    };
    if (editandoId.value) {
      await api.put('/productos/' + editandoId.value, cuerpo);
      ok.value = 'Producto actualizado.';
    } else {
      await api.post('/productos', cuerpo);
      ok.value = 'Producto creado.';
    }
    modalAbierto.value = false;
    await cargar();
  } catch (e) {
    error.value = e?.response?.data?.mensaje || 'No se pudo guardar el producto.';
  } finally {
    guardando.value = false;
  }
}

async function eliminar(p) {
  if (!confirm('¿Eliminar "' + p.nombre + '"?')) return;
  try {
    await api.delete('/productos/' + p.id);
    ok.value = 'Producto eliminado.';
    await cargar();
  } catch (e) {
    error.value = e?.response?.data?.mensaje || 'No se pudo eliminar.';
  }
}

function claseStock(s) {
  if ((s ?? 0) <= 0) return 'et-critico';
  if ((s ?? 0) < 15) return 'et-bajo';
  return 'et-ok';
}

onMounted(cargar);
</script>

<template>
  <div>
    <div class="fila entre encabezado">
      <div>
        <h2>Productos</h2>
        <p class="gris">Catálogo de la panadería · {{ productos.length }} productos</p>
      </div>
      <button class="btn btn-principal" @click="abrirNuevo">+ Nuevo producto</button>
    </div>

    <p v-if="ok" class="mensaje-ok">{{ ok }}</p>
    <p v-if="error" class="mensaje-error">{{ error }}</p>

    <!-- Filtros por categoria -->
    <div class="chips">
      <button class="chip" :class="{ activo: categoriaSel === 0 }" @click="categoriaSel = 0">
        Todos
      </button>
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

    <p v-if="cargando" class="cargando">Cargando...</p>

    <!-- Tarjetas de producto -->
    <div class="grilla">
      <div v-for="p in productosFiltrados" :key="p.id" class="prod">
        <div class="prod-foto">Foto del producto</div>
        <div class="prod-cuerpo">
          <div class="fila entre">
            <h3>{{ p.nombre }}</h3>
            <span class="etiqueta" :class="claseStock(p.stock)">{{ p.stock ?? 0 }} u.</span>
          </div>
          <div class="gris chico">{{ nombreCategoria(p.categoriaId) }}</div>
          <div class="prod-precio">Bs {{ Number(p.precio).toFixed(2) }}</div>
          <div class="fila acciones-prod">
            <button class="btn btn-linea btn-mini" @click="abrirEditar(p)">Editar</button>
            <button class="btn btn-peligro btn-mini" @click="eliminar(p)">Eliminar</button>
          </div>
        </div>
      </div>
      <div v-if="!cargando && productosFiltrados.length === 0" class="gris">
        No hay productos en esta categoría.
      </div>
    </div>

    <!-- Modal -->
    <div v-if="modalAbierto" class="fondo-modal" @click.self="modalAbierto = false">
      <div class="caja-modal">
        <h3>{{ editandoId ? 'Editar' : 'Nuevo' }} producto</h3>
        <form @submit.prevent="guardar">
          <div class="campo-form">
            <label>Nombre</label>
            <input v-model="form.nombre" required placeholder="Ej: Cuñapé" />
          </div>
          <div class="campo-form">
            <label>Categoría</label>
            <select v-model="form.categoriaId">
              <option :value="null">Sin categoría</option>
              <option v-for="c in categorias" :key="c.id" :value="c.id">{{ c.nombre }}</option>
            </select>
          </div>
          <div class="fila">
            <div class="campo-form" style="flex:1">
              <label>Precio (Bs)</label>
              <input v-model="form.precio" type="number" step="0.01" min="0" required />
            </div>
            <div class="campo-form" style="flex:1">
              <label>Stock</label>
              <input v-model="form.stock" type="number" min="0" />
            </div>
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
.chico { font-size: 12px; }

.chips {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  margin-bottom: 20px;
}

.chip {
  border: 1px solid var(--linea);
  background: #fff;
  border-radius: 20px;
  padding: 7px 16px;
  font-size: 13px;
  cursor: pointer;
  color: var(--texto);
}

.chip.activo {
  background: var(--caramelo);
  color: #fff;
  border-color: var(--caramelo);
}

.grilla {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 18px;
}

.prod {
  background: #fff;
  border: 1px solid var(--linea);
  border-radius: 10px;
  overflow: hidden;
  box-shadow: var(--sombra);
}

.prod-foto {
  height: 130px;
  background: var(--crema-2);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #c2a888;
  font-size: 13px;
}

.prod-cuerpo { padding: 14px; }
.prod-cuerpo h3 { font-size: 16px; }

.prod-precio {
  color: var(--caramelo);
  font-size: 20px;
  font-weight: 700;
  margin: 8px 0;
}

.acciones-prod { gap: 8px; }

.campo-form { margin-bottom: 14px; }
.acciones-modal { margin-top: 20px; }
</style>
