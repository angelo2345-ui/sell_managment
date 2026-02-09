import axios from 'axios';

const API_URL = 'http://localhost:5227/api/sales';

export default {
  async createSale(saleData) {
    const response = await axios.post(API_URL, saleData);
    // Retornamos todo el cuerpo para poder verificar response.success en el componente
    return response.data; 
  }
};