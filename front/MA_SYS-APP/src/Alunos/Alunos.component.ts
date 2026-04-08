import { ChangeDetectorRef, Component, OnInit, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AlunosService, Alunos } from '../Services/AlunosService/Alunosservice';
import { FormsModule } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { Modalidades, ModalidadesService } from '../Services/ModalidadeService/Modalidades.service';
import { Academias, AcademiasService } from '../Services/AcademiaService/Academias.service';

export enum TabsCadastroAluno {
  Pesquisa = 'PESQUISA',
  Cadastro = 'CADASTRO',
}

@Component({
  selector: 'app-cadastro-alunos',
  standalone: true,
  templateUrl: './Alunos.component.html',
  styleUrls: ['./Alunos.component.css'],
  imports: [CommonModule, FormsModule],
})
export class AlunosComponent implements OnInit {
  modalRef?: BsModalRef;
  @ViewChild('inputNomeModal') inputNomeModal!: ElementRef;

  activeTab: TabsCadastroAluno = TabsCadastroAluno.Pesquisa;
  idAluno: number = 0;
  nome: string = '';
  cpf: string = '';
  endereco: string = '';
  bairro: string = '';
  cidade: string = '';
  cep: string = '';
  estado: string = '';
  graduacao: string = '';
  modalidadeId: number = 0;
  telefone: string = '';
  email: string = '';
  modoEdicao = false;
  dataNascimento: string = '';
  dataCadastro: string = '';
  modalidades: Modalidades[] = [];
  redeSocial: string = '';
  ativo: boolean = true;
  obs: string = '';
  

  totalAlunos: number = 0;

  TabsCadastroAluno = TabsCadastroAluno;

  filtroAlunos = {
    id: '',
    nome: '',
    CPF: '',
    graduacao: '',
    modalidadeId: 0,
  };

  tabs = [
    {
      id: TabsCadastroAluno.Pesquisa,
      label: 'Pesquisa de Alunos',
      icon: 'bi bi-search',
    },
    {
      id: TabsCadastroAluno.Cadastro,
      label: 'Cadastro de Alunos',
      icon: 'bi bi-person-vcard',
    },
  ];

  mostrarListaAlunos = false;

  alunosFiltrados: Alunos[] = [];

  selectedAluno?: Alunos & {academiaNome?: string};

  animacaoStatus: 'pulse' | 'shake' | null = null;

  academias: Academias[] = [];

  academiaMap = new Map<number, string>();

  constructor(
    private toastr: ToastrService,
    private alunoService: AlunosService,
    private modalService: BsModalService,
    private spinner: NgxSpinnerService,
    private modalidadesService: ModalidadesService,
    private cd: ChangeDetectorRef,
    private acad: AcademiasService,
  ) {}

  ngOnInit() {
    this.carregarAcademias();
    this.carregarModalidades();
  }

  private debounceTimer: any;

  setActiveTab(tab: TabsCadastroAluno) {
    this.activeTab = tab;
  }

  // Métodos para pesquisar alunos

  pesquisarAlunosAutocomplete(nome: string): void {
    clearTimeout(this.debounceTimer);

    const termo = String(nome ?? '').trim();

    if (termo.length < 2) {
      this.alunosFiltrados = [];
      this.mostrarListaAlunos = false;
      return;
    }

    this.debounceTimer = setTimeout(() => {
      const filtro: any = { nome: termo };

      this.alunoService.pesquisarAlunos(filtro).subscribe({
        next: (res) => {
          this.alunosFiltrados = res ?? [];
          this.mostrarListaAlunos = this.alunosFiltrados.length > 0;
        },
        error: () => {
          this.alunosFiltrados = [];
          this.mostrarListaAlunos = false;
        },
      });
    }, 300); // debounce
  }

