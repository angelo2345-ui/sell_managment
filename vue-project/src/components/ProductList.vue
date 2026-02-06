<script setup>
import { ref, onMounted } from 'vue';
import productService from '../services/productService';

// Aquí guardaremos la lista de productos que nos traiga el backend
const products = ref([]);
const loading = ref(true);
const error = ref(null);

// Función para cargar los productos
const loadProducts = async () => {
  try {
    loading.value = true;
    const response = await productService.getProducts();
    products.value = response.data;
  } catch (err) {
    console.error("Error cargando productos:", err);
    error.value = "No se pudieron cargar los productos. ¿Está encendido el backend?";
  } finally {
    loading.value = false;
  }
};

// Cuando el componente "nace" (se monta), cargamos los productos
onMounted(() => {
  loadProducts();
});
</script>

<template>
  <div class="container">
    <h2>📦 Lista de Productos</h2>

    <div v-if="loading" class="loading">Cargando productos...</div>
    
    <div v-else-if="error" class="error">{{ error }}</div>

    <div v-else>
      <div v-if="products.length === 0" class="empty">
        No hay productos registrados aún.
      </div>

      <table v-else class="product-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Descripción</th>
            <th>Precio</th>
            <th>Stock</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="product in products" :key="product.id">
            <td>{{ product.id }}</td>
            <td>{{ product.name }}</td>
            <td>{{ product.description }}</td>
            <td>${{ product.price }}</td>
            <td>{{ product.stock }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.container {
  max-width: 800px;
  margin: 0 auto;
  padding: 20px;
  font-family: Arial, sans-serif;
}

h2 {
  color: #2c3e50;
  text-align: center;
}

.loading {
  color: #666;
  text-align: center;
  padding: 20px;
}

.error {
  color: red;
  text-align: center;
  padding: 20px;
  background-color: #ffe6e6;
  border-radius: 4px;
}

.empty {
  text-align: center;
  color: #888;
  padding: 20px;
  border: 1px dashed #ccc;
  border-radius: 4px;
}

.product-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 20px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.product-table th, .product-table td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid #ddd;
}

.product-table th {
  background-color: #42b983;
  color: white;
}

.product-table tr:hover {
  background-color: #f5f5f5;
}
</style>