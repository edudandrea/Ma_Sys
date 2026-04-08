import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { environment } from '../../app/environments/environment';

export interface Alunos {
  id: number;
  nome: string;
  cpf: string;
  cidade: string;
  estado: string;
  graduacao: string;
  modalidadeId: number;
  endereco: string;
  bairro: string;
  cep: string;
  telefone: string;
  email: string;
  dataNascimento: string;
  redeSocial?: string;
  dataCadastro: string;
  totalAlunos: number;
  ativo: boolean;
  academiaId: number;
  academiaNome?: string;
}

@Injectable({
  providedIn: 'root',
})
export class AlunosService {
  private apiUrl = `${environment.apiUrl}/Alunos`;

  constructor(private http: HttpClient) {}

  pesquisarAlunos(filtro: any): Observable<Alunos[]> {
    const params: any = {};

    if (filtro.id) params.id = filtro.id;
    if (filtro.nome) params.nome = filtro.nome;
    if (filtro.CPF) params.CPF = filtro.CPF;
    if (filtro.graduacao) params.graduacao = filtro.graduacao;

    return this.http.get<Alunos[]>(this.apiUrl, { params });
  }

  getAlunos(): Observable<Alunos[]> {
    return this.http.get<Alunos[]>(this.apiUrl);
  }

  novoAluno(aluno: Partial<Alunos>): Observable<Alunos> {
    return this.http.post<Alunos>(this.apiUrl, aluno);
  }

  atualizarAluno(id: number, aluno: Partial<Alunos>): Observable<Alunos> {
    return this.http.put<Alunos>(`${this.apiUrl}/${id}`, aluno);
  }

  atualizarStatusAluno(id: number, ativo: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, ativo);
  }
}
