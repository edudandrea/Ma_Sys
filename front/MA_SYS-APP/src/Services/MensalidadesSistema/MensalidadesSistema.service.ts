import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../app/environments/environment';

export interface MensalidadeSistema {
  id: number;
  valor: number;
  prazoPagamentoDias: number;
  mesesUso: number;
  ativo: boolean;
  aceitaPix: boolean;
  aceitaCartao: boolean;
  descricao?: string;
  mercadoPagoPublicKey?: string;
  mercadoPagoAccessToken?: string;
  dataCadastro: string;
}

@Injectable({
  providedIn: 'root',
})
export class MensalidadesSistemaService {
  private apiUrl = `${environment.apiUrl}/MensalidadesSistema`;

  constructor(private http: HttpClient) {}

  listar(): Observable<MensalidadeSistema[]> {
    return this.http.get<MensalidadeSistema[]>(this.apiUrl);
  }

  criar(payload: Partial<MensalidadeSistema>): Observable<void> {
    return this.http.post<void>(this.apiUrl, payload);
  }

  atualizar(payload: Partial<MensalidadeSistema>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${payload.id}`, payload);
  }

  atualizarStatus(id: number, ativo: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, ativo);
  }
}
