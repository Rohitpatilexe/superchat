// src/app/pages/login.component.ts

/*import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NgxCaptchaModule } from 'ngx-captcha';
import { environment } from '../../environments/environment'

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="flex items-center justify-center min-h-screen bg-gray-50">
      <div class="w-full max-w-md p-8 space-y-6 bg-white rounded-lg shadow-md">
        <h2 class="text-2xl font-bold text-center text-gray-900">Sign in to your account</h2>
        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="space-y-6">
          <div>
            <label for="email" class="text-sm font-medium text-gray-700">Email</label>
            <input id="email" type="email" formControlName="email" required class="w-full px-3 py-2 mt-1 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500">
          </div>
          <div>
            <label for="password" class="text-sm font-medium text-gray-700">Password</label>
            <input id="password" type="password" formControlName="password" required class="w-full px-3 py-2 mt-1 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500">
          </div>
          <p *ngIf="errorMessage" class="text-sm text-red-600">{{ errorMessage }}</p>
          <button type="submit" [disabled]="loginForm.invalid || isSubmitting" class="w-full px-4 py-2 font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:bg-gray-400">
            {{ isSubmitting ? 'Signing in...' : 'Sign in' }}
          </button>
        </form>
      </div>
    </div>
  `,
})
export class LoginComponent {
  loginForm: FormGroup;
  errorMessage = '';
  isSubmitting = false;

  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit() {
    if (this.loginForm.invalid) return;
    this.isSubmitting = true;
    this.errorMessage = '';

    this.authService.login(this.loginForm.value as any).subscribe({
      next: (res) => {
        localStorage.setItem('token', res.token);
        const roles = this.authService.getUserRole();
        
        // Determine the primary role for navigation
        const primaryRole = Array.isArray(roles) ? roles[0] : roles;
        
        console.log('Decoded Primary Role:', primaryRole);

        // Use the clear if/else if navigation logic
        if (primaryRole === 'Admin') {
          this.router.navigate(['/admin/dashboard']);
        } else if (primaryRole === 'Leadership') {
          this.router.navigate(['/leadership/dashboard']);
        } else if (primaryRole === 'Vendor') {
          this.router.navigate(['/vendor/dashboard']);
        } else {
          this.isSubmitting = false;
          this.errorMessage = 'Invalid role received from token.';
        }
      },
      error: () => {
        this.errorMessage = 'Invalid email or password';
        this.isSubmitting = false;
      }
    });
  }
}*/

import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ReCaptchaV3Service, NgxCaptchaModule } from 'ngx-captcha';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NgxCaptchaModule],
  template: `
    <div class="flex items-center justify-center min-h-screen bg-gray-50">
      <div class="w-full max-w-md p-8 space-y-6 bg-white rounded-lg shadow-md">
        <h2 class="text-2xl font-bold text-center text-gray-900">Sign in to your account</h2>
        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="space-y-6">
          <div>
            <label for="email" class="text-sm font-medium text-gray-700">Email</label>
            <input id="email" type="email" formControlName="email" required class="w-full px-3 py-2 mt-1 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500">
          </div>
          <div>
            <label for="password" class="text-sm font-medium text-gray-700">Password</label>
            <input id="password" type="password" formControlName="password" required class="w-full px-3 py-2 mt-1 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500">
          </div>
          <p *ngIf="errorMessage" class="text-sm text-red-600">{{ errorMessage }}</p>
          <button type="submit" [disabled]="loginForm.invalid || isSubmitting" class="w-full px-4 py-2 font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:bg-gray-400">
            {{ isSubmitting ? 'Signing in...' : 'Sign in' }}
          </button>
        </form>
      </div>
    </div>
  `,
})
export class LoginComponent {
  loginForm: FormGroup;
  errorMessage = '';
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private recaptchaV3Service: ReCaptchaV3Service
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit() {
    if (this.loginForm.invalid) return;
    this.isSubmitting = true;
    this.errorMessage = '';

    try {
      this.recaptchaV3Service.execute(
        environment.recaptchaSiteKey,
        'login',
        (token: string) => {
          const payload = {
            ...this.loginForm.value,
            captchaToken: token
          };

          this.authService.login(payload).subscribe({
            next: (res) => {
              localStorage.setItem('token', res.token);
              const roles = this.authService.getUserRole();
              const primaryRole = Array.isArray(roles) ? roles[0] : roles;
              console.log('Decoded Primary Role:', primaryRole);

              if (primaryRole === 'Admin') {
                this.router.navigate(['/admin/dashboard']);
              } else if (primaryRole === 'Leadership') {
                this.router.navigate(['/leadership/dashboard']);
              } else if (primaryRole === 'Vendor') {
                this.router.navigate(['/vendor/dashboard']);
              } else {
                this.isSubmitting = false;
                this.errorMessage = 'Invalid role received from token.';
              }
            },
            error: () => {
              this.errorMessage = 'Invalid email or password';
              this.isSubmitting = false;
            }
          });
        }
      );
    } catch (error) {
      this.errorMessage = 'CAPTCHA verification failed.';
      this.isSubmitting = false;
      console.error('reCAPTCHA error:', error);
    }
  }
}


