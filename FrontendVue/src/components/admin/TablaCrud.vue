<script setup>
// Componente generico para hacer el CRUD de cualquier entidad simple.
// Se le pasan los campos y el endpoint, y el arma la tabla + el formulario.
import { ref, onMounted, watch } from 'vue';
import api from '@/api/axios';

const props = defineProps({
  titulo: String,
  subtitulo: String,
  endpoint: String, // ej: '/sucursales'
  campos: Array, // [{ campo, etiqueta, tipo, opciones, requerido, ocultarEnTabla }]
  nuevoModelo: Function, // devuelve un objeto vacio
});

const items = ref([]);
const cargando = ref(false);
const error = ref('');
const ok = ref('');

const modalAbierto = ref(false);
const editandoId = ref(null);
const form = ref({});
const guardando = ref(false);
const busqueda = ref('');

async function cargar() {
  cargando.value = true;
  error.value = '';
  try {
    const resp = await api.get(props.endpoint);
    items.value = resp.data || [];
  } catch {
    error.value = 'No se pudo cargar la información. ¿Está encendido el backend?';
  } finally {
    cargando.value = false;
  }
}

function abrirNuevo() {
  editandoId.value = null;
  form.value = props.nuevoModelo ? props.nuevoModelo() : {};
  ok.value = '';
  error.value = '';
  modalAbierto.value = true;
}

function abrirEditar(item) {
  editandoId.value = item.id;
  form.value = { ...item };
  ok.value = '';
  error.value = '';
  modalAbierto.value = true;
}

function cerrar() {
  modalAbierto.value = false;
}

async function guardar() {
  guardando.value = true;
  error.value = '';
  try {
    if (editandoId.value) {
      await api.put(props.endpoint + '/' + editandoId.value, form.value);
      ok.value = 'Registro actualizado correctamente.';
    } else {
      await api.post(props.endpoint, form.value);
      ok.value = 'Registro creado correctamente.';
    }
    modalAbierto.value = false;
    await cargar();
  } catch (e) {
    const msg = e?.response?.data?.mensaje;
    error.value = msg || 'No se pudo guardar. Revisá los datos.';
  } finally {
    guardando.value = false;
  }
}

async function eliminar(item) {
  if (!confirm('¿Seguro que querés eliminar este registro?')) return;
  error.value = '';
  try {
    await api.delete(props.endpoint + '/' + item.id);
    ok.value = 'Registro eliminado.';
    await cargar();
  } catch (e) {
    const msg = e?.response?.data?.mensaje;
    error.value = msg || 'No se pudo eliminar (puede estar en uso por otro registro).';
  }
}

// Para las columnas tipo select mostramos el texto y no el id.
function mostrar(item, campo) {
  const def = props.campos.find((c) => c.campo === campo.campo);
  const valor = item[campo.campo];
  if (def && def.opciones) {
    const op = def.opciones.find((o) => o.valor === valor);
    return op ? op.texto : valor;
  }
  if (campo.tipo === 'checkbox') return valor ? 'Sí' : 'No';
  return valor;
}

const columnasVisibles = () => props.campos.filter((c) => !c.ocultarEnTabla);

const itemsFiltrados = () => {
  if (!busqueda.value) return items.value;
  const t = busqueda.value.toLowerCase();
  return items.value.filter((it) =>
    props.campos.some((c) => String(it[c.campo] ?? '').toLowerCase().includes(t))
  );
};

// Si cambian los campos (cambio de modulo), recargamos.
watch(() => props.endpoint, cargar);
onMounted(cargar);
</script>

<template>
  <div>
    <div class="fila entre encabezado">
      <div>
        <h2>{{ titulo }}</h2>
        <p class="gris" v-if="subtitulo">{{ subtitulo }}</p>
      </div>
      <button class="btn btn-principal" @click="abrirNuevo">+ Nuevo</button>
    </div>

    <p v-if="ok" class="mensaje-ok">{{ ok }}</p>
    <p v-if="error" class="mensaje-error">{{ error }}</p>

    <div class="tarjeta">
      <input v-model="busqueda" class="buscador-tabla" placeholder="Buscar..." />

      <p v-if="cargando" class="cargando">Cargando...</p>

      <table v-else class="tabla">
        <thead>
          <tr>
            <th>#</th>
            <th v-for="c in columnasVisibles()" :key="c.campo">{{ c.etiqueta }}</th>
            <th style="width: 150px">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in itemsFiltrados()" :key="item.id">
            <td class="gris">{{ item.id }}</td>
            <td v-for="c in columnasVisibles()" :key="c.campo">{{ mostrar(item, c) }}</td>
            <td>
              <button class="btn btn-linea btn-mini" @click="abrirEditar(item)">Editar</button>
              <button class="btn btn-peligro btn-mini" @click="eliminar(item)">Eliminar</button>
            </td>
          </tr>
          <tr v-if="itemsFiltrados().length === 0">
            <td :colspan="columnasVisibles().length + 2" class="gris" style="text-align:center;padding:24px">
              No hay registros.
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal del formulario -->
    <div v-if="modalAbierto" class="fondo-modal" @click.self="cerrar">
      <div class="caja-modal">
        <h3>{{ editandoId ? 'Editar' : 'Nuevo' }} — {{ titulo }}</h3>

        <form @submit.prevent="guardar">
          <div v-for="c in campos" :key="c.campo" class="campo-form">
            <label>{{ c.etiqueta }}</label>

            <select v-if="c.tipo === 'select'" v-model="form[c.campo]">
              <option :value="null" disabled>Seleccioná...</option>
              <option v-for="o in c.opciones" :key="o.valor" :value="o.valor">
                {{ o.texto }}
              </option>
            </select>

            <label v-else-if="c.tipo === 'checkbox'" class="check-form">
              <input type="checkbox" v-model="form[c.campo]" />
              <span>{{ c.etiqueta }}</span>
            </label>

            <input
              v-else
              v-model="form[c.campo]"
              :type="c.tipo === 'number' ? 'number' : 'text'"
              :step="c.tipo === 'number' ? 'any' : null"
              :required="c.requerido"
            />
          </div>

          <div class="fila entre acciones-modal">
            <button type="button" class="btn btn-linea" @click="cerrar">Cancelar</button>
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
.encabezado {
  margin-bottom: 18px;
}

.buscador-tabla {
  max-width: 280px;
  margin-bottom: 14px;
}

.btn-mini {
  margin-right: 6px;
}

.campo-form {
  margin-bottom: 14px;
}

.check-form {
  display: flex;
  align-items: center;
  gap: 8px;
  text-transform: none;
  letter-spacing: normal;
  font-weight: normal;
  color: var(--texto);
  font-size: 14px;
}

.check-form input {
  width: auto;
}

.acciones-modal {
  margin-top: 22px;
}
</style>
