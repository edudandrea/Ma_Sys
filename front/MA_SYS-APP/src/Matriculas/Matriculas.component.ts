import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnInit,
  TemplateRef,
} from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { ModalidadesService } from '../Services/ModalidadeService/Modalidades.service';
import { AcademiasService } from '../Services/AcademiaService/Academias.service';
import { ToastrService } from 'ngx-toastr';
import { NgxSpinnerService } from 'ngx-spinner';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Matriculas, MatriculasService } from '../Services/MatriculasService/Matriculas.service';
import { AlunosService } from '../Services/AlunosService/Alunosservice';
import { PlanosService } from '../Services/Planos/Planos.service';
import { PagamentosService } from '../Services/PagamentosService/Pagamentos.service';

@Component({
  selector: 'app-Matriculas',
  templateUrl: './Matriculas.component.html',
  styleUrls: ['./Matriculas.component.css'],
  imports: [CommonModule, FormsModule],
  
})
export class MatriculasComponent implements OnInit {
  modalRef?: BsModalRef;
  matriculas: (Matriculas & { menuAberto?: boolean , aluno?: any })[] = [];
  matriculaId: number = 0;
  alunoId: number = 0;
  modalidadeId: number = 0;
  dataMatricula: string = '';
  status: string = '';
  valor: number = 0;

  editarId: number | null = null;

  filtroAlunos: string = '';
  alunos: any[] = [];
  alunosFiltrados: any[] = [];
  alunoSelecionado: any = null;

  planos: any[] = [];
  planoId: number = 0;
  planoSelecionado: any = null;

  formasPagamento: any[] = [];
  formaPagamentoId: number = 0;

  novaMatricula = {
    alunoId: 0,
    modalidadeId: 0,
    dataMatricula: '',
    status: '',
    valor: 0,
    planiId: 0,
    nome: '',
  };

  constructor(
    private modalService: BsModalService,
    private cd: ChangeDetectorRef,
    private spinner: NgxSpinnerService,
    private toastr: ToastrService,
    private acad: AcademiasService,
    private modalidadesService: ModalidadesService,
    private matriculasService: MatriculasService,
    private alunoService: AlunosService,
    private planoService: PlanosService,
    private pgService: PagamentosService,
  ) {}

  ngOnInit() {
    const hoje = new Date();
    this.dataMatricula = hoje.toISOString().split('T')[0];
    this.carregarMatriculas();
    
  }

  resetForm() {
    this.alunoSelecionado = null;
    this.planoId = 0;
    this.formaPagamentoId = 0;
    this.planoSelecionado = null;
    this.filtroAlunos = '';
    this.alunosFiltrados = [];
  }

  getInicial(nome?: string): string {
    if (!nome) return '?';

    const partes = nome.split(' ');
    return partes.length > 1
      ? (partes[0][0] + partes[1][0]).toUpperCase()
      : partes[0][0].toUpperCase();
  }

  // ----------MODAL ----------
  modalNovaMatricula(template: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template, {
      class: 'modal-md modal-dialog-centered',
    });
    this.carregarAlunos();
    this.carregarPlanos();
    this.carregarFormaPagamento();
  }

  // ------SEARCHS -----------
  filtrarAlunos() {
    const filtro = this.filtroAlunos.toLowerCase();
    this.alunosFiltrados = this.alunos.filter((aluno) => aluno.nome.toLowerCase().includes(filtro));
  }
  selecionarAluno(aluno: any) {
    this.alunoSelecionado = aluno;
    this.filtroAlunos = aluno.nome;
    this.alunosFiltrados = [];
  }

  onPlanoChange() {
    this.planoSelecionado = this.planos.find((p) => p.id === this.planoId);
    console.log('Plano selecionado:', this.planoSelecionado);
  }

  // ---------- GETS ---------

  carregarMatriculas() {
    this.matriculasService.getMatriculas().subscribe({
      next: (res) => {
        console.log('Matriculas:', res);
        this.matriculas = [...res];
        this.cd.detectChanges();
        
      },
      error: (err) => {
        console.error(err);
        this.toastr.error('Erro ao carregar matriculas');
      },
    });
  }

  carregarAlunos() {
    this.alunoService.getAlunos().subscribe({
      next: (res) => {
        console.log('Alunos:', res);
        this.alunos = res;
      },
      error: (err) => {
        console.error(err);
        this.toastr.error('Erro ao carregar alunos');
      },
    });
  }

  carregarPlanos() {
    this.planoService.getPlanos().subscribe({
      next: (res) => {
        console.log('Planos:', res);
        this.planos = res;
      },
      error: (err) => {
        console.error(err);
        this.toastr.error('Erro ao carregar planos');
      },
    });
  }

  carregarFormaPagamento() {
    this.pgService.getFormaPagamentos().subscribe({
      next: (res) => {
        console.log('Formas de Pagamento:', res);
        this.formasPagamento = res;
      },
      error: (err) => {
        console.error(err);
        this.toastr.error('Erro ao carregar formas de pagamento');
      },
    });
  }

  //---------- POST ----------

  cadastrarMatricula() {
    if (!this.alunoSelecionado) {
      this.toastr.warning('Selecione um aluno');
      return;
    }

    if (!this.planoId) {
      this.toastr.warning('Selecione um plano');
      return;
    }

    if (!this.formaPagamentoId) {
      this.toastr.warning('Selecione a forma de pagamento');
      return;
    }

    this.spinner.show();

    const matricula = {
      alunoId: this.alunoSelecionado.id,
      planoId: this.planoId,
      formaPagamentoId: this.formaPagamentoId,
      dataMatricula: this.dataMatricula,
    };

    console.group('📤 NOVA MATRÍCULA');
    console.log(JSON.stringify(matricula, null, 2));
    console.groupEnd();

    this.matriculasService.novaMatricula(matricula).subscribe({
      next: (res) => {
        this.spinner.hide();

        this.toastr.success('Matrícula cadastrada!', 'Sucesso');

        this.resetForm();
        this.modalRef?.hide();
        this.cd.detectChanges();
        this.carregarMatriculas();
      },

      error: (err) => {
        this.spinner.hide();
        console.error(err);

        this.toastr.error('Erro ao salvar matrícula', 'Erro');
      },
    });
  }
}
