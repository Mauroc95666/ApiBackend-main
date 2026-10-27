<script setup>
// Pantalla de reportes (RF-12). El calculo lo hace el servidor: aca solo se
// elige el rango, se muestra el resultado y se exporta.
import { ref, computed, onMounted, watch } from 'vue';
import api from '@/api/axios';
import { store } from '@/store';

const sucursales = ref([]);
const reporte = ref(null);
// Tiempos de servicio de los pedidos entregados en el mismo rango (HU-12).
const tiempos = ref(null);
const cargando = ref(false);
const guardando = ref(false);
const reporteId = ref(null);
const error = ref('');
const ok = ref('');

// Por defecto, el mes en curso.
const hoy = new Date();
const primeroDelMes = new Date(hoy.getFullYear(), hoy.getMonth(), 1);
const aTexto = (d) => d.toISOString().slice(0, 10);

const desde = ref(aTexto(primeroDelMes));
const hasta = ref(aTexto(hoy));
const sucursalId = ref(0);

watch([desde, hasta, sucursalId], () => {
  reporte.value = null;
  tiempos.value = null;
  reporteId.value = null;
});

async function cargarSucursales() {
  try {
    const r = await api.get('/sucursales');
    sucursales.value = r.data || [];
  } catch {
    sucursales.value = [];
  }
}

async function generar() {
  error.value = '';
  ok.value = '';

  if (desde.value > hasta.value) {
    error.value = 'La fecha desde tiene que ser anterior a la fecha hasta.';
    return;
  }

  reporte.value = null;
  tiempos.value = null;
  reporteId.value = null;
  cargando.value = true;
  try {
    const r = await api.get('/reportes/ventas', {
      params: {
        desde: desde.value,
        hasta: hasta.value,
        sucursalId: sucursalId.value || undefined,
      },
    });
    reporte.value = r.data;
    reporteId.value = null;

    // Los tiempos se piden aparte: que fallen no debe tumbar el reporte de
    // ventas, que es lo principal de esta pantalla.
    try {
      const rt = await api.get('/pedidos/tiempos', {
        params: { desde: desde.value, hasta: hasta.value },
      });
      tiempos.value = rt.data;
    } catch {
      tiempos.value = null;
    }
  } catch (e) {
    error.value = e?.response?.data?.mensaje || 'No se pudo generar el reporte.';
    reporte.value = null;
    tiempos.value = null;
  } finally {
    cargando.value = false;
  }
}

// Minutos a algo legible.
function enTiempo(minutos) {
  if (minutos == null) return '—';
  if (minutos < 90) return Math.round(minutos) + ' min';
  return (minutos / 60).toFixed(1) + ' h';
}

function fechaYHora(f) {
  if (!f) return '';
  const [fecha, resto] = String(f).split('T');
  const [, mes, dia] = fecha.split('-');
  return dia + '/' + mes + ' ' + (resto || '').slice(0, 5);
}

function fechaArchivo(fecha) {
  const [anio, mes, dia] = fecha.split('-');
  return `${dia}-${mes}-${anio}`;
}

function nombreArchivo(extension) {
  return `Reporte # ${reporteId.value} ${fechaArchivo(desde.value)} - ${fechaArchivo(hasta.value)}.${extension}`;
}

async function asegurarReporteGuardado() {
  if (reporteId.value) return;
  const respuesta = await api.post('/reportes/ventas/guardar', {
    emprendimientoId: store.usuario?.emprendimientoId || 1,
    tipo: 'VENTAS',
    datos: JSON.stringify({
      desde: desde.value,
      hasta: hasta.value,
      sucursal: sucursalId.value
        ? sucursales.value.find((s) => s.id === sucursalId.value)?.nombre || 'Sucursal seleccionada'
        : 'Todas las sucursales',
      reporte: reporte.value,
      tiempos: tiempos.value,
    }),
  });
  reporteId.value = respuesta.data.id;
}

function agregarSeccion(doc, titulo, y) {
  if (y > 265) {
    doc.addPage();
    y = 20;
  }
  doc.setFont('helvetica', 'bold');
  doc.setFontSize(12);
  doc.setTextColor(58, 35, 24);
  doc.text(titulo, 14, y);
  doc.setDrawColor(191, 125, 44);
  doc.line(14, y + 2, 196, y + 2);
  doc.setFont('helvetica', 'normal');
  doc.setFontSize(9);
  return y + 9;
}

