import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, TemplateRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { forkJoin } from 'rxjs';
import { Alunos, AlunosService } from '../Services/AlunosService/Alunosservice';
import { ProfessorService, Professores } from '../Services/ProfessorService/Professor.service';
import {
  Exercicio,
  Treino,
  TreinoExercicio,
  TreinosService,
} from '../Services/TreinosService/Treinos.service';

@Component({
  selector: 'app-treinos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './Treinos.component.html',
  styleUrls: ['./Treinos.component.css'],
})
export class TreinosComponent implements OnInit {
  modalRef?: BsModalRef;
  exercicios: Exercicio[] = [];
  treinos: Treino[] = [];
  alunos: Alunos[] = [];
  professores: Professores[] = [];
  aba: 'exercicios' | 'treinos' = 'exercicios';
  editandoExercicioId: number | null = null;
  editandoTreinoId: number | null = null;
  exercicioParaExcluir: Exercicio | null = null;
  treinoParaExcluir: Treino | null = null;

  exercicioForm = this.createExercicioForm();
  treinoForm = this.createTreinoForm();

  constructor(
    private treinosService: TreinosService,
    private alunosService: AlunosService,
    private professorService: ProfessorService,
    private modalService: BsModalService,
    private spinner: NgxSpinnerService,
    private toastr: ToastrService,
    private cd: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.carregarTudo();
  }

  carregarTudo(): void {
    this.spinner.show();
    forkJoin({
      exercicios: this.treinosService.getExercicios(),
      treinos: this.treinosService.getTreinos(),
      alunos: this.alunosService.getAlunos(),
      professores: this.professorService.getProfessores(),
    }).subscribe({
      next: ({ exercicios, treinos, alunos, professores }) => {
        this.spinner.hide();
        this.exercicios = exercicios ?? [];
        this.treinos = treinos ?? [];
        this.alunos = alunos ?? [];
        this.professores = (professores ?? []).filter((professor) => professor.ativo !== false);
        this.cd.detectChanges();
      },
      error: () => {
        this.spinner.hide();
        this.exercicios = [];
        this.treinos = [];
        this.alunos = [];
        this.professores = [];
        this.toastr.error('Erro ao carregar treinos e exercicios');
        this.cd.detectChanges();
      },
    });
  }

  abrirModalExercicio(template: TemplateRef<any>, exercicio?: Exercicio): void {
    this.editandoExercicioId = exercicio?.id ?? null;
    this.exercicioForm = exercicio
      ? {
          nome: exercicio.nome,
          grupoMuscular: exercicio.grupoMuscular ?? '',
          descricao: exercicio.descricao ?? '',
          ativo: exercicio.ativo,
        }
      : this.createExercicioForm();

    this.modalRef = this.modalService.show(template, { class: 'modal-lg modal-dialog-centered' });
  }

  abrirModalTreino(template: TemplateRef<any>, treino?: Treino): void {
    this.editandoTreinoId = treino?.id ?? null;
    this.treinoForm = treino
      ? {
          alunoId: treino.alunoId,
          professorId: treino.professorId ?? null,
          nome: treino.nome,
          objetivo: treino.objetivo ?? '',
          observacoes: treino.observacoes ?? '',
          ativo: treino.ativo,
          exercicios: treino.exercicios.map((item) => ({ ...item })),
        }
      : this.createTreinoForm();

    if (!this.treinoForm.exercicios.length) {
      this.adicionarExercicioTreino();
    }

    this.modalRef = this.modalService.show(template, { class: 'modal-xl modal-dialog-centered' });
  }

  abrirModalExclusaoExercicio(template: TemplateRef<any>, exercicio: Exercicio): void {
    this.exercicioParaExcluir = exercicio;
    this.modalRef = this.modalService.show(template, { class: 'modal-md modal-dialog-centered' });
  }

  abrirModalExclusaoTreino(template: TemplateRef<any>, treino: Treino): void {
    this.treinoParaExcluir = treino;
    this.modalRef = this.modalService.show(template, { class: 'modal-md modal-dialog-centered' });
  }

  salvarExercicio(): void {
    if (!this.exercicioForm.nome.trim()) {
      this.toastr.warning('Informe o nome do exercicio');
      return;
    }

    const request = this.editandoExercicioId
      ? this.treinosService.atualizarExercicio(this.editandoExercicioId, this.exercicioForm)
      : this.treinosService.salvarExercicio(this.exercicioForm);

    this.spinner.show();
    request.subscribe({
      next: () => {
        this.spinner.hide();
        this.toastr.success(this.editandoExercicioId ? 'Exercicio atualizado' : 'Exercicio cadastrado');
        this.modalRef?.hide();
        this.carregarTudo();
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Erro ao salvar exercicio');
      },
    });
  }

