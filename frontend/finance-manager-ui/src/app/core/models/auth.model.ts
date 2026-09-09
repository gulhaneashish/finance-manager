export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  userId: number;
  name: string;
  email: string;
}

export interface AuthUser {
  userId: number;
  name: string;
  email: string;
}