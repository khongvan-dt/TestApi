import apiClient from '../utils/axios'

export interface CreateCollectionDto {
  name: string
  description: string
}

export interface CollectionResult {
  id: number
  userId: number
  name: string
  description: string
 }

 export interface DeleteRequestResult {
  success: boolean
  message?: string
  requestId: number
}

export const deleteRequest = async (requestId: number): Promise<DeleteRequestResult | null> => {
  try {
    const response = await apiClient.post(`/DataExport/requestId`, requestId) 

    if (response.data.success) {
       return response.data.data
    } else {
      throw new Error(response.data.message || 'Failed to delete request')
    }
  } catch (error: any) {
     throw error
  }
}


export const getMyCollections = async () => {
  try {
     const response = await apiClient.post('/Collection/myCollections')
    
    if (response.data.success) {
      return response.data.data
    } else {
      throw new Error(response.data.message || 'Failed to get collections')
    }
  } catch (error: any) {
     throw error
  }
}

 export const createCollection = async (data: CreateCollectionDto): Promise<CollectionResult | null> => {
  try {
     
    const response = await apiClient.post('/Collection', data)
    
    if (response.data.success) {
       return response.data.data
    } else {
      throw new Error(response.data.message || 'Failed to create collection')
    }
  } catch (error: any) {
     throw error
  }
}
