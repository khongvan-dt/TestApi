import axios from 'axios'
import { API_CONFIG } from '../config/config'

// Tạo axios instance
const apiClient = axios.create({
  baseURL: API_CONFIG.BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// ✅ Request interceptor - Tự động thêm token vào mọi request
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem(API_CONFIG.TOKEN_KEY)
    
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
     } else {
      console.warn(' No token found in localStorage')
    }
    
     
    return config
  },
  (error) => {
     return Promise.reject(error)
  }
)

// ✅ Response interceptor - Handle errors
apiClient.interceptors.response.use(
  (response) => {
     return response
  },
  (error) => {
     
    if (error.response?.status === 401) {
       localStorage.removeItem(API_CONFIG.TOKEN_KEY)
      localStorage.removeItem('user')
      window.location.href = '/'
    }
    
    return Promise.reject(error)
  }
)

export default apiClient