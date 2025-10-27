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
      console.log('🔑 Token added to request:', token.substring(0, 20) + '...')
    } else {
      console.warn('⚠️ No token found in localStorage')
    }
    
    console.log('📤 Request:', config.method?.toUpperCase(), config.url)
    
    return config
  },
  (error) => {
    console.error('❌ Request error:', error)
    return Promise.reject(error)
  }
)

// ✅ Response interceptor - Handle errors
apiClient.interceptors.response.use(
  (response) => {
    console.log('📥 Response:', response.status, response.config.url)
    return response
  },
  (error) => {
    console.error('❌ Response error:', error.response?.status, error.config?.url)
    
    if (error.response?.status === 401) {
      console.log('🚪 Unauthorized - Logging out...')
      localStorage.removeItem(API_CONFIG.TOKEN_KEY)
      localStorage.removeItem('user')
      window.location.href = '/'
    }
    
    return Promise.reject(error)
  }
)

export default apiClient