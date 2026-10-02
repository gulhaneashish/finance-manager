export interface UserProfile {
  id: number;
  name: string;
  email: string;
  username: string;
  phoneNumber?: string | null;
  profilePictureUrl?: string | null;
  role: string;
  isActive: boolean;
  createdAt: string;
  lastLoginAt?: string | null;
}

export interface UpdateProfileRequest {
  name: string;
  email: string;
  phoneNumber?: string | null;
  profilePictureUrl?: string | null;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}
