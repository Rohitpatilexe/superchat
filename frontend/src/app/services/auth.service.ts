import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = `${environment.apiUrl}/Auth`;

  constructor(private http: HttpClient) {}

  login(data: { email: string; password: string }): Observable<{ token: string }> {
    return this.http.post<{ token: string }>(`${this.apiUrl}/login`, data);
  }

  setupVendor(token: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/setup-vendor/${token}`, { password });
  }

  logout(): void {
    localStorage.removeItem('token');
  }

  /**
   * NEW METHOD: Decodes the JWT token from local storage.
   * This provides access to all claims within the token, like PublicId and roles.
   * @returns The decoded token object, or null if no token exists.
   */
  public getDecodedToken(): any | null {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        return jwtDecode(token);
      } catch (e) {
        console.error('Error decoding token:', e);
        return null;
      }
    }
    return null;
  }

  getUserRole(): string | string[] | null {
    // This method can now be simplified by using the new helper method.
    const decodedToken = this.getDecodedToken();
    if (decodedToken) {
      const roleClaim = decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      return roleClaim || null;
    }
    return null;
  }
}