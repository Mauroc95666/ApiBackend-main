<script setup>
import { ref, computed, onMounted } from 'vue';
import api from '@/api/axios';

const productos = ref([]);
const categorias = ref([]);
const proveedores = ref([]);
// Filas de inventario: de ahi sale el stock minimo real de cada producto.
const inventarios = ref([]);
// Confirmacion de borrado con el modal del sistema, no el del navegador.
const productoABorrar = ref(null);
// Imagenes cuya URL fallo: se les muestra el respaldo en vez de un roto.
const imagenesRotas = ref([]);
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
    categoriaId: null,
    proveedorId: null,
    emprendimientoId: 1,
    imagenUrl: '',
  };
}

async function cargar() {
  cargando.value = true;
  error.value = '';
  try {
    const [p, c, pr, inv] = await Promise.all([
      api.get('/productos'),
      api.get('/categorias').catch(() => ({ data: [] })),
      api.get('/proveedores').catch(() => ({ data: [] })),
      api.get('/inventarios').catch(() => ({ data: [] })),
    ]);
    productos.value = p.data || [];
    categorias.value = c.data || [];
    proveedores.value = pr.data || [];
    inventarios.value = inv.data || [];
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
  form.value = {
    nombre: p.nombre,
    precio: p.precio,
    categoriaId: p.categoriaId,
    proveedorId: p.proveedorId,
    emprendimientoId: p.emprendimientoId,
    imagenUrl: p.imagenUrl,
  };
  modalAbierto.value = true;
}

async function guardar() {
  error.value = '';
  // Validamos el precio antes de guardar para evitar valores negativos.
  const precio = Number(form.value.precio);
  if (!form.value.nombre || !form.value.nombre.trim()) {
    error.value = 'Poné un nombre al producto.';
    return;
  }
  if (isNaN(precio) || precio < 0) {
    error.value = 'El precio tiene que ser un número mayor o igual a 0.';
    return;
  }
  guardando.value = true;
  try {
    // aseguramos que los numeros vayan como numeros
    const cuerpo = {
      ...form.value,
      nombre: form.value.nombre.trim(),
      precio,
      imagenUrl: (form.value.imagenUrl || '').trim() || null,
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

function pedirBorrado(p) {
  productoABorrar.value = p;
}

async function eliminar() {
  const p = productoABorrar.value;
  if (!p) return;
  productoABorrar.value = null;
  try {
    await api.delete('/productos/' + p.id);
    ok.value = 'Producto eliminado.';
    await cargar();
  } catch (e) {
    error.value = e?.response?.data?.mensaje || 'No se pudo eliminar.';
  }
}

// Minimo configurado para el producto. Se suman los minimos de sus sucursales;
// si el producto todavia no esta en ningun inventario se usa 15, que es el
// valor que tenia fijo la pantalla.
function minimoDe(productoId) {
  const filas = inventarios.value.filter((i) => i.productoId === productoId);
  if (filas.length === 0) return 15;
  return filas.reduce((s, i) => s + (i.stockMinimo ?? 0), 0);
}

function claseStock(stock, productoId) {
  const s = stock ?? 0;
  if (s <= 0) return 'et-critico';
  if (s <= minimoDe(productoId)) return 'et-bajo';
  return 'et-ok';
}

function imagenFallo(id) {
  if (!imagenesRotas.value.includes(id)) imagenesRotas.value.push(id);
}

function nombreProveedor(id) {
  const pr = proveedores.value.find((x) => x.id === id);
  return pr ? pr.nombre : null;
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
        <div class="prod-foto">
          <img
            v-if="p.imagenUrl && !imagenesRotas.includes(p.id)"
            :src="p.imagenUrl"
            :alt="p.nombre"
            class="img-prod"
            @error="imagenFallo(p.id)"
          />
          <span v-else-if="p.imagenUrl">No se pudo cargar la foto</span>
          <span v-else>Foto del producto</span>
        </div>
        <div class="prod-cuerpo">
          <div class="fila entre">
            <h3>{{ p.nombre }}</h3>
            <span class="etiqueta" :class="claseStock(p.stock, p.id)">{{ p.stock ?? 0 }} u.</span>
          </div>
          <div class="gris chico">{{ nombreCategoria(p.categoriaId) }}</div>
          <div v-if="nombreProveedor(p.proveedorId)" class="gris chico">
            Proveedor: {{ nombreProveedor(p.proveedorId) }}
          </div>
          <div class="prod-precio">Bs {{ Number(p.precio).toFixed(2) }}</div>
          <div class="fila acciones-prod">
            <button class="btn btn-linea btn-mini" @click="abrirEditar(p)">Editar</button>
            <button class="btn btn-peligro btn-mini" @click="pedirBorrado(p)">Eliminar</button>
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
            <label>Imagen (URL)</label>
            <input v-model="form.imagenUrl" placeholder="https://... (pegá el enlace de la foto)" />
            <div v-if="form.imagenUrl" class="previa">
              <img :src="form.imagenUrl" alt="Vista previa" />
            </div>
          </div>
          <div class="campo-form">
            <label>Categoría</label>
            <select v-model="form.categoriaId">
              <option :value="null">Sin categoría</option>
              <option v-for="c in categorias" :key="c.id" :value="c.id">{{ c.nombre }}</option>
            </select>
          </div>
          <div class="campo-form">
            <label>Proveedor</label>
            <select v-model="form.proveedorId">
              <option :value="null">Sin proveedor</option>
              <option v-for="pr in proveedores" :key="pr.id" :value="pr.id">{{ pr.nombre }}</option>
            </select>
            <small v-if="proveedores.length === 0" class="gris">
              No hay proveedores cargados todavía.
            </small>
          </div>
          <div class="campo-form">
            <label>Precio (Bs)</label>
            <input v-model="form.precio" type="number" step="0.01" min="0" required />
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

    <!-- Modal: confirmar el borrado, en vez del confirm() del navegador -->
    <div v-if="productoABorrar" class="fondo-modal" @click.self="productoABorrar = null">
      <div class="caja-modal">
        <h3>Eliminar producto</h3>
        <p class="gris">
          Se va a eliminar "{{ productoABorrar.nombre }}". Esta acción no se puede deshacer.
        </p>
        <div class="fila entre acciones-modal">
          <button class="btn btn-linea" @click="productoABorrar = null">Cancelar</button>
          <button class="btn btn-peligro" @click="eliminar">Eliminar</button>
        </div>
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
  overflow: hidden;
}
.img-prod { width: 100%; height: 100%; object-fit: cover; }
.previa { margin-top: 8px; }
.previa img { max-height: 120px; border-radius: 6px; border: 1px solid var(--linea); }

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
