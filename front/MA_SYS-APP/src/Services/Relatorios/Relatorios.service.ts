import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../app/environments/environment';

export interface RelatorioResumo {
  escopo: string;
  dataInicio: string;
  dataFim: string;
  totalCadastrados: number;
  totalAtivos: number;
  totalInativos: number;
  totalEmDia: number;
  totalPendentes: number;
  totalEmAtraso: number;
  totalMensalidadesSistema: number;
  totalMensalidadesSistemaPagas: number;
  totalMensalidadesSistemaPendentes: number;
  totalMensalidadesSistemaAtrasadas: number;
  valorMensalidadesSistema: number;
  valorMensalidadesSistemaRecebido: number;
  valorMensalidadesSistemaPendente: number;
}

@Injectable({
  providedIn: 'root',
})
export class RelatoriosService {
  private apiUrl = `${environment.apiUrl}/Relatorios`;

  constructor(private http: HttpClient) {}

  resumo(inicio: string, fim: string): Observable<RelatorioResumo> {
    return this.http.get<RelatorioResumo>(`${this.apiUrl}/resumo`, {
      params: { inicio, fim },
    });
  }
}
