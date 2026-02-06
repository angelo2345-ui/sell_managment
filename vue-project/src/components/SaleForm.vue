<script setup>
import { ref, computed, onMounted } from 'vue';
import axios from 'axios';

// Eventos para comunicar al padre
const emit = defineEmits(['sale-completed']);

// ESTADO: Datos externos (Listas para los desplegables)
const clients = ref([]);
const products = ref([]);

// ESTADO: Datos del formulario actual
const selectedClientId = ref('');
const selectedProductId = ref('');
const quantity = ref(1);

// ESTADO: Carrito de compras (Lista de productos agregados)
const cart = ref([]);

// 1. Cargar Clientes y Productos al iniciar
onMounted(async () => {
  try {
    const clientsRes = await axios.get('http://localhost:5227/api/clients');
    clients.value = clientsRes.data;

    const productsRes = await axios.get('http://localhost:5227/api/products');
    products.value = productsRes.data;
  } catch (error) {
    console.error("Error cargando datos:", error);
    alert("Error cargando listas. Asegúrate de que el backend esté corriendo.");
  }
});

// 2. Función para agregar producto al carrito local
const addToCart = () => {
  // Buscamos el producto completo basado en el ID seleccionado
  const product = products.value.find(p => p.id === selectedProductId.value);
  
  if (!product) return;

  // Creamos el objeto detalle
  const item = {
    productId: product.id,
    productName: product.name,
    quantity: quantity.value,
    unitPrice: product.price,
    subtotal: product.price * quantity.value
  };

  // Lo agregamos al carrito
  cart.value.push(item);
  
  // Limpiamos los campos de selección de producto
  selectedProductId.value = '';
  quantity.value = 1;
};

// 3. Propiedad computada para el Total General
const total = computed(() => {
  return cart.value.reduce((sum, item) => sum + item.subtotal, 0);
});

// 4. Enviar la Venta completa al Backend
const submitSale = async () => {
  if (!selectedClientId.value || cart.value.length === 0) {
    alert("Por favor selecciona un cliente y agrega al menos un producto.");
    return;
  }

  // Armamos el objeto tal cual lo espera el Backend (SaleRequest)
  const saleData = {
    clientId: selectedClientId.value,
    total: total.value,
    details: cart.value.map(item => ({
      productId: item.productId,
      quantity: item.quantity,
      unitPrice: item.unitPrice
    }))
  };

  try {
    await axios.post('http://localhost:5227/api/sales', saleData);
    alert("¡Venta registrada con éxito!");
    
    // Reseteamos el formulario
    cart.value = [];
    selectedClientId.value = '';
    emit('sale-completed'); 
  } catch (error) {
    console.error("Error registrando venta:", error);
    alert("Ocurrió un error al registrar la venta.");
  }
};
</script>

<template>
  <div class="sale-form-container">
    <h3>Registrar Nueva Venta</h3>
    
    <!-- SECCIÓN 1: CABECERA (CLIENTE) -->
    <div class="form-group">
      <label>Cliente:</label>
      <select v-model="selectedClientId" class="form-control">
        <option disabled value="">Seleccione un cliente</option>
        <option v-for="client in clients" :key="client.id" :value="client.id">
          {{ client.name }}
        </option>
      </select>
    </div>

    <!-- SECCIÓN 2: AGREGAR PRODUCTOS -->
    <div class="product-selection-box">
      <h4>Agregar Productos</h4>
      <div class="row">
        <div class="col">
          <label>Producto:</label>
          <select v-model="selectedProductId" class="form-control">
            <option disabled value="">Seleccione un producto</option>
            <option v-for="product in products" :key="product.id" :value="product.id">
              {{ product.name }} - ${{ product.price }}
            </option>
          </select>
        </div>
        <div class="col-small">
          <label>Cant:</label>
          <input type="number" v-model="quantity" min="1" class="form-control" />
        </div>
        <div class="col-btn">
          <button @click="addToCart" :disabled="!selectedProductId" class="btn-add">
            + Agregar
          </button>
        </div>
      </div>
    </div>

    <!-- SECCIÓN 3: TABLA DE DETALLES (CARRITO) -->
    <table v-if="cart.length > 0" class="cart-table">
      <thead>
        <tr>
          <th>Producto</th>
          <th>Cant.</th>
          <th>Precio</th>
          <th>Subtotal</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(item, index) in cart" :key="index">
          <td>{{ item.productName }}</td>
          <td>{{ item.quantity }}</td>
          <td>${{ item.unitPrice }}</td>
          <td>${{ item.subtotal }}</td>
        </tr>
      </tbody>
      <tfoot>
        <tr>
          <td colspan="3" class="text-right"><strong>TOTAL:</strong></td>
          <td class="total-cell"><strong>${{ total }}</strong></td>
        </tr>
      </tfoot>
    </table>

    <!-- BOTÓN FINAL -->
    <button v-if="cart.length > 0" @click="submitSale" class="btn-submit">
      CONFIRMAR VENTA
    </button>
  </div>
</template>

<style scoped>
.sale-form-container {
  background: #fff;
  padding: 20px;
  border: 1px solid #ddd;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
}

.form-group {
  margin-bottom: 15px;
}

.form-control {
  width: 100%;
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
}

.product-selection-box {
  background-color: #f8f9fa;
  padding: 15px;
  border-radius: 6px;
  margin-bottom: 20px;
}

.row {
  display: flex;
  gap: 10px;
  align-items: flex-end;
}

.col { flex: 1; }
.col-small { width: 80px; }
.col-btn { padding-bottom: 2px; }

.btn-add {
  background-color: #3498db;
  color: white;
  border: none;
  padding: 9px 15px;
  border-radius: 4px;
  cursor: pointer;
}

.cart-table {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 20px;
}

.cart-table th, .cart-table td {
  border: 1px solid #eee;
  padding: 10px;
  text-align: left;
}

.cart-table th { background-color: #f1f1f1; }
.text-right { text-align: right; }
.total-cell { color: #27ae60; font-size: 1.2em; }

.btn-submit {
  width: 100%;
  padding: 12px;
  background-color: #27ae60;
  color: white;
  border: none;
  font-size: 1.1em;
  font-weight: bold;
  border-radius: 4px;
  cursor: pointer;
}
.btn-submit:hover { background-color: #219150; }
</style>