import { Injectable } from '@angular/core';
import { environment } from '../../app/environments/environment.prod';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/internal/Observable';

export interface Modalidades {
  id: number;
  academiaId: number;
  nomeModalidade: string;
  ativo: boolean;
  menuAberto?: boolean;
  academiaNome?: string;
  totalAlunos: number;
  totalProf: number;
}

@Injectable({
  providedIn: 'root',
})
export class ModalidadesService {
  private apiUrl = `${environment.apiUrl}/Modalidade`;

  constructor(private http: HttpClient) {}

  getModalidades() {
    return this.http.get<any[]>(this.apiUrl);
  }

  novaModalidade(modalidade: Partial<Modalidades>): Observable<Modalidades> {
    return this.http.post<Modalidades>(this.apiUrl, modalidade);
  }

  atualizarStatus(id: number, ativo: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, ativo);
  }

  atualizarModalidade(modalidade: Partial<Modalidades>): Observable<Modalidades> {
    return this.http.put<Modalidades>(`${this.apiUrl}/${modalidade.id}`, modalidade);
  }

  excluirModalidade(id: number): Observable<Modalidades> {
    return this.http.delete<Modalidades>(`${this.apiUrl}/${id}`);
  }
}
