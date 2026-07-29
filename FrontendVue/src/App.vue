<script setup>
// Componente raiz. Segun la pantalla del store mostramos:
//  - Login
//  - Tienda (vista publica de clientes)
//  - PanelAdmin (gestion interna con CRUD)
import { store } from './store';
import Login from './components/Login.vue';
import TiendaCliente from './components/TiendaCliente.vue';
import PanelAdmin from './components/PanelAdmin.vue';
</script>

<template>
  <Login v-if="store.pantalla === 'login'" />
  <TiendaCliente v-else-if="store.pantalla === 'tienda'" />
  <PanelAdmin v-else-if="store.pantalla === 'panel'" />
</template>
