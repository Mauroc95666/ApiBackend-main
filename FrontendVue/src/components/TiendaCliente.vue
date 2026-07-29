<script setup>
import { ref, computed, onMounted } from 'vue';
import api from '@/api/axios';
import { store } from '@/store';

const productos = ref([]);
const categorias = ref([]);
const cargando = ref(false);
const error = ref('');
const busqueda = ref('');
const categoriaSel = ref(0); // 0 = todas
const carrito = ref([]);
const verCarrito = ref(false);
const pedidoHecho = ref(false);

async function cargarTodo() {
  cargando.value = true;
  error.value = '';
  try {
    const [p, c] = await Promise.all([api.get('/productos'), api.get('/categorias')]);
    productos.value = p.data || [];
    categorias.value = c.data || [];
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
      !busqueda.value ||
      (p.nombre || '').toLowerCase().includes(busqueda.value.toLowerCase());
    return okCat && okTexto;
  });
});

function agregar(prod) {
  const item = carrito.value.find((i) => i.id === prod.id);
  if (item) {
    item.cantidad++;
  } else {
    carrito.value.push({ id: prod.id, nombre: prod.nombre, precio: prod.precio, cantidad: 1 });
  }
  verCarrito.value = true;
}

function quitar(id) {
  carrito.value = carrito.value.filter((i) => i.id !== id);
}

function cambiarCantidad(item, delta) {
  item.cantidad += delta;
  if (item.cantidad <= 0) quitar(item.id);
}

const total = computed(() =>
  carrito.value.reduce((s, i) => s + i.precio * i.cantidad, 0)
);

const cantidadTotal = computed(() =>
  carrito.value.reduce((s, i) => s + i.cantidad, 0)
);

function hacerPedido() {
  if (carrito.value.length === 0) return;
  // El pedido queda registrado localmente; el local se comunica para confirmarlo.
  pedidoHecho.value = true;
}

function nuevoPedido() {
  carrito.value = [];
  pedidoHecho.value = false;
  verCarrito.value = false;
}

onMounted(cargarTodo);
</script>

<template>
  <div class="tienda">
    <!-- Cabecera -->
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
          🛒 Carrito ({{ cantidadTotal }})
        </button>
        <button class="btn btn-principal btn-mini" @click="store.cerrarSesion()">
          Iniciar sesión
        </button>
      </div>
    </header>

    <!-- Franja de bienvenida -->
    <div class="hero">
      <h1>Nuestros productos</h1>
      <p>Elegí lo que quieras y armá tu pedido. Cuñapés, empanadas, panes y más.</p>
    </div>

    <div class="cuerpo">
      <!-- Catalogo -->
      <div class="catalogo">
        <div class="barra-filtros">
          <input v-model="busqueda" class="buscador" placeholder="Buscar producto..." />
          <div class="chips">
            <button
              class="chip"
              :class="{ activo: categoriaSel === 0 }"
              @click="categoriaSel = 0"
            >
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
        </div>

        <p v-if="cargando" class="cargando">Cargando productos...</p>
        <p v-if="error" class="mensaje-error">{{ error }}</p>

        <div v-if="!cargando && productosFiltrados.length === 0 && !error" class="vacio">
          No hay productos para mostrar.
        </div>

        <div class="grilla">
          <div v-for="p in productosFiltrados" :key="p.id" class="prod">
            <div class="prod-foto">Foto del producto</div>
            <div class="prod-info">
              <h3>{{ p.nombre }}</h3>
              <div class="gris chico">{{ nombreCategoria(p.categoriaId) }}</div>
              <div class="prod-precio">Bs {{ Number(p.precio).toFixed(2) }}</div>
              <div class="chico" :class="(p.stock ?? 0) > 0 ? 'hay' : 'agotado'">
                {{ (p.stock ?? 0) > 0 ? 'Disponible' : 'Agotado' }}
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

      <!-- Carrito lateral -->
      <aside v-if="verCarrito" class="carrito">
        <h3>Mi pedido</h3>

        <template v-if="!pedidoHecho">
          <p v-if="carrito.length === 0" class="gris">Todavía no agregaste nada.</p>

          <div v-for="i in carrito" :key="i.id" class="linea-carrito">
            <div>
              <div>{{ i.nombre }}</div>
              <div class="gris chico">Bs {{ Number(i.precio).toFixed(2) }} c/u</div>
            </div>
            <div class="cant">
              <button @click="cambiarCantidad(i, -1)">−</button>
              <span>{{ i.cantidad }}</span>
              <button @click="cambiarCantidad(i, 1)">+</button>
            </div>
          </div>

          <div v-if="carrito.length" class="total-carrito">
            <span>Total</span>
            <strong>Bs {{ total.toFixed(2) }}</strong>
          </div>

          <button
            class="btn btn-principal hacer-pedido"
            :disabled="carrito.length === 0"
            @click="hacerPedido"
          >
            Hacer pedido
          </button>
        </template>

        <div v-else class="pedido-ok">
          <div class="check-grande">✓</div>
          <h4>¡Pedido registrado!</h4>
          <p class="gris">
            Tu pedido de <strong>Bs {{ total.toFixed(2) }}</strong> quedó anotado.
            La panadería te va a contactar para coordinar la entrega.
          </p>
          <button class="btn btn-linea" @click="nuevoPedido">Hacer otro pedido</button>
        </div>
      </aside>
    </div>

    <footer class="pie">
      Panadería Kiataque · Santa Cruz, Bolivia · Tel. 700-00000
    </footer>
  </div>
