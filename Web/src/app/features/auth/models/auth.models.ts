export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  userId: number;
  name: string;
  email: string;
  role: string;
  departmentId: number;
}

export interface RefreshTokenResponse {
  accessToken: string;
  refreshToken: string;
}

export interface AuthUser {
  userId: number;
  name: string;
  email: string;
  role: string;
  departmentId: number;
}
