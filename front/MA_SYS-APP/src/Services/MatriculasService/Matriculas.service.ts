import { Injectable } from '@angular/core';
import { environment } from '../../app/environments/environment.prod';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/internal/Observable';

export interface Matriculas {
  id: number;
  alunoId: number;
  modalidadeId: number;
  dataMatricula: string;
  nome: string;
  status: string;
  alunoNome?: string;
  modalidadeNome?: string;
  valor: number;
  planiId: number;
}

@Injectable({
  providedIn: 'root',
})
export class MatriculasService {
  private apiUrl = `${environment.apiUrl}/Matriculas`;

  constructor(private http: HttpClient) {}

  getMatriculas(){
    return this.http.get<any[]>(this.apiUrl);
  }

  novaMatricula(matricula: Partial<Matriculas>): Observable<Matriculas> {
    return this.http.post<Matriculas>(this.apiUrl, matricula);
  }
}
