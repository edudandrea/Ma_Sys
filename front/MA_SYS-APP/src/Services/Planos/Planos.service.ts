import { Injectable } from '@angular/core';
import { environment } from '../../app/environments/environment.prod';
import { Observable } from 'rxjs/internal/Observable';
import { HttpClient } from '@angular/common/http';

export interface Planos {
  id: number;
  academiaId?: number | null;
  ownerUserId?: number | null;
  escopo?: string;
  nome: string;
  valor: number;
  ativo: boolean;
  menuAberto?: boolean;
  academiaNome?: string;
  duracaoMeses: number;
  totalAlunos: number;
}

@Injectable({
  providedIn: 'root',
})
export class PlanosService {
  private apiUrl = `${environment.apiUrl}/Planos`;
  constructor(private http: HttpClient) {}

  getPlanos() {
    return this.http.get<any[]>(this.apiUrl);
  }

  novoPlano(plano: Partial<Planos>): Observable<Planos> {
    return this.http.post<Planos>(this.apiUrl, plano);
  }

  atualizarPlano(plano: Partial<Planos>): Observable<Planos> {
    return this.http.put<Planos>(`${this.apiUrl}/${plano.id}`, plano);
  }

  atualizarStatus(id: number, ativo: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, ativo);
  }

  getTotalAlunos(planoId: number) {
    return this.http.get<any>(`${this.apiUrl}/totalAlunos`, {
      params: {planoId: planoId ?? ''},
    });
  }
}
