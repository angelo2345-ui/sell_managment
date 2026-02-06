<script setup>
import { ref } from 'vue';
import axios from 'axios';

const emit = defineEmits(['login-success']);

const username = ref('');
const password = ref('');
const errorMessage = ref('');

const handleLogin = async () => {
  try {
    // 1. Enviamos credenciales al backend
    const response = await axios.post('http://localhost:5227/api/auth/login', {
      username: username.value,
      password: password.value
    });

    // 2. Si es exitoso, guardamos el TOKEN en localStorage
    const token = response.data.token;
    localStorage.setItem('authToken', token);
    
    // 3. Configuramos axios para que siempre use este token en el futuro
    axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;

    // 4. Avisamos al padre (App.vue) que ya entramos
    emit('login-success');

  } catch (error) {
    console.error(error);
    errorMessage.value = "Credenciales incorrectas o error de servidor";
  }
};
</script>

<template>
  <div class="login-container">
    <h2>Iniciar Sesión</h2>
    <form @submit.prevent="handleLogin">
      <div class="form-group">
        <label>Usuario:</label>
        <input v-model="username" type="text" required />
      </div>
      <div class="form-group">
        <label>Contraseña:</label>
        <input v-model="password" type="password" required />
      </div>
      
      <p v-if="errorMessage" class="error">{{ errorMessage }}</p>

      <button type="submit">Ingresar</button>
    </form>
  </div>
</template>

<style scoped>
.login-container {
  max-width: 300px;
  margin: 50px auto;
  padding: 20px;
  border: 1px solid #ccc;
  border-radius: 8px;
  background: white;
}
.form-group {
  margin-bottom: 15px;
}
input {
  width: 100%;
  padding: 8px;
  margin-top: 5px;
}
button {
  width: 100%;
  padding: 10px;
  background-color: #2c3e50;
  color: white;
  border: none;
  cursor: pointer;
}
.error {
  color: red;
  font-size: 0.9em;
}
</style>