export const useApiClient = () => {
  const sendRequest = async (config: {
    url: string
    method: string
    body?: string
    params?: Array<{ key: string; value: string; enabled: boolean }>
    headers?: Array<{ key: string; value: string; enabled: boolean }>
    auth?: {
      type: string
      username?: string
      password?: string
      token?: string
    }
  }) => {
    try {
      // Build URL with params
      let finalUrl = config.url
      if (config.params && config.params.length > 0) {
        const enabledParams = config.params.filter(p => p.enabled && p.key)
        if (enabledParams.length > 0) {
          const queryString = enabledParams
            .map(p => `${encodeURIComponent(p.key)}=${encodeURIComponent(p.value)}`)
            .join('&')
          finalUrl += (finalUrl.includes('?') ? '&' : '?') + queryString
        }
      }

      // Build headers
      const headers: Record<string, string> = {
        'Content-Type': 'application/json'
      }

      if (config.headers && config.headers.length > 0) {
        config.headers
          .filter(h => h.enabled && h.key)
          .forEach(h => {
            headers[h.key] = h.value
          })
      }

      // Handle authentication
      if (config.auth) {
        if (config.auth.type === 'bearer' && config.auth.token) {
          headers['Authorization'] = `Bearer ${config.auth.token}`
        } else if (config.auth.type === 'basic' && config.auth.username && config.auth.password) {
          const credentials = btoa(`${config.auth.username}:${config.auth.password}`)
          headers['Authorization'] = `Basic ${credentials}`
        }
      }

      // Build request options
      const options: RequestInit = {
        method: config.method,
        headers
      }

      // Add body for non-GET requests
      if (config.method !== 'GET' && config.body) {
        options.body = config.body
      }

      // Send request
      const startTime = Date.now()
      const response = await fetch(finalUrl, options)
      const endTime = Date.now()
      const duration = endTime - startTime

      // Parse response
      const contentType = response.headers.get('content-type')
      let data: any

      if (contentType?.includes('application/json')) {
        data = await response.json()
      } else {
        data = await response.text()
      }

      // Build response object
      return {
        success: response.ok,
        status: response.status,
        statusText: response.statusText,
        duration,
        headers: Object.fromEntries(response.headers.entries()),
        data,
        size: JSON.stringify(data).length
      }
    } catch (error: any) {
      return {
        success: false,
        status: 0,
        statusText: 'Network Error',
        duration: 0,
        headers: {},
        data: null,
        error: error.message || 'Failed to send request'
      }
    }
  }

  return {
    sendRequest
  }
}