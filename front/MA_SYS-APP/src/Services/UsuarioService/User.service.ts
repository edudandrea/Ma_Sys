import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { environment } from '../../app/environments/environment.prod';
import { HttpClient } from '@angular/common/http';

export interface Usuarios {
  userId: number;
  userName: string;
  login: string;
  email?: string;
  password: string;
  academiaId?: number;
  federacaoId?: number;
  role: string;
  nomeAcademia: string;
  federacaoNome?: string;
}

export interface BootstrapStatus {
  requiresBootstrap: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private apiUrl = `${environment.apiUrl}/User`;

  constructor(private http: HttpClient) {}

  getBootstrapStatus(): Observable<BootstrapStatus> {
    return this.http.get<BootstrapStatus>(`${this.apiUrl}/bootstrap-status`);
  }

  getUsuarios() {
    return this.http.get<any[]>(this.apiUrl);
  }

  novoUsuario(usuario: Partial<Usuarios>): Observable<Usuarios> {
    return this.http.post<Usuarios>(this.apiUrl, usuario);
  }

  atualizarUsuario(usuario: Partial<Usuarios>): Observable<Usuarios> {
    return this.http.put<Usuarios>(`${this.apiUrl}/${usuario.userId}`, usuario);
  }

  deleteUsuario(userId: number): Observable<Usuarios> {
    return this.http.delete<Usuarios>(`${this.apiUrl}/${userId}`);
  }
}
