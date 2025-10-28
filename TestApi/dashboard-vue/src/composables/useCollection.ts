 import apiClient from '../utils/axios'

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