import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../app/environments/environment';

export interface PagamentoAcademia {
  id: number;
  academiaId: number;
  nomeAcademia: string;
  valor: number;
  dataCriacao: string;
  dataVencimento: string;
  dataPagamento?: string;
  status: string;
  descricao?: string;
  formaPagamentoNome?: string;
  mensalidadeSistemaId?: number;
  aceitaPix: boolean;
  aceitaCartao: boolean;
  mercadoPagoPublicKey?: string;
}

export interface PagamentoAcademiaPixResponse {
  pagamentoId: number;
  status: string;
  payload?: string;
  qrCodeBase64?: string;
  externalId?: string;
  verificacaoAutomaticaDisponivel: boolean;
  mensagem: string;
}

export interface PagamentoAcademiaStatusResponse {
  pagamentoId: number;
  status: string;
  formaPagamentoNome?: string;
  dataPagamento?: string;
}

@Injectable({
  providedIn: 'root',
})
export class PagamentosAcademiasService {
  private apiUrl = `${environment.apiUrl}/PagamentosAcademias`;

  constructor(private http: HttpClient) {}

  listar(): Observable<PagamentoAcademia[]> {
    return this.http.get<PagamentoAcademia[]>(this.apiUrl);
  }

  listarPorAcademia(academiaId: number): Observable<PagamentoAcademia[]> {
    return this.http.get<PagamentoAcademia[]>(`${this.apiUrl}?academiaId=${academiaId}`);
  }

  criarCobranca(payload: {
    academiaId: number;
    mensalidadeSistemaId?: number | null;
    valor: number;
    dataVencimento: string;
    descricao?: string;
  }): Observable<any> {
    return this.http.post(this.apiUrl, payload);
  }

  baixar(id: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/baixar`, {});
  }

  gerarPix(id: number): Observable<PagamentoAcademiaPixResponse> {
    return this.http.post<PagamentoAcademiaPixResponse>(`${this.apiUrl}/${id}/pix`, {});
  }

  pagarComCartao(id: number, payload: {
    payerEmail: string;
    cardToken: string;
    paymentMethodId: string;
    parcelas?: number;
  }): Observable<PagamentoAcademiaStatusResponse> {
    return this.http.post<PagamentoAcademiaStatusResponse>(`${this.apiUrl}/${id}/cartao`, payload);
  }

  consultarStatusAtualizado(id: number): Observable<PagamentoAcademiaStatusResponse> {
    return this.http.get<PagamentoAcademiaStatusResponse>(`${this.apiUrl}/${id}/status-atualizado`);
  }
}
