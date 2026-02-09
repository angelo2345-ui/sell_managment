import axios from 'axios';

const API_URL = 'http://localhost:5227/api/clients';

export default {
  async getClients() {
    const response = await axios.get(API_URL);
    return response.data.data; 
  },

  async createClient(client) {
    const response = await axios.post(API_URL, client);
    return response.data;
  }
};
