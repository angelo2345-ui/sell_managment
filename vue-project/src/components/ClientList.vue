<script setup>
import { ref, onMounted } from 'vue';
import clientService from '../services/clientService'; // Usar el servicio

const clients = ref([]);
const loading = ref(true);
const error = ref(null);

// Estado para edición
const showEditModal = ref(false);
const editingClient = ref({ id: 0, name: '', email: '', phone: '' });

// Estado para eliminación
const showDeleteModal = ref(false);
const clientToDelete = ref(null);

const loadClients = async () => {
  try {
    loading.value = true;
    clients.value = await clientService.getClients(); // Usar servicio
  } catch (err) {
    console.error("Error al cargar clientes:", err);
    error.value = "No se pudieron cargar los clientes.";
  } finally {
    loading.value = false;
  }
};

const openDeleteModal = (client) => {
  clientToDelete.value = client;
  showDeleteModal.value = true;
};

const closeDeleteModal = () => {
  showDeleteModal.value = false;
  clientToDelete.value = null;
};

const executeDelete = async () => {
  if (!clientToDelete.value) return;
  const id = clientToDelete.value.id;
  
  try {
    await clientService.deleteClient(id);
    loadClients();
    // alert('Cliente eliminado correctamente (Soft Delete).'); // Ya se ve visualmente
    closeDeleteModal();
  } catch (err) {
    console.error('[Frontend] Error al eliminar cliente:', err);
    
    let message = 'Error desconocido al eliminar.';
    
    if (err.response) {
      // El servidor respondió
      message = err.response.data?.message || `Error del servidor (${err.response.status})`;
    } else if (err.request) {
      // No hubo respuesta
      message = 'No se recibió respuesta del servidor. Verifica la conexión.';
    } else {
      message = err.message;
    }

    alert(`⚠️ ${message}`);
    closeDeleteModal();
  }
};

const openEditModal = (client) => {
  editingClient.value = { ...client };
  showEditModal.value = true;
};

const closeEditModal = () => {
  showEditModal.value = false;
  editingClient.value = { id: 0, name: '', email: '', phone: '' };
};

const saveClientChanges = async () => {
  try {
    await clientService.updateClient(editingClient.value.id, editingClient.value);
    closeEditModal();
    loadClients();
    alert('Cliente actualizado correctamente.');
  } catch (err) {
    console.error(err);
    alert('Error al actualizar cliente.');
  }
};

onMounted(() => {
  loadClients();
});
</script>

<template>
  <div class="card">
    <div class="card-header">
      <h3>Cartera de Clientes</h3>
      <span class="badge" v-if="clients.length">{{ clients.length }} registros</span>
    </div>

    <div v-if="loading" class="state-message">
      <div class="spinner"></div>
      <p>Cargando clientes...</p>
    </div>
    
    <div v-else-if="error" class="alert alert-error" role="alert">
      {{ error }}
    </div>

    <div v-else>
      <div v-if="clients.length === 0" class="empty-state">
        <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" class="empty-icon"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path><circle cx="9" cy="7" r="4"></circle><path d="M23 21v-2a4 4 0 0 0-3-3.87"></path><path d="M16 3.13a4 4 0 0 1 0 7.75"></path></svg>
        <p>No hay clientes registrados aún.</p>
      </div>

      <div v-else class="table-responsive">
        <table class="clients-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Nombre Completo</th>
              <th>Contacto</th>
              <th>Teléfono</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="client in clients" :key="client.id">
              <td class="text-muted" data-label="ID">#{{ client.id }}</td>
              <td class="font-bold" data-label="Nombre">
                <div class="user-cell">
                  <div class="avatar-sm">{{ client.name.charAt(0).toUpperCase() }}</div>
                  <span class="client-name">{{ client.name }}</span>
                </div>
              </td>
              <td data-label="Email">
                <a :href="`mailto:${client.email}`" class="link-email">{{ client.email }}</a>
              </td>
              <td class="font-mono" data-label="Teléfono">{{ client.phone || 'N/A' }}</td>
              <td data-label="Acciones">
                <button @click="openEditModal(client)" class="btn-icon btn-edit" title="Editar">
                  <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path></svg>
                </button>
                <button @click="openDeleteModal(client)" class="btn-icon btn-delete" title="Eliminar">
                   <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path><line x1="10" y1="11" x2="10" y2="17"></line><line x1="14" y1="11" x2="14" y2="17"></line></svg>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>

  <!-- Modal Editar Cliente -->
  <div v-if="showEditModal" class="modal-overlay">
    <div class="modal-content">
      <h3>Editar Cliente</h3>
      
      <div class="form-group">
        <label>Nombre:</label>
        <input type="text" v-model="editingClient.name" class="form-control" />
      </div>

      <div class="form-group">
        <label>Email:</label>
        <input type="email" v-model="editingClient.email" class="form-control" />
      </div>

      <div class="form-group">
        <label>Teléfono:</label>
        <input type="text" v-model="editingClient.phone" class="form-control" />
      </div>

      <div class="modal-actions">
        <button @click="closeEditModal" class="btn-cancel">Cancelar</button>
        <button @click="saveClientChanges" class="btn-confirm">Guardar</button>
      </div>
    </div>
  </div>

  <!-- Modal Confirmación Eliminar -->
  <div v-if="showDeleteModal" class="modal-overlay">
    <div class="modal-content">
      <h3>¿Eliminar Cliente?</h3>
      <p>Estás a punto de eliminar a: <strong>{{ clientToDelete?.name }}</strong></p>
      <p class="text-sm text-muted">Si el cliente tiene historial de compras, se marcará como inactivo (Soft Delete) para no perder los datos.</p>
      
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
  flex-wrap: wrap;
  gap: 0.5rem;
}

