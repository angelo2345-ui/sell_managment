<script setup>
import { ref } from 'vue';
import productService from '../services/productService';

const emit = defineEmits(['product-created']);

const product = ref({
  name: '',
  description: '',
  price: 0,
  stock: 0
});

const isSubmitting = ref(false);
const message = ref('');
const isError = ref(false);

const submitForm = async () => {
  isSubmitting.value = true;
  message.value = '';
  isError.value = false;

  try {
    await productService.createProduct(product.value);
    
    message.value = '¡Producto guardado con éxito!';
    product.value = { name: '', description: '', price: 0, stock: 0 };
    emit('product-created');
    setTimeout(() => message.value = '', 3000);

  } catch (error) {
    console.error(error);
    isError.value = true;
    message.value = 'Error al guardar el producto.';
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<template>
  <div class="card">
    <div class="card-header">
      <h3>Nuevo Producto</h3>
    </div>
    
    <form @submit.prevent="submitForm">
      <div class="form-group">
        <label>Nombre del Producto</label>
        <input v-model="product.name" required placeholder="Ej: Laptop Gamer" />
      </div>

      <div class="form-group">
        <label>Descripción</label>
        <textarea v-model="product.description" placeholder="Detalles del producto..." rows="2"></textarea>
      </div>

      <div class="form-row">
        <div class="form-group half">
          <label>Precio ($)</label>
          <input type="number" v-model="product.price" required min="0" step="0.01" />
        </div>

        <div class="form-group half">
          <label>Stock Inicial</label>
          <input type="number" v-model="product.stock" required min="0" />
        </div>
      </div>

      <div v-if="message" :class="['alert', isError ? 'alert-error' : 'alert-success']" role="alert">
        {{ message }}
      </div>

      <button type="submit" class="btn btn-primary w-full" :disabled="isSubmitting">
        {{ isSubmitting ? 'Guardando...' : 'Guardar Producto' }}
      </button>
    </form>
  </div>
</template>

<style scoped>
.card-header h3 {
  margin-bottom: 1.5rem;
  color: var(--text-main);
  font-size: 1.25rem;
  border-bottom: 1px solid var(--border-color);
  padding-bottom: 1rem;
}

.form-row {
  display: flex;
  gap: 1rem;
}

.half {
  flex: 1;
}

.w-full {
  width: 100%;
  margin-top: 1rem;
}
</style>
