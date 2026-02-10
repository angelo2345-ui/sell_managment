import axios from 'axios';

const API_URL = 'http://127.0.0.1:5227/api/sales';

export default {
  async createSale(saleData) {
    const response = await axios.post(API_URL, saleData);
    // Retornamos todo el cuerpo para poder verificar response.success en el componente
    return response.data; 
  },

  async deleteSale(id) {
    const response = await axios.delete(`${API_URL}/${id}`);
    return response.data;
  },

  async updateSale(id, saleData) {
    const response = await axios.put(`${API_URL}/${id}`, saleData);
    return response.data;
  }
};