  onPesquisarClick(): void {
    const filtro: any = {};
    if (this.filtroAlunos.id) filtro.id = Number(this.filtroAlunos.id);
    if (this.filtroAlunos.nome) filtro.nome = this.filtroAlunos.nome;
    if (this.filtroAlunos.CPF) filtro.CPF = this.filtroAlunos.CPF;
    if (this.filtroAlunos.graduacao) filtro.graduacao = this.filtroAlunos.graduacao;
    if (this.filtroAlunos.modalidadeId > 0) filtro.modalidadeId = this.filtroAlunos.modalidadeId;

    this.pesquisarAlunos(filtro);
  }

  fecharListaAlunosComDelay() {
    setTimeout(() => {
      this.mostrarListaAlunos = false;
    }, 0);
  }

  carregarAcademias() {
    this.spinner.show();
    this.acad.getAcademias().subscribe({
      next: (res) => {
        console.log('Academias recebidas:', res);
        this.spinner.hide();
        this.academias = res;

        this.academiaMap.clear();
        this.academias.forEach((a) => this.academiaMap.set(a.id, a.nome));

        this.cd.markForCheck(); // força Angular atualizar
      },

      error: (err) => {
        console.error(err);
        this.toastr.error('Erro ao carregar Academias');
      },
    });
  }

  selecionarAlunoDaLista(aluno: Alunos) {
    if (!aluno) return;
    this.filtroAlunos.nome = aluno.nome;
    this.filtroAlunos.CPF = aluno.cpf;
    this.mostrarListaAlunos = false;
    this.alunosFiltrados = [];

    this.setAlunoAtual(aluno);
    this.onPesquisarClick();
  }

  onInputNome(event: any) {
    const valor = event.target.value;

    this.filtroAlunos.nome = valor;

    clearTimeout(this.debounceTimer);

    this.debounceTimer = setTimeout(() => {
      if (!valor || valor.length < 2) {
        this.mostrarListaAlunos = false;
        this.alunosFiltrados = [];
        return;
      }

      this.pesquisarAlunosAutocomplete(valor);
    }, 100);
  }

  abrirAutocomplete() {
    if (this.alunosFiltrados.length > 0) {
      this.mostrarListaAlunos = true;
    }
  }

  setAlunoAtual(aluno: Alunos) {
  if (!aluno) return;

  this.selectedAluno = {
    ...aluno,
    academiaNome: this.academiaMap.get(aluno.academiaId) || 'Sem Academia'
  };

  this.idAluno = aluno.id;
  this.nome = aluno.nome;
  this.cpf = aluno.cpf;
  this.endereco = aluno.endereco ?? '';
  this.graduacao = aluno.graduacao;
  this.modalidadeId = aluno.modalidadeId;
  this.telefone = aluno.telefone;
  this.email = aluno.email;
  this.redeSocial = aluno.redeSocial ?? '';
  this.dataNascimento = aluno.dataNascimento?.split('T')[0] ?? '';
  this.bairro = aluno.bairro ?? '';
  this.cidade = aluno.cidade ?? '';
  this.cep = aluno.cep ?? '';
  this.estado = aluno.estado ?? '';
  this.dataCadastro = aluno.dataCadastro?.substring(0, 10);
  this.ativo = aluno.ativo;
}

  pesquisarAlunos(filtro: any): void {
    this.spinner.show();
    this.alunoService.pesquisarAlunos(filtro).subscribe({
      next: (res) => {
        this.spinner.hide();
        if (!res?.length) {
          this.toastr.info('Nenhum aluno encontrado.');
          return;
        }
        this.setAlunoAtual(res[0]);

        this.activeTab = TabsCadastroAluno.Cadastro;
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Erro ao buscar alunos');
      },
    });
  }

