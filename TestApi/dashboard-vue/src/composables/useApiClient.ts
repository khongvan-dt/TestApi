import { ref } from 'vue'
import axios from 'axios'

interface RequestParams {
  url: string
  method: string
  body?: any
  params?: Array<{ key: string; value: string; enabled?: boolean }>
  headers?: Array<{ key: string; value: string; enabled?: boolean }>
  auth?: {
    type: string
    username?: string
    password?: string
    token?: string
  } | null
}

interface ApiResponse {
  success: boolean
  data: any
  error?: string
  status: number
  statusText: string
  duration: number
  size: number
}

export const useApiClient = () => {
  const loading = ref(false)

  const sendRequest = async (params: RequestParams): Promise<ApiResponse> => {
    const startTime = Date.now()
    loading.value = true

    try {

      let finalUrl = params.url
      const requestHeaders: Record<string, string> = {}

      const isExternalUrl = finalUrl.startsWith('http://') || finalUrl.startsWith('https://')
      
      if (isExternalUrl) {
        requestHeaders['x-target-url'] = params.url
        finalUrl = 'http://localhost:3001/proxy'
       
      }

      // Build query params
      if (params.params && params.params.length > 0) {
        const queryParams = params.params
          .filter(p => p.enabled !== false && p.key && p.value)
          .map(p => `${encodeURIComponent(p.key)}=${encodeURIComponent(p.value)}`)
          .join('&')
        
        if (queryParams) {
          if (isExternalUrl) {
            requestHeaders['x-target-url'] += (params.url.includes('?') ? '&' : '?') + queryParams
          } else {
            finalUrl += (finalUrl.includes('?') ? '&' : '?') + queryParams
          }
        }
      }

      // Build headers
      if (params.headers && params.headers.length > 0) {
        params.headers
          .filter(h => h.enabled !== false && h.key && h.value)
          .forEach(h => {
            requestHeaders[h.key] = h.value
          })
      }

      if (params.body && !requestHeaders['Content-Type']) {
        requestHeaders['Content-Type'] = 'application/json'
      }

      // Handle Authorization
      if (params.auth) {
        if (params.auth.type === 'Bearer' && params.auth.token) {
          requestHeaders['Authorization'] = `Bearer ${params.auth.token}`
        } else if (params.auth.type === 'Basic' && params.auth.username && params.auth.password) {
          const credentials = btoa(`${params.auth.username}:${params.auth.password}`)
          requestHeaders['Authorization'] = `Basic ${credentials}`
        }
      }

      // Build axios config
      const axiosConfig: any = {
        method: params.method,
        url: finalUrl,
        headers: requestHeaders,
        timeout: 30000,
      }

      // Add body for POST/PUT/PATCH
      if (['POST', 'PUT', 'PATCH'].includes(params.method.toUpperCase())) {
        if (params.body) {
          try {
            axiosConfig.data = typeof params.body === 'string' 
              ? JSON.parse(params.body) 
              : params.body
          } catch {
            axiosConfig.data = params.body
          }
        }
      }


      // Send request
      const response = await axios(axiosConfig)
      
      const duration = Date.now() - startTime
      const size = JSON.stringify(response.data).length


      return {
        success: true,
        data: response.data,
        status: response.status,
        statusText: response.statusText,
        duration,
        size
      }

    } catch (error: any) {
      const duration = Date.now() - startTime
      

      if (error.response) {
        return {
          success: false,
          data: error.response.data,
          error: error.message,
          status: error.response.status,
          statusText: error.response.statusText,
          duration,
          size: JSON.stringify(error.response.data || {}).length
        }
      } else {
        return {
          success: false,
          data: null,
          error: error.message + ' - Make sure proxy server is running on port 3001',
          status: 0,
          statusText: 'Network Error',
          duration,
          size: 0
        }
      }
    } finally {
      loading.value = false
    }
  }

  return {
    loading,
    sendRequest
  }
}