  salvarTreino(): void {
    if (!this.treinoForm.nome.trim() || !this.treinoForm.alunoId) {
      this.toastr.warning('Informe o treino e o aluno');
      return;
    }

    const exercicios = this.treinoForm.exercicios
      .filter((item) => item.exercicioId)
      .map((item, index) => ({ ...item, ordem: index + 1 }));

    if (!exercicios.length) {
      this.toastr.warning('Adicione ao menos um exercicio');
      return;
    }

    const payload = { ...this.treinoForm, exercicios };
    const request = this.editandoTreinoId
      ? this.treinosService.atualizarTreino(this.editandoTreinoId, payload)
      : this.treinosService.salvarTreino(payload);

    this.spinner.show();
    request.subscribe({
      next: () => {
        this.spinner.hide();
        this.toastr.success(this.editandoTreinoId ? 'Treino atualizado' : 'Treino cadastrado');
        this.modalRef?.hide();
        this.carregarTudo();
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Erro ao salvar treino');
      },
    });
  }

  adicionarExercicioTreino(): void {
    this.treinoForm.exercicios = [
      ...this.treinoForm.exercicios,
      {
        exercicioId: 0,
        ordem: this.treinoForm.exercicios.length + 1,
        series: 3,
        repeticoes: 12,
        descanso: '60s',
        observacoes: '',
      },
    ];
  }

  removerExercicioTreino(index: number): void {
    this.treinoForm.exercicios = this.treinoForm.exercicios.filter((_, i) => i !== index);
  }

  excluirExercicio(): void {
    if (!this.exercicioParaExcluir) {
      return;
    }

    this.spinner.show();
    this.treinosService.excluirExercicio(this.exercicioParaExcluir.id).subscribe({
      next: () => {
        this.spinner.hide();
        this.exercicios = this.exercicios.filter((item) => item.id !== this.exercicioParaExcluir?.id);
        this.exercicioParaExcluir = null;
        this.modalRef?.hide();
        this.toastr.success('Exercicio excluido com sucesso');
        this.cd.detectChanges();
      },
      error: (err) => {
        this.spinner.hide();
        this.toastr.error(err?.error?.message || 'Erro ao excluir exercicio');
      },
    });
  }

  excluirTreino(): void {
    if (!this.treinoParaExcluir) {
      return;
    }

    this.spinner.show();
    this.treinosService.excluirTreino(this.treinoParaExcluir.id).subscribe({
      next: () => {
        this.spinner.hide();
        this.treinos = this.treinos.filter((item) => item.id !== this.treinoParaExcluir?.id);
        this.treinoParaExcluir = null;
        this.modalRef?.hide();
        this.toastr.success('Treino excluido com sucesso');
        this.cd.detectChanges();
      },
      error: (err) => {
        this.spinner.hide();
        this.toastr.error(err?.error?.message || 'Erro ao excluir treino');
      },
    });
  }

