<script setup>
import { onMounted, onUnmounted } from 'vue';
// Componente raiz. Segun la pantalla del store mostramos:
//  - Login
//  - Tienda (vista publica de clientes)
//  - PanelAdmin (gestion interna con CRUD)
import { store } from './store';
import Login from './components/Login.vue';
import TiendaCliente from './components/TiendaCliente.vue';
import PanelAdmin from './components/PanelAdmin.vue';

const selectorPorFlechas = 'button:not(:disabled), input[type="button"]:not(:disabled), input[type="submit"]:not(:disabled), input[type="reset"]:not(:disabled)';

function esCampoEditable(elemento) {
  return elemento instanceof HTMLElement && (
    elemento.isContentEditable ||
    elemento.matches('input:not([type="button"]):not([type="submit"]):not([type="reset"]), textarea, select')
  );
}

function centro(elemento) {
  const rect = elemento.getBoundingClientRect();
  return { x: rect.left + rect.width / 2, y: rect.top + rect.height / 2 };
}

function manejarTeclas(evento) {
  const direcciones = {
    ArrowUp: { x: 0, y: -1 },
    ArrowDown: { x: 0, y: 1 },
    ArrowLeft: { x: -1, y: 0 },
    ArrowRight: { x: 1, y: 0 },
  };
  const direccion = direcciones[evento.key];
  if (!direccion || esCampoEditable(evento.target)) return;

  const modales = [...document.querySelectorAll('.fondo-modal')].filter((modal) => {
    const estilo = window.getComputedStyle(modal);
    return modal.getClientRects().length > 0 && estilo.visibility !== 'hidden' && estilo.display !== 'none';
  });
  const contenedor = modales.at(-1) ?? document;
  const botones = [...contenedor.querySelectorAll(selectorPorFlechas)].filter((boton) => {
    const estilo = window.getComputedStyle(boton);
    return boton.getClientRects().length > 0 && estilo.visibility !== 'hidden' && estilo.display !== 'none';
  });
  if (botones.length === 0) return;

  evento.preventDefault();
  const actual = document.activeElement;
  if (!botones.includes(actual)) {
    botones[0].focus();
    return;
  }

  const origen = centro(actual);
  const candidatos = botones
    .filter((boton) => boton !== actual)
    .map((boton) => {
      const posicion = centro(boton);
      const dx = posicion.x - origen.x;
      const dy = posicion.y - origen.y;
      const adelante = dx * direccion.x + dy * direccion.y;
      const lateral = Math.abs(dx * direccion.y - dy * direccion.x);
      return { boton, adelante, lateral, posicion };
    });

  const enDireccion = candidatos
    .filter((candidato) => candidato.adelante > 0)
    .sort((a, b) => (a.adelante + a.lateral * 1.5) - (b.adelante + b.lateral * 1.5));

  if (enDireccion.length > 0) {
    enDireccion[0].boton.focus();
    return;
  }

  candidatos.sort((a, b) => {
    const ejeA = direccion.x !== 0 ? a.posicion.x : a.posicion.y;
    const ejeB = direccion.x !== 0 ? b.posicion.x : b.posicion.y;
    const extremo = direccion.x + direccion.y > 0 ? 1 : -1;
    return (ejeA - ejeB) * extremo || a.lateral - b.lateral;
  });
  candidatos[0]?.boton.focus();
}

onMounted(() => document.addEventListener('keydown', manejarTeclas));
onUnmounted(() => document.removeEventListener('keydown', manejarTeclas));
</script>

<template>
  <Login v-if="store.pantalla === 'login'" />
  <TiendaCliente v-else-if="store.pantalla === 'tienda'" />
  <PanelAdmin v-else-if="store.pantalla === 'panel'" />
</template>
