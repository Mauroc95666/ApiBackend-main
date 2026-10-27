<script setup>
// Pantalla de respaldos (RNF-09 / HU-13). El trabajo pesado lo hace el
// servidor con BACKUP DATABASE y RESTORE DATABASE: aca solo se dispara, se
// lista lo que hay y se confirma la restauracion, que es irreversible.
import { ref, onMounted } from 'vue';
import api from '@/api/axios';

const datos = ref(null);
const cargando = ref(false);
const generando = ref(false);
const restaurando = ref(false);
const error = ref('');
const ok = ref('');

// Respaldo elegido para restaurar y texto de confirmacion que escribe el
// usuario. Sin escribir RESTAURAR el servidor rechaza el pedido.
const aRestaurar = ref(null);
const confirmacion = ref('');

async function cargar() {
  cargando.value = true;
  error.value = '';
  try {
    const r = await api.get('/respaldos');
    datos.value = r.data;
  } catch (e) {
    error.value = e.response?.data?.mensaje || 'No se pudo leer la lista de respaldos.';
  } finally {
    cargando.value = false;
  }
}

async function generar() {
  generando.value = true;
  error.value = '';
  ok.value = '';
  try {
    const r = await api.post('/respaldos');
    ok.value = `${r.data.mensaje}: ${r.data.archivo.archivo} (${r.data.archivo.tamano})`;
    await cargar();
  } catch (e) {
    error.value = e.response?.data?.mensaje || 'No se pudo generar el respaldo.';
  } finally {
    generando.value = false;
  }
}

function pedirRestaurar(archivo) {
  aRestaurar.value = archivo;
  confirmacion.value = '';
  error.value = '';
  ok.value = '';
}

async function confirmarRestaurar() {
  restaurando.value = true;
  error.value = '';
  try {
    const r = await api.post('/respaldos/restaurar', {
      archivo: aRestaurar.value.archivo,
      confirmacion: confirmacion.value,
    });
    ok.value = `${r.data.mensaje}. Antes de pisar los datos se guardó ${r.data.respaldoPrevio}.`;
    aRestaurar.value = null;
    await cargar();
  } catch (e) {
    error.value = e.response?.data?.mensaje || 'No se pudo restaurar.';
  } finally {
    restaurando.value = false;
  }
}

function descargar(archivo) {
  // Se baja con axios y no con un enlace directo: el endpoint pide el token
  // de sesion en el encabezado y un <a href> no lo manda.
  api
    .get(`/respaldos/descargar/${archivo}`, { responseType: 'blob' })
    .then((r) => {
      const url = URL.createObjectURL(new Blob([r.data]));
      const a = document.createElement('a');
      a.href = url;
      a.download = archivo;
      a.click();
      URL.revokeObjectURL(url);
    })
    .catch(() => {
      error.value = 'No se pudo descargar el respaldo.';
    });
}

function fechaHora(iso) {
  if (!iso) return '-';
  // Se arma a mano para no depender del huso del navegador.
  const f = String(iso);
  const dia = f.slice(8, 10);
  const mes = f.slice(5, 7);
  const anio = f.slice(0, 4);
  const hora = f.slice(11, 16);
  return `${dia}/${mes}/${anio} ${hora}`;
}

onMounted(cargar);
</script>

