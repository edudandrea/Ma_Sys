import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../app/environments/environment';

export interface Exercicio {
  id: number;
  nome: string;
  grupoMuscular?: string;
  descricao?: string;
  ativo: boolean;
}

export interface TreinoExercicio {
  exercicioId: number;
  exercicioNome?: string;
  ordem: number;
  series: number;
  repeticoes: number;
  descanso?: string;
  observacoes?: string;
}

export interface Treino {
  id: number;
  alunoId: number;
  alunoNome: string;
  nome: string;
  objetivo?: string;
  observacoes?: string;
  ativo: boolean;
  exercicios: TreinoExercicio[];
}

@Injectable({
  providedIn: 'root',
})
export class TreinosService {
  private exerciciosUrl = `${environment.apiUrl}/Exercicios`;
  private treinosUrl = `${environment.apiUrl}/Treinos`;

  constructor(private http: HttpClient) {}

  getExercicios(): Observable<Exercicio[]> {
    return this.http.get<Exercicio[]>(this.exerciciosUrl);
  }

  salvarExercicio(payload: Partial<Exercicio>): Observable<Exercicio> {
    return this.http.post<Exercicio>(this.exerciciosUrl, payload);
  }

  atualizarExercicio(id: number, payload: Partial<Exercicio>): Observable<Exercicio> {
    return this.http.put<Exercicio>(`${this.exerciciosUrl}/${id}`, payload);
  }

  excluirExercicio(id: number): Observable<void> {
    return this.http.delete<void>(`${this.exerciciosUrl}/${id}`);
  }

  getTreinos(): Observable<Treino[]> {
    return this.http.get<Treino[]>(this.treinosUrl);
  }

  salvarTreino(payload: Partial<Treino>): Observable<Treino> {
    return this.http.post<Treino>(this.treinosUrl, payload);
  }

  atualizarTreino(id: number, payload: Partial<Treino>): Observable<Treino> {
    return this.http.put<Treino>(`${this.treinosUrl}/${id}`, payload);
  }

  excluirTreino(id: number): Observable<void> {
    return this.http.delete<void>(`${this.treinosUrl}/${id}`);
  }
}
