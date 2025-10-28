import apiClient from '../utils/axios'

export interface CreateExecutionHistoryDto {
  userId: number
  requestId: number
  method: string
  url: string
  headers: string
  queryParams: string
  body: string
  statusCode: number
  statusText: string
  responseHeaders: string
  responseBody: string
  responseTime: number
  errorMessage: string
}


// Tạo mới execution history
export const createExecutionHistory = async (data: CreateExecutionHistoryDto): Promise<CreateExecutionHistoryDto> => {
  try {
    const response = await apiClient.post('/ExecutionHistory', data)

    if (response.data.success) {
      return response.data.data as CreateExecutionHistoryDto
    } else {
      throw new Error(response.data.message || 'Failed to create ExecutionHistory')
    }
  } catch (error: any) {
    throw error
  }
}
