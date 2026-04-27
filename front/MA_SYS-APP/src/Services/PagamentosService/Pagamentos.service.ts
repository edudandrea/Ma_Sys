import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../app/environments/environment.prod';
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

  gerarPixPublico(valor: number, nome = 'Aluno', cidade = 'Cidade'): Observable<PixResponse> {
    return this.http.post<PixResponse>(`${this.apiUrlPix}/public`, { valor, nome, cidade });
  }

  pagarCartaoPublico(payload: any) {
    return this.http.post(`${this.apiUrl}/public/cartao`, payload);
  }
}
