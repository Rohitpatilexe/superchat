// src/app/services/auth.service.ts

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = `${environment.apiUrl}/Auth`;

  constructor(private http: HttpClient) {}

  //added parameter for captcha

  login(data: { email: string; password: string; captchaToken: string }): Observable<{ token: string }> {
  return this.http.post<{ token: string }>(`${this.apiUrl}/login`, data);
}

  /**
   * THE FIX: This method was missing and has been added back.
   * It allows a new vendor to set their password.
   */

   //added parameter for captcha
 setupVendor(token: string, password: string, captchaToken: string): Observable<any> {
  return this.http.post(`${this.apiUrl}/setup-vendor/${token}`, {
    password,
    captchaToken
  });
}


  logout(): void {
    localStorage.removeItem('token');
  }

  getUserRole(): string | string[] | null {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const decodedToken: any = jwtDecode(token);
        const roleClaim = decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
        return roleClaim || null;
      } catch (e) {
        console.error('Error decoding token:', e);
        return null;
      }
    }
    return null;
  }
}