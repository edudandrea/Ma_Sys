import { ChangeDetectorRef, Component, OnInit, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { ToastrService } from 'ngx-toastr';
import { NgxSpinnerService } from 'ngx-spinner';
import * as QRCode from 'qrcode';
import { Matriculas, MatriculasService } from '../Services/MatriculasService/Matriculas.service';
import { Alunos, AlunosService } from '../Services/AlunosService/Alunosservice';
import { PlanosService } from '../Services/Planos/Planos.service';
import { PagamentosService } from '../Services/PagamentosService/Pagamentos.service';

@Component({
  selector: 'app-Matriculas',
  standalone: true,
  templateUrl: './Matriculas.component.html',
  styleUrls: ['./Matriculas.component.css'],
  imports: [CommonModule, FormsModule],
})
export class MatriculasComponent implements OnInit {
  modalRef?: BsModalRef;
  matriculas: Matriculas[] = [];

  filtroAlunos = '';
  alunos: Alunos[] = [];
  alunosFiltrados: Alunos[] = [];
  alunoSelecionado: Alunos | null = null;

  planos: any[] = [];
  planoId = 0;
  planoSelecionado: any = null;

  formasPagamento: any[] = [];
  formaPagamentoId = 0;
  formaPagamentoSelecionada: any = null;

  dataMatricula = '';
  status = 'Ativa';
  editarId: number | null = null;
  modoEdicao = false;

  qrCodePix = '';
  showQrCode = false;
  pagamentoPixId: number | null = null;
  verificacaoAutomaticaPix = false;
  mensagemPix = '';
  marcarComoPagoManual = false;

  numeroCartao = '';
  validadeCartao = '';
  cvvCartao = '';
  isCartao = false;
  bandeiraCartao = '';

  constructor(
    private modalService: BsModalService,
    private cd: ChangeDetectorRef,
    private spinner: NgxSpinnerService,
    private toastr: ToastrService,
    private matriculasService: MatriculasService,
    private alunoService: AlunosService,
    private planoService: PlanosService,
    private pgService: PagamentosService,
  ) {}

  ngOnInit(): void {
    this.dataMatricula = new Date().toISOString().split('T')[0];
    this.carregarMatriculas();
  }

  getInicial(nome?: string): string {
    if (!nome) return '?';
    const partes = nome.split(' ');
    return partes.length > 1
      ? (partes[0][0] + partes[1][0]).toUpperCase()
      : partes[0][0].toUpperCase();
  }

  resetForm(): void {
    this.alunoSelecionado = null;
    this.planoId = 0;
    this.planoSelecionado = null;
    this.formaPagamentoId = 0;
    this.formaPagamentoSelecionada = null;
    this.filtroAlunos = '';
    this.alunosFiltrados = [];
    this.dataMatricula = new Date().toISOString().split('T')[0];
    this.status = 'Ativa';
    this.editarId = null;
    this.modoEdicao = false;
    this.qrCodePix = '';
    this.showQrCode = false;
    this.pagamentoPixId = null;
    this.verificacaoAutomaticaPix = false;
    this.mensagemPix = '';
    this.marcarComoPagoManual = false;
    this.numeroCartao = '';
    this.validadeCartao = '';
    this.cvvCartao = '';
    this.isCartao = false;
    this.bandeiraCartao = '';
  }

  modalNovaMatricula(template: TemplateRef<any>): void {
    this.resetForm();
    this.carregarAlunos();
    this.carregarPlanos();
    this.carregarFormaPagamento();
    this.modalRef = this.modalService.show(template, {
      class: 'modal-lg modal-dialog-centered',
    });
  }

  editarMatricula(template: TemplateRef<any>, matricula: Matriculas): void {
    this.modalNovaMatricula(template);
    this.modoEdicao = true;
    this.editarId = matricula.id;
    this.alunoSelecionado = this.alunos.find((aluno) => aluno.id === matricula.alunoId) ?? {
      id: matricula.alunoId,
      nome: matricula.alunoNome ?? 'Aluno',
    } as Alunos;
    this.filtroAlunos = matricula.alunoNome ?? '';
    this.planoId = matricula.planoId;
    this.formaPagamentoId = matricula.formaPagamentoId;
    this.dataMatricula = (matricula.dataInicio ?? '').split('T')[0];
    this.status = matricula.status;
    this.marcarComoPagoManual = false;

    setTimeout(() => {
      this.onPlanoChange();
      this.onFormaPagamentoChange();
    }, 150);
  }

  filtrarAlunos(): void {
    const filtro = this.filtroAlunos.toLowerCase();
    this.alunosFiltrados = this.alunos.filter((aluno) =>
      (aluno.nome || '').toLowerCase().includes(filtro),
    );
  }

  selecionarAluno(aluno: Alunos): void {
    this.alunoSelecionado = aluno;
    this.filtroAlunos = aluno.nome;
    this.alunosFiltrados = [];
  }

  onPlanoChange(): void {
    this.planoSelecionado = this.planos.find((plano) => plano.id == this.planoId);
    this.showQrCode = false;
    this.qrCodePix = '';
  }

  onFormaPagamentoChange(): void {
    this.formaPagamentoSelecionada = this.formasPagamento.find(
      (forma) => forma.id == this.formaPagamentoId,
    );
    const nome = this.formaPagamentoSelecionada?.nome?.toLowerCase() || '';
    this.isCartao = nome.includes('credito') || nome.includes('crédito') || nome.includes('debito');
    this.showQrCode = false;
    this.qrCodePix = '';
    this.numeroCartao = '';
    this.validadeCartao = '';
    this.cvvCartao = '';
  }

  onCartaoInput(event: any): void {
    let valor = event.target.value.replace(/\D/g, '').substring(0, 16);
    valor = valor.replace(/(\d{4})(?=\d)/g, '$1 ');
    this.numeroCartao = valor;
    this.detectarBandeira(valor.replace(/\s/g, ''));
  }

  detectarBandeira(numero: string): void {
    if (/^4/.test(numero)) this.bandeiraCartao = 'visa';
    else if (/^5[1-5]/.test(numero)) this.bandeiraCartao = 'mastercard';
    else if (/^3[47]/.test(numero)) this.bandeiraCartao = 'amex';
    else if (/^6(?:011|5)/.test(numero)) this.bandeiraCartao = 'discover';
    else this.bandeiraCartao = '';
  }

  gerarCodePix(payload: string): void {
    QRCode.toDataURL(payload).then((url) => {
      this.qrCodePix = url;
      this.showQrCode = true;
    });
  }

  gerarPix(): void {
    if (!this.alunoSelecionado || !this.planoSelecionado || !this.formaPagamentoId || !this.editarId) {
      this.toastr.warning('Salve a matricula antes de gerar o PIX');
      return;
    }

    this.pgService
      .gerarPagamentoPix({
        alunoId: this.alunoSelecionado.id,
        matriculaId: this.editarId,
        planoId: this.planoId,
        formaPagamentoId: this.formaPagamentoId,
        valor: this.planoSelecionado.valor,
        nome: this.alunoSelecionado.nome,
        email: this.alunoSelecionado.email,
        cidade: 'Cidade',
      })
      .subscribe({
        next: (res) => {
          this.pagamentoPixId = res.pagamentoId;
          this.verificacaoAutomaticaPix = res.verificacaoAutomaticaDisponivel;
          this.mensagemPix = res.mensagem;

          if (res.qrCodeBase64) {
            this.qrCodePix = `data:image/png;base64,${res.qrCodeBase64}`;
            this.showQrCode = true;
            return;
          }

          if (res.payload) {
            this.gerarCodePix(res.payload);
          }
        },
        error: () => this.toastr.error('Erro ao gerar cobranca PIX'),
      });
  }

  verificarPagamentoPix(): void {
    if (!this.pagamentoPixId) {
      return;
    }

    this.pgService.consultarStatusPagamentoAtualizado(this.pagamentoPixId).subscribe({
      next: (res) => {
        this.toastr.info(`Status atualizado: ${res.status}`);
        this.carregarMatriculas();
      },
      error: () => this.toastr.error('Erro ao consultar status do PIX'),
    });
  }

  carregarMatriculas(): void {
    this.matriculasService.getMatriculas().subscribe({
      next: (res) => {
        this.matriculas = res ?? [];
        this.cd.detectChanges();
      },
      error: () => this.toastr.error('Erro ao carregar matriculas'),
    });
  }

  carregarAlunos(): void {
    this.alunoService.getAlunos().subscribe({
      next: (res) => (this.alunos = res ?? []),
      error: () => this.toastr.error('Erro ao carregar alunos'),
    });
  }

  carregarPlanos(): void {
    this.planoService.getPlanos().subscribe({
      next: (res) => (this.planos = res ?? []),
      error: () => this.toastr.error('Erro ao carregar planos'),
    });
  }

  carregarFormaPagamento(): void {
    this.pgService.getFormaPagamentos().subscribe({
      next: (res) => (this.formasPagamento = res ?? []),
      error: () => this.toastr.error('Erro ao carregar formas de pagamento'),
    });
  }

  cadastrarMatricula(): void {
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

    const request = this.modoEdicao && this.editarId
      ? this.matriculasService.atualizarMatricula(this.editarId, {
          alunoId: this.alunoSelecionado.id,
          planoId: this.planoId,
          formaPagamentoId: this.formaPagamentoId,
          dataInicio: this.dataMatricula,
          status: this.status,
        })
      : this.matriculasService.novaMatricula({
          alunoId: this.alunoSelecionado.id,
          planoId: this.planoId,
          formaPgtoId: this.formaPagamentoId,
          dataInicio: this.dataMatricula,
        } as any);

    this.spinner.show();
    request.subscribe({
      next: (res) => {
        this.spinner.hide();
        this.editarId = res.id;
        this.toastr.success(this.modoEdicao ? 'Matricula atualizada!' : 'Matricula cadastrada!', 'Sucesso');

        if (this.marcarComoPagoManual && this.editarId) {
          this.registrarPagamentoManual();
          return;
        }

        if (this.formaPagamentoSelecionada?.nome?.toLowerCase() === 'pix') {
          this.gerarPix();
        } else {
          this.resetForm();
          this.modalRef?.hide();
        }

        this.carregarMatriculas();
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Erro ao salvar matricula', 'Erro');
      },
    });
  }

  registrarPagamentoManual(): void {
    if (!this.alunoSelecionado || !this.editarId || !this.planoSelecionado || !this.formaPagamentoId) {
      this.spinner.hide();
      this.toastr.warning('Dados insuficientes para marcar a matricula como paga.');
      return;
    }

    this.pgService.registrarPagamentoManual({
      alunoId: this.alunoSelecionado.id,
      matriculaId: this.editarId,
      planoId: this.planoId,
      formaPagamentoId: this.formaPagamentoId,
      valor: this.planoSelecionado.valor || 0,
      dataPagamento: new Date().toISOString(),
    }).subscribe({
      next: () => {
        this.spinner.hide();
        this.toastr.success('Mensalidade marcada como paga.');
        this.resetForm();
        this.modalRef?.hide();
        this.carregarMatriculas();
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Erro ao registrar pagamento manual.');
      }
    });
  }
}
