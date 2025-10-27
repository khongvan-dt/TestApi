import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { API_CONFIG } from '../config/config'

interface User {
  id: string
  username: string
  email: string
  fullName?: string
  avatarUrl?: string
}

export const useAuth = () => {
  const router = useRouter()

  const storedUser = localStorage.getItem('user')
  const storedToken = localStorage.getItem(API_CONFIG.TOKEN_KEY)
  
  const user = ref<User | null>(storedUser ? JSON.parse(storedUser) : null)
  const token = ref<string | null>(storedToken)
  const isAuthenticated = computed(() => !!token.value)

  const login = async (usernameOrEmail: string, password: string) => {
    try {
      const response = await axios.post(`${API_CONFIG.BASE_URL}/Auth/login`, {
        usernameOrEmail,
        password
      })

      if (response.data.success && response.data.data) {
        const authToken = response.data.data.token
        token.value = authToken
        localStorage.setItem(API_CONFIG.TOKEN_KEY, authToken)
        
        user.value = response.data.data.user
        localStorage.setItem('user', JSON.stringify(response.data.data.user))
        
        return { success: true }
      }
      
      return { 
        success: false, 
        message: response.data.message || 'Login failed' 
      }
    } catch (error: any) {
      return { 
        success: false, 
        message: error.response?.data?.message || error.message || 'An error occurred' 
      }
    }
  }

  // ✅ FIX: Logout với redirect và force reload
  const logout = async () => {
    console.log('🚪 Logging out...')
    
    // 1. Clear state ngay lập tức
    user.value = null
    token.value = null
    
    // 2. Clear ALL localStorage
    localStorage.clear()
    
    // 3. Clear sessionStorage nếu có
    sessionStorage.clear()
    
    console.log('✅ Logout completed - redirecting...')
    
    // 4. Force redirect về home và reload
    await router.push('/')
    
    // 5. Force reload để clear toàn bộ state
    window.location.reload()
  }

  const fetchProfile = async () => {
    // ✅ FIX: Không fetch nếu không authenticated
    if (!isAuthenticated.value || !token.value) {
      return
    }
    
    try {
      const response = await axios.get(`${API_CONFIG.BASE_URL}/Auth/profile`, {
        headers: {
          'Authorization': `Bearer ${token.value}`
        }
      })
      
      if (response.data.success) {
        user.value = response.data.data
        localStorage.setItem('user', JSON.stringify(response.data.data))
      }
    } catch (error: any) {
      console.error('Failed to fetch profile:', error)
      
      // ✅ FIX: Nếu token invalid, logout
      if (error.response?.status === 401) {
        logout()
      }
    }
  }

  return {
    user,
    token,
    isAuthenticated,
    login,
    logout,
    fetchProfile
  }
}