import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, TemplateRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { Alunos, AlunosService } from '../Services/AlunosService/Alunosservice';
import { Turma, TurmasService } from '../Services/TurmasService/Turmas.service';

@Component({
  selector: 'app-turmas',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './Turmas.component.html',
  styleUrls: ['./Turmas.component.css'],
})
export class TurmasComponent implements OnInit {
  modalRef?: BsModalRef;
  turmas: Turma[] = [];
  alunos: Alunos[] = [];
  editandoId: number | null = null;
  turmaParaExcluir: Turma | null = null;
  diasDisponiveis = ['Segunda', 'Terca', 'Quarta', 'Quinta', 'Sexta', 'Sabado', 'Domingo'];

  form = this.createForm();

  constructor(
    private turmasService: TurmasService,
    private alunosService: AlunosService,
    private modalService: BsModalService,
    private spinner: NgxSpinnerService,
    private toastr: ToastrService,
    private cd: ChangeDetectorRef, 
  ) {}

  ngOnInit(): void {
    this.carregarTurmas();
    this.carregarAlunos();
  }

  carregarDados(): void {
    this.carregarTurmas();
    this.carregarAlunos();
  }

  carregarTurmas(): void {
    this.spinner.show();
    this.turmasService.getTurmas().subscribe({
      next: (res) => {
        this.spinner.hide();
        this.turmas = res ?? [];
        this.cd.detectChanges();
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Erro ao carregar turmas');
      },
    });
  }

  carregarAlunos(): void {
    this.alunosService.getAlunos().subscribe({
      next: (res) => {
        this.alunos = res ?? [];
        this.cd.detectChanges();
      },
      error: () => {
        this.alunos = [];
        this.cd.detectChanges();
      },
    });
  }

  abrirModal(template: TemplateRef<any>, turma?: Turma): void {
    this.editandoId = turma?.id ?? null;
    this.form = turma
      ? {
          nome: turma.nome,
          descricao: turma.descricao ?? '',
          ativo: turma.ativo,
          diasSemana: [...turma.diasSemana],
          alunoIds: turma.alunos.map((aluno) => aluno.alunoId),
        }
      : this.createForm();

    this.modalRef = this.modalService.show(template, { class: 'modal-lg modal-dialog-centered' });
  }

  abrirModalExclusao(template: TemplateRef<any>, turma: Turma): void {
    this.turmaParaExcluir = turma;
    this.modalRef = this.modalService.show(template, { class: 'modal-md modal-dialog-centered' });
  }

  alternarDia(dia: string): void {
    if (this.form.diasSemana.includes(dia)) {
      this.form.diasSemana = this.form.diasSemana.filter((item) => item !== dia);
      return;
    }

    this.form.diasSemana = [...this.form.diasSemana, dia];
  }

  salvar(): void {
    if (!this.form.nome.trim()) {
      this.toastr.warning('Informe o nome da turma');
      return;
    }

    if (!this.form.diasSemana.length) {
      this.toastr.warning('Selecione ao menos um dia da semana');
      return;
    }

    const request = this.editandoId
      ? this.turmasService.atualizarTurma(this.editandoId, this.form)
      : this.turmasService.salvarTurma(this.form);

    this.spinner.show();
    request.subscribe({
      next: () => {
        this.spinner.hide();
        this.toastr.success(this.editandoId ? 'Turma atualizada' : 'Turma cadastrada');
        this.modalRef?.hide();
        this.carregarTurmas();
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Erro ao salvar turma');
      },
    });
  }

  excluirTurma(): void {
    if (!this.turmaParaExcluir) {
      return;
    }

    this.spinner.show();
    this.turmasService.excluirTurma(this.turmaParaExcluir.id).subscribe({
      next: () => {
        this.spinner.hide();
        this.toastr.success('Turma excluida com sucesso');
        this.turmas = this.turmas.filter((item) => item.id !== this.turmaParaExcluir?.id);
        this.turmaParaExcluir = null;
        this.modalRef?.hide();
        this.cd.detectChanges();
      },
      error: (err) => {
        this.spinner.hide();
        this.toastr.error(err?.error?.message || 'Erro ao excluir turma');
      },
    });
  }

  nomesAlunos(turma: Turma): string {
    return turma.alunos.length
      ? turma.alunos.map((aluno) => aluno.nome).join(', ')
      : 'Nenhum aluno vinculado';
  }

  private createForm() {
    return {
      nome: '',
      descricao: '',
      ativo: true,
      diasSemana: [] as string[],
      alunoIds: [] as number[],
    };
  }
}
