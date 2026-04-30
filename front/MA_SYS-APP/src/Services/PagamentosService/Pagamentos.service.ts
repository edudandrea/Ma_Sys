import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../app/environments/environment';
import { Observable } from 'rxjs/internal/Observable';
import { PixResponse } from '../../Model/pix-response.model';

export interface Pagamentos {
  id: number;
  nome: string;
  ativo: boolean;
  taxa: number;
  parcelas: number;
  dias: number;
  menuAberto?: boolean;
}

export interface PagamentoCartaoResponse {
  pagamentoId: number;
  status: string;
  statusDetail?: string;
  mensagem: string;
}

export interface PagamentoStatusResponse {
  pagamentoId: number;
  status: string;
}

export interface PagamentoPixResponse {
  pagamentoId: number;
  status: string;
  payload?: string;
  qrCodeBase64?: string;
  ticketUrl?: string;
  externalId?: string;
  statusDetail?: string;
  ambienteTeste?: boolean;
  verificacaoAutomaticaDisponivel: boolean;
  mensagem: string;
}

@Injectable({
  providedIn: 'root',
})
export class PagamentosService {
  private apiUrl = `${environment.apiUrl}/Pagamentos`;
  private apiUrlFormaPagamento = `${environment.apiUrl}/FormaPagamento`;
  private apiUrlPix = `${environment.apiUrl}/Pix`;

  constructor(private http: HttpClient) {}

  getFormaPagamentos(): Observable<Pagamentos[]> {
    return this.http.get<Pagamentos[]>(this.apiUrlFormaPagamento);
  }

  getFormaPagamentosPublico(slug: string): Observable<Pagamentos[]> {
    return this.http.get<Pagamentos[]>(`${this.apiUrlFormaPagamento}/public/${slug}`);
  }

  novaFormaPgto(formaPg: Partial<Pagamentos>): Observable<Pagamentos> {
    return this.http.post<Pagamentos>(this.apiUrlFormaPagamento, formaPg);
  }

  atualizarStatus(id: number, ativo: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, ativo);
  }

  atualizarFormaPagamento(formaPg: Partial<Pagamentos>): Observable<Pagamentos> {
    return this.http.put<Pagamentos>(`${this.apiUrlFormaPagamento}/${formaPg.id}`, formaPg);
  }

  gerarPix(valor: number): Observable<PixResponse> {
    return this.http.post<PixResponse>(`${this.apiUrlPix}/pix`, { valor });
  }

  gerarPixPublico(slug: string, valor: number, nome = 'Aluno', cidade = 'Cidade'): Observable<PixResponse> {
    return this.http.post<PixResponse>(`${this.apiUrlPix}/public`, { slug, valor, nome, cidade });
  }

  gerarPagamentoPixPublico(payload: any): Observable<PagamentoPixResponse> {
    return this.http.post<PagamentoPixResponse>(`${this.apiUrl}/public/pix`, payload);
  }

  pagarCartaoPublico(payload: any): Observable<PagamentoCartaoResponse> {
    return this.http.post<PagamentoCartaoResponse>(`${this.apiUrl}/public/cartao`, payload);
  }

  pagarCartao(payload: any): Observable<PagamentoCartaoResponse> {
    return this.http.post<PagamentoCartaoResponse>(`${this.apiUrl}/cartao`, payload);
  }

  consultarStatusPagamentoPublico(pagamentoId: number, slug: string): Observable<PagamentoStatusResponse> {
    return this.http.get<PagamentoStatusResponse>(`${this.apiUrl}/public/${pagamentoId}/status`, {
      params: { slug },
    });
  }

  gerarPagamentoPix(payload: any): Observable<PagamentoPixResponse> {
    return this.http.post<PagamentoPixResponse>(`${this.apiUrl}/pix`, payload);
  }

  consultarStatusPagamentoAtualizado(pagamentoId: number): Observable<PagamentoStatusResponse> {
    return this.http.get<PagamentoStatusResponse>(`${this.apiUrl}/${pagamentoId}/status-atualizado`);
  }

  registrarPagamentoManual(payload: any): Observable<PagamentoStatusResponse> {
    return this.http.post<PagamentoStatusResponse>(`${this.apiUrl}/manual`, payload);
  }
}
