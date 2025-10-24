import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useApiClient } from './useApiClient'
import { API_CONFIG } from '../config/config'

export const useAuth = () => {
  const { auth } = useApiClient()
  const router = useRouter()

  const user = ref<any>(null)
  const token = ref<string | null>(localStorage.getItem(API_CONFIG.TOKEN_KEY))
  const isAuthenticated = computed(() => !!token.value)

  const login = async (usernameOrEmail: string, password: string) => {
    try {
      const response = await auth.login({ usernameOrEmail, password })
      
      if (response.success && response.data) {
        user.value = response.data.user
        token.value = response.data.token
        return { success: true }
      }
      
      return { 
        success: false, 
        message: response.message || 'Login failed' 
      }
    } catch (error: any) {
      return { 
        success: false, 
        message: error.message || 'An error occurred' 
      }
    }
  }

  const logout = () => {
    user.value = null
    token.value = null
    auth.logout()
    router.push('/login')
  }

  const fetchProfile = async () => {
    if (!isAuthenticated.value) return
    try {
      const response = await auth.getProfile()
      if (response.success) user.value = response.data
    } catch (error) {
      console.error('Failed to fetch profile:', error)
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