<script setup>
import { ref, computed, onMounted } from 'vue';
import clientService from '../services/clientService';
import saleService from '../services/saleService';
import productService from '../services/productService';

const emit = defineEmits(['sale-completed']);

const clients = ref([]);
const products = ref([]);

const selectedClientId = ref('');
const selectedProductId = ref('');
const quantity = ref(1);

const cart = ref([]);
const errorMessage = ref('');
const successMessage = ref('');

onMounted(async () => {
  try {
    const clientsData = await clientService.getClients();
    clients.value = clientsData;

    const productsResponse = await productService.getProducts(); 
    products.value = productsResponse.data.data || productsResponse.data;
  } catch (error) {
    console.error("Error cargando datos:", error);
    errorMessage.value = "Error cargando listas. Asegúrate de que el backend esté corriendo.";
  }
});

const addToCart = () => {
  errorMessage.value = '';
  
  const product = products.value.find(p => p.id === selectedProductId.value);
  if (!product) return;

  if (quantity.value > product.stock) {
    errorMessage.value = `Solo hay ${product.stock} unidades disponibles de ${product.name}.`;
    return;
  }

  // Verificamos si ya tenemos este producto en el carrito
  const existingItemIndex = cart.value.findIndex(item => item.productId === product.id);
  if (existingItemIndex !== -1) {
    const newQuantity = cart.value[existingItemIndex].quantity + quantity.value;
    if (newQuantity > product.stock) {
      errorMessage.value = `No puedes agregar más. Stock total: ${product.stock}.`;
      return;
    }
    cart.value[existingItemIndex].quantity = newQuantity;
    cart.value[existingItemIndex].subtotal = newQuantity * product.price;
  } else {
    cart.value.push({
      productId: product.id,
      productName: product.name,
      quantity: quantity.value,
      unitPrice: product.price,
      subtotal: product.price * quantity.value
    });
  }
  
  selectedProductId.value = '';
  quantity.value = 1;
};

const removeItem = (index) => {
  cart.value.splice(index, 1);
};

const total = computed(() => {
  return cart.value.reduce((sum, item) => sum + item.subtotal, 0);
});

const submitSale = async () => {
  errorMessage.value = '';
  successMessage.value = '';

  if (!selectedClientId.value) {
    errorMessage.value = "Por favor selecciona un cliente.";
    return;
  }
  if (cart.value.length === 0) {
    errorMessage.value = "Agrega al menos un producto al carrito.";
    return;
  }

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
    await saleService.createSale(saleData);
    successMessage.value = "¡Venta registrada con éxito!";
    
    // Limpiamos el formulario para la siguiente venta
    cart.value = [];
    selectedClientId.value = '';
    
    emit('sale-completed');
    setTimeout(() => successMessage.value = '', 3000);
  } catch (error) {
    console.error(error);
    errorMessage.value = "Hubo un error al registrar la venta.";
  }
};
</script>

<template>
  <div class="sale-grid">
    <!-- Panel Izquierdo: Formulario de Selección -->
    <div class="card form-panel">
      <h3>Nueva Venta</h3>
      
      <div v-if="errorMessage" class="alert alert-error" role="alert">
        {{ errorMessage }}
      </div>
      <div v-if="successMessage" class="alert alert-success" role="alert">
        {{ successMessage }}
      </div>

      <div class="form-group">
        <label for="client-select">Cliente</label>
        <select id="client-select" v-model="selectedClientId" class="form-control">
          <option value="" disabled>Seleccione un cliente</option>
          <option v-for="client in clients" :key="client.id" :value="client.id">
            {{ client.name }}
          </option>
        </select>
      </div>

      <div class="divider"></div>

      <h4>Agregar Producto</h4>
      <div class="add-product-row">
        <div class="form-group product-select">
          <label for="product-select">Producto</label>
          <select id="product-select" v-model="selectedProductId" class="form-control">
            <option value="" disabled>Seleccione...</option>
            <option v-for="product in products" :key="product.id" :value="product.id">
              {{ product.name }} (${{ product.price }}) - Stock: {{ product.stock }}
            </option>
          </select>
        </div>
        
        <div class="form-group quantity-input">
          <label for="quantity-input">Cant.</label>
          <input 
            id="quantity-input"
            type="number" 
            v-model.number="quantity" 
            min="1" 
            class="form-control"
          />
        </div>
      </div>

      <button @click="addToCart" class="btn btn-primary btn-add-full" :disabled="!selectedProductId" title="Agregar">
        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"></line><line x1="5" y1="12" x2="19" y2="12"></line></svg>
        Agregar Producto
      </button>
    </div>

    <!-- Panel Derecho: Carrito -->
    <div class="card cart-panel">
      <div class="cart-header">
        <h3>Carrito de Compra</h3>
        <span class="badge">{{ cart.length }} items</span>
      </div>

      <div v-if="cart.length === 0" class="empty-cart">
        <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" class="empty-icon"><circle cx="9" cy="21" r="1"></circle><circle cx="20" cy="21" r="1"></circle><path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6"></path></svg>
        <p>El carrito está vacío</p>
      </div>

      <div v-else class="cart-items">
        <div v-for="(item, index) in cart" :key="index" class="cart-item">
          <div class="item-info">
            <span class="item-name">{{ item.productName }}</span>
            <span class="item-details">{{ item.quantity }} x ${{ item.unitPrice.toFixed(2) }}</span>
          </div>
          <div class="item-total">
            ${{ item.subtotal.toFixed(2) }}
            <button @click="removeItem(index)" class="btn-remove" title="Quitar">×</button>
          </div>
        </div>
      </div>

      <div class="cart-footer">
        <div class="total-row">
          <span>Total a Pagar:</span>
          <span class="total-amount">${{ total.toFixed(2) }}</span>
        </div>
        <button @click="submitSale" class="btn btn-success btn-block" :disabled="cart.length === 0">
          Confirmar Venta
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.sale-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2rem;
}

.form-panel h3, .cart-panel h3 {
  margin-bottom: 1.5rem;
  font-size: 1.25rem;
  color: var(--text-main);
}

.divider {
  height: 1px;
  background: var(--border-color);
  margin: 1.5rem 0;
}

.add-product-row {
  display: flex;
  gap: 10px;
  align-items: flex-end;
}

.product-select {
  flex: 1;
}

.quantity-input {
  width: 80px;
}

.btn-add-full {
  width: 100%;
  margin-top: 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 0.75rem;
  font-weight: 600;
}

@media (max-width: 768px) {
  .sale-grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
  
  .form-panel, .cart-panel {
    width: 100%;
  }

  /* Stack inputs vertically on mobile for better UX */
  .add-product-row {
    flex-direction: column;
    align-items: stretch;
    gap: 1rem;
  }
  
  .product-select {
    width: 100%;
    margin-bottom: 0;
  }
  
  .quantity-input {
    width: 100%;
  }
}

/* Cart Styles */
.cart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid var(--border-color);
}

.empty-cart {
  text-align: center;
  padding: 3rem 0;
  color: var(--text-muted);
  font-size: 1rem;
}

.cart-items {
  max-height: 400px;
  overflow-y: auto;
  margin-bottom: 1.5rem;
}

.cart-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 0;
  border-bottom: 1px dashed var(--border-color);
}

.item-info {
  display: flex;
  flex-direction: column;
}

.item-name {
  font-weight: 500;
  color: var(--text-main);
}

.item-details {
  font-size: 0.85rem;
  color: var(--text-muted);
}

.item-total {
  font-family: var(--font-mono);
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 10px;
}

.btn-remove {
  background: none;
  border: none;
  color: var(--danger);
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-remove:hover {
  background-color: #fee2e2;
}

.cart-footer {
  border-top: 2px solid var(--border-color);
  padding-top: 1rem;
}

.total-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 1.25rem;
  font-weight: 700;
  margin-bottom: 1.5rem;
}

.total-amount {
  color: var(--primary);
}
</style>
