<script setup>
import { ref } from 'vue';
import ProductList from './components/ProductList.vue';
import ProductForm from './components/ProductForm.vue';
import ClientList from './components/ClientList.vue';
import SaleForm from './components/SaleForm.vue';
import Login from './components/Login.vue';

// Estado para controlar si el usuario está autenticado
const isAuthenticated = ref(false);

// Estado para controlar qué vista se muestra: 'products' | 'clients' | 'sales'
const currentView = ref('products');

// Claves para forzar recargas si es necesario
const productKey = ref(0);
const clientKey = ref(0);

const reloadProducts = () => productKey.value++;
const reloadClients = () => clientKey.value++;
</script>

<template>
  <!-- Login -->
  <Login v-if="!isAuthenticated"
  @login-success="isAuthenticated = true" />
  <div v-else>
    <header>
      <h1>Sistema de Ventas</h1>
      <nav>
        <!-- Botones para navegar -->
        <button 
          @click="currentView = 'products'" 
          :class="{ active: currentView === 'products' }"
        >Productos</button>
        
        <button 
          @click="currentView = 'clients'" 
          :class="{ active: currentView === 'clients' }"
        >Clientes</button>
        
        <button 
          @click="currentView = 'sales'" 
          :class="{ active: currentView === 'sales' }"
        >Ventas</button>
      </nav>
    </header>

    <main>
        <!-- VISTA DE PRODUCTOS -->
        <div v-if="currentView === 'products'">
          <h2>Gestión de Productos</h2>
          <ProductForm @product-created="reloadProducts" />
          <hr />
          <ProductList :key="productKey" />
        </div>

        <!-- VISTA DE CLIENTES -->
        <div v-if="currentView === 'clients'">
          <h2>Gestión de Clientes</h2>
          <!-- Aquí podrías agregar un ClientForm similar a ProductForm en el futuro -->
          <ClientList :key="clientKey" />
        </div>

        <!-- VISTA DE VENTAS -->
        <!-- VISTA DE VENTAS -->
        <div v-if="currentView === 'sales'">
          <!-- Cuando la venta termina, podemos recargar productos por si bajó el stock (opcional) -->
          <SaleForm @sale-completed="reloadProducts" />
        </div>
    </main>
  </div>
</template>

<style scoped>
header {
  background-color: #2c3e50;
  color: white;
  padding: 1rem;
  text-align: center;
}

nav {
  margin-top: 10px;
  display: flex;
  justify-content: center;
  gap: 10px;
}

button {
  padding: 8px 16px;
  border: none;
  background-color: #34495e;
  color: white;
  cursor: pointer;
  border-radius: 4px;
}

button:hover {
  background-color: #41b883;
}

button.active {
  background-color: #41b883;
  font-weight: bold;
}

main {
  padding: 20px;
  max-width: 800px;
  margin: 0 auto;
}
</style>