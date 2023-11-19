// api.js or similar file
import axios from 'axios';

const api = axios.create({
    baseURL: 'https://localhost:7072',
});

export default api;
