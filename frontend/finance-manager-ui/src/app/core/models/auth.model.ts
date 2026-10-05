export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken?: string;
  refreshTokenExpiresAt?: string;
  userId: number;
  name: string;
  email: string;
  role?: string;
}

export interface RefreshTokenRequest {
  token: string;
  refreshToken: string;
}

export interface AuthUser {
  userId: number;
  name: string;
  email: string;
  role?: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface RegisterResponse {
  id: number;
  name: string;
  email: string;
  createdAt: string;
}