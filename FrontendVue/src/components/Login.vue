<script setup>
import { ref } from 'vue';
import api from '@/api/axios';
import { store, rolEsAdmin, rolEsStaff } from '@/store';

const correo = ref('');
const clave = ref('');
const recordar = ref(false);
const verClave = ref(false);
const cargando = ref(false);
const error = ref('');

// Valida el usuario contra la API. Como el backend no tiene endpoint /login,
// traemos la lista de usuarios y comparamos correo + contraseña aca en el front.
async function iniciarSesion() {
  error.value = '';
  if (!correo.value || !clave.value) {
    error.value = 'Completá el correo y la contraseña.';
    return;
  }
  cargando.value = true;
  try {
    const resp = await api.get('/usuarios');
    const lista = resp.data || [];
    const u = lista.find(
      (x) => (x.email || '').toLowerCase() === correo.value.trim().toLowerCase()
    );

    if (!u) {
      error.value = 'No existe un usuario con ese correo.';
      return;
    }
    // El campo se llama passwordHash en la tabla Usuario.
    if ((u.passwordHash || '') !== clave.value) {
      error.value = 'La contraseña no es correcta.';
      return;
    }
    if (u.activo === false) {
      error.value = 'Este usuario está desactivado. Contactá al administrador.';
      return;
    }

    // Averiguamos el nombre del rol para saber si es admin.
    let nombreRol = '';
    try {
      const r = await api.get('/roles/' + u.rolId);
      nombreRol = r.data.nombre;
    } catch {
      nombreRol = '';
    }

    if (rolEsAdmin(nombreRol)) {
      // Administrador / gerente: ve el panel completo.
      store.entrarComoAdmin(u, nombreRol);
    } else if (rolEsStaff(nombreRol)) {
      // Personal interno (cajero, inventario, vendedor): panel limitado a su rol.
      store.entrarComoStaff(u, nombreRol);
    } else {
      // Usuario sin rol de panel: lo mandamos a la tienda.
      store.usuario = u;
      store.entrarComoCliente();
    }
  } catch {
    error.value = 'No se pudo conectar con el servidor. ¿Está encendido el backend?';
  } finally {
    cargando.value = false;
  }
}

function entrarComoVisitante() {
  store.entrarComoCliente();
}
</script>

<template>
  <div class="login-pantalla">
    <!-- Panel de marca (izquierda) -->
    <div class="login-marca">
      <div class="marca-centro">
        <div class="logo-circulo">K</div>
        <h1>Panadería Kiataque</h1>
        <p class="subtitulo">Sistema de Gestión Comercial</p>
        <div class="separador"></div>
        <p class="lema">Pan fresco, cuentas claras.</p>
      </div>
    </div>

    <!-- Formulario (derecha) -->
    <div class="login-form">
      <div class="form-caja">
        <h2>Iniciar sesión</h2>
        <p class="gris">Ingresá tus credenciales para continuar</p>

        <p v-if="error" class="mensaje-error">{{ error }}</p>

        <form @submit.prevent="iniciarSesion">
          <div class="campo">
            <label>Correo electrónico</label>
            <input v-model="correo" type="email" placeholder="rosario@kiataque.bo" />
          </div>

          <div class="campo">
            <label>Contraseña</label>
            <div class="campo-clave">
              <input
                v-model="clave"
                :type="verClave ? 'text' : 'password'"
                placeholder="••••••••"
              />
              <button type="button" class="ojo" @click="verClave = !verClave">
                {{ verClave ? 'ocultar' : 'ver' }}
              </button>
            </div>
          </div>

          <div class="fila entre opciones">
            <label class="check">
              <input type="checkbox" v-model="recordar" />
              <span>Recordarme</span>
            </label>
            <a href="#" @click.prevent>¿Olvidaste tu contraseña?</a>
          </div>

          <button class="btn btn-principal btn-ingresar" :disabled="cargando">
            {{ cargando ? 'Ingresando...' : 'INGRESAR' }}
          </button>
        </form>

        <div class="separador-form"></div>
        <p class="gris nota-cliente">
          ¿Sos cliente?
          <a href="#" @click.prevent="entrarComoVisitante">Entrá a ver los productos</a>
        </p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.login-pantalla {
  display: flex;
  min-height: 100vh;
}

/* izquierda */
.login-marca {
  width: 40%;
  background: var(--cafe-panel);
  color: #f4ead9;
  display: flex;
  align-items: center;
  justify-content: center;
}

.marca-centro {
  text-align: center;
  padding: 30px;
}

.logo-circulo {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  background: var(--caramelo);
  color: #fff;
  font-size: 60px;
  font-weight: 800;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 24px auto;
}

.login-marca h1 {
  color: #fff;
  font-size: 30px;
}

.subtitulo {
  color: #d8c3a5;
  margin-top: 4px;
}

.separador {
  width: 120px;
  height: 1px;
  background: #6b4d38;
  margin: 22px auto;
}

.lema {
  color: #c9b193;
  font-style: italic;
}

/* derecha */
.login-form {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--crema);
}

.form-caja {
  width: 100%;
  max-width: 380px;
  padding: 30px;
}

.campo {
  margin-bottom: 16px;
}

.campo-clave {
  position: relative;
}

.ojo {
  position: absolute;
  right: 10px;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: var(--texto-suave);
  cursor: pointer;
  font-size: 12px;
}

.opciones {
  margin: 6px 0 20px 0;
  font-size: 13px;
}

.check {
  display: flex;
  align-items: center;
  gap: 6px;
  text-transform: none;
  letter-spacing: normal;
  font-weight: normal;
  color: var(--texto);
  margin: 0;
  font-size: 14px;
}

.check input {
  width: auto;
}

.btn-ingresar {
  width: 100%;
  padding: 13px;
}

.separador-form {
  height: 1px;
  background: var(--linea);
  margin: 24px 0 14px 0;
}

.nota-cliente {
  text-align: center;
  font-size: 14px;
}

/* responsive: en pantallas chicas se oculta el panel de marca */
@media (max-width: 800px) {
  .login-marca {
    display: none;
  }
}
</style>
