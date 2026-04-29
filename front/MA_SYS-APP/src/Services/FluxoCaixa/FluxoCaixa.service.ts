import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../app/environments/environment';

export interface FluxoCaixaResumo {
  totalEntradas: number;
  totalSaidas: number;
  saldo: number;
  totalMovimentos: number;
}

export interface FluxoCaixaMovimento {
  tipo: string;
  origem: string;
  categoria: string;
  descricao: string;
  academiaNome: string;
  valor: number;
  data: string;
  status: string;
  formaPagamentoNome?: string;
}

export interface FluxoCaixaResponse {
  resumo: FluxoCaixaResumo;
  movimentos: FluxoCaixaMovimento[];
}

@Injectable({
  providedIn: 'root',
})
export class FluxoCaixaService {
  private apiUrl = `${environment.apiUrl}/FluxoCaixa`;

  constructor(private http: HttpClient) {}

  listar(academiaId?: number): Observable<FluxoCaixaResponse> {
    let params = new HttpParams();
    if (academiaId) {
      params = params.set('academiaId', academiaId);
    }

    return this.http.get<FluxoCaixaResponse>(this.apiUrl, { params });
  }

  lancar(payload: {
    academiaId?: number | null;
    valor: number;
    data: string;
    tipo: string;
    categoria: string;
    descricao: string;
  }): Observable<void> {
    return this.http.post<void>(this.apiUrl, payload);
  }
}
