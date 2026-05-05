import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../app/environments/environment';

export interface Federacao {
  id: number;
  nome: string;
  cidade?: string;
  estado?: string;
  email?: string;
  telefone?: string;
  responsavel?: string;
  redeSocial?: string;
  logoUrl?: string;
  ativo: boolean;
  mercadoPagoPublicKey: string;
  mercadoPagoAccessToken?: string;
}

@Injectable({
  providedIn: 'root',
})
export class FederacoesService {
  private apiUrl = `${environment.apiUrl}/Federacoes`;

  constructor(private http: HttpClient) {}

  listar(): Observable<Federacao[]> {
    return this.http.get<Federacao[]>(this.apiUrl);
  }

  criar(payload: Partial<Federacao>): Observable<void> {
    return this.http.post<void>(this.apiUrl, payload);
  }

  atualizar(payload: Partial<Federacao>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${payload.id}`, payload);
  }

  atualizarStatus(id: number, ativo: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, ativo);
  }

  uploadLogo(file: File): Observable<{ logoUrl: string }> {
    const formData = new FormData();
    formData.append('file', file);
    
    // When using FormData with Angular HttpClient, do not set Content-Type header
    // Angular will automatically use multipart/form-data with correct boundary
    return this.http.post<{ logoUrl: string }>(
      `${this.apiUrl}/upload-logo`, 
      formData
    );
  }
}
