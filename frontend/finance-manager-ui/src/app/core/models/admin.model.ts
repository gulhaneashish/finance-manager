export interface AdminUser {
  id: number;
  name: string;
  email: string;
  role: string;
  isActive: boolean;
  createdAt: string;
}

export interface AdminCategory {
  id: number;
  name: string;
  type: string;
  userId: number;
  userName: string;
  userEmail: string;
}

export interface AdminCreateCategory {
  name: string;
  type: string;
  targetUserId?: number | null;
}

export interface AdminUpdateCategory {
  name: string;
  type: string;
}

export interface AdminRecentActivity {
  title: string;
  details: string;
  type: string;
  timestamp: string;
}

export interface AdminSystemStats {
  totalUsers: number;
  activeUsers: number;
  deactivatedUsers: number;
  newUsersThisMonth: number;
  totalCategories: number;
  totalExpenseCategories: number;
  totalIncomeCategories: number;
  totalAuditLogs: number;
  recentActivities: AdminRecentActivity[];
}

export interface AuditLog {
  id: number;
  userId?: number;
  userEmail: string;
  action: string;
  details: string;
  timestamp: string;
}