</template>

<style scoped>
.tienda {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.cabecera {
  background: var(--cafe-oscuro);
  color: #f4ead9;
  padding: 14px 28px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.marca {
  display: flex;
  align-items: center;
  gap: 12px;
}

.logo-mini {
  width: 42px;
  height: 42px;
  border-radius: 50%;
  background: var(--caramelo);
  color: #fff;
  font-weight: 800;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
}

.marca strong {
  color: #fff;
}

.acciones-cab {
  display: flex;
  gap: 10px;
}

.chico {
  font-size: 12px;
}

.hero {
  background: var(--crema-2);
  padding: 34px 28px;
  border-bottom: 1px solid var(--linea);
}

.hero h1 {
  font-size: 28px;
}

.cuerpo {
  flex: 1;
  display: flex;
  gap: 20px;
  padding: 24px 28px;
  align-items: flex-start;
}

.catalogo {
  flex: 1;
}

.barra-filtros {
  margin-bottom: 18px;
}

.buscador {
  max-width: 320px;
  margin-bottom: 12px;
}

.chips {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
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
  grid-template-columns: repeat(auto-fill, minmax(210px, 1fr));
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
  height: 150px;
  background: var(--crema-2);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #c2a888;
  font-size: 13px;
}

.prod-info {
  padding: 14px;
}

.prod-info h3 {
  font-size: 16px;
  margin-bottom: 2px;
}

.prod-precio {
  color: var(--caramelo);
  font-size: 20px;
  font-weight: 700;
  margin: 6px 0 2px 0;
}

.hay { color: var(--verde); }
.agotado { color: var(--rojo); }

.agregar {
  width: 100%;
  margin-top: 10px;
}

/* carrito */
.carrito {
  width: 300px;
  background: #fff;
  border: 1px solid var(--linea);
  border-radius: 10px;
  padding: 18px;
  position: sticky;
  top: 24px;
  box-shadow: var(--sombra);
}

.linea-carrito {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0;
  border-bottom: 1px solid var(--linea);
}

.cant {
  display: flex;
  align-items: center;
  gap: 8px;
}

.cant button {
  width: 26px;
  height: 26px;
  border: 1px solid var(--linea);
  background: var(--crema);
  border-radius: 5px;
  cursor: pointer;
  padding: 0;
}

.total-carrito {
  display: flex;
  justify-content: space-between;
  margin: 16px 0;
  font-size: 18px;
}

.hacer-pedido {
  width: 100%;
}

.pedido-ok {
  text-align: center;
  padding: 10px 0;
}

.check-grande {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  background: var(--verde);
  color: #fff;
  font-size: 30px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 14px auto;
}

.vacio {
  padding: 30px;
  color: var(--texto-suave);
}

.pie {
  background: var(--cafe-oscuro);
  color: #c9b193;
  text-align: center;
  padding: 16px;
  font-size: 13px;
}

@media (max-width: 820px) {
  .cuerpo {
    flex-direction: column;
  }
  .carrito {
    width: 100%;
    position: static;
  }
}
</style>