  salvarNovoAluno() {
    this.spinner.show();

    const aluno = {
      nome: this.nome,
      cpf: this.cpf,
      graduacao: this.graduacao,
      modalidadeId: this.modalidadeId,
      telefone: this.telefone,
      email: this.email,
      redeSocial: this.redeSocial,
      dataCadastro: this.dataCadastro || new Date().toISOString(),
    };

    console.group('📤 NOVO ALUNO');
    console.log(JSON.stringify(aluno, null, 2));
    console.groupEnd();

    if (this.modalidadeId <= 0) {
      this.toastr.warning('Selecione uma modalidade');
      return;
    }

    this.alunoService.novoAluno(aluno).subscribe({
      next: (res) => {
        this.spinner.hide();
        this.toastr.success('Aluno cadastrado!', 'Sucesso');

        this.setAlunoAtual(res);

        this.activeTab = TabsCadastroAluno.Cadastro;
        this.fecharModal();
      },
      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao salvar aluno', 'Erro');
      },
    });
  }

  atualizarAluno() {
    if (!this.idAluno) {
      this.toastr.warning('Aluno não selecionado.', 'Atenção');
      return;
    }

    if (!this.modoEdicao) return;

    const payload: Partial<Alunos> = {
      nome: String(this.nome ?? '').trim(),
      cpf: this.cpf ?? undefined,
      graduacao: this.graduacao ?? undefined,
      modalidadeId: this.modalidadeId ?? undefined,
      telefone: String(this.telefone ?? '').replace(/\D/g, ''),
      email: this.email ?? undefined,
      endereco: this.endereco ?? undefined,
      bairro: this.bairro ?? undefined,
      cidade: this.cidade ?? undefined,
      estado: this.estado ?? undefined,
      cep: this.cep ?? undefined,
      redeSocial: this.redeSocial ?? undefined,
      dataNascimento: this.dataNascimento,
      ativo: this.ativo,
    };

    console.group('📤 ATUALIZAR ALUNO');
    console.log(JSON.stringify(payload, null, 2));
    console.groupEnd();

    this.spinner.show();

    this.alunoService.atualizarAluno(this.idAluno, payload).subscribe({
      next: (res) => {
        this.spinner.hide();
        this.toastr.success('Aluno atualizado com sucesso!', 'Sucesso');

        // 🔥 mantém estado atualizado
        this.modoEdicao = false;
        this.setAlunoAtual(res);
      },
      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao atualizar aluno', 'Erro');
      },
    });
  }

  ativarEdicao() {
    if (!this.selectedAluno) {
      this.toastr.warning('Pesquise um aluno antes de editar.');

      return;
    }

    this.modoEdicao = true;
  }

  cancelarEdicao() {
    this.modoEdicao = false;

    if (this.selectedAluno) {
      this.setAlunoAtual(this.selectedAluno);
    }
  }
  confirmarCancelarEdicao() {
    this.modalRef?.hide();
    this.cancelarEdicao();
  }

  openModalCancelarEdicao(template: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template, {
      class: 'modal-sm modal-dialog-centered',
    });
  }
  // Métodos para cadastro/edição de alunos - MODAL
  fecharModal() {
    this.modalRef?.hide();
  }

  openModalNovoAluno(template: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template, {
      class: 'modal-lg modal-dialog-centered',
    });

    setTimeout(() => {
      this.inputNomeModal?.nativeElement.focus();
    }, 200);
  }

  carregarModalidades() {
    this.modalidadesService.getModalidades().subscribe({
      next: (res) => {
        this.modalidades = res;
        if (this.selectedAluno) {
          this.modalidadeId = this.selectedAluno.modalidadeId;
        }
      },

      error: (err) => {
        console.error(err);
        this.toastr.error('Erro ao carregar modalidades');
      },
    });
  }

  alterarStatusAluno() {
    this.modoEdicao = false;

    const novoStatus = !this.ativo;

    this.alunoService.atualizarStatusAluno(this.idAluno, novoStatus).subscribe({
      next: () => {
        this.ativo = novoStatus;

        if (this.selectedAluno) {
          this.selectedAluno.ativo = novoStatus;
        }
        this.animacaoStatus = novoStatus ? 'pulse' : 'shake';

        setTimeout(() => {
          this.animacaoStatus = null;
        }, 600);

        this.cd.markForCheck();

        this.toastr.success(`Aluno ${novoStatus ? 'ativado' : 'desativado'}`);
      },

      error: () => {
        this.toastr.error('Erro ao alterar status');
      },
    });
  }
}
