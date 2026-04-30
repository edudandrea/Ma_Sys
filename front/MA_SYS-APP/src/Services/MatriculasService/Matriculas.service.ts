import { Injectable } from '@angular/core';
import { environment } from '../../app/environments/environment.prod';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/internal/Observable';

export interface Matriculas {
  id: number;
  alunoId: number;
  academiaId?: number;
  planoId: number;
  formaPagamentoId: number;
  dataInicio: string;
  dataFim?: string;
  status: string;
  alunoNome?: string;
  email?: string;
  telefone?: string;
  planoNome?: string;
  planoValor?: number;
  formaPagamentoNome?: string;
  mensalidadeStatus?: string;
  dataVencimentoMensalidade?: string;
  diasParaVencimento?: number;
}

@Injectable({
  providedIn: 'root',
})
export class MatriculasService {
  private apiUrl = `${environment.apiUrl}/Matriculas`;

  constructor(private http: HttpClient) {}

  getMatriculas(): Observable<Matriculas[]> {
    return this.http.get<Matriculas[]>(this.apiUrl);
  }

  novaMatricula(matricula: Partial<Matriculas>): Observable<Matriculas> {
    return this.http.post<Matriculas>(this.apiUrl, matricula);
  }

  atualizarMatricula(id: number, matricula: Partial<Matriculas>): Observable<Matriculas> {
    return this.http.put<Matriculas>(`${this.apiUrl}/${id}`, matricula);
  }

  excluirMatricula(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
