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

export const useApiClient = () => {
  const loading = ref(false)

  const sendRequest = async (params: RequestParams) => {
    const startTime = Date.now()
    loading.value = true

    try {
      const requestHeaders: Record<string, string> = {}
      requestHeaders['Content-Type'] = 'application/json'

      const isExternalUrl = params.url.startsWith('http://') || params.url.startsWith('https://')
      let finalUrl = params.url

      if (isExternalUrl) {
        requestHeaders['x-target-url'] = params.url
        finalUrl = 'http://localhost:3001/proxy'
      }

      let requestBody = params.body


      if (Array.isArray(requestBody)) {
        
        const results = []
        for (let i = 0; i < requestBody.length; i++) {
          const testCase = requestBody[i]
          
          try {
            const response = await axios({
              method: params.method,
              url: finalUrl,
              headers: requestHeaders,
              data: testCase
            })
            
            results.push({
              success: true,
              status: response.status,
              statusText: response.statusText,
              duration: Date.now() - startTime,
              size: JSON.stringify(response.data).length,
              data: response.data,
              testCase: i + 1,
              requestData: testCase
            })
          } catch (error: any) {
            results.push({
              success: false,
              status: error.response?.status || 0,
              statusText: error.response?.statusText || 'Error',
              duration: Date.now() - startTime,
              data: error.response?.data || error.message,
              testCase: i + 1,
              requestData: testCase
            })
          }
        }
        
        return results
      }

      
      const response = await axios({
        method: params.method,
        url: finalUrl,
        headers: requestHeaders,
        data: requestBody
      })

      return [{
        success: true,
        status: response.status,
        statusText: response.statusText,
        duration: Date.now() - startTime,
        size: JSON.stringify(response.data).length,
        data: response.data
      }]
    } catch (error: any) {
      return [{
        success: false,
        status: error.response?.status || 0,
        statusText: error.response?.statusText || 'Network Error',
        duration: Date.now() - startTime,
        data: error.response?.data || error.message
      }]
    } finally {
      loading.value = false
    }
  }

  return { loading, sendRequest }
}