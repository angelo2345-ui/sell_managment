<script setup>
import { ref } from 'vue';
import clientService from '../services/clientService';

const emit = defineEmits(['client-created']);

const client = ref({
  name: '',
  email: '',
  phone: ''
});

const errors = ref({});
const isSubmitting = ref(false);
const serverMessage = ref('');
const serverError = ref(false);

const validate = () => {
  errors.value = {};
  let isValid = true;

  if (!client.value.name.trim()) {
    errors.value.name = 'El nombre es obligatorio.';
    isValid = false;
  } else if (client.value.name.length < 3) {
    errors.value.name = 'El nombre debe tener al menos 3 caracteres.';
    isValid = false;
  }

  if (!client.value.email.trim()) {
    errors.value.email = 'El email es obligatorio.';
    isValid = false;
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(client.value.email)) {
    errors.value.email = 'Ingresa un email válido.';
    isValid = false;
  }

  if (client.value.phone && client.value.phone.length < 7) {
    errors.value.phone = 'El teléfono debe tener al menos 7 dígitos.';
    isValid = false;
  }

  return isValid;
};

const submitForm = async () => {
  serverMessage.value = '';
  serverError.value = false;

  if (!validate()) return;

  isSubmitting.value = true;

  try {
    const response = await clientService.createClient(client.value);

    if (response.success) {
      serverMessage.value = '¡Cliente registrado con éxito!';
      client.value = { name: '', email: '', phone: '' };
      emit('client-created');
      
      setTimeout(() => serverMessage.value = '', 3000);
    } else {
      serverError.value = true;
      serverMessage.value = response.message || 'Error al guardar el cliente.';
    }
  } catch (error) {
    console.error(error);
    serverError.value = true;
    if (error.response && error.response.data) {
       serverMessage.value = error.response.data.message;
    } else {
       serverMessage.value = 'Error de conexión con el servidor.';
    }
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<template>
  <div class="card">
    <div class="card-header">
      <h3>Nuevo Cliente</h3>
    </div>
    
    <form @submit.prevent="submitForm" novalidate class="client-form">
      <!-- Nombre -->
      <div class="form-group">
        <label for="name">Nombre</label>
        <input 
          id="name" 
          v-model="client.name" 
          type="text" 
          :class="{ 'is-invalid': errors.name }"
          placeholder="Ej: Juan Pérez"
        />
        <span v-if="errors.name" class="error-msg">{{ errors.name }}</span>
      </div>

      <!-- Email -->
      <div class="form-group">
        <label for="email">Email</label>
        <input 
          id="email" 
          v-model="client.email" 
          type="email" 
          :class="{ 'is-invalid': errors.email }"
          placeholder="juan@ejemplo.com"
        />
        <span v-if="errors.email" class="error-msg">{{ errors.email }}</span>
      </div>

      <!-- Teléfono -->
      <div class="form-group">
        <label for="phone">Teléfono</label>
        <input 
          id="phone" 
          v-model="client.phone" 
          type="tel" 
          :class="{ 'is-invalid': errors.phone }"
          placeholder="Ej: 555-1234"
        />
        <span v-if="errors.phone" class="error-msg">{{ errors.phone }}</span>
      </div>

      <!-- Botón -->
      <button type="submit" :disabled="isSubmitting" class="btn btn-primary w-full">
        {{ isSubmitting ? 'Guardando...' : 'Guardar Cliente' }}
      </button>

      <!-- Mensaje del Servidor -->
      <div v-if="serverMessage" :class="['alert mt-4', serverError ? 'alert-error' : 'alert-success']" role="alert">
        {{ serverMessage }}
      </div>
    </form>
  </div>
</template>

<style scoped>
.card {
  width: 100%;
  max-width: 100%;
  box-sizing: border-box;
  overflow: hidden;
}

.card-header {
  width: 100%;
  box-sizing: border-box;
}

.card-header h3 {
  margin-bottom: 1.5rem;
  color: var(--text-main);
  font-size: 1.25rem;
  border-bottom: 1px solid var(--border-color);
  padding-bottom: 1rem;
  word-wrap: break-word;
}

.client-form {
  width: 100%;
  box-sizing: border-box;
}

.form-group {
  width: 100%;
  box-sizing: border-box;
  margin-bottom: 1.25rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: var(--text-main);
}

.form-group input {
  width: 100%;
  min-width: 0;
  box-sizing: border-box;
  padding: 0.75rem;
  border: 1px solid var(--border-color);
  border-radius: 0.375rem;
  font-size: 1rem;
  transition: border-color 0.2s;
}

.form-group input:focus {
  outline: none;
  border-color: var(--primary);
}

.is-invalid {
  border-color: var(--danger) !important;
  background-color: #fef2f2;
}

.error-msg {
  color: var(--danger);
  font-size: 0.85rem;
  margin-top: 0.25rem;
  display: block;
}

.w-full {
  width: 100%;
  min-width: 0;
  box-sizing: border-box;
  margin-top: 1rem;
  margin-bottom: 0.5rem;
  padding: 0.875rem 1rem;
}

.mt-4 {
  margin-top: 1.5rem;
  word-wrap: break-word;
}

/* Prevenir overflow horizontal */
* {
  box-sizing: border-box;
}

/* Responsive para móviles */
@media (max-width: 768px) {
  .card-header h3 {
    font-size: 1.1rem;
    margin-bottom: 1rem;
  }

  .form-group {
    margin-bottom: 1rem;
  }

  .form-group input {
    font-size: 16px; /* Previene zoom en iOS */
    padding: 0.625rem;
  }

  .w-full {
    font-size: 16px;
    padding: 0.75rem;
  }
}

@media (max-width: 480px) {
  .card-header h3 {
    font-size: 1rem;
  }

  .form-group input {
    padding: 0.5rem;
  }
}
</style>