.card-header h3 {
  font-size: 1.25rem;
  color: var(--text-main);
  margin: 0;
}

.badge {
  background: var(--bg-body);
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--text-muted);
  white-space: nowrap;
}

.state-message {
  text-align: center;
  padding: 3rem 1rem;
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
  padding: 3rem 1rem;
  border: 2px dashed var(--border-color);
  border-radius: 8px;
  color: var(--text-muted);
}

.text-muted { 
  color: var(--text-muted); 
}

.font-bold { 
  font-weight: 600; 
}

.font-mono { 
  font-family: monospace; 
  font-size: 0.95rem; 
}

.user-cell {
  display: flex;
  align-items: center;
  gap: 10px;
}

.client-name {
  word-break: break-word;
}

.avatar-sm {
  width: 32px;
  height: 32px;
  min-width: 32px;
  background-color: #e0e7ff;
  color: var(--primary);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 0.8rem;
}

.link-email {
  color: var(--primary);
  text-decoration: none;
  word-break: break-all;
}

.link-email:hover {
  text-decoration: underline;
}

.btn-icon {
  background: none;
  border: none;
  cursor: pointer;
  font-size: 1rem;
  padding: 4px 8px;
  border-radius: 4px;
  transition: background-color 0.2s;
}

.btn-icon:hover {
  background-color: var(--bg-body);
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

/* Tabla responsive */
.table-responsive {
  width: 100%;
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
  margin: 0;
}

.clients-table {
  width: 100%;
  min-width: 600px;
  border-collapse: collapse;
  box-sizing: border-box;
}

.clients-table th,
.clients-table td {
  padding: 0.75rem;
  text-align: left;
  border-bottom: 1px solid var(--border-color);
}

.clients-table th {
  background-color: var(--bg-body);
  font-weight: 600;
  font-size: 0.875rem;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.clients-table tbody tr:hover {
  background-color: var(--bg-hover, #f9fafb);
}

/* Responsive - Vista de tarjetas en móvil */
@media (max-width: 768px) {
  .card-header h3 {
    font-size: 1.1rem;
  }

  .badge {
    font-size: 0.75rem;
    padding: 0.2rem 0.5rem;
  }

  .table-responsive {
    overflow-x: auto;
    margin: 0 -1rem;
    padding: 0 1rem;
  }

  .clients-table {
    min-width: 500px;
    font-size: 0.9rem;
  }

  .clients-table th,
  .clients-table td {
    padding: 0.5rem 0.4rem;
  }

  .avatar-sm {
    width: 28px;
    height: 28px;
    min-width: 28px;
    font-size: 0.75rem;
  }

  .user-cell {
    gap: 8px;
  }
}

@media (max-width: 480px) {
  .clients-table {
    min-width: 450px;
    font-size: 0.85rem;
  }

  .clients-table th,
  .clients-table td {
    padding: 0.4rem 0.3rem;
  }

  .btn-icon {
    font-size: 0.9rem;
    padding: 2px 4px;
  }
}

/* Vista de tarjetas alternativa (opcional - descomentar si prefieres) */
/*
@media (max-width: 640px) {
  .table-responsive {
    overflow-x: visible;
  }

  .clients-table thead {
    display: none;
  }

  .clients-table,
  .clients-table tbody,
  .clients-table tr,
  .clients-table td {
    display: block;
    width: 100%;
  }

  .clients-table tr {
    margin-bottom: 1rem;
    border: 1px solid var(--border-color);
    border-radius: 8px;
    padding: 0.75rem;
    background-color: white;
  }

  .clients-table td {
    padding: 0.5rem 0;
    border: none;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .clients-table td::before {
    content: attr(data-label);
    font-weight: 600;
    color: var(--text-muted);
    flex-shrink: 0;
    margin-right: 1rem;
  }

  .clients-table td:last-child {
    border-top: 1px solid var(--border-color);
    padding-top: 0.75rem;
    margin-top: 0.5rem;
  }
}
*/
</style>