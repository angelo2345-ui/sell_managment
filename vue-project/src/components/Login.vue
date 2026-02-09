<script setup>
import { ref } from 'vue';

const emit = defineEmits(['login-success']);

const email = ref('');
const password = ref('');
const error = ref('');
const isLoading = ref(false);

const handleLogin = async () => {
  error.value = '';
  isLoading.value = true;

  // Simulación de login (Fake API call)
  setTimeout(() => {
    if (email.value === 'admin@test.com' && password.value === '123456') {
      localStorage.setItem('authToken', 'fake-token-123');
      emit('login-success');
    } else {
      error.value = 'Credenciales inválidas (admin@test.com / 123456)';
    }
    isLoading.value = false;
  }, 1000);
};
</script>

<template>
  <div class="login-card card">
    <div class="login-header">
      <div class="brand-logo-container">
        <img src="/imagenes/dga-logo.jpeg" alt="DGA Aduanas Logo" class="logo-img" />
      </div>
      <h2>Bienvenido</h2>
      <p>Ingresa a tu panel de control</p>
    </div>

    <form @submit.prevent="handleLogin">
      <div class="form-group">
        <label for="email">Correo Electrónico</label>
        <input 
          id="email" 
          v-model="email" 
          type="email" 
          placeholder="admin@test.com" 
          required 
          autofocus
        />
      </div>

      <div class="form-group">
        <label for="password">Contraseña</label>
        <input 
          id="password" 
          v-model="password" 
          type="password" 
          placeholder="••••••" 
          required 
        />
      </div>

      <div v-if="error" class="alert alert-error" role="alert">
        {{ error }}
      </div>

      <button type="submit" class="btn btn-primary btn-block" :disabled="isLoading">
        {{ isLoading ? 'Entrando...' : 'Iniciar Sesión' }}
      </button>
    </form>
  </div>
</template>

<style scoped>
.login-card {
  width: 100%;
  max-width: 400px;
  background: var(--bg-surface);
  padding: 2.5rem;
  border-radius: var(--radius);
  box-shadow: var(--shadow-lg);
  border: 1px solid var(--border-color);
  animation: slideUp 0.5s ease-out;
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

.login-header {
  text-align: center;
  margin-bottom: 2rem;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.brand-logo-container {
  margin-bottom: 1.5rem;
  display: flex;
  justify-content: center;
}

.logo-img {
  max-width: 180px; /* Ajustar según preferencia */
  height: auto;
  display: block;
}

.login-header h2 {
  font-size: 1.5rem;
  font-weight: 600;
  color: var(--text-main);
  margin-bottom: 0.5rem;
}

.login-header p {
  color: var(--text-muted);
  font-size: 0.95rem;
}

.btn-block {
  width: 100%;
  padding: 0.875rem;
  font-size: 1rem;
}
</style>
