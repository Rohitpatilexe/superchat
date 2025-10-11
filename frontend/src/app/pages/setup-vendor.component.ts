// src/app/pages/setup-vendor.component.ts

/*import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="flex items-center justify-center min-h-screen bg-gray-50">
      <div class="w-full max-w-md p-8 space-y-6 bg-white rounded-lg shadow-md">
        <h2 class="text-2xl font-bold text-center">Complete Your Account Setup</h2>
        <form *ngIf="!message" #setupForm="ngForm" (ngSubmit)="onSubmit()" class="space-y-4">
          <div>
            <label for="password" class="text-sm font-medium">Create Password</label>
            <input id="password" type="password" name="password" [(ngModel)]="password" required minlength="6" class="w-full px-3 py-2 mt-1 border rounded-md">
          </div>
          <div>
            <label for="confirmPassword" class="text-sm font-medium">Confirm Password</label>
            <input id="confirmPassword" type="password" name="confirmPassword" [(ngModel)]="confirmPassword" required class="w-full px-3 py-2 mt-1 border rounded-md">
          </div>
           <p *ngIf="error" class="text-sm text-red-600">{{ error }}</p>
          <button type="submit" [disabled]="setupForm.invalid || loading" class="w-full px-4 py-2 font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:bg-gray-400">
            {{ loading ? 'Submitting...' : 'Set Password and Finish' }}
          </button>
        </form>
        <div *ngIf="message" class="text-center">
            <p class="text-lg text-green-700">{{ message }}</p>
            <button (click)="router.navigate(['/login'])" class="mt-4 px-4 py-2 text-white bg-blue-600 rounded-md">Go to Login</button>
        </div>
      </div>
    </div>
  `,
})
export class SetupVendorComponent implements OnInit {
  token = '';
  password = '';
  confirmPassword = '';
  error = '';
  message = '';
  loading = false;

  constructor(
    private route: ActivatedRoute,
    public router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.token = this.route.snapshot.paramMap.get('token') || '';
    if (!this.token) {
        this.error = 'Invalid setup link. No token found.';
    }
  }

  onSubmit() {
    this.error = '';
    if (this.password !== this.confirmPassword) {
      this.error = 'Passwords do not match.';
      return;
    }

    // Check password strength
  const passwordValid = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?":{}|<>]).{8,}$/.test(this.password);
  if (!passwordValid) {
    this.error = 'Password must be at least 8 characters and include uppercase, lowercase, number, and special character.';
    return;
  }
  
    if (!this.token) {
        this.error = 'Cannot submit without a valid setup token.';
        return;
    }
    this.loading = true;
    this.authService.setupVendor(this.token, this.password).subscribe({
        // THE FIX: Added 'any' type to 'res'
        next: (res: any) => {
            this.loading = false;
            this.message = res.message || 'Setup successful! You can now log in.';
        },
        // THE FIX: Added 'any' type to 'err'
        error: (err: any) => {
            this.loading = false;
            this.error = err.error?.message || 'Setup failed. The link may be expired or invalid.';
        }
    });
  }
}*/
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ReCaptchaV3Service, NgxCaptchaModule } from 'ngx-captcha';
import { environment } from '../../environments/environment';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule, NgxCaptchaModule],
  template: `
    <div class="flex items-center justify-center min-h-screen bg-gray-50">
      <div class="w-full max-w-md p-8 space-y-6 bg-white rounded-lg shadow-md">
        <h2 class="text-2xl font-bold text-center">Complete Your Account Setup</h2>
        <form *ngIf="!message" #setupForm="ngForm" (ngSubmit)="onSubmit()" class="space-y-4">
          <div>
            <label for="password" class="text-sm font-medium">Create Password</label>
            <input id="password" type="password" name="password" [(ngModel)]="password" required minlength="6" class="w-full px-3 py-2 mt-1 border rounded-md">
          </div>
          <div>
            <label for="confirmPassword" class="text-sm font-medium">Confirm Password</label>
            <input id="confirmPassword" type="password" name="confirmPassword" [(ngModel)]="confirmPassword" required class="w-full px-3 py-2 mt-1 border rounded-md">
          </div>
          <p *ngIf="error" class="text-sm text-red-600">{{ error }}</p>
          <button type="submit" [disabled]="setupForm.invalid || loading" class="w-full px-4 py-2 font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:bg-gray-400">
            {{ loading ? 'Submitting...' : 'Set Password and Finish' }}
          </button>
        </form>
        <div *ngIf="message" class="text-center">
          <p class="text-lg text-green-700">{{ message }}</p>
          <button (click)="router.navigate(['/login'])" class="mt-4 px-4 py-2 text-white bg-blue-600 rounded-md">Go to Login</button>
        </div>
      </div>
    </div>
  `,
})
export class SetupVendorComponent implements OnInit {
  token = '';
  password = '';
  confirmPassword = '';
  error = '';
  message = '';
  loading = false;

  constructor(
    private route: ActivatedRoute,
    public router: Router,
    private authService: AuthService,
    private recaptchaV3Service: ReCaptchaV3Service
  ) {}

  ngOnInit(): void {
    this.token = this.route.snapshot.paramMap.get('token') || '';
    if (!this.token) {
      this.error = 'Invalid setup link. No token found.';
    }
  }

  onSubmit() {
    this.error = '';
    if (this.password !== this.confirmPassword) {
      this.error = 'Passwords do not match.';
      return;
    }

    const passwordValid = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?":{}|<>]).{8,}$/.test(this.password);
    if (!passwordValid) {
      this.error = 'Password must be at least 8 characters and include uppercase, lowercase, number, and special character.';
      return;
    }

    if (!this.token) {
      this.error = 'Cannot submit without a valid setup token.';
      return;
    }

    this.loading = true;

    this.recaptchaV3Service.execute(
      environment.recaptchaSiteKey,
      'setup_vendor',
      (captchaToken: string) => {
        this.authService.setupVendor(this.token, this.password, captchaToken).subscribe({
          next: (res: any) => {
            this.loading = false;
            this.message = res.message || 'Setup successful! You can now log in.';
          },
          error: (err: any) => {
            this.loading = false;
            this.error = err.error?.message || 'Setup failed. The link may be expired or invalid.';
          }
        });
      }
    );
  }
}
