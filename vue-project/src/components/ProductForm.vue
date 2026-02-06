<script setup>
import { ref } from 'vue';
import productService from '../services/productService';

// Definimos los eventos que este componente puede "emitir" hacia arriba (al padre)
const emit = defineEmits(['product-created']);

// Variables reactivas para el formulario
const product = ref({
  name: '',
  description: '',
  price: 0,
  stock: 0
});

const isSubmitting = ref(false);
const message = ref('');

// Función para enviar el formulario
const submitForm = async () => {
  isSubmitting.value = true;
  message.value = '';

  try {
    // Enviamos los datos al backend
    await productService.createProduct(product.value);
    
    message.value = '¡Producto guardado con éxito!';
    
    // Limpiamos el formulario
    product.value = { name: '', description: '', price: 0, stock: 0 };

    // Avisamos al componente padre (App.vue) que ya se creó uno nuevo
    emit('product-created');

  } catch (error) {
    console.error(error);
    message.value = 'Error al guardar el producto.';
  } finally {
    isSubmitting.value = false;
    // Borrar el mensaje de éxito después de 3 segundos
    setTimeout(() => message.value = '', 3000);
  }
};
</script>

<template>
  <div class="form-container">
    <h3>➕ Nuevo Producto</h3>
    
    <form @submit.prevent="submitForm" class="product-form">
      <div class="form-group">
        <label>Nombre:</label>
        <input v-model="product.name" required placeholder="Ej: Laptop Gamer" />
      </div>

      <div class="form-group">
        <label>Descripción:</label>
        <input v-model="product.description" placeholder="Ej: 16GB RAM, 512GB SSD" />
      </div>

      <div class="form-row">
        <div class="form-group">
          <label>Precio:</label>
          <input type="number" v-model="product.price" required min="0" step="0.01" />
        </div>

        <div class="form-group">
          <label>Stock:</label>
          <input type="number" v-model="product.stock" required min="0" />
        </div>
      </div>

      <button type="submit" :disabled="isSubmitting">
        {{ isSubmitting ? 'Guardando...' : 'Guardar Producto' }}
      </button>

      <p v-if="message" :class="{'success': message.includes('éxito'), 'error': message.includes('Error')}">
        {{ message }}
      </p>
    </form>
  </div>
</template>

<style scoped>
.form-container {
  max-width: 500px;
  margin: 20px auto;
  padding: 20px;
  background: #f9f9f9;
  border-radius: 8px;
  box-shadow: 0 2px 5px rgba(0,0,0,0.1);
}

h3 {
  text-align: center;
  color: #2c3e50;
  margin-bottom: 20px;
}

.product-form {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.form-group {
  display: flex;
  flex-direction: column;
}

.form-row {
  display: flex;
  gap: 15px;
}

.form-row .form-group {
  flex: 1;
}

label {
  font-weight: bold;
  margin-bottom: 5px;
  color: #555;
}

input {
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 16px;
}

input:focus {
  border-color: #42b983;
  outline: none;
}

button {
  background-color: #42b983;
  color: white;
  padding: 12px;
  border: none;
  border-radius: 4px;
  font-size: 16px;
  cursor: pointer;
  transition: background 0.3s;
}

button:hover {
  background-color: #3aa876;
}

button:disabled {
  background-color: #a8d5c2;
  cursor: not-allowed;
}

.success {
  color: green;
  text-align: center;
  font-weight: bold;
}

.error {
  color: red;
  text-align: center;
  font-weight: bold;
}
</style>