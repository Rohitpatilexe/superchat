import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  // Public routes
  { path: 'login', loadComponent: () => import('./pages/login.component').then(c => c.LoginComponent) },
  { path: 'setup-vendor/:token', loadComponent: () => import('./pages/setup-vendor.component').then(c => c.SetupVendorComponent) },

  // Main layout for authenticated users
  {
    path: '',
    loadComponent: () => import('./pages/layout.component').then(c => c.LayoutComponent),
    children: [
      // Admin Routes
      { path: 'admin/dashboard', canActivate: [authGuard], data: { expectedRole: 'Admin' }, loadComponent: () => import('./pages/admin/admin-dashboard.component').then(c => c.AdminDashboardComponent) },
      { path: 'admin/leaders', canActivate: [authGuard], data: { expectedRole: 'Admin' }, loadComponent: () => import('./pages/admin/manage-leaders.component').then(c => c.ManageLeadersComponent) },
      
      // Leadership Routes
      { path: 'leadership/dashboard', canActivate: [authGuard], data: { expectedRole: 'Leadership' }, loadComponent: () => import('./pages/leadership/leadership-dashboard.component').then(c => c.LeadershipDashboardComponent) },
      { path: 'leadership/vendors', canActivate: [authGuard], data: { expectedRole: 'Leadership' }, loadComponent: () => import('./pages/leadership/manage-vendors.component').then(c => c.ManageVendorsComponent) },
      { path: 'leadership/jobs', canActivate: [authGuard], data: { expectedRole: 'Leadership' }, loadComponent: () => import('./pages/leadership/manage-jobs.component').then(c => c.ManageJobsComponent) },
      { path: 'leadership/jobs/:id', canActivate: [authGuard], data: { expectedRole: 'Leadership' }, loadComponent: () => import('./pages/leadership/job-detail.component').then(c => c.JobDetailComponent) },
      { path: 'leadership/employees/:id', canActivate: [authGuard], data: { expectedRole: 'Leadership' }, loadComponent: () => import('./pages/leadership/employee-detail.component').then(c => c.EmployeeDetailComponent) },
      // FIX: Added Conversations Route for Leadership
      { path: 'leadership/conversations', canActivate: [authGuard], data: { expectedRole: 'Leadership' }, loadComponent: () => import('./pages/conversations.component').then(c => c.ConversationsComponent) },

      // Vendor Routes
      { path: 'vendor/dashboard', canActivate: [authGuard], data: { expectedRole: 'Vendor' }, loadComponent: () => import('./pages/vendor/vendor-dashboard.component').then(c => c.VendorDashboardComponent) },
      { path: 'vendor/jobs', canActivate: [authGuard], data: { expectedRole: 'Vendor' }, loadComponent: () => import('./pages/vendor/my-jobs.component').then(c => c.MyJobsComponent) },
      { path: 'vendor/jobs/:id', canActivate: [authGuard], data: { expectedRole: 'Vendor' }, loadComponent: () => import('./pages/vendor/vendor-job-detail.component').then(c => c.VendorJobDetailComponent) },
      // FIX: Added Conversations Route for Vendor
      { path: 'vendor/conversations', canActivate: [authGuard], data: { expectedRole: 'Vendor' }, loadComponent: () => import('./pages/conversations.component').then(c => c.ConversationsComponent) },
    ]
  },

  // Fallback routes
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: '**', redirectTo: '/login' }
];