<script setup>
import { ref } from 'vue';
import api, { guardarToken } from '@/api/axios';
import { store } from '@/store';

const correo = ref('');
const clave = ref('');
const recordar = ref(false);
const verClave = ref(false);
const cargando = ref(false);
const error = ref('');

// Cambio de contraseña obligatorio. Cuando el backend avisa que esta cuenta
// tiene que cambiarla, se muestra este paso antes de dejar entrar.
const pidiendoClaveNueva = ref(false);
const usuarioPendiente = ref(null);
const claveNueva = ref('');
const claveRepetida = ref('');
const cambiando = ref(false);

// La verificacion ocurre en el servidor: se manda correo y contraseña a
// POST /auth/login y el backend responde quien es, sin contraseña. Antes esta
// pantalla pedia GET /usuarios y comparaba en el navegador, con lo cual el
// sistema entregaba todos los usuarios con sus claves antes de que nadie
// hubiera iniciado sesion.
async function iniciarSesion() {
  error.value = '';
  if (!correo.value || !clave.value) {
    error.value = 'Completá el correo y la contraseña.';
    return;
  }
  cargando.value = true;
  try {
    let u;
    try {
      const resp = await api.post('/auth/login', {
        email: correo.value.trim(),
        password: clave.value,
      });
      u = resp.data;
    } catch (e) {
      // 401 y 400 son respuestas del servidor, no fallas de conexion: se
      // muestra el motivo que el servidor da.
      if (e?.response) {
        error.value = e.response.data?.mensaje || 'No se pudo iniciar sesión.';
        return;
      }
      throw e;
    }

    // Desde acá cada pedido a la API viaja con este token.
    guardarToken(u.token);

    // Si la cuenta tiene que cambiar la clave, no se entra todavía.
    if (u.debeCambiarPassword) {
      usuarioPendiente.value = u;
      pidiendoClaveNueva.value = true;
      return;
    }

    // El rol y sus permisos ya vienen resueltos en la respuesta del login.
    const nombreRol = u.rol || '';
    const permisos = u.permisos || [];

    if (permisos.length > 0) {
      // Entra al panel viendo solo los modulos que su rol tiene permitidos.
      store.entrarAlPanel(u, nombreRol, permisos);
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

// Guarda la contraseña nueva y recién ahí entra al sistema.
async function confirmarClaveNueva() {
  error.value = '';

  if (claveNueva.value.length < 8) {
    error.value = 'La contraseña nueva tiene que tener al menos 8 caracteres.';
    return;
  }
  if (claveNueva.value !== claveRepetida.value) {
    error.value = 'Las dos contraseñas no coinciden.';
    return;
  }

  cambiando.value = true;
  try {
    await api.post('/auth/cambiar-password', {
      email: correo.value.trim(),
      passwordActual: clave.value,
      passwordNueva: claveNueva.value,
    });

    // Se vuelve a entrar con la clave nueva para traer el token y los permisos.
    clave.value = claveNueva.value;
    claveNueva.value = '';
    claveRepetida.value = '';
    pidiendoClaveNueva.value = false;
    usuarioPendiente.value = null;
    await iniciarSesion();
  } catch (e) {
    error.value = e?.response?.data?.mensaje || 'No se pudo cambiar la contraseña.';
  } finally {
    cambiando.value = false;
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

        <!-- Cambio de contraseña obligatorio antes del primer ingreso -->
        <form v-if="pidiendoClaveNueva" @submit.prevent="confirmarClaveNueva">
          <p class="gris" style="margin-bottom:14px">
            Esta cuenta todavía usa su contraseña inicial. Elegí una nueva para
            poder entrar.
          </p>
          <div class="campo-form">
            <label>Contraseña nueva</label>
            <input v-model="claveNueva" type="password" autocomplete="new-password" required />
            <small class="gris">Mínimo 8 caracteres</small>
          </div>
          <div class="campo-form">
            <label>Repetila</label>
            <input v-model="claveRepetida" type="password" autocomplete="new-password" required />
          </div>
          <button class="btn btn-principal" style="width:100%" :disabled="cambiando">
            {{ cambiando ? 'Guardando...' : 'GUARDAR Y ENTRAR' }}
          </button>
        </form>

        <form v-else @submit.prevent="iniciarSesion">
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
