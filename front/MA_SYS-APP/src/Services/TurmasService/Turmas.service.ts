import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../app/environments/environment';

export interface TurmaAluno {
  alunoId: number;
  nome: string;
}

export interface Turma {
  id: number;
  nome: string;
  descricao?: string;
  diasSemana: string[];
  ativo: boolean;
  alunos: TurmaAluno[];
}

@Injectable({
  providedIn: 'root',
})
export class TurmasService {
  private apiUrl = `${environment.apiUrl}/Turmas`;

  constructor(private http: HttpClient) {}

  getTurmas(): Observable<Turma[]> {
    return this.http.get<Turma[]>(this.apiUrl);
  }

  salvarTurma(payload: Partial<Turma>): Observable<Turma> {
    return this.http.post<Turma>(this.apiUrl, payload);
  }

  atualizarTurma(id: number, payload: Partial<Turma>): Observable<Turma> {
    return this.http.put<Turma>(`${this.apiUrl}/${id}`, payload);
  }

  excluirTurma(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