<template>
  <section>
    <h2>Respaldos</h2>
    <p class="gris">Copias de seguridad de la base de datos</p>

    <p v-if="ok" class="mensaje-ok">{{ ok }}</p>
    <p v-if="error" class="mensaje-error">{{ error }}</p>

    <div class="tarjeta">
      <div class="fila entre barra-acciones">
        <p class="gris chico sin-margen">
          El respaldo guarda toda la base en un archivo .bak dentro del servidor.
          Conviene además descargarlo y guardarlo fuera.
        </p>
        <div>
          <button class="btn btn-linea" :disabled="cargando" @click="cargar">
            {{ cargando ? 'Actualizando…' : 'Actualizar' }}
          </button>
          <button class="btn btn-principal" :disabled="generando" @click="generar">
            {{ generando ? 'Generando…' : 'Generar respaldo ahora' }}
          </button>
        </div>
      </div>
    </div>

    <div v-if="datos" class="tarjetas-kpi">
      <div class="kpi">
        <div class="kpi-titulo">Respaldos guardados</div>
        <div class="kpi-numero">{{ datos.cantidad }}</div>
      </div>
      <div class="kpi">
        <div class="kpi-titulo">Último respaldo</div>
        <div class="kpi-numero chico-kpi">{{ fechaHora(datos.ultimo) }}</div>
      </div>
      <div class="kpi">
        <div class="kpi-titulo">Respaldo automático</div>
        <div class="kpi-numero chico-kpi">
          {{ datos.cadaHoras > 0 ? 'cada ' + datos.cadaHoras + ' h' : 'desactivado' }}
        </div>
      </div>
    </div>

    <div v-if="datos" class="tarjeta">
      <h3>Archivos</h3>
      <p class="gris chico">
        Carpeta en el servidor: {{ datos.carpeta }} · se conservan los últimos
        {{ datos.conservar }} automáticos
      </p>

      <p v-if="datos.archivos.length === 0" class="gris">
        Todavía no hay ningún respaldo. Generá el primero con el botón de arriba.
      </p>

      <table v-else class="tabla">
        <thead>
          <tr>
            <th>Archivo</th>
            <th>Fecha</th>
            <th>Tamaño</th>
            <th>Origen</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="a in datos.archivos" :key="a.archivo">
            <td class="chico">{{ a.archivo }}</td>
            <td>{{ fechaHora(a.fecha) }}</td>
            <td>{{ a.tamano }}</td>
            <td>
              <span class="etiqueta" :class="a.origen === 'automatico' ? 'et-auto' : 'et-manual'">
                {{ a.origen === 'automatico' ? 'AUTOMÁTICO' : 'MANUAL' }}
              </span>
            </td>
            <td class="derecha">
              <button class="btn btn-linea btn-mini" @click="descargar(a.archivo)">Descargar</button>
              <button class="btn btn-peligro btn-mini" @click="pedirRestaurar(a)">Restaurar</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Confirmación de restauración -->
    <div v-if="aRestaurar" class="fondo-modal" @click.self="aRestaurar = null">
      <div class="caja-modal">
        <h3>Restaurar la base de datos</h3>
        <p>
          Se va a reemplazar <strong>toda</strong> la información actual por la del
          respaldo <strong>{{ aRestaurar.archivo }}</strong>
          ({{ fechaHora(aRestaurar.fecha) }}).
        </p>
        <p class="gris chico">
          Todo lo que se cargó después de ese respaldo se pierde. Antes de restaurar,
          el sistema guarda un respaldo del estado actual, así que hay vuelta atrás.
        </p>
        <div class="campo-form">
          <label>Para confirmar, escribí RESTAURAR</label>
          <input v-model="confirmacion" placeholder="RESTAURAR" />
        </div>
        <div class="fila derecha">
          <button class="btn btn-linea" @click="aRestaurar = null">Cancelar</button>
          <button
            class="btn btn-peligro"
            :disabled="restaurando || confirmacion.trim().toUpperCase() !== 'RESTAURAR'"
            @click="confirmarRestaurar"
          >
            {{ restaurando ? 'Restaurando…' : 'Restaurar' }}
          </button>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.barra-acciones { gap: 14px; flex-wrap: wrap; align-items: center; }
.barra-acciones .btn { margin-left: 8px; }
.sin-margen { margin: 0; max-width: 520px; }

.tarjetas-kpi {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 14px;
  margin: 18px 0;
}

.chico-kpi { font-size: 1.05rem; }
.derecha { text-align: right; }
.derecha .btn { margin-left: 6px; }

.et-auto { background: #e7f0ff; color: #1f4f8b; }
.et-manual { background: #eef3ec; color: #3d6b3a; }

.btn-peligro {
  background: #b3261e;
  color: #fff;
  border: 1px solid #b3261e;
}
.btn-peligro:disabled { opacity: 0.5; }

@media (max-width: 620px) {
  .barra-acciones { flex-direction: column; align-items: stretch; }
  .barra-acciones .btn { margin: 6px 0 0 0; width: 100%; }
  .derecha { text-align: left; }
  .derecha .btn { margin: 6px 6px 0 0; }
}
</style>
