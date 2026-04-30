import { Injectable } from '@angular/core';
import { environment } from '../../app/environments/environment';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { Router } from '@angular/router';

const SESSION_TIMEOUT_MS = 30 * 60 * 1000;
const SESSION_LAST_ACTIVITY_KEY = 'sessionLastActivityAt';
const ACTIVITY_EVENTS = ['click', 'keydown', 'mousemove', 'scroll', 'touchstart'];

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/User`;
  private idleTimerId: ReturnType<typeof setTimeout> | null = null;
  private idleMonitorStarted = false;
  private lastActivityWriteAt = 0;

  constructor(private http: HttpClient, private router: Router) {}

  login(login: string, password: string) {
    return this.http.post<any>(`${this.apiUrl}/login`, { login, password }).pipe(
      tap((res) => {
        localStorage.setItem(`token`, res.token);
        localStorage.setItem(`usuario`, JSON.stringify(res.usuario));
        localStorage.setItem(`role`, res.usuario?.role ?? '');
        this.registerActivity();
        this.scheduleIdleLogout();
      }),
    );
  }

  logout() {
    this.clearSession();
    this.router.navigate(['/login']);
  }

  initializeIdleTimeout() {
    if (typeof window === 'undefined' || this.idleMonitorStarted) {
      return;
    }

    this.idleMonitorStarted = true;
    ACTIVITY_EVENTS.forEach((eventName) => {
      window.addEventListener(eventName, this.handleUserActivity, { passive: true });
    });

    if (this.getToken()) {
      this.scheduleIdleLogout();
    }
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

  isLogged(): boolean {
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem(`token`);
      if (!token) {
        return false;
      }

      if (this.isIdleExpired()) {
        this.clearSession();
        return false;
      }

      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const exp = Number(payload.exp || 0);
        if (exp && exp * 1000 <= Date.now()) {
          this.clearSession();
          return false;
        }

        return true;
      } catch {
        this.clearSession();
        return false;
      }
    }
    return false;
  }

  shouldAttachToken(): boolean {
    if (!this.isLogged()) {
      return false;
    }

    this.registerActivity();
    return true;
  }

  registerActivity() {
    if (typeof window === 'undefined' || !localStorage.getItem('token')) {
      return;
    }

    const now = Date.now();
    if (now - this.lastActivityWriteAt < 30000) {
      return;
    }

    this.lastActivityWriteAt = now;
    localStorage.setItem(SESSION_LAST_ACTIVITY_KEY, now.toString());
    this.scheduleIdleLogout();
  }

  private readonly handleUserActivity = () => {
    if (!this.isLogged()) {
      return;
    }

    this.registerActivity();
  };

  private isIdleExpired(): boolean {
    const lastActivity = Number(localStorage.getItem(SESSION_LAST_ACTIVITY_KEY) || 0);
    return !lastActivity || Date.now() - lastActivity >= SESSION_TIMEOUT_MS;
  }

  private scheduleIdleLogout() {
    if (typeof window === 'undefined') {
      return;
    }

    if (this.idleTimerId) {
      clearTimeout(this.idleTimerId);
      this.idleTimerId = null;
    }

    const lastActivity = Number(localStorage.getItem(SESSION_LAST_ACTIVITY_KEY) || 0);
    if (!lastActivity || !localStorage.getItem('token')) {
      return;
    }

    const remainingTime = Math.max(SESSION_TIMEOUT_MS - (Date.now() - lastActivity), 0);
    this.idleTimerId = setTimeout(() => {
      if (this.isIdleExpired()) {
        this.logout();
      }
    }, remainingTime);
  }

  private clearSession() {
    localStorage.clear();

    if (this.idleTimerId) {
      clearTimeout(this.idleTimerId);
      this.idleTimerId = null;
    }
  }
}
