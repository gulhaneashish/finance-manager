import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  inject
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { Store } from '@ngrx/store';

import { ProfileService } from '../../../core/services/profile.service';
import { UserProfile, UpdateProfileRequest, ChangePasswordRequest } from '../../../core/models/profile.model';
import { setUser } from '../../../store/auth/auth.actions';

@Component({
  selector: 'app-profile-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatIconModule],
  templateUrl: './profile-modal.html',
  styleUrl: './profile-modal.css'
})
export class ProfileModal implements OnInit, OnChanges {
  private fb = inject(FormBuilder);
  private profileService = inject(ProfileService);
  private store = inject(Store);

  @Input() isOpen = false;
  @Output() closed = new EventEmitter<void>();
  @Output() profileUpdated = new EventEmitter<UserProfile>();

  activeTab: 'details' | 'security' = 'details';

  profile: UserProfile | null = null;
  isLoading = false;
  isSaving = false;
  isChangingPassword = false;

  statusMessage: { type: 'success' | 'error'; text: string } | null = null;

  profilePhotoPreview: string | null = null;

  showCurrentPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;

  profileForm: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: ['', [Validators.pattern('^[+0-9\\s-()]{7,20}$')]],
    profilePictureUrl: ['']
  });

  passwordForm: FormGroup = this.fb.group({
    currentPassword: ['', [Validators.required]],
    newPassword: [
      '',
      [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern('^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$')
      ]
    ],
    confirmNewPassword: ['', [Validators.required]]
  });

  ngOnInit(): void {
    if (this.isOpen) {
      this.loadProfile();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isOpen'] && this.isOpen) {
      this.loadProfile();
      this.resetForms();
    }
  }

  loadProfile(): void {
    this.isLoading = true;
    this.statusMessage = null;

    this.profileService.getProfile().subscribe({
      next: (profile) => {
        this.profile = profile;
        this.profilePhotoPreview = profile.profilePictureUrl || null;
        this.profileForm.patchValue({
          name: profile.name,
          email: profile.email,
          phoneNumber: profile.phoneNumber || '',
          profilePictureUrl: profile.profilePictureUrl || ''
        });
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.statusMessage = {
          type: 'error',
          text: err.error?.detail || err.error?.message || 'Unable to retrieve your profile details.'
        };
      }
    });
  }

  resetForms(): void {
    this.statusMessage = null;
    this.passwordForm.reset();
    this.showCurrentPassword = false;
    this.showNewPassword = false;
    this.showConfirmPassword = false;
  }

  onPhotoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];
    if (!file.type.startsWith('image/')) {
      this.statusMessage = {
        type: 'error',
        text: 'Please select a valid image file (PNG, JPG, WEBP).'
      };
      return;
    }

    if (file.size > 2 * 1024 * 1024) {
      this.statusMessage = {
        type: 'error',
        text: 'Selected image must be under 2MB in size.'
      };
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      const dataUrl = reader.result as string;
      this.profilePhotoPreview = dataUrl;
      this.profileForm.patchValue({ profilePictureUrl: dataUrl });
      this.profileForm.markAsDirty();
    };
    reader.readAsDataURL(file);
  }

  removePhoto(): void {
    this.profilePhotoPreview = null;
    this.profileForm.patchValue({ profilePictureUrl: '' });
    this.profileForm.markAsDirty();
  }

  onSaveProfile(): void {
    if (this.profileForm.invalid || !this.profile) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.statusMessage = null;

    const payload: UpdateProfileRequest = {
      name: this.profileForm.value.name.trim(),
      email: this.profileForm.value.email.trim(),
      phoneNumber: this.profileForm.value.phoneNumber?.trim() || null,
      profilePictureUrl: this.profilePhotoPreview || null
    };

    this.profileService.updateProfile(payload).subscribe({
      next: (updated) => {
        this.profile = updated;
        this.isSaving = false;
        this.statusMessage = {
          type: 'success',
          text: 'Profile information updated successfully.'
        };

        // Update NgRx store so Header and other areas sync instantly
        this.store.dispatch(setUser({
          user: {
            userId: updated.id,
            name: updated.name,
            email: updated.email,
            role: updated.role
          }
        }));

        this.profileUpdated.emit(updated);
        this.profileForm.markAsPristine();
      },
      error: (err) => {
        this.isSaving = false;
        this.statusMessage = {
          type: 'error',
          text: err.error?.detail || err.error?.message || 'Failed to update profile.'
        };
      }
    });
  }

  onChangePassword(): void {
    if (this.passwordForm.invalid || !this.profile) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    const { currentPassword, newPassword, confirmNewPassword } = this.passwordForm.value;

    if (newPassword !== confirmNewPassword) {
      this.statusMessage = {
        type: 'error',
        text: 'New password and confirm password do not match.'
      };
      return;
    }

    if (currentPassword === newPassword) {
      this.statusMessage = {
        type: 'error',
        text: 'New password cannot be identical to your current password.'
      };
      return;
    }

    this.isChangingPassword = true;
    this.statusMessage = null;

    const payload: ChangePasswordRequest = {
      currentPassword,
      newPassword,
      confirmNewPassword
    };

    this.profileService.changePassword(payload).subscribe({
      next: (res) => {
        this.isChangingPassword = false;
        this.statusMessage = {
          type: 'success',
          text: res.message || 'Password changed successfully! Active sessions have been invalidated for security.'
        };
        this.passwordForm.reset();
      },
      error: (err) => {
        this.isChangingPassword = false;
        this.statusMessage = {
          type: 'error',
          text: err.error?.detail || err.error?.message || 'Failed to change password. Please verify current password.'
        };
      }
    });
  }

  closeModal(): void {
    this.statusMessage = null;
    this.closed.emit();
  }

  get passwordStrength(): { score: number; label: string; class: string } {
    const pwd = this.passwordForm.get('newPassword')?.value || '';
    if (!pwd) return { score: 0, label: 'None', class: 'none' };

    let score = 0;
    if (pwd.length >= 8) score++;
    if (pwd.length >= 12) score++;
    if (/[a-z]/.test(pwd) && /[A-Z]/.test(pwd)) score++;
    if (/\d/.test(pwd)) score++;
    if (/[^A-Za-z0-9]/.test(pwd)) score++;

    if (score <= 2) return { score: 1, label: 'Weak', class: 'weak' };
    if (score === 3) return { score: 2, label: 'Fair', class: 'fair' };
    if (score === 4) return { score: 3, label: 'Good', class: 'good' };
    return { score: 4, label: 'Strong', class: 'strong' };
  }

  get hasMinLength(): boolean {
    const pwd = this.passwordForm.get('newPassword')?.value || '';
    return pwd.length >= 8;
  }

  get hasUpperAndLower(): boolean {
    const pwd = this.passwordForm.get('newPassword')?.value || '';
    return /[a-z]/.test(pwd) && /[A-Z]/.test(pwd);
  }

  get hasNumber(): boolean {
    const pwd = this.passwordForm.get('newPassword')?.value || '';
    return /\d/.test(pwd);
  }

  get passwordsMatch(): boolean {
    const pwd = this.passwordForm.get('newPassword')?.value;
    const confirm = this.passwordForm.get('confirmNewPassword')?.value;
    return !!pwd && pwd === confirm;
  }
}
