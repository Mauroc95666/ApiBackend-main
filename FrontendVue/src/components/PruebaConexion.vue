<script setup>
import { ref, onMounted } from 'vue';
import api from '@/api/axios';

const productos = ref([]);
const error = ref(null);
const cargando = ref(false);

const nuevoProducto = ref({
  nombre: '',
  precio: 0,
  stock: 0,
  emprendimientoId: null,
  categoriaId: null,
  proveedorId: null,
});

const extraerMensajeError = (err) => {
  if (err.response && err.response.data && err.response.data.mensaje) {
    return err.response.data.mensaje;
  }
  if (err.response && err.response.status === 500) {
    return 'Error interno del servidor. Consulta la consola del backend para m�s detalles.';
  }
  return 'Error de red: no se pudo conectar con la API.';
};

const obtenerProductos = async () => {
  cargando.value = true;
  error.value = null;
  try {
    const response = await api.get('/productos');
    productos.value = response.data;
  } catch (err) {
    error.value = extraerMensajeError(err);
  } finally {
    cargando.value = false;
  }
};

const agregarProducto = async () => {
  error.value = null;
  try {
    await api.post('/productos', nuevoProducto.value);
    nuevoProducto.value = { nombre: '', precio: 0, stock: 0, emprendimientoId: 1, categoriaId: 1, proveedorId: null };
    await obtenerProductos();
  } catch (err) {
    error.value = extraerMensajeError(err);
  }
};

const eliminarProducto = async (id) => {
  error.value = null;
  try {
    await api.delete(`/productos/${id}`);
    await obtenerProductos();
  } catch (err) {
    error.value = extraerMensajeError(err);
  }
};

onMounted(() => {
  obtenerProductos();
});
</script>

<template>
  <div class="prueba-conexion">
    <h2>Prueba de conexión con la API</h2>

    <p v-if="cargando">Cargando productos...</p>

    <p v-if="error" class="error">{{ error }}</p>

    <form @submit.prevent="agregarProducto" class="form-agregar">
      <input v-model="nuevoProducto.nombre" type="text" placeholder="Nombre del producto" required />
      <input v-model.number="nuevoProducto.precio" type="number" step="0.01" placeholder="Precio" required />
      <input v-model.number="nuevoProducto.stock" type="number" placeholder="Stock" required />
      <button type="submit">Agregar Producto</button>
    </form>

    <ul v-if="!cargando && !error && productos.length > 0">
      <li v-for="producto in productos" :key="producto.id">
        {{ producto.nombre }} - ${{ producto.precio }} - Stock: {{ producto.stock }}
        <button @click="eliminarProducto(producto.id)" class="btn-eliminar">Eliminar</button>
      </li>
    </ul>

    <p v-if="!cargando && !error && productos.length === 0">
      No se encontraron productos.
    </p>
  </div>
</template>

<style scoped>
.prueba-conexion {
  padding: 1rem;
}

.form-agregar {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 1rem;
  flex-wrap: wrap;
}

.form-agregar input {
  padding: 0.25rem;
}

.form-agregar button {
  padding: 0.25rem 0.75rem;
}

.error {
  color: red;
}

.btn-eliminar {
  margin-left: 0.5rem;
  color: red;
  cursor: pointer;
}
</style>