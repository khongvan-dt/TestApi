import type { 
  ApiResponse, 
  LoginRequest, 
  LoginResponse,
  RegisterRequest,
  Collection,
  Request,
  RequestRunPayload,
  RequestRunResponse,
  TestData,
  Environment
} from '../types/api'
import { API_CONFIG } from '../config/config'

const tokenKey = API_CONFIG.TOKEN_KEY

// Token helpers
const getAuthToken = () => localStorage.getItem(tokenKey)
const setAuthToken = (token: string | null) => {
  if (token) localStorage.setItem(tokenKey, token)
  else localStorage.removeItem(tokenKey)
}

export const useApiClient = () => {
  const baseURL = API_CONFIG.BASE_URL

  // Generic request
  const getHeaders = () => ({
    'Content-Type': 'application/json',
    ...(getAuthToken() ? { 'Authorization': `Bearer ${getAuthToken()}` } : {})
  })

  const request = async <T>(endpoint: string, options: RequestInit = {}): Promise<ApiResponse<T>> => {
    try {
      const response = await fetch(`${baseURL}${endpoint}`, {
        ...options,
        headers: {
          ...getHeaders(),
          ...options.headers
        }
      })
      
      const data = await response.json()
      
      if (!response.ok) {
        return {
          success: false,
          message: data.message || 'API request failed',
          data: undefined
        }
      }
      
      // Backend trả về format: { success, data, message }
      return data
    } catch (error: any) {
      console.error('API Error:', error)
      return {
        success: false,
        message: error.message || 'Network error',
        data: undefined
      }
    }
  }

  // AUTH
  const auth = {
    login: async (payload: LoginRequest) => {
      const response = await request<LoginResponse>('/Auth/login', {
        method: 'POST',
        body: JSON.stringify(payload)
      })
      
      if (response.success && response.data?.token) {
        setAuthToken(response.data.token)
      }
      
      return response
    },

    register: async (payload: RegisterRequest) => {
      return request<any>('/Auth/register', { 
        method: 'POST', 
        body: JSON.stringify(payload) 
      })
    },

    logout: () => setAuthToken(null),

    getProfile: async () => request<any>('/Auth/profile')
  }

  // WORKSPACES
  const workspaces = {
    getAll: async () => request<any[]>('/workspaces'),
    getById: async (id: string) => request<any>(`/workspaces/${id}`),
    create: async (payload: { name: string; description?: string }) =>
      request<any>('/workspaces', { method: 'POST', body: JSON.stringify(payload) }),
    update: async (id: string, payload: any) =>
      request<any>(`/workspaces/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),
    delete: async (id: string) => request<any>(`/workspaces/${id}`, { method: 'DELETE' })
  }

  // COLLECTIONS
  const collections = {
    getAll: async (workspaceId: string) => request<Collection[]>(`/collections?workspaceId=${workspaceId}`),
    getById: async (id: string) => request<Collection>(`/collections/${id}`),
    create: async (payload: any) =>
      request<Collection>('/collections', { method: 'POST', body: JSON.stringify(payload) }),
    update: async (id: string, payload: any) =>
      request<Collection>(`/collections/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),
    delete: async (id: string) => request<any>(`/collections/${id}`, { method: 'DELETE' })
  }

  // REQUESTS
  const requests = {
    getAll: async (collectionId: string) => request<Request[]>(`/requests?collectionId=${collectionId}`),
    getById: async (id: string) => request<Request>(`/requests/${id}`),
    create: async (payload: any) =>
      request<Request>('/requests', { method: 'POST', body: JSON.stringify(payload) }),
    update: async (id: string, payload: any) =>
      request<Request>(`/requests/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),
    delete: async (id: string) => request<any>(`/requests/${id}`, { method: 'DELETE' }),
    run: async (payload: RequestRunPayload) => {
      const startTime = Date.now()
      try {
        const response = await request<RequestRunResponse>('/requests/run', {
          method: 'POST',
          body: JSON.stringify(payload)
        })
        const duration = Date.now() - startTime
        return {
          success: response.success,
          data: response.data,
          status: response.data?.statusCode || 0,
          statusText: response.data?.statusText || '',
          duration: response.data?.responseTimeMs || duration,
          size: response.data?.responseSizeBytes || 0,
          error: null
        }
      } catch (error: any) {
        return {
          success: false,
          data: null,
          status: 0,
          statusText: 'Error',
          duration: Date.now() - startTime,
          size: 0,
          error: error.message
        }
      }
    }
  }

  // TEST DATA
  const testData = {
    getAll: async (requestId: string) => request<TestData[]>(`/requests/${requestId}/testdata`),
    getById: async (id: string) => request<TestData>(`/testdata/${id}`),
    create: async (payload: any) => request<TestData>('/testdata', { method: 'POST', body: JSON.stringify(payload) }),
    update: async (id: string, payload: any) => request<TestData>(`/testdata/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),
    delete: async (id: string) => request<any>(`/testdata/${id}`, { method: 'DELETE' })
  }

  // ENVIRONMENTS
  const environments = {
    getAll: async (workspaceId: string) => request<Environment[]>(`/environments?workspaceId=${workspaceId}`),
    getById: async (id: string) => request<Environment>(`/environments/${id}`),
    create: async (payload: any) => request<Environment>('/environments', { method: 'POST', body: JSON.stringify(payload) }),
    update: async (id: string, payload: any) => request<Environment>(`/environments/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),
    delete: async (id: string) => request<any>(`/environments/${id}`, { method: 'DELETE' })
  }

  // EXECUTION HISTORIES
  const executionHistories = {
    getAll: async (requestId?: string) => {
      const url = requestId ? `/executionhistories?requestId=${requestId}` : '/executionhistories'
      return request<any[]>(url)
    },
    getById: async (id: string) => request<any>(`/executionhistories/${id}`),
    delete: async (id: string) => request<any>(`/executionhistories/${id}`, { method: 'DELETE' })
  }

  return {
    auth,
    workspaces,
    collections,
    requests,
    testData,
    environments,
    executionHistories
  }
}