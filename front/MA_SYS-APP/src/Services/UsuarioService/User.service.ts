import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { environment } from '../../app/environments/environment.prod';
import { HttpClient } from '@angular/common/http';

export interface Usuarios {
  userId: number;
  userName: string;
  login: string;
  password: string;
  academiaId: number;
  role: string;
}

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private apiUrl = `${environment.apiUrl}/User`;

  constructor(private http: HttpClient) {}

  getUsuarios() {
    return this.http.get<any[]>(this.apiUrl);
  }

  novoUsuario(usuario: Partial<Usuarios>): Observable<Usuarios> {
    return this.http.post<Usuarios>(this.apiUrl, usuario);
  }

  deleteUsuario(userId: number): Observable<Usuarios> {
    return this.http.delete<Usuarios>(`${this.apiUrl}/${userId}`);
  }

}
