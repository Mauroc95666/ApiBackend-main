<script setup>
import { ref, computed, onMounted } from 'vue';
import api from '@/api/axios';

const productos = ref([]);
const inventarios = ref([]);
const ventas = ref([]);
const pedidos = ref([]);
const cargando = ref(false);

async function cargar() {
  cargando.value = true;
  try {
    const [p, i, v, pe] = await Promise.all([
      api.get('/productos').catch(() => ({ data: [] })),
      api.get('/inventarios').catch(() => ({ data: [] })),
      api.get('/ventas').catch(() => ({ data: [] })),
      api.get('/pedidos').catch(() => ({ data: [] })),
    ]);
    productos.value = p.data || [];
    inventarios.value = i.data || [];
    ventas.value = v.data || [];
    pedidos.value = pe.data || [];
  } finally {
    cargando.value = false;
  }
}

const hoy = new Date().toISOString().slice(0, 10);

const ventasHoy = computed(() =>
  ventas.value
    .filter((v) => (v.fecha || '').slice(0, 10) === hoy)
    .reduce((s, v) => s + Number(v.total || 0), 0)
);

const pedidosPendientes = computed(
  () =>
    pedidos.value.filter(
      (p) => (p.estado || '').toUpperCase() !== 'ENTREGADO'
    ).length
);

// Inventario bajo el minimo.
const stockCritico = computed(() =>
  inventarios.value.filter(
    (i) => i.stockMinimo != null && i.stockActual <= i.stockMinimo
  )
);

// Barras: ventas agrupadas por dia de la semana.
const dias = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'];
const barras = computed(() => {
  const acum = [0, 0, 0, 0, 0, 0, 0];
  ventas.value.forEach((v) => {
    if (!v.fecha) return;
    const d = new Date(v.fecha).getDay();
    acum[d] += Number(v.total || 0);
  });
  // reordenamos Lun..Dom
  const orden = [1, 2, 3, 4, 5, 6, 0];
  const valores = orden.map((idx) => ({ dia: dias[idx], valor: acum[idx] }));
  const max = Math.max(1, ...valores.map((x) => x.valor));
  return valores.map((x) => ({ ...x, alto: Math.round((x.valor / max) * 100) }));
});

const hayVentas = computed(() => ventas.value.length > 0);

function estadoStock(i) {
  if (i.stockActual <= 0) return { txt: 'SIN STOCK', clase: 'et-critico' };
  if (i.stockMinimo != null && i.stockActual < i.stockMinimo)
    return { txt: 'CRÍTICO', clase: 'et-critico' };
  if (i.stockMinimo != null && i.stockActual === i.stockMinimo)
    return { txt: 'AL LÍMITE', clase: 'et-bajo' };
  return { txt: 'BAJO', clase: 'et-bajo' };
}

function nombreProducto(id) {
  const p = productos.value.find((x) => x.id === id);
  return p ? p.nombre : 'Producto #' + id;
}

onMounted(cargar);
</script>

<template>
  <div>
    <h2>Panel de Control</h2>
    <p class="gris">Resumen general de la panadería</p>

    <p v-if="cargando" class="cargando">Cargando datos...</p>

    <!-- Tarjetas de resumen -->
    <div class="tarjetas-kpi">
      <div class="kpi">
        <div class="kpi-titulo">Ventas de hoy</div>
        <div class="kpi-numero">Bs {{ ventasHoy.toFixed(0) }}</div>
        <div class="gris chico">registradas hoy</div>
      </div>
      <div class="kpi">
        <div class="kpi-titulo">Productos</div>
        <div class="kpi-numero">{{ productos.length }}</div>
        <div class="gris chico">en catálogo</div>
      </div>
      <div class="kpi">
        <div class="kpi-titulo">Pedidos pendientes</div>
        <div class="kpi-numero">{{ pedidosPendientes }}</div>
        <div class="gris chico">por entregar</div>
      </div>
      <div class="kpi">
        <div class="kpi-titulo">Stock crítico</div>
        <div class="kpi-numero">{{ stockCritico.length }}</div>
        <div class="gris chico">bajo el mínimo</div>
      </div>
    </div>

    <!-- Grafico + info -->
    <div class="fila panel-medio">
      <div class="tarjeta grafico">
        <h3>Ventas de la semana</h3>
        <p class="gris chico">En bolivianos (Bs)</p>
        <div v-if="hayVentas" class="barras">
          <div v-for="b in barras" :key="b.dia" class="col-barra">
            <div class="barra" :style="{ height: b.alto + '%' }" :title="'Bs ' + b.valor"></div>
            <span class="gris chico">{{ b.dia }}</span>
          </div>
        </div>
        <p v-else class="gris" style="padding:30px 0">
          Todavía no hay ventas registradas para graficar.
        </p>
      </div>

      <div class="tarjeta lista-productos">
        <h3>Productos en catálogo</h3>
        <div v-for="p in productos.slice(0, 6)" :key="p.id" class="item-lista">
          <span>{{ p.nombre }}</span>
          <strong>Bs {{ Number(p.precio).toFixed(2) }}</strong>
        </div>
        <p v-if="productos.length === 0" class="gris">Sin productos.</p>
      </div>
    </div>

    <!-- Alertas de stock -->
    <div class="tarjeta">
      <h3>Alertas de stock bajo</h3>
      <table class="tabla">
        <thead>
          <tr>
            <th>Producto</th>
            <th>Stock actual</th>
            <th>Stock mínimo</th>
            <th>Estado</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="i in stockCritico" :key="i.id">
            <td>{{ nombreProducto(i.productoId) }}</td>
            <td>{{ i.stockActual }}</td>
            <td>{{ i.stockMinimo }}</td>
            <td><span class="etiqueta" :class="estadoStock(i).clase">{{ estadoStock(i).txt }}</span></td>
          </tr>
          <tr v-if="stockCritico.length === 0">
            <td colspan="4" class="gris" style="text-align:center;padding:20px">
              No hay productos bajo el mínimo. 👍
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.tarjetas-kpi {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
  margin: 18px 0;
}

.kpi {
  background: #fff;
  border: 1px solid var(--linea);
  border-radius: 10px;
  padding: 18px;
  box-shadow: var(--sombra);
}

.kpi-titulo {
  color: var(--texto-suave);
  font-size: 13px;
  margin-bottom: 8px;
}

.kpi-numero {
  font-size: 30px;
  font-weight: 800;
  color: var(--cafe-oscuro);
}

.chico {
  font-size: 12px;
}

.panel-medio {
  margin: 18px 0;
  align-items: stretch;
}

.grafico {
  flex: 2;
}

.lista-productos {
  flex: 1;
}

.barras {
  display: flex;
  align-items: flex-end;
  gap: 14px;
  height: 200px;
  margin-top: 20px;
}

.col-barra {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  height: 100%;
  justify-content: flex-end;
  gap: 6px;
}

.barra {
  width: 60%;
  background: var(--caramelo);
  border-radius: 5px 5px 0 0;
  min-height: 4px;
}

.item-lista {
  display: flex;
  justify-content: space-between;
  padding: 10px 0;
  border-bottom: 1px solid var(--linea);
}

@media (max-width: 820px) {
  .panel-medio {
    flex-direction: column;
  }
}
</style>
