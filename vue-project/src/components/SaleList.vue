<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';

const sales = ref([]);
const loading = ref(true);
const error = ref(null);

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
            </tr>
          </tbody>
        </table>
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

/* Scroll horizontal para móviles */
.table-responsive {
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
}
</style>
