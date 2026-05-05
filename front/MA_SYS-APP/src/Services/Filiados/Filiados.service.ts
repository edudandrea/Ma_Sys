import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../app/environments/environment';

export interface Filiado {
  id: number;
  slug?: string;
  nome: string;
  cidade?: string;
  estado?: string;
  email?: string;
  telefone?: string;
  logoUrl?: string;
  redeSocial?: string;
  responsavel?: string;
  dataCadastro?: string;
  ativo: boolean;
  menuAberto?: boolean;
}

export interface PagamentoFiliado {
  id: number;
  filiadoId: number;
  nomeFiliado: string;
  valor: number;
  dataCriacao: string;
  dataVencimento: string;
  dataPagamento?: string;
  status: string;
  descricao?: string;
  formaPagamentoNome?: string;
}

export interface PagamentoFiliadoPixResponse {
  pagamentoId: number;
  status: string;
  payload?: string;
  qrCodeBase64?: string;
  externalId?: string;
  verificacaoAutomaticaDisponivel: boolean;
  mensagem: string;
}

export interface PagamentoFiliadoStatusResponse {
  pagamentoId: number;
  status: string;
  formaPagamentoNome?: string;
  dataPagamento?: string;
  mensagem?: string;
}

@Injectable({
  providedIn: 'root',
})
export class FiliadosService {
  private apiUrl = `${environment.apiUrl}/Filiados`;
  private pagamentosUrl = `${environment.apiUrl}/PagamentosFiliados`;

  constructor(private http: HttpClient) {}

  getFiliados(): Observable<Filiado[]> {
    return this.http.get<Filiado[]>(this.apiUrl);
  }

  novoFiliado(filiado: Partial<Filiado>): Observable<Filiado> {
    return this.http.post<Filiado>(this.apiUrl, filiado);
  }

  atualizarFiliado(filiado: Partial<Filiado>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${filiado.id}`, filiado);
  }

  atualizarStatus(id: number, ativo: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, ativo);
  }

  excluirFiliado(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  uploadLogo(file: File): Observable<{ logoUrl: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ logoUrl: string }>(`${this.apiUrl}/upload-logo`, formData);
  }

  listarCobrancas(filiadoId: number): Observable<PagamentoFiliado[]> {
    return this.http.get<PagamentoFiliado[]>(this.pagamentosUrl, { params: { filiadoId } });
  }

  criarCobranca(payload: any): Observable<PagamentoFiliado> {
    return this.http.post<PagamentoFiliado>(this.pagamentosUrl, payload);
  }

  baixarCobranca(id: number): Observable<void> {
    return this.http.patch<void>(`${this.pagamentosUrl}/${id}/baixar`, {});
  }

  gerarPix(id: number): Observable<any> {
    return this.http.post<any>(`${this.pagamentosUrl}/${id}/pix`, {});
  }

  getPagamentoConfigPublico(federacaoId: number): Observable<any> {
    return this.http.get<any>(`${this.pagamentosUrl}/public/${federacaoId}/config`);
  }

  getFormasPagamentoPublicas(federacaoId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.pagamentosUrl}/public/${federacaoId}/formas-pagamento`);
  }

  buscarFiliadoPublico(federacaoId: number, payload: { email: string; telefone: string }): Observable<any> {
    return this.http.post<any>(`${this.pagamentosUrl}/public/${federacaoId}/buscar`, payload);
  }

  gerarPixPublico(payload: any): Observable<PagamentoFiliadoPixResponse> {
    return this.http.post<PagamentoFiliadoPixResponse>(`${this.pagamentosUrl}/public/pix`, payload);
  }

  pagarCartaoPublico(payload: any): Observable<PagamentoFiliadoStatusResponse> {
    return this.http.post<PagamentoFiliadoStatusResponse>(`${this.pagamentosUrl}/public/cartao`, payload);
  }

  consultarStatusPublico(federacaoId: number, pagamentoId: number): Observable<PagamentoFiliadoStatusResponse> {
    return this.http.get<PagamentoFiliadoStatusResponse>(`${this.pagamentosUrl}/public/${federacaoId}/${pagamentoId}/status`);
  }
}
