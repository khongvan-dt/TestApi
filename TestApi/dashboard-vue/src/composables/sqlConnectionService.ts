import apiClient from '../utils/axios'

// ==================== INTERFACES ====================
export interface SQLConnection {
  id?: number
  name: string
  connectString: string
  isActive: boolean
  createdAt?: string
  updatedAt?: string
}

export interface CreateSQLConnectionDto {
  id?: number
  name: string
  connectString: string
  isActive?: boolean
}

export interface TestConnectionDto {
  connectionString: string
}

export interface TestConnectionResult {
  success: boolean
  message: string
}

export interface SQLConnectionResult {
  success: boolean
  message: string
  data: SQLConnection
}

export interface SQLConnectionsResult {
  success: boolean
  message: string
  data: SQLConnection[]
}

export interface DeleteConnectionResult {
  success: boolean
  message: string
}
 

export interface SQLQueryRequest {
  connectionString: string
  query: string
  timeout?: number
}

export interface SQLQueryResponse {
  success: boolean
  message: string
  data?: Array<Record<string, any>>
  rowsAffected: number
  executionTimeMs: number
}

export const executeSQLQuery = async (
  connectionString: string,
  query: string,
  timeout: number = 30
): Promise<SQLQueryResponse> => {
  try {
    const response = await apiClient.post('/SQLQuery/execute', {
      connectionString,
      query,
      timeout
    })

    return response.data
  } catch (error: any) {
    return {
      success: false,
      message: error.response?.data?.message || error.message || 'Failed to execute query',
      rowsAffected: 0,
      executionTimeMs: 0
    }
  }
}
// ==================== API FUNCTIONS ====================

/**
 * Get all SQL connections for current user
 */
// export const getMySQLConnections = async (): Promise<SQLConnection[]> => {
//   try {
//     const response = await apiClient.get('/SQLConnectionDB')
    
//     if (response.data.success) {
//       return response.data.data
//     } else {
//       throw new Error(response.data.message || 'Failed to get SQL connections')
//     }
//   } catch (error: any) {
//     throw error
//   }
// }
export const getMySQLConnections = async (): Promise<SQLConnectionsResult> => {
  try {
    console.log("📡 Calling API: GET /SQLConnectionDB")

    const response = await apiClient.get('/SQLConnectionDB')

    console.log("✅ Response:", response.data)

    return {
      success: response.data.success,
      message: response.data.message,
      data: response.data.data || []
    }

  } catch (error: any) {
    console.error("❌ Error getMySQLConnections:", error)

    return {
      success: false,
      message: error.response?.data?.message || error.message || "Error calling API",
      data: []
    }
  }
}

/**
 * Get SQL connection by ID
 */
export const getSQLConnectionById = async (id: number): Promise<SQLConnection> => {
  try {
    const response = await apiClient.get(`/SQLConnectionDB/${id}`)
    
    if (response.data.success) {
      return response.data.data
    } else {
      throw new Error(response.data.message || 'Failed to get SQL connection')
    }
  } catch (error: any) {
    throw error
  }
}

/**
 * Create or Update SQL connection
 * If dto.id exists -> Update
 * If dto.id is null/undefined -> Create
 */
export const saveSQLConnection = async (dto: CreateSQLConnectionDto): Promise<SQLConnection> => {
  try {
    const response = await apiClient.post('/SQLConnectionDB/save', dto)
    
    if (response.data.success) {
      return response.data.data
    } else {
      throw new Error(response.data.message || 'Failed to save SQL connection')
    }
  } catch (error: any) {
    throw error
  }
}

/**
 * Test SQL connection string
 * ✅ FIX: Luôn trả về result, không throw error
 */
export const testSQLConnection = async (connectionString: string): Promise<TestConnectionResult> => {
  try {
    const response = await apiClient.post('/SQLConnectionDB/test', {
      connectionString
    })
    
    // ✅ Luôn trả về result
    return {
      success: response.data.success,
      message: response.data.message || (response.data.success ? 'Connection successful' : 'Connection failed')
    }
  } catch (error: any) {
    // ✅ THÊM DÒNG NÀY - Trả về fail thay vì throw
    return {
      success: false,
      message: error.response?.data?.message || error.message || 'Failed to test connection'
    }
  }
}

/**
 * Delete SQL connection
 */
export const deleteSQLConnection = async (id: number): Promise<DeleteConnectionResult> => {
  try {
    const response = await apiClient.delete(`/SQLConnectionDB/${id}`)
    
    if (response.data.success) {
      return {
        success: true,
        message: response.data.message || 'Connection deleted successfully'
      }
    } else {
      throw new Error(response.data.message || 'Failed to delete SQL connection')
    }
  } catch (error: any) {
    throw error
  }
}

/**
 * Toggle SQL connection active status
 */
export const toggleSQLConnectionStatus = async (connection: SQLConnection): Promise<SQLConnection> => {
  try {
    const updatedConnection: CreateSQLConnectionDto = {
      id: connection.id,
      name: connection.name,
      connectString: connection.connectString,
      isActive: !connection.isActive
    }
    
    return await saveSQLConnection(updatedConnection)
  } catch (error: any) {
    throw error
  }
}