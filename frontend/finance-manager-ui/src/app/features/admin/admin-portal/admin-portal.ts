import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { AdminService } from '../../../core/services/admin.service';
import { Auth } from '../../../core/services/auth';
import {
  AdminUser,
  AdminCategory,
  AdminCreateCategory,
  AdminSystemStats,
  AuditLog
} from '../../../core/models/admin.model';
import { ProfileModal } from '../../../shared/components/profile-modal/profile-modal';

@Component({
  selector: 'app-admin-portal',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, ProfileModal],
  templateUrl: './admin-portal.html',
  styleUrl: './admin-portal.css'
})
export class AdminPortal implements OnInit {
  private adminService = inject(AdminService);
  private authService = inject(Auth);

  activeTab: 'stats' | 'users' | 'categories' | 'audit' = 'stats';
  isLoading = false;
  feedbackMessage: { text: string; type: 'success' | 'error' } | null = null;
  showProfileModal = false;

  // Data
  stats: AdminSystemStats | null = null;
  users: AdminUser[] = [];
  categories: AdminCategory[] = [];
  auditLogs: AuditLog[] = [];

  // Filter/Search states
  userSearch = '';
  userRoleFilter = 'ALL';
  categorySearch = '';
  categoryTypeFilter = 'ALL';
  auditSearch = '';
  auditActionFilter = 'ALL';

  // Category creation form
  showAddCategoryModal = false;
  newCategory: AdminCreateCategory = {
    name: '',
    type: 'EXPENSE',
    targetUserId: null
  };

  currentAdminEmail = '';

  ngOnInit(): void {
    const currentUser = this.authService.getToken();
    if (currentUser) {
      this.currentAdminEmail = this.authService.getRole();
    }
    this.loadAllData();
  }

  loadAllData(): void {
    this.loadStats();
    this.loadUsers();
    this.loadCategories();
    this.loadAuditLogs();
  }

  setTab(tab: 'stats' | 'users' | 'categories' | 'audit'): void {
    this.activeTab = tab;
    this.clearFeedback();
  }

  // ==========================================
  // STATS
  // ==========================================
  loadStats(): void {
    this.isLoading = true;
    this.adminService.getSystemStats().subscribe({
      next: (data) => {
        this.stats = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.showFeedback('Failed to load system stats', 'error');
        this.isLoading = false;
      }
    });
  }

  // ==========================================
  // USERS
  // ==========================================
  loadUsers(): void {
    this.adminService.getUsers().subscribe({
      next: (data) => {
        this.users = data;
      },
      error: () => {
        this.showFeedback('Failed to load users list', 'error');
      }
    });
  }

  get filteredUsers(): AdminUser[] {
    return this.users.filter((user) => {
      const matchesSearch =
        user.name.toLowerCase().includes(this.userSearch.toLowerCase()) ||
        user.email.toLowerCase().includes(this.userSearch.toLowerCase());
      const matchesRole =
        this.userRoleFilter === 'ALL' || user.role.toUpperCase() === this.userRoleFilter;
      return matchesSearch && matchesRole;
    });
  }

  toggleUserStatus(user: AdminUser): void {
    const nextStatus = !user.isActive;
    const actionLabel = nextStatus ? 'activate' : 'deactivate';

    if (!confirm(`Are you sure you want to ${actionLabel} account "${user.email}"?`)) {
      return;
    }

    this.adminService.updateUserStatus(user.id, nextStatus).subscribe({
      next: () => {
        user.isActive = nextStatus;
        this.showFeedback(`User ${user.email} successfully ${nextStatus ? 'activated' : 'deactivated'}.`, 'success');
        this.loadStats();
        this.loadAuditLogs();
      },
      error: (err) => {
        const msg = err.error?.message || 'Failed to update user status.';
        this.showFeedback(msg, 'error');
      }
    });
  }

