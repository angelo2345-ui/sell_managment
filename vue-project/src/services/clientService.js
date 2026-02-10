import axios from 'axios';

const API_URL = 'http://127.0.0.1:5227/api/clients';

export default {
  async getClients() {
    const response = await axios.get(API_URL);
    return response.data.data; 
  },

  async createClient(client) {
    const response = await axios.post(API_URL, client);
    return response.data;
  },

  async updateClient(id, client) {
    const response = await axios.put(`${API_URL}/${id}`, client);
    return response.data;
  },

  async deleteClient(id) {
    const response = await axios.delete(`${API_URL}/${id}`);
    return response.data;
  }
};
