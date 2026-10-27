<script setup>
import { onBeforeUnmount, onMounted, ref } from 'vue';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import iconRetinaUrl from 'leaflet/dist/images/marker-icon-2x.png';
import iconUrl from 'leaflet/dist/images/marker-icon.png';
import shadowUrl from 'leaflet/dist/images/marker-shadow.png';

L.Icon.Default.mergeOptions({ iconRetinaUrl, iconUrl, shadowUrl });

const props = defineProps({
  modelValue: {
    type: String,
    default: '',
  },
  readonly: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(['update:modelValue']);
const mapaEl = ref(null);
const cargando = ref(false);
const error = ref('');
let mapa;
let marcador;
let controlador;

async function seleccionar(latlng) {
  controlador?.abort();
  controlador = new AbortController();
  const solicitud = controlador;
  cargando.value = true;
  error.value = '';

  try {
    const parametros = new URLSearchParams({
      format: 'jsonv2',
      lat: String(latlng.lat),
      lon: String(latlng.lng),
      zoom: '18',
      'accept-language': 'es',
    });
    const respuesta = await fetch(
      `https://nominatim.openstreetmap.org/reverse?${parametros}`,
      { signal: controlador.signal }
    );
    if (!respuesta.ok) throw new Error('El servicio de direcciones no está disponible.');

    const resultado = await respuesta.json();
    if (!resultado.display_name) throw new Error('No se encontró una dirección para ese punto.');

    emit(
      'update:modelValue',
      `${resultado.display_name} (${latlng.lat.toFixed(6)}, ${latlng.lng.toFixed(6)})`
    );
  } catch (e) {
    if (e.name !== 'AbortError') {
      error.value = e.message || 'No se pudo obtener la dirección. Probá seleccionar otro punto.';
    }
  } finally {
    if (controlador === solicitud) cargando.value = false;
  }
}

onMounted(() => {
  const coordenadas = props.modelValue.match(/\((-?\d+(?:\.\d+)?),\s*(-?\d+(?:\.\d+)?)\)$/);
  const centroInicial = coordenadas
    ? [Number(coordenadas[1]), Number(coordenadas[2])]
    : [-17.7833, -63.1821];
  mapa = L.map(mapaEl.value).setView(centroInicial, coordenadas ? 17 : 13);
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
  }).addTo(mapa);

  if (coordenadas) marcador = L.marker(centroInicial, { draggable: !props.readonly }).addTo(mapa);
  if (!props.readonly) {
    mapa.on('click', (evento) => {
      if (!marcador) {
        marcador = L.marker(evento.latlng, { draggable: true }).addTo(mapa);
        marcador.on('dragend', () => seleccionar(marcador.getLatLng()));
      } else {
        marcador.setLatLng(evento.latlng);
      }
      seleccionar(evento.latlng);
    });
  }
});

onBeforeUnmount(() => {
  controlador?.abort();
  mapa?.remove();
});
</script>

<template>
  <div>
    <div ref="mapaEl" class="mapa-direccion" aria-label="Mapa para seleccionar la dirección"></div>
    <p v-if="cargando" class="gris chico">Buscando la dirección del punto...</p>
    <p v-else-if="modelValue" class="direccion-seleccionada">{{ modelValue }}</p>
    <p v-else class="gris chico">Hacé clic en el mapa para seleccionar la dirección.</p>
    <p v-if="error" class="mensaje-error">{{ error }}</p>
  </div>
</template>

<style scoped>
.mapa-direccion {
  width: 100%;
  height: 260px;
  border: 1px solid var(--linea);
  border-radius: 8px;
  z-index: 0;
}

.direccion-seleccionada {
  margin: 8px 0 0;
  font-size: 13px;
  overflow-wrap: anywhere;
}
</style>
