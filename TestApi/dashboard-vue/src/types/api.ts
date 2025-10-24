export interface ApiResponse<T> {
  success: boolean
  message?: string
  data?: T
  errors?: Record<string, string[]>
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  username: string
  password: string
}

export interface LoginResponse {
  token: string
  expires: string
  user: {
    id: string
    email: string
    username: string
    fullName: string
  }
}

export interface Collection {
  id: string
  workspaceId: string
  name: string
  description?: string
  parentId?: string
  position: number
  createdAt: string
}

export interface Request {
  id: string
  collectionId: string
  name: string
  description?: string
  method: string
  url: string
  position: number
  createdAt: string
}

export interface TestData {
  id: string
  requestId: string
  name: string
  description?: string
  bodyContent: string
  position: number
}

export interface RequestRunPayload {
  method: string
  url: string
  headers?: Array<{ key: string; value: string }>
  body?: string
  params?: Array<{ key: string; value: string }>
}

export interface RequestRunResponse {
  statusCode: number
  statusText: string
  responseTimeMs: number
  responseSizeBytes: number
  responseHeaders: Record<string, string>
  responseBody: any
}

export interface Environment {
  id: string
  workspaceId: string
  name: string
  description?: string
  isActive: boolean
  variables: Array<{
    key: string
    value: string
  }>
}