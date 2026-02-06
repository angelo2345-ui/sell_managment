import axios from 'axios';

// Aquí le decimos dónde vive nuestro backend
const API_URL = 'http://localhost:5227/api/products';

export default {
  // Función para pedir todos los productos
  getProducts() {
    return axios.get(API_URL);
  },

  // Función para enviar un nuevo producto
  createProduct(product) {
    return axios.post(API_URL, product);
  }
};