import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { environment } from '../../app/environments/environment.prod';
import { HttpClient } from '@angular/common/http';

export interface Professores {
  id: number;
  academiaId: number;  
  nome: string;
  modalidadeId: number;
  graduacao: string;
  email: string;
  telefone: string;
  ativo: boolean;
  nomeAcademia: string;
  totalAlunos: number;
}

@Injectable({
  providedIn: 'root',
})
export class ProfessorService {
  private apiUrl = `${environment.apiUrl}/Professores`;

  constructor(private http: HttpClient) {}

  getProfessores(){
    return this.http.get<any[]>(this.apiUrl);
  }

  novoProfessor(professor: Partial<Professores>): Observable<Professores> {
    return this.http.post<Professores>(this.apiUrl, professor);
  }

  atualizarStatus(id: number, ativo: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, ativo);
  }

  excluirProfessor(id: number): Observable<Professores>{
    return this.http.delete<Professores>(`${this.apiUrl}/${id}`)
  }
}
