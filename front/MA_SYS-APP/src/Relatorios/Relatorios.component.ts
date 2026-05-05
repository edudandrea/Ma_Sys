import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { RelatorioResumo, RelatoriosService } from '../Services/Relatorios/Relatorios.service';

@Component({
  selector: 'app-Relatorios',
  templateUrl: './Relatorios.component.html',
  styleUrls: ['./Relatorios.component.css'],
  imports: [CommonModule, FormsModule],
})
export class RelatoriosComponent implements OnInit {
  inicio = new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().slice(0, 10);
  fim = new Date().toISOString().slice(0, 10);
  resumo?: RelatorioResumo;
  role = '';

  constructor(
    private relatoriosService: RelatoriosService,
    private toastr: ToastrService,
  ) {}

  ngOnInit() {
    const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
    this.role = usuario.role || '';
    this.carregar();
  }

  get isSistema(): boolean {
    return this.role === 'Admin' || this.role === 'SuperAdmin';
  }

  get entidadeLabel(): string {
    if (this.role === 'Federacao') return 'Filiados';
    if (this.isSistema) return 'Academias';
    return 'Alunos';
  }

  carregar() {
    this.relatoriosService.resumo(this.inicio, this.fim).subscribe({
      next: (res) => this.resumo = res,
      error: () => this.toastr.error('Nao foi possivel carregar o relatorio.'),
    });
  }
}
