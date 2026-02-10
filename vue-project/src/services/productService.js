import axios from 'axios';

// Usamos 127.0.0.1 directo para evitar problemas de resolución de localhost y proxies
const API_URL = 'http://127.0.0.1:5227/api/products';

export default {
  // Función para pedir todos los productos
  getProducts() {
    return axios.get(API_URL);
  },

  // Función para enviar un nuevo producto
  createProduct(product) {
    return axios.post(API_URL, product);
  },

  // Función para actualizar un producto existente
  updateProduct(id, product) {
    return axios.put(`${API_URL}/${id}`, product);
  },

  // Función para eliminar un producto
  deleteProduct(id) {
    return axios.delete(`${API_URL}/${id}`);
  }
};