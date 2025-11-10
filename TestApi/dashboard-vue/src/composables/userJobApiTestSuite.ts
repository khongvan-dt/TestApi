import apiClient from '../utils/axios'

// --- DTO CHO TEST CASE (CHÁU) ---
export interface JobApiTestCaseDto {
    id?: number
    caseName?: string
    testData?: Record<string, any> | string 
    expectedStatus?: number
}

// --- DTO CHO TEST SUITE (CON) (Dùng cho Payload) ---
export interface JobApiTestSuiteDto {
    id?: number
    endpoint: string
    method: string
    headers?: Record<string, any> | string
    dataBase?: Record<string, any> | string
    description?: string
    testCases?: JobApiTestCaseDto[]
}

// --- DTO CHO JOB SCHEDULE PAYLOAD (MẸ) ---
export interface JobSchedulePayload {
    id?: number
    name: string
    description?: string
    scheduleType: 'daily' | 'interval'
    dailyTime?: string // Dùng string "HH:MM"
    intervalValue?: number
    intervalUnit?: 'minutes' | 'hours'
    testSuites: JobApiTestSuiteDto[] // Danh sách các Test Suites con
}

// --- INTERFACE CHO TEST SUITE DETAIL (Để dùng trong Modal Detail) ---
export interface JobApiTestSuiteDetail {
    id: number;
    endpoint: string;
    method: string;
    description?: string;
    // Bổ sung các trường cần hiển thị:
    headers?: string;
    dataBaseTest?: string;
    caseTest?: string;
    // testCases?: JobApiTestCaseDetail[]; // Không cần fetch sâu hơn cho Modal chính
}


// --- INTERFACE CHO DANH SÁCH JOB (LIST ITEM) ---
export interface JobScheduleListItem {
    id: number
    name: string
    description?: string
    scheduleType: 'daily' | 'interval' | string // Thêm string vì có thể có giá trị khác
    runAtTime?: string // TimeSpan format "HH:mm:ss"
    intervalMinutes?: number
    isActive: boolean
    createdAt: string
    updatedAt?: string
    lastRunAt?: string
    nextRunAt?: string
}

// --- INTERFACE CHO CHI TIẾT JOB (DETAIL ITEM) ---
// ✅ KHẮC PHỤC LỖI TYPE SAFETY: Mở rộng JobScheduleListItem và thêm danh sách Suites
export interface JobScheduleDetailItem extends JobScheduleListItem {
    // Thêm các thuộc tính chỉ có trong detail API
    jobApiTestSuites?: JobApiTestSuiteDetail[]; 
    
    // Nếu API trả về User chi tiết, bạn có thể khai báo:
    user?: any; 
}

// Giả định server trả về JobScheduleApiTest Entity đã tạo
export interface JobScheduleResponse {
    id: number;
    name: string;
    scheduleType: string;
}

// --- HÀM API ĐÃ SỬA KIỂU TRẢ VỀ ---

// Lấy danh sách Job Schedule (Giữ nguyên)
export const getJobScheduleList = async (): Promise<JobScheduleListItem[]> => {
    const response = await apiClient.get('/JobApiTestSuite/list')
    return response.data
}

 export const getJobScheduleDetail = async (jobScheduleId: number): Promise<JobScheduleDetailItem> => {
    const response = await apiClient.get(`/JobApiTestSuite/detail/${jobScheduleId}`)
    return response.data
}

// Xóa Job Schedule (Giữ nguyên)
export const deleteJobSchedule = async (jobScheduleId: number): Promise<any> => {
    const response = await apiClient.delete(`/JobApiTestSuite/${jobScheduleId}`)
    return response.data
}

// Upsert Job Schedule (Giữ nguyên)
export async function upsertJobSchedule(payload: JobSchedulePayload): Promise<JobScheduleResponse> {
    const resp = await apiClient.post('/JobApiTestSuite/upsert-schedule', payload)
    return resp.data
}

// Toggle Job Schedule Status (Giữ nguyên)
export const toggleJobScheduleStatus = async (jobScheduleId: number): Promise<any> => {
    const response = await apiClient.patch(`/JobApiTestSuite/toggle-status/${jobScheduleId}`)
    return response.data
}