  toggleUserRole(user: AdminUser): void {
    const newRole = user.role.toLowerCase() === 'admin' ? 'User' : 'Admin';
    if (!confirm(`Change role of "${user.email}" to "${newRole}"?`)) {
      return;
    }

    this.adminService.updateUserRole(user.id, newRole).subscribe({
      next: () => {
        user.role = newRole;
        this.showFeedback(`User ${user.email} role updated to ${newRole}.`, 'success');
        this.loadAuditLogs();
      },
      error: (err) => {
        const msg = err.error?.message || 'Failed to update user role.';
        this.showFeedback(msg, 'error');
      }
    });
  }

  // ==========================================
  // CATEGORIES
  // ==========================================
  loadCategories(): void {
    this.adminService.getCategories().subscribe({
      next: (data) => {
        this.categories = data;
      },
      error: () => {
        this.showFeedback('Failed to load categories', 'error');
      }
    });
  }

  get filteredCategories(): AdminCategory[] {
    return this.categories.filter((cat) => {
      const matchesSearch =
        cat.name.toLowerCase().includes(this.categorySearch.toLowerCase()) ||
        cat.userName.toLowerCase().includes(this.categorySearch.toLowerCase());
      const matchesType =
        this.categoryTypeFilter === 'ALL' || cat.type.toUpperCase() === this.categoryTypeFilter;
      return matchesSearch && matchesType;
    });
  }

  openAddCategoryModal(): void {
    this.newCategory = {
      name: '',
      type: 'EXPENSE',
      targetUserId: null
    };
    this.showAddCategoryModal = true;
  }

  closeAddCategoryModal(): void {
    this.showAddCategoryModal = false;
  }

  submitAddCategory(): void {
    if (!this.newCategory.name.trim()) {
      this.showFeedback('Category name is required', 'error');
      return;
    }

    this.adminService.createCategory(this.newCategory).subscribe({
      next: (cat) => {
        this.showFeedback(`Category "${cat.name}" created successfully.`, 'success');
        this.showAddCategoryModal = false;
        this.loadCategories();
        this.loadStats();
        this.loadAuditLogs();
      },
      error: (err) => {
        const msg = err.error?.message || 'Failed to create category.';
        this.showFeedback(msg, 'error');
      }
    });
  }

  deleteCategory(cat: AdminCategory): void {
    if (!confirm(`Are you sure you want to delete category "${cat.name}"?`)) {
      return;
    }

    this.adminService.deleteCategory(cat.id).subscribe({
      next: () => {
        this.showFeedback(`Category "${cat.name}" deleted.`, 'success');
        this.loadCategories();
        this.loadStats();
        this.loadAuditLogs();
      },
      error: (err) => {
        const msg = err.error?.message || 'Failed to delete category.';
        this.showFeedback(msg, 'error');
      }
    });
  }

  // ==========================================
  // AUDIT LOGS
  // ==========================================
  loadAuditLogs(): void {
    this.adminService.getAuditLogs(200).subscribe({
      next: (data) => {
        this.auditLogs = data;
      },
      error: () => {
        this.showFeedback('Failed to load audit logs', 'error');
      }
    });
  }

  get filteredAuditLogs(): AuditLog[] {
    return this.auditLogs.filter((log) => {
      const matchesSearch =
        log.details.toLowerCase().includes(this.auditSearch.toLowerCase()) ||
        log.userEmail.toLowerCase().includes(this.auditSearch.toLowerCase()) ||
        log.action.toLowerCase().includes(this.auditSearch.toLowerCase());
      const matchesAction =
        this.auditActionFilter === 'ALL' || log.action === this.auditActionFilter;
      return matchesSearch && matchesAction;
    });
  }

  // Helpers
  showFeedback(text: string, type: 'success' | 'error'): void {
    this.feedbackMessage = { text, type };
    setTimeout(() => {
      if (this.feedbackMessage?.text === text) {
        this.feedbackMessage = null;
      }
    }, 5000);
  }

  clearFeedback(): void {
    this.feedbackMessage = null;
  }

  openProfileModal(): void {
    this.showProfileModal = true;
  }

  closeProfileModal(): void {
    this.showProfileModal = false;
  }
}
