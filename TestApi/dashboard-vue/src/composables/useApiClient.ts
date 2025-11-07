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
    console.log('🟢 [useApiClient] ========== sendRequest START ==========')
    console.log('🟢 [useApiClient] Input params:', params)
    
    const startTime = Date.now()
    loading.value = true

    try {
      const requestHeaders: Record<string, string> = {}
      requestHeaders['Content-Type'] = 'application/json'

      console.log('🟢 [useApiClient] Initial headers:', requestHeaders)
      console.log('🟢 [useApiClient] params.headers:', params.headers)

      if (params.headers && Array.isArray(params.headers)) {
        params.headers.forEach((header: any) => {
          if (header.enabled !== false && header.key) {
            console.log(`🟢 [useApiClient] Adding header: ${header.key} = ${header.value}`)
            requestHeaders[header.key] = header.value
          } else {
            console.log(`🟢 [useApiClient] Skipping header (disabled or no key):`, header)
          }
        })
      } else {
        console.warn('⚠️ [useApiClient] No headers array found!')
      }

      console.log('🟢 [useApiClient] Final requestHeaders:', requestHeaders)

      const isExternalUrl = params.url.startsWith('http://') || params.url.startsWith('https://')
      let finalUrl = params.url

      if (isExternalUrl) {
        requestHeaders['x-target-url'] = params.url
        finalUrl = 'http://localhost:3001/proxy'
        console.log('🟢 [useApiClient] Using proxy:', finalUrl)
      } else {
        console.log('🟢 [useApiClient] Direct URL:', finalUrl)
      }

      let requestBody = params.body

      if (Array.isArray(requestBody)) {
        console.log('🟢 [useApiClient] Processing array body (multiple requests)')
        
        const results = []
        for (let i = 0; i < requestBody.length; i++) {
          const testCase = requestBody[i]
          
          console.log(`🟢 [useApiClient] Sending request ${i + 1}/${requestBody.length}`)
          console.log(`🟢 [useApiClient] Request config:`, {
            method: params.method,
            url: finalUrl,
            headers: requestHeaders,
            data: testCase
          })
          
          try {
            const response = await axios({
              method: params.method,
              url: finalUrl,
              headers: requestHeaders,
              data: testCase
            })
            
            console.log(`✅ [useApiClient] Request ${i + 1} success:`, response.status)
            
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
            console.error(`❌ [useApiClient] Request ${i + 1} failed:`, {
              status: error.response?.status,
              statusText: error.response?.statusText,
              data: error.response?.data,
              message: error.message
            })
            
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

      console.log('🟢 [useApiClient] Sending single request')
      console.log('🟢 [useApiClient] Request config:', {
        method: params.method,
        url: finalUrl,
        headers: requestHeaders,
        data: requestBody
      })

      const response = await axios({
        method: params.method,
        url: finalUrl,
        headers: requestHeaders,
        data: requestBody
      })

      console.log('✅ [useApiClient] Request success:', response.status)

      return [{
        success: true,
        status: response.status,
        statusText: response.statusText,
        duration: Date.now() - startTime,
        size: JSON.stringify(response.data).length,
        data: response.data
      }]
    } catch (error: any) {
      console.error('❌ [useApiClient] Error:', {
        status: error.response?.status,
        statusText: error.response?.statusText,
        data: error.response?.data,
        message: error.message
      })
      
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
 