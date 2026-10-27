<script setup>
import { ref, computed, onMounted } from 'vue';
import api from '@/api/axios';

// El panel ya no se baja las tablas enteras para sacar cuentas en el
// navegador: pide un resumen que el servidor calcula en una sola consulta.
const resumen = ref(null);
const sucursales = ref([]);
// 0 significa todas las sucursales juntas, que es como venia funcionando.
const sucursalId = ref(0);
const cargando = ref(false);
const error = ref('');

async function cargarSucursales() {
  try {
    const r = await api.get('/sucursales');
    sucursales.value = r.data || [];
  } catch {
    sucursales.value = [];
  }
}

async function cargar() {
  cargando.value = true;
  error.value = '';
  try {
    const r = await api.get('/dashboards/resumen', {
      params: { sucursalId: sucursalId.value || undefined },
    });
    resumen.value = r.data;
  } catch {
    error.value = 'No se pudo cargar el resumen del panel.';
  } finally {
    cargando.value = false;
  }
}

// Al cambiar de sucursal se vuelve a pedir el resumen: el filtrado lo hace
// el servidor, no el navegador.
function cambiarSucursal() {
  cargar();
}

const ventasHoy = computed(() => Number(resumen.value?.ventasDeHoy ?? 0));
const cantidadProductos = computed(() => resumen.value?.cantidadProductos ?? 0);
const pedidosPendientes = computed(() => resumen.value?.pedidosPendientes ?? 0);
const alertas = computed(() => resumen.value?.alertas ?? []);

// Tiempo promedio de servicio de los últimos 30 días. Viene en minutos y se
// muestra en minutos u horas según cuánto sea.
const tiempoPromedio = computed(() => {
  const m = resumen.value?.tiempoPromedioMinutos;
  if (m == null) return null;
  if (m < 90) return Math.round(m) + ' min';
  return (m / 60).toFixed(1) + ' h';
});

const pedidosMedidos = computed(() => resumen.value?.pedidosMedidos ?? 0);
const productos = computed(() => resumen.value?.productos ?? []);

// Barras de la semana en curso. El servidor manda los siete dias de lunes a
// domingo con su total; aca solo se calcula la altura relativa.
const barras = computed(() => {
  const dias = resumen.value?.semana?.barras ?? [];
  const max = Math.max(1, ...dias.map((d) => Number(d.total || 0)));
  return dias.map((d) => ({
    dia: d.dia,
    valor: Number(d.total || 0),
    alto: Math.round((Number(d.total || 0) / max) * 100),
  }));
});

const hayVentas = computed(() => barras.value.some((b) => b.valor > 0));

// Rango de la semana que se esta mostrando, para que el titulo no mienta.
// Se leen los numeros del texto que manda el servidor en vez de pasarlos por
// new Date(): esa conversion aplica la zona horaria y corre la fecha un dia.
function diaYMes(fechaIso) {
  if (!fechaIso) return '';
  const [, mes, dia] = String(fechaIso).slice(0, 10).split('-');
  return dia + '/' + mes;
}

const rangoSemana = computed(() => {
  const s = resumen.value?.semana;
  if (!s) return '';
  return diaYMes(s.desde) + ' al ' + diaYMes(s.hasta);
});

function estadoStock(i) {
  if (i.stockActual <= 0) return { txt: 'SIN STOCK', clase: 'et-critico' };
  if (i.stockActual < i.stockMinimo) return { txt: 'CRÍTICO', clase: 'et-critico' };
  if (i.stockActual === i.stockMinimo) return { txt: 'AL LÍMITE', clase: 'et-bajo' };
  return { txt: 'BAJO', clase: 'et-bajo' };
}

onMounted(() => {
  cargarSucursales();
  cargar();
});
</script>

<template>
  <div>
    <div class="fila entre encabezado-panel">
      <div>
        <h2>Panel de Control</h2>
        <p class="gris">Resumen general de la panadería</p>
      </div>
      <div class="campo-form selector-sucursal">
        <label>Sucursal</label>
        <select v-model="sucursalId" @change="cambiarSucursal">
          <option :value="0">Todas las sucursales</option>
          <option v-for="s in sucursales" :key="s.id" :value="s.id">{{ s.nombre }}</option>
        </select>
      </div>
    </div>

    <p v-if="cargando" class="cargando">Cargando datos...</p>
    <p v-if="error" class="mensaje-error">{{ error }}</p>

    <!-- Tarjetas de resumen -->
    <div class="tarjetas-kpi">
      <div class="kpi">
        <div class="kpi-titulo">Ventas de hoy</div>
        <div class="kpi-numero">Bs {{ ventasHoy.toFixed(0) }}</div>
        <div class="gris chico">registradas hoy</div>
      </div>
      <div class="kpi">
        <div class="kpi-titulo">Productos</div>
        <div class="kpi-numero">{{ cantidadProductos }}</div>
        <div class="gris chico">en catálogo</div>
      </div>
      <div class="kpi">
        <div class="kpi-titulo">Pedidos pendientes</div>
        <div class="kpi-numero">{{ pedidosPendientes }}</div>
        <div class="gris chico">por entregar</div>
      </div>
      <div class="kpi">
        <div class="kpi-titulo">Stock crítico</div>
        <div class="kpi-numero">{{ alertas.length }}</div>
        <div class="gris chico">bajo el mínimo</div>
      </div>
      <div class="kpi">
        <div class="kpi-titulo">Tiempo de servicio</div>
        <div class="kpi-numero">{{ tiempoPromedio ?? '—' }}</div>
        <div class="gris chico">
          {{ pedidosMedidos > 0
              ? 'promedio de ' + pedidosMedidos + ' pedido(s) entregados'
              : 'sin pedidos entregados aún' }}
        </div>
      </div>
    </div>

    <!-- Grafico + info -->
    <div class="fila panel-medio">
      <div class="tarjeta grafico">
        <h3>Ventas de la semana</h3>
        <p class="gris chico">
          En bolivianos (Bs)<span v-if="rangoSemana"> · {{ rangoSemana }}</span>
        </p>
        <div v-if="hayVentas" class="barras">
          <div v-for="b in barras" :key="b.dia" class="col-barra">
            <div class="barra" :style="{ height: b.alto + '%' }" :title="'Bs ' + b.valor"></div>
            <span class="gris chico">{{ b.dia }}</span>
          </div>
        </div>
        <p v-else class="gris" style="padding:30px 0">
          Todavía no hay ventas en esta semana.
        </p>
      </div>

      <div class="tarjeta lista-productos">
        <h3>Productos en catálogo</h3>
        <div v-for="p in productos" :key="p.id" class="item-lista">
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
          <tr v-for="i in alertas" :key="i.id">
            <td>{{ i.producto }}</td>
            <td>{{ i.stockActual }}</td>
            <td>{{ i.stockMinimo }}</td>
            <td><span class="etiqueta" :class="estadoStock(i).clase">{{ estadoStock(i).txt }}</span></td>
          </tr>
          <tr v-if="alertas.length === 0">
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
.encabezado-panel { align-items: flex-end; gap: 16px; flex-wrap: wrap; }
.selector-sucursal { margin-bottom: 0; min-width: 220px; }

@media (max-width: 620px) {
  .selector-sucursal { width: 100%; }
}

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
