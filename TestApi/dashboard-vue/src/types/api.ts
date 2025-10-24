export interface ApiResponse<T> {
  success: boolean
  data?: T
  message?: string
}

export interface LoginRequest {
  usernameOrEmail: string
  password: string
}

export interface LoginResponse {
  token: string
  expiresIn: number
  user: {
    id: string
    username: string
    email: string
    fullName?: string
  }
}

export interface RegisterRequest {
  username: string
  email: string
  password: string
  fullName?: string
}

export interface Collection {
  id: string
  name: string
  description?: string
}

export interface Request {
  id: string
  name: string
  method: string
  url: string
}

export interface RequestRunPayload {
  url: string
  method: string
  headers?: Record<string, string>
  body?: string
}

export interface RequestRunResponse {
  statusCode: number
  statusText: string
  headers: Record<string, string>
  body: string
  responseTimeMs: number
  responseSizeBytes: number
}

export interface TestData {
  id: string
  name: string
  value: string
}

export interface Environment {
  id: string
  name: string
  variables: Record<string, string>
}