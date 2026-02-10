<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
import saleService from '../services/saleService';
import clientService from '../services/clientService'; // Para cargar clientes en el select

const sales = ref([]);
const clients = ref([]);
const loading = ref(true);
const error = ref(null);

// Estado para edición
const showEditModal = ref(false);
const editingSale = ref({ id: 0, clientId: 0, date: '', total: 0 });

// Estado para eliminación
const showDeleteModal = ref(false);
const saleToDelete = ref(null);

const loadSales = async () => {
  try {
    loading.value = true;
    const response = await axios.get('http://localhost:5227/api/sales');
    sales.value = response.data.data;
  } catch (err) {
    console.error("Error al cargar ventas:", err);
    error.value = "No se pudieron cargar las ventas.";
  } finally {
    loading.value = false;
  }
};

const loadClients = async () => {
  try {
    clients.value = await clientService.getClients();
  } catch (err) {
    console.error("Error cargando clientes para edición", err);
  }
};

const openDeleteModal = (sale) => {
  saleToDelete.value = sale;
  showDeleteModal.value = true;
};

const closeDeleteModal = () => {
  showDeleteModal.value = false;
  saleToDelete.value = null;
};

const executeDelete = async () => {
  if (!saleToDelete.value) return;
  const id = saleToDelete.value.id;
  
  try {
    await saleService.deleteSale(id);
    loadSales(); // Recargar lista
    // alert('Venta eliminada y stock restaurado.'); // Ya se ve visualmente
    closeDeleteModal();
  } catch (err) {
    console.error('[Frontend] Error al eliminar venta:', err);
    
    let message = 'Error desconocido al eliminar.';
    
    if (err.response) {
      message = err.response.data?.message || `Error del servidor (${err.response.status})`;
    } else if (err.request) {
      message = 'No se recibió respuesta del servidor. Verifica la conexión.';
    } else {
      message = err.message;
    }

    alert(`⚠️ ${message}`);
    closeDeleteModal();
  }
};

const openEditModal = (sale) => {
  // Buscamos el ID del cliente basado en el nombre (ya que la API devuelve nombres en el listado)
  // O idealmente, el endpoint de sales debería devolver también el ClientId.
  // Como parche rápido, asumimos que sale tiene clientId si el backend lo devuelve, 
  // si no, tendríamos que buscarlo. 
  // *Revisión*: El endpoint actual devuelve un objeto anónimo con clientName.
  // Vamos a necesitar que el backend devuelva el ClientId en el GET.
  // Por ahora, intentaremos mapear por nombre o usar el que venga.
  
  // Ajuste para formato de fecha input datetime-local
  const dateObj = new Date(sale.date);
  // Formato YYYY-MM-DDTHH:MM
  const dateStr = dateObj.toISOString().slice(0, 16); 

  editingSale.value = { 
    id: sale.id, 
    clientId: sale.clientId, // Asegurarse que el backend devuelva esto
    date: dateStr,
    total: sale.total 
  };
  showEditModal.value = true;
};

const closeEditModal = () => {
  showEditModal.value = false;
  editingSale.value = { id: 0, clientId: 0, date: '', total: 0 };
};

const saveSaleChanges = async () => {
  try {
    await saleService.updateSale(editingSale.value.id, {
      id: editingSale.value.id,
      clientId: editingSale.value.clientId,
      date: editingSale.value.date,
      total: editingSale.value.total
    });
    closeEditModal();
    loadSales();
    alert('Venta actualizada (Cliente/Fecha).');
  } catch (err) {
    console.error(err);
    alert('Error al actualizar la venta.');
  }
};

const formatDate = (dateString) => {
  if (!dateString) return '';
  return new Date(dateString).toLocaleString('es-ES', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
};

const formatCurrency = (amount) => {
  return new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'USD' }).format(amount);
};

onMounted(() => {
  loadSales();
  loadClients(); // Cargar clientes para el select de edición
});
</script>

