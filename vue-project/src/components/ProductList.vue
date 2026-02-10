<script setup>
import { ref, onMounted } from 'vue';
import productService from '../services/productService';

const products = ref([]);
const loading = ref(true);
const error = ref(null);

// Estado para el modal de recarga
const showRestockModal = ref(false);
const selectedProduct = ref(null);
const stockToAdd = ref(1);

// Estado para el modal de edición
const showEditModal = ref(false);
const editingProduct = ref({ id: 0, name: '', description: '', price: 0, stock: 0 });

// Estado para el modal de eliminación
const showDeleteModal = ref(false);
const productToDelete = ref(null);

const loadProducts = async () => {
  try {
    loading.value = true;
    const response = await productService.getProducts();
    products.value = response.data.data;
  } catch (err) {
    console.error("Error cargando productos:", err);
    error.value = "No se pudieron cargar los productos. ¿Está encendido el backend?";
  } finally {
    loading.value = false;
  }
};

const openDeleteModal = (product) => {
  productToDelete.value = product;
  showDeleteModal.value = true;
};

const closeDeleteModal = () => {
  showDeleteModal.value = false;
  productToDelete.value = null;
};

const executeDelete = async () => {
  if (!productToDelete.value) return;
  
  const id = productToDelete.value.id;
  console.log(`[Frontend] Ejecutando eliminación de producto ID: ${id}`);
  
  try {
    await productService.deleteProduct(id);
    
    console.log('[Frontend] Eliminación exitosa. Recargando lista...');
    await loadProducts();
    // alert('Producto eliminado correctamente.'); // Opcional, ya se ve visualmente que desaparece
    closeDeleteModal();
  } catch (err) {
    console.error('[Frontend] Error al eliminar:', err);
    
    let message = 'Error desconocido al eliminar.';
    
    if (err.response) {
      message = err.response.data?.message || `Error del servidor (${err.response.status})`;
    } else if (err.request) {
      message = 'No se recibió respuesta del servidor. Verifica la conexión.';
    } else {
      message = err.message;
    }

    alert(`⚠️ ${message}`);
    closeDeleteModal(); // Cerramos el modal incluso si falla para no bloquear
  }
};

const openRestockModal = (product) => {
  selectedProduct.value = { ...product }; // Copia para no mutar directo
  stockToAdd.value = 10; // Valor por defecto sugerido
  showRestockModal.value = true;
};

const closeRestockModal = () => {
  showRestockModal.value = false;
  selectedProduct.value = null;
  stockToAdd.value = 1;
};

// Funciones para Edición
const openEditModal = (product) => {
  editingProduct.value = { ...product };
  showEditModal.value = true;
};

const closeEditModal = () => {
  showEditModal.value = false;
  editingProduct.value = { id: 0, name: '', description: '', price: 0, stock: 0 };
};

const saveProductChanges = async () => {
  try {
    await productService.updateProduct(editingProduct.value.id, editingProduct.value);
    closeEditModal();
    loadProducts();
    alert('Producto actualizado con éxito.');
  } catch (err) {
    console.error(err);
    alert('Error al actualizar el producto.');
  }
};

const confirmRestock = async () => {
  if (!selectedProduct.value || stockToAdd.value < 1) return;

  try {
    const updatedProduct = {
      ...selectedProduct.value,
      stock: selectedProduct.value.stock + stockToAdd.value
    };

    await productService.updateProduct(selectedProduct.value.id, updatedProduct);
    closeRestockModal();
    loadProducts(); // Recargar lista para ver cambios
  } catch (err) {
    alert('Error al actualizar el stock.');
    console.error(err);
  }
};

onMounted(() => {
  loadProducts();
});
</script>

<template>
  <div class="card">
    <div class="card-header">
      <h3>Inventario Actual</h3>
      <span class="badge" v-if="products.length">{{ products.length }} items</span>
    </div>

    <div v-if="loading" class="state-message">
      <div class="spinner"></div>
      <p>Cargando productos...</p>
    </div>
    
    <div v-else-if="error" class="alert alert-error" role="alert">
      {{ error }}
    </div>

    <div v-else>
      <div v-if="products.length === 0" class="empty-state">
        <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" class="empty-icon"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"></path><polyline points="3.27 6.96 12 12.01 20.73 6.96"></polyline><line x1="12" y1="22.08" x2="12" y2="12"></line></svg>
        <p>No hay productos registrados aún.</p>
      </div>

      <div v-else class="table-responsive">
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Producto</th>
              <th>Descripción</th>
              <th>Precio</th>
              <th>Stock</th>
              <th>Estado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="product in products" :key="product.id">
              <td class="text-muted">#{{ product.id }}</td>
              <td class="font-bold">{{ product.name }}</td>
              <td class="text-sm text-muted">{{ product.description }}</td>
              <td class="font-mono text-primary">${{ product.price.toFixed(2) }}</td>
              <td>
                <span :class="['badge-stock', product.stock > 5 ? 'in-stock' : 'low-stock']">
                  {{ product.stock }} un.
                </span>
              </td>
              <td>
                <span class="status-dot" :class="{ active: product.stock > 0 }"></span>
                {{ product.stock > 0 ? 'Disponible' : 'Agotado' }}
              </td>
              <td>
                <div class="action-buttons">
                  <button @click="openRestockModal(product)" class="btn-icon btn-restock" title="Recargar Stock">
                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><polyline points="7 10 12 15 17 10"></polyline><line x1="12" y1="15" x2="12" y2="3"></line></svg>
                  </button>
                  <button @click="openEditModal(product)" class="btn-icon btn-edit" title="Editar">
                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path></svg>
                  </button>
                  <button @click="openDeleteModal(product)" class="btn-icon btn-delete" title="Eliminar">
                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path><line x1="10" y1="11" x2="10" y2="17"></line><line x1="14" y1="11" x2="14" y2="17"></line></svg>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>

  <!-- Modal de Recarga -->
  <div v-if="showRestockModal" class="modal-overlay">
    <div class="modal-content">
      <h3>Recargar Stock</h3>
      <p>Producto: <strong>{{ selectedProduct.name }}</strong></p>
      <p>Stock actual: {{ selectedProduct.stock }}</p>
      
      <div class="form-group">
        <label>Cantidad a agregar:</label>
        <input type="number" v-model.number="stockToAdd" min="1" class="form-control" />
      </div>

      <div class="modal-actions">
        <button @click="closeRestockModal" class="btn-cancel">Cancelar</button>
        <button @click="confirmRestock" class="btn-confirm">Guardar</button>
      </div>
    </div>
  </div>

  <!-- Modal de Edición -->
  <div v-if="showEditModal" class="modal-overlay">
    <div class="modal-content">
      <h3>Editar Producto</h3>
      
      <div class="form-group">
        <label>Nombre:</label>
        <input type="text" v-model="editingProduct.name" class="form-control" />
      </div>

      <div class="form-group">
        <label>Descripción:</label>
        <input type="text" v-model="editingProduct.description" class="form-control" />
      </div>

      <div class="form-group">
        <label>Precio ($):</label>
        <input type="number" v-model.number="editingProduct.price" class="form-control" step="0.01" />
      </div>

      <div class="modal-actions">
        <button @click="closeEditModal" class="btn-cancel">Cancelar</button>
        <button @click="saveProductChanges" class="btn-confirm">Guardar Cambios</button>
      </div>
    </div>
  </div>

  <!-- Modal de Confirmación de Eliminación -->
  <div v-if="showDeleteModal" class="modal-overlay">
    <div class="modal-content">
      <h3>¿Eliminar Producto?</h3>
      <p>Estás a punto de eliminar: <strong>{{ productToDelete?.name }}</strong></p>
      <p class="text-sm text-muted">Esta acción moverá el producto a inactivo si tiene historial de ventas, o lo borrará permanentemente si es nuevo.</p>
      
      <div class="modal-actions">
        <button @click="closeDeleteModal" class="btn-cancel">Cancelar</button>
        <button @click="executeDelete" class="btn-confirm btn-danger">Sí, Eliminar</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.card-header h3 {
  font-size: 1.25rem;
  color: var(--text-main);
}

.badge {
  background: var(--bg-body);
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--text-muted);
}

.state-message {
  text-align: center;
  padding: 3rem;
  color: var(--text-muted);
}

.spinner {
  border: 3px solid rgba(0,0,0,0.1);
  width: 36px;
  height: 36px;
  border-radius: 50%;
  border-left-color: var(--primary);
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.empty-state {
  text-align: center;
  padding: 3rem;
  border: 2px dashed var(--border-color);
  border-radius: 8px;
  color: var(--text-muted);
}

.text-muted { color: var(--text-muted); }
.text-sm { font-size: 0.875rem; }
.font-bold { font-weight: 600; }
.font-mono { font-family: monospace; font-size: 1rem; }
.text-primary { color: var(--primary); font-weight: 700; }

.badge-stock {
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.85rem;
  font-weight: 600;
}

.in-stock {
  background-color: #ecfdf5;
  color: #059669;
}

.low-stock {
  background-color: #fef2f2;
  color: #dc2626;
}

.status-dot {
  display: inline-block;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background-color: #d1d5db;
  margin-right: 6px;
}

.status-dot.active {
  background-color: #10b981;
}

/* Scroll horizontal para móviles */
.table-responsive {
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
}

/* Acciones */
.action-buttons {
  display: flex;
  gap: 0.5rem;
}

.btn-icon {
  background: none;
  border: none;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 4px;
  transition: background 0.2s;
}

.btn-restock {
  color: var(--secondary);
}
.btn-restock:hover {
  background-color: rgba(6, 182, 212, 0.1);
}

.btn-delete {
  color: #dc2626;
}
.btn-delete:hover {
  background-color: #fee2e2;
}

/* Modal */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0,0,0,0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  width: 90%;
  max-width: 400px;
  box-shadow: 0 4px 6px rgba(0,0,0,0.1);
}

.modal-content h3 {
  margin-top: 0;
  color: var(--text-main);
}

.form-group {
  margin: 1.5rem 0;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 600;
}

.form-control {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 1rem;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
}

.btn-cancel, .btn-confirm {
  padding: 0.5rem 1rem;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  border: none;
}

.btn-cancel {
  background: #f3f4f6;
  color: #4b5563;
}

.btn-confirm {
  background: var(--primary);
  color: white;
}

.btn-danger {
  background-color: #dc2626;
  color: white;
}
.btn-danger:hover {
  background-color: #b91c1c;
}
</style>
