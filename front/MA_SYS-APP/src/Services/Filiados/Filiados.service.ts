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
}
