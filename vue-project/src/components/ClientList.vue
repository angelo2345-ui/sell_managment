<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';

const clients = ref([]);

const loadClients = async () => {
  try {
    // Asegúrate que el puerto 5227 sea el correcto de tu backend
    const response = await axios.get('http://localhost:5227/api/clients');
    clients.value = response.data;
  } catch (error) {
    console.error("Error al cargar clientes:", error);
    alert("Error al cargar clientes");
  }
};

onMounted(() => {
  loadClients();
});
</script>

<template>
  <div class="client-container">
    <h2>Listado de Clientes</h2>
    <table>
      <thead>
        <tr>
          <th>ID</th>
          <th>Nombre</th>
          <th>Email</th>
          <th>Teléfono</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="client in clients" :key="client.id">
          <td>{{ client.id }}</td>
          <td>{{ client.name }}</td>
          <td>{{ client.email }}</td>
          <td>{{ client.phone }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.client-container {
  margin-top: 20px;
}
table {
  width: 100%;
  border-collapse: collapse;
}
th, td {
  border: 1px solid #ddd;
  padding: 8px;
  text-align: left;
}
th {
  background-color: #f2f2f2;
}
</style>