function agregarLinea(doc, textos, y, opciones = {}) {
  const margenIzquierdo = opciones.margenIzquierdo ?? 14;
  const ancho = opciones.ancho ?? 182;
  const lineas = doc.splitTextToSize(textos.join('   |   '), ancho);
  if (y + lineas.length * 5 > 280) {
    doc.addPage();
    y = 20;
  }
  doc.text(lineas, margenIzquierdo, y);
  return y + lineas.length * 5 + 1;
}

async function generarPdf() {
  const { jsPDF } = await import('jspdf');
  const doc = new jsPDF({ unit: 'mm', format: 'a4' });
  const ancho = doc.internal.pageSize.getWidth();
  const sucursal = sucursalId.value
    ? sucursales.value.find((s) => s.id === sucursalId.value)?.nombre || 'Sucursal seleccionada'
    : 'Todas las sucursales';
  let y = 20;

  doc.setFillColor(58, 35, 24);
  doc.rect(0, 0, ancho, 35, 'F');
  doc.setTextColor(255, 255, 255);
  doc.setFont('helvetica', 'bold');
  doc.setFontSize(18);
  doc.text('Reporte de ventas', 14, 17);
  doc.setFont('helvetica', 'normal');
  doc.setFontSize(10);
  doc.text(`Reporte # ${reporteId.value}`, 14, 26);
  doc.text(`${fechaArchivo(desde.value)} - ${fechaArchivo(hasta.value)}`, ancho - 14, 26, { align: 'right' });

  y = 46;
  doc.setTextColor(58, 42, 30);
  y = agregarLinea(doc, [`Sucursal: ${sucursal}`], y);
  y = agregarLinea(doc, [`Generado: ${new Date().toLocaleString('es-BO')}`], y);
  y += 3;

  y = agregarSeccion(doc, 'Resumen del período', y);
  y = agregarLinea(doc, [`Ventas: ${reporte.value.cantidadVentas}`], y);
  y = agregarLinea(doc, [`Total: Bs ${Number(reporte.value.total).toFixed(2)}`], y);
  y = agregarLinea(doc, [`Promedio por venta: Bs ${Number(reporte.value.promedioPorVenta).toFixed(2)}`], y);
  y += 3;

  y = agregarSeccion(doc, 'Ventas por día', y);
  y = agregarLinea(doc, ['Fecha', 'Ventas', 'Total (Bs)'], y);
  reporte.value.porDia.forEach((dia) => {
    y = agregarLinea(doc, [
      fechaCorta(dia.fecha),
      String(dia.cantidad),
      Number(dia.total).toFixed(2),
    ], y);
  });
  if (reporte.value.porDia.length === 0) y = agregarLinea(doc, ['Sin ventas en el período.'], y);
  y += 3;

  y = agregarSeccion(doc, 'Productos más vendidos', y);
  y = agregarLinea(doc, ['#', 'Producto', 'Unidades', 'Total (Bs)'], y);
  reporte.value.porProducto.forEach((producto, indice) => {
    y = agregarLinea(doc, [
      String(indice + 1),
      producto.producto,
      String(producto.unidades),
      Number(producto.total).toFixed(2),
    ], y);
  });
  if (reporte.value.porProducto.length === 0) y = agregarLinea(doc, ['Sin detalle de productos.'], y);

  if (tiempos.value) {
    y += 3;
    y = agregarSeccion(doc, 'Tiempos de servicio', y);
    y = agregarLinea(doc, [`Meta: ${enTiempo(tiempos.value.metaMinutos)}`], y);
    y = agregarLinea(doc, [`Promedio: ${enTiempo(tiempos.value.promedioMinutos)}`], y);
    y = agregarLinea(doc, [`Pedidos entregados: ${tiempos.value.cantidadEntregados}`], y);
    y = agregarLinea(doc, [`Fuera de meta: ${tiempos.value.fueraDeMeta}`], y);
    if (tiempos.value.porEtapa?.length) {
      y += 2;
      y = agregarLinea(doc, ['Etapa', 'Promedio', 'Veces medida'], y);
      tiempos.value.porEtapa.forEach((etapa) => {
        y = agregarLinea(doc, [
          etapa.estado,
          enTiempo(etapa.promedioMinutos),
          String(etapa.vecesMedido),
        ], y);
      });
    }
    if (tiempos.value.pedidos?.length) {
      y += 2;
      y = agregarLinea(doc, ['Pedido', 'Creado', 'Entregado', 'Duración', 'Estado'], y);
      tiempos.value.pedidos.forEach((pedido) => {
        y = agregarLinea(doc, [
          `#${pedido.id}`,
          fechaYHora(pedido.creado),
          fechaYHora(pedido.entregado),
          enTiempo(pedido.minutos),
          pedido.superaLaMeta ? 'Fuera de meta' : 'En meta',
        ], y);
      });
    }
  }

  for (let pagina = 1; pagina <= doc.getNumberOfPages(); pagina++) {
    doc.setPage(pagina);
    doc.setFontSize(8);
    doc.setTextColor(120, 110, 100);
    doc.text(`Reporte # ${reporteId.value}  ·  Página ${pagina} de ${doc.getNumberOfPages()}`, ancho / 2, 290, { align: 'center' });
  }

  doc.save(nombreArchivo('pdf'));
}

async function guardar() {
  if (!reporte.value || guardando.value) return;
  guardando.value = true;
  error.value = '';
  try {
    await asegurarReporteGuardado();
    await generarPdf();
    ok.value = `Reporte # ${reporteId.value} guardado como PDF.`;
  } catch (e) {
    error.value = e?.response?.data?.mensaje || 'No se pudo guardar el reporte PDF.';
  } finally {
    guardando.value = false;
  }
}

function celdaCsv(valor) {
  let texto = String(valor ?? '');
  if (/^[=+\-@]/.test(texto)) texto = `'${texto}`;
  return `"${texto.replaceAll('"', '""')}"`;
}

async function exportar() {
  if (!reporte.value || guardando.value) return;
  guardando.value = true;
  error.value = '';
  try {
    await asegurarReporteGuardado();
    const lineas = [
      [celdaCsv(`Reporte de ventas # ${reporteId.value}`)],
      [celdaCsv('Período'), celdaCsv(`${desde.value} - ${hasta.value}`)],
      [celdaCsv('Sucursal'), celdaCsv(sucursalId.value ? sucursales.value.find((s) => s.id === sucursalId.value)?.nombre || 'Sucursal seleccionada' : 'Todas las sucursales')],
      [celdaCsv('')],
      [celdaCsv('Resumen')],
      [celdaCsv('Cantidad de ventas'), celdaCsv(reporte.value.cantidadVentas)],
      [celdaCsv('Total (Bs)'), celdaCsv(Number(reporte.value.total).toFixed(2))],
      [celdaCsv('Promedio por venta (Bs)'), celdaCsv(Number(reporte.value.promedioPorVenta).toFixed(2))],
      [celdaCsv('')],
      [celdaCsv('VENTAS POR DÍA')],
      [celdaCsv('Fecha'), celdaCsv('Cantidad de ventas'), celdaCsv('Total (Bs)')],
      ...reporte.value.porDia.map((dia) => [
        celdaCsv(String(dia.fecha).slice(0, 10)),
        celdaCsv(dia.cantidad),
        celdaCsv(Number(dia.total).toFixed(2)),
      ]),
      [celdaCsv('')],
      [celdaCsv('PRODUCTOS MÁS VENDIDOS')],
      [celdaCsv('Posición'), celdaCsv('Producto'), celdaCsv('Unidades'), celdaCsv('Total (Bs)')],
      ...reporte.value.porProducto.map((producto, indice) => [
        celdaCsv(indice + 1),
        celdaCsv(producto.producto),
        celdaCsv(producto.unidades),
        celdaCsv(Number(producto.total).toFixed(2)),
      ]),
    ];

    if (tiempos.value) {
      lineas.push(
        [celdaCsv('')],
        [celdaCsv('TIEMPOS DE SERVICIO')],
        [celdaCsv('Meta'), celdaCsv(enTiempo(tiempos.value.metaMinutos))],
        [celdaCsv('Promedio'), celdaCsv(enTiempo(tiempos.value.promedioMinutos))],
        [celdaCsv('Pedidos entregados'), celdaCsv(tiempos.value.cantidadEntregados)],
        [celdaCsv('Fuera de meta'), celdaCsv(tiempos.value.fueraDeMeta)]
      );
      if (tiempos.value.porEtapa?.length) {
        lineas.push(
          [celdaCsv('')],
          [celdaCsv('Etapa'), celdaCsv('Promedio'), celdaCsv('Veces medida')],
          ...tiempos.value.porEtapa.map((etapa) => [
            celdaCsv(etapa.estado),
            celdaCsv(enTiempo(etapa.promedioMinutos)),
            celdaCsv(etapa.vecesMedido),
          ])
        );
      }
      if (tiempos.value.pedidos?.length) {
        lineas.push(
          [celdaCsv('')],
          [celdaCsv('Pedido'), celdaCsv('Creado'), celdaCsv('Entregado'), celdaCsv('Duración'), celdaCsv('Estado')],
          ...tiempos.value.pedidos.map((pedido) => [
            celdaCsv(`#${pedido.id}`),
            celdaCsv(fechaYHora(pedido.creado)),
            celdaCsv(fechaYHora(pedido.entregado)),
            celdaCsv(enTiempo(pedido.minutos)),
            celdaCsv(pedido.superaLaMeta ? 'Fuera de meta' : 'En meta'),
          ])
        );
      }
    }

    const contenido = `sep=;\r\n\uFEFF${lineas.map((linea) => linea.join(';')).join('\r\n')}`;
    const blob = new Blob([contenido], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = nombreArchivo('csv');
    a.click();
    URL.revokeObjectURL(url);
    ok.value = `Reporte # ${reporteId.value} exportado como CSV.`;
  } catch (e) {
    error.value = e?.response?.data?.mensaje || 'No se pudo exportar el reporte CSV.';
  } finally {
    guardando.value = false;
  }
}

const maxDia = computed(() =>
  Math.max(1, ...(reporte.value?.porDia ?? []).map((d) => Number(d.total || 0)))
);

// Se leen los numeros del texto en vez de usar new Date(), que al aplicar la
// zona horaria corre la fecha un dia hacia atras.
function fechaCorta(f) {
  if (!f) return '';
  const [, mes, dia] = String(f).slice(0, 10).split('-');
  return dia + '/' + mes;
}

onMounted(cargarSucursales);
</script>

<template>
  <div>
    <h2>Reportes</h2>
    <p class="gris">Ventas por rango de fechas</p>

    <p v-if="ok" class="mensaje-ok">{{ ok }}</p>
    <p v-if="error" class="mensaje-error">{{ error }}</p>

    <div class="tarjeta">
      <div class="fila filtros">
        <div class="campo-form">
          <label>Desde</label>
          <input v-model="desde" type="date" :max="hasta" />
        </div>
        <div class="campo-form">
          <label>Hasta</label>
          <input v-model="hasta" type="date" :min="desde" />
        </div>
        <div class="campo-form">
          <label>Sucursal</label>
          <select v-model="sucursalId">
            <option :value="0">Todas</option>
            <option v-for="s in sucursales" :key="s.id" :value="s.id">{{ s.nombre }}</option>
          </select>
        </div>
        <div class="campo-form acciones">
          <button class="btn btn-principal" :disabled="cargando" @click="generar">
            {{ cargando ? 'Generando...' : 'Generar reporte' }}
          </button>
        </div>
      </div>
    </div>

    <template v-if="reporte">
      <div class="tarjetas-kpi">
        <div class="kpi">
          <div class="kpi-titulo">Total del período</div>
          <div class="kpi-numero">Bs {{ Number(reporte.total).toFixed(2) }}</div>
        </div>
        <div class="kpi">
          <div class="kpi-titulo">Ventas</div>
          <div class="kpi-numero">{{ reporte.cantidadVentas }}</div>
        </div>
        <div class="kpi">
          <div class="kpi-titulo">Promedio por venta</div>
          <div class="kpi-numero">Bs {{ Number(reporte.promedioPorVenta).toFixed(2) }}</div>
        </div>
      </div>

      <div class="fila entre acciones-reporte">
        <span class="gris chico">
          {{ reporte.porDia.length }} día(s) con ventas · {{ reporte.porProducto.length }} producto(s)
        </span>
        <span>
          <button class="btn btn-linea" :disabled="guardando" @click="guardar">
            {{ guardando ? 'Guardando...' : 'Guardar PDF' }}
          </button>
          <button class="btn btn-principal" :disabled="guardando" @click="exportar">Guardar CSV</button>
        </span>
      </div>

      <div class="tarjeta">
        <h3>Ventas por día</h3>
        <p v-if="reporte.porDia.length === 0" class="gris">
          No hubo ventas en el período elegido.
        </p>
        <table v-else class="tabla">
          <thead>
            <tr><th>Fecha</th><th>Ventas</th><th>Total</th><th>Participación</th></tr>
          </thead>
          <tbody>
            <tr v-for="d in reporte.porDia" :key="d.fecha">
              <td>{{ fechaCorta(d.fecha) }}</td>
              <td>{{ d.cantidad }}</td>
              <td><strong>Bs {{ Number(d.total).toFixed(2) }}</strong></td>
              <td>
                <div class="barra-fondo">
                  <div class="barra-valor" :style="{ width: (d.total / maxDia * 100) + '%' }"></div>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Tiempos de servicio de los pedidos (HU-12 / RNF-10) -->
      <div v-if="tiempos" class="tarjeta">
        <h3>Tiempos de servicio</h3>
        <p class="gris chico">
          Desde que se crea el pedido hasta que queda entregado.
          Meta actual: {{ tiempos.metaMinutos }} min.
        </p>

        <p v-if="tiempos.cantidadEntregados === 0" class="gris">
          No hubo pedidos entregados en el período elegido.
        </p>

        <template v-else>
          <div class="tarjetas-kpi">
            <div class="kpi">
              <div class="kpi-titulo">Promedio</div>
              <div class="kpi-numero">{{ enTiempo(tiempos.promedioMinutos) }}</div>
            </div>
            <div class="kpi">
              <div class="kpi-titulo">Entregados</div>
              <div class="kpi-numero">{{ tiempos.cantidadEntregados }}</div>
            </div>
            <div class="kpi">
              <div class="kpi-titulo">Fuera de meta</div>
              <div class="kpi-numero">{{ tiempos.fueraDeMeta }}</div>
            </div>
          </div>

          <h4 v-if="tiempos.porEtapa.length">Promedio por etapa</h4>
          <table v-if="tiempos.porEtapa.length" class="tabla">
            <thead>
              <tr><th>Etapa</th><th>Promedio</th><th>Veces medida</th></tr>
            </thead>
            <tbody>
              <tr v-for="e in tiempos.porEtapa" :key="e.estado">
                <td><span class="etiqueta et-prep">{{ e.estado }}</span></td>
                <td><strong>{{ enTiempo(e.promedioMinutos) }}</strong></td>
                <td class="gris">{{ e.vecesMedido }}</td>
              </tr>
            </tbody>
          </table>

          <h4>Pedido por pedido</h4>
          <table class="tabla">
            <thead>
              <tr><th>N.°</th><th>Creado</th><th>Entregado</th><th>Tardó</th><th>Estado</th></tr>
            </thead>
            <tbody>
              <tr v-for="p in tiempos.pedidos" :key="p.id">
                <td><strong>#{{ p.id }}</strong></td>
                <td class="gris">{{ fechaYHora(p.creado) }}</td>
                <td class="gris">{{ fechaYHora(p.entregado) }}</td>
                <td><strong>{{ enTiempo(p.minutos) }}</strong></td>
                <td>
                  <span class="etiqueta" :class="p.superaLaMeta ? 'et-critico' : 'et-ok'">
                    {{ p.superaLaMeta ? 'FUERA DE META' : 'EN META' }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </template>
      </div>

      <div class="tarjeta">
        <h3>Productos más vendidos</h3>
        <p v-if="reporte.porProducto.length === 0" class="gris">
          Las ventas del período no tienen detalle por producto.
        </p>
        <table v-else class="tabla">
          <thead>
            <tr><th>#</th><th>Producto</th><th>Unidades</th><th>Total</th></tr>
          </thead>
          <tbody>
            <tr v-for="(p, i) in reporte.porProducto" :key="p.productoId">
              <td class="gris">{{ i + 1 }}</td>
              <td><strong>{{ p.producto }}</strong></td>
              <td>{{ p.unidades }}</td>
              <td>Bs {{ Number(p.total).toFixed(2) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <p v-else-if="!cargando" class="gris" style="padding:24px 0">
      Elegí un rango de fechas y tocá "Generar reporte".
    </p>
  </div>
</template>

<style scoped>
.filtros { gap: 14px; flex-wrap: wrap; align-items: flex-end; }
.filtros .campo-form { margin-bottom: 0; }
.filtros .acciones { display: flex; align-items: flex-end; }

.tarjetas-kpi {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 14px;
  margin: 18px 0;
}

.acciones-reporte { margin-bottom: 14px; gap: 10px; flex-wrap: wrap; }
.acciones-reporte .btn { margin-left: 8px; }

.barra-fondo {
  background: var(--linea, #e8e0d6);
  border-radius: 6px;
  height: 10px;
  width: 100%;
  overflow: hidden;
}

.barra-valor {
  background: var(--principal, #b5762a);
  height: 100%;
  border-radius: 6px;
}

@media (max-width: 620px) {
  .filtros { flex-direction: column; align-items: stretch; }
  .acciones-reporte .btn { margin: 6px 0 0 0; width: 100%; }
}
</style>
