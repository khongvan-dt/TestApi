import apiClient from '../utils/axios'

export interface JobApiTestCaseDto {
  id?: number
  caseName?: string
  testData?: Record<string, any> | string  // 👈 chấp nhận cả object & string
  expectedStatus?: number
}

export interface JobApiTestSuiteDto {
  id?: number
  name?: string
  endpoint: string
  method: string
  headers?: Record<string, any> | string   // 👈 chấp nhận cả object & string
  dataBase?: Record<string, any> | string  // 👈 chấp nhận cả object & string
  description?: string
  testCases?: JobApiTestCaseDto[]
}

/**
 * Upsert một suite đơn lẻ
 */
// Hàm upsertSuite phải nhận array
export async function upsertSuite(dtos: JobApiTestSuiteDto[]): Promise<JobApiTestSuiteDto[]> {
  const resp = await apiClient.post('/JobApiTestSuite/upsert', dtos)
  return resp.data
}


/**
 * Upsert nhiều suite song song
 */
export async function upsertSuites(dtos: JobApiTestSuiteDto[]) {
  const promises = dtos.map(d => upsertSuite(d))
  return Promise.all(promises)
}
