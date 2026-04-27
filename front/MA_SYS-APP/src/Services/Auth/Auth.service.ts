import { Injectable } from '@angular/core';
import { environment } from '../../app/environments/environment';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/User`;

  constructor(private http: HttpClient, private router: Router) {}

  login(login: string, password: string) {
    return this.http.post<any>(`${this.apiUrl}/login`, { login, password }).pipe(
      tap((res) => {
        localStorage.setItem(`token`, res.token);
        localStorage.setItem(`usuario`, JSON.stringify(res.usuario));
        localStorage.setItem(`role`, res.usuario?.role ?? '');
      }),
    );
  }

  logout() {
    localStorage.clear();
    this.router.navigate(['/login']);
  }

  getToken() {
    if(typeof window !== 'undefined'){
      return localStorage.getItem(`token`);    
    }
    return null;
    
  }
  
  getRole(): string | null {
    const usuario = localStorage.getItem(`usuario`);
    if (usuario) {
      return JSON.parse(usuario).role ?? null;
    }

    const token = localStorage.getItem(`token`);
    if (!token) return null;

    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.role ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? null;
  }

  isLogged(): boolean{
    if(typeof window !== 'undefined'){
      return !!localStorage.getItem(`token`);  
    }
    return false;
  }
}
