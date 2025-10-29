import apiClient from '../utils/axios'

export interface UpdateTestDataRequestDto {
  requestId: number
  newTestDataContent: string
}


export const UpdateTestdataRequest = async (data: UpdateTestDataRequestDto): Promise<UpdateTestDataRequestDto> => {
  try {
    const response = await apiClient.post('/Request/updateTestdata', data)

    if (response.data.success) {
      return response.data.data as UpdateTestDataRequestDto
    } else {
      throw new Error(response.data.message || 'Failed to create Request/updateTestdata')
    }
  } catch (error: any) {
    throw error
  }
}