  imprimirTreino(treino: Treino): void {
    if (typeof window === 'undefined') {
      return;
    }

    const printWindow = window.open('', '_blank', 'width=980,height=720');
    if (!printWindow) {
      this.toastr.error('Nao foi possivel abrir a janela de impressao');
      return;
    }

    const exerciciosHtml = treino.exercicios
      .map(
        (exercicio, index) => `
          <tr>
            <td>${index + 1}</td>
            <td>${exercicio.exercicioNome ?? 'Exercicio'}</td>
            <td>${exercicio.series}x${exercicio.repeticoes}</td>
            <td>${exercicio.descanso || '-'}</td>
            <td>${exercicio.observacoes || '-'}</td>
          </tr>`,
      )
      .join('');

    const html = `<!DOCTYPE html>
      <html lang="pt-BR">
        <head>
          <meta charset="utf-8" />
          <title>Treino - ${treino.nome}</title>
          <style>
            :root {
              --ink: #0f172a;
              --muted: #475569;
              --accent: #1d4ed8;
              --line: #dbe4f0;
              --panel: #f8fbff;
            }
            * { box-sizing: border-box; }
            body {
              margin: 0;
              font-family: 'Poppins', Arial, sans-serif;
              color: var(--ink);
              background: #eef4ff;
            }
            .sheet {
              width: 210mm;
              min-height: 297mm;
              margin: 0 auto;
              padding: 20mm 16mm;
              background: white;
            }
            .hero {
              padding: 18px 20px;
              border-radius: 24px;
              background: linear-gradient(135deg, #0f172a, #1d4ed8);
              color: #eff6ff;
            }
            .eyebrow {
              font-size: 12px;
              text-transform: uppercase;
              letter-spacing: 0.12em;
              opacity: 0.8;
            }
            h1 {
              margin: 10px 0 6px;
              font-size: 30px;
            }
            .hero p {
              margin: 0;
              color: rgba(239, 246, 255, 0.86);
            }
            .meta {
              display: grid;
              grid-template-columns: repeat(3, minmax(0, 1fr));
              gap: 12px;
              margin: 20px 0 24px;
            }
            .meta-card {
              padding: 16px;
              border: 1px solid var(--line);
              border-radius: 18px;
              background: var(--panel);
            }
            .meta-card span {
              display: block;
              margin-bottom: 8px;
              color: var(--muted);
              font-size: 12px;
              text-transform: uppercase;
              letter-spacing: 0.08em;
            }
            .meta-card strong {
              font-size: 18px;
            }
            .section-title {
              margin: 0 0 12px;
              font-size: 18px;
            }
            table {
              width: 100%;
              border-collapse: collapse;
              overflow: hidden;
              border-radius: 18px;
              border: 1px solid var(--line);
            }
            thead {
              background: #e0ecff;
            }
            th, td {
              padding: 14px 12px;
              text-align: left;
              border-bottom: 1px solid var(--line);
              vertical-align: top;
            }
            th {
              font-size: 12px;
              text-transform: uppercase;
              letter-spacing: 0.08em;
              color: var(--muted);
            }
            tbody tr:nth-child(even) {
              background: #f8fbff;
            }
            .notes {
              margin-top: 24px;
              padding: 16px 18px;
              border-radius: 18px;
              border: 1px solid var(--line);
              background: #fcfdff;
            }
            .notes p {
              margin: 8px 0 0;
              color: var(--muted);
              line-height: 1.6;
            }
            .footer {
              margin-top: 28px;
              display: flex;
              justify-content: space-between;
              gap: 16px;
              color: var(--muted);
              font-size: 12px;
            }
            @media print {
              body {
                background: white;
              }
              .sheet {
                width: auto;
                min-height: auto;
                padding: 0;
              }
            }
          </style>
        </head>
        <body>
          <div class="sheet">
            <section class="hero">
              <span class="eyebrow">Plano de treino</span>
              <h1>${treino.nome}</h1>
              <p>${treino.objetivo || 'Treino personalizado para evolucao tecnica e fisica do aluno.'}</p>
            </section>

            <section class="meta">
              <article class="meta-card">
                <span>Aluno</span>
                <strong>${treino.alunoNome}</strong>
              </article>
              <article class="meta-card">
                <span>Total de exercicios</span>
                <strong>${treino.exercicios.length}</strong>
              </article>
              <article class="meta-card">
                <span>Status</span>
                <strong>${treino.ativo ? 'Ativo' : 'Inativo'}</strong>
              </article>
            </section>

            <section>
              <h2 class="section-title">Execucao do treino</h2>
              <table>
                <thead>
                  <tr>
                    <th>#</th>
                    <th>Exercicio</th>
                    <th>Series</th>
                    <th>Descanso</th>
                    <th>Observacoes</th>
                  </tr>
                </thead>
                <tbody>
                  ${exerciciosHtml}
                </tbody>
              </table>
            </section>

            <section class="notes">
              <h2 class="section-title">Orientacoes</h2>
              <p>${treino.observacoes || 'Mantenha a execucao com tecnica controlada, respeite o descanso entre as series e registre qualquer ajuste necessario antes do proximo treino.'}</p>
            </section>

            <footer class="footer">
              <span>Impresso em ${new Date().toLocaleDateString('pt-BR')}</span>
              <span>Martial Arts System</span>
            </footer>
          </div>
        </body>
      </html>`;

    printWindow.document.open();
    printWindow.document.write(html);
    printWindow.document.close();
    printWindow.focus();
    printWindow.print();
  }

  private createExercicioForm() {
    return {
      nome: '',
      grupoMuscular: '',
      descricao: '',
      ativo: true,
    };
  }

  private createTreinoForm() {
    return {
      alunoId: 0,
      professorId: null as number | null,
      nome: '',
      objetivo: '',
      observacoes: '',
      ativo: true,
      exercicios: [] as TreinoExercicio[],
    };
  }
}
