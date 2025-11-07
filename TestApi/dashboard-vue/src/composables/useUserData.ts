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
  dataBaseTest: string | null
  bodies: RequestBody[] 
  collectionId: number
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

 export interface SaveRequestData {
  requestId?: number | null
  collectionId: number
  name: string
  method: string
  url: string
  authType?: string | null
  authValue?: string | null
  queryParams: Array<{ key: string; value: string }>
  headers: Array<{ key: string; value: string }>
  body?: {
    bodyType: string
    content: string
  } | null
}

export interface SaveRequestResult {
  success: boolean
  message?: string
  requestId: number
  isNew: boolean
}
export interface ImportResult {
  success: boolean
  importedCollections: number
  updatedCollections: number
  importedRequests: number
  updatedRequests: number
  totalProcessed: number
  errorMessage?: string
}

export const useUserData = () => {
  const loading = ref(false)
  const error = ref<string | null>(null)
  const data = ref<UserData | null>(null)

  const fetchUserData = async () => {
    loading.value = true
    error.value = null

    try {
       
      const response = await apiClient.get('/DataExport/my-data')

 
      if (response.data.success) {
        data.value = response.data.data
        localStorage.setItem('userData', JSON.stringify(response.data.data))
        return response.data.data
      } else {
        error.value = response.data.message || 'Failed to fetch data'
        return null
      }
    } catch (err: any) {
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
       } catch (err) {
        console.error('Error parsing cached data:', err)
      }
    }
  }

   const exportUserData = async (): Promise<UserData | null> => {
    loading.value = true
    error.value = null

    try {
       
      const response = await apiClient.get('/UserData/export')

 
      if (response.data.success) {
        return response.data.data
      } else {
        error.value = response.data.message || 'Failed to export data'
        return null
      }
    } catch (err: any) {
       error.value = err.response?.data?.message || err.message || 'Failed to export data'
      return null
    } finally {
      loading.value = false
    }
  }

  //  THÊM: Import data
  const importUserData = async (importData: UserData): Promise<ImportResult | null> => {
    loading.value = true
    error.value = null

    try {
       
      const response = await apiClient.post('/UserData/import', importData)

 
      if (response.data.success) {
        return response.data.data
      } else {
        error.value = response.data.message || 'Failed to import data'
        return null
      }
    } catch (err: any) {
       error.value = err.response?.data?.message || err.message || 'Failed to import data'
      return null
    } finally {
      loading.value = false
    }
  }

   const downloadAsFile = (exportData: UserData, filename: string = 'api-collections.json') => {
    try {
      const blob = new Blob([JSON.stringify(exportData, null, 2)], { type: 'application/json' })
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = filename
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      URL.revokeObjectURL(url)
     } catch (err) {
       error.value = 'Failed to download file'
    }
  }

  const readFile = (file: File): Promise<UserData | null> => {
    return new Promise((resolve) => {
      const reader = new FileReader()
      
      reader.onload = (e) => {
        try {
          const jsonData = JSON.parse(e.target?.result as string)
          resolve(jsonData)
        } catch (err) {
          error.value = 'Invalid JSON file format'
          resolve(null)
        }
      }
      
      reader.onerror = () => {
        error.value = 'Failed to read file'
        resolve(null)
      }
      
      reader.readAsText(file)
    })
  } 
  const saveRequest = async (requestData: SaveRequestData[]): Promise<SaveRequestResult | null> => {
    loading.value = true
    error.value = null

    try {
       
      const response = await apiClient.post('/DataExport/save', requestData)

 
      if (response.data.success) {
        return response.data.data
      } else {
        error.value = response.data.message 
        return null
      }
    } catch (err: any) {
       error.value = err.response?.data?.message || err.message 
      return null
    } finally {
      loading.value = false
    }
  }

  return {
    loading,
    error,
    data,
    fetchUserData,
    clearUserData,
    loadCachedData,
    exportUserData,
    importUserData,
    downloadAsFile,
    readFile,
    saveRequest
  }
}