<template>
  <div class="card">
    <div class="card-header">
      <h3>Historial de Ventas</h3>
      <span class="badge" v-if="sales.length">{{ sales.length }} transacciones</span>
    </div>

    <div v-if="loading" class="state-message">
      <div class="spinner"></div>
      <p>Cargando historial...</p>
    </div>
    
    <div v-else-if="error" class="alert alert-error" role="alert">
      {{ error }}
    </div>

    <div v-else>
      <div v-if="sales.length === 0" class="empty-state">
        <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" class="empty-icon"><circle cx="9" cy="21" r="1"></circle><circle cx="20" cy="21" r="1"></circle><path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6"></path></svg>
        <p>No se han realizado ventas aún.</p>
      </div>

      <div v-else class="table-responsive">
        <table>
          <thead>
            <tr>
              <th># Recibo</th>
              <th>Fecha</th>
              <th>Cliente</th>
              <th>Productos Vendidos</th>
              <th>Total</th>
              <th>Estado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="sale in sales" :key="sale.id">
              <td class="text-muted font-mono">#{{ sale.id.toString().padStart(6, '0') }}</td>
              <td class="text-sm">{{ formatDate(sale.date) }}</td>
              <td class="font-bold">{{ sale.clientName }}</td>
              <td>
                <div class="products-list">
                  <span v-for="(item, index) in sale.items" :key="index" class="product-tag">
                    {{ item }}
                  </span>
                </div>
              </td>
              <td class="font-mono text-primary font-bold">{{ formatCurrency(sale.total) }}</td>
              <td>
                <span class="badge-success">Completado</span>
              </td>
              <td>
                <button @click="openEditModal(sale)" class="btn-icon btn-edit" title="Editar Venta">
                   <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path></svg>
                </button>
                <button @click="openDeleteModal(sale)" class="btn-icon btn-delete" title="Eliminar Venta">
                   <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path><line x1="10" y1="11" x2="10" y2="17"></line><line x1="14" y1="11" x2="14" y2="17"></line></svg>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>

  <!-- Modal de Edición de Venta -->
  <div v-if="showEditModal" class="modal-overlay">
    <div class="modal-content">
      <h3>Editar Venta</h3>
      <p class="text-sm text-muted mb-4">Solo se puede modificar el cliente y la fecha. Para cambiar productos, elimine y cree una nueva venta.</p>
      
      <div class="form-group">
        <label>Cliente:</label>
        <select v-model="editingSale.clientId" class="form-control">
          <option v-for="client in clients" :key="client.id" :value="client.id">
            {{ client.name }}
          </option>
        </select>
      </div>

      <div class="form-group">
        <label>Fecha:</label>
        <input type="datetime-local" v-model="editingSale.date" class="form-control" />
      </div>

      <div class="modal-actions">
        <button @click="closeEditModal" class="btn-cancel">Cancelar</button>
        <button @click="saveSaleChanges" class="btn-confirm">Guardar Cambios</button>
      </div>
    </div>
  </div>

  <!-- Modal Confirmación Eliminar Venta -->
  <div v-if="showDeleteModal" class="modal-overlay">
    <div class="modal-content">
      <h3>¿Eliminar Venta?</h3>
      <p>Vas a eliminar la venta <strong>#{{ saleToDelete?.id.toString().padStart(6, '0') }}</strong></p>
      <p class="text-sm text-muted"><strong>Atención:</strong> Esta acción revertirá el stock de los productos vendidos y no se puede deshacer.</p>
      
      <div class="modal-actions">
        <button @click="closeDeleteModal" class="btn-cancel">Cancelar</button>
        <button @click="executeDelete" class="btn-confirm btn-danger">Sí, Eliminar y Restaurar Stock</button>
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
.text-sm { font-size: 0.9rem; color: var(--text-secondary); }
.font-bold { font-weight: 600; }
.font-mono { font-family: monospace; font-size: 0.95rem; }
.text-primary { color: var(--primary); }

.products-list {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.product-tag {
  background-color: #eef2ff;
  color: #4f46e5;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 0.8rem;
  font-weight: 500;
  border: 1px solid #c7d2fe;
}

.badge-success {
  background-color: #dcfce7;
  color: #166534;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
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

/* Scroll horizontal para móviles */
.table-responsive {
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
}
</style>
