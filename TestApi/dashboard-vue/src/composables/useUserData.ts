// composables/useUserData.ts
import { ref } from 'vue'
import apiClient from '../utils/axios'

export interface RequestBody {
  bodyType: string
  content: string
}

export interface RequestHeader {
  key: string
  value: string
}

export interface QueryParam {
  key: string
  value: string
}

export interface RequestItem {
  id: number
  name: string
  method: string
  url: string
  authType: string
  authValue: string | null
  createdAt: string
  queryParams: QueryParam[]
  headers: RequestHeader[]
  body: RequestBody | null
}

export interface Collection {
  id: number
  userId: number
  name: string
  description: string
  createdAt: string
  requests: RequestItem[]
}

export interface UserData {
  user: {
    id: number
    username: string
    email: string
    fullName?: string
    isActive: boolean
    createdAt: string
  }
  collections: Collection[]
  summary: {
    totalCollections: number
    totalRequests: number
  }
}

export const useUserData = () => {
  const loading = ref(false)
  const error = ref<string | null>(null)
  const data = ref<UserData | null>(null)

  const fetchUserData = async () => {
    loading.value = true
    error.value = null

    try {
      console.log('🌐 Fetching user data from: /DataExport/my-data')
      
      const response = await apiClient.get('/DataExport/my-data')

      console.log('✅ Data received:', response.data)

      if (response.data.success) {
        data.value = response.data.data
        localStorage.setItem('userData', JSON.stringify(response.data.data))
        return response.data.data
      } else {
        error.value = response.data.message || 'Failed to fetch data'
        return null
      }
    } catch (err: any) {
      console.error('❌ Fetch error:', err)
      error.value = err.response?.data?.message || err.message || 'An error occurred'
      return null
    } finally {
      loading.value = false
    }
  }

  const clearUserData = () => {
    data.value = null
    localStorage.removeItem('userData')
  }

  const loadCachedData = () => {
    const cached = localStorage.getItem('userData')
    if (cached) {
      try {
        data.value = JSON.parse(cached)
        console.log('📦 Loaded cached data:', data.value)
      } catch (err) {
        console.error('Error parsing cached data:', err)
      }
    }
  }

  return {
    loading,
    error,
    data,
    fetchUserData,
    clearUserData,
    loadCachedData
  }
}