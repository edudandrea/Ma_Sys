import { ChangeDetectorRef, Component, OnDestroy, OnInit, TemplateRef } from '@angular/core';
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
import { loadMercadoPago } from '@mercadopago/sdk-js';
import { environment } from '../app/environments/environment';
import { AcademiasService } from '../Services/AcademiaService/Academias.service';

@Component({
  selector: 'app-Matriculas',
  standalone: true,
  templateUrl: './Matriculas.component.html',
  styleUrls: ['./Matriculas.component.css'],
  imports: [CommonModule, FormsModule],
})
export class MatriculasComponent implements OnInit, OnDestroy {
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
  pixPayload = '';
  isGerandoPix = false;
  marcarComoPagoManual = false;

  numeroCartao = '';
  nomeCartao = '';
  cpfCartao = '';
  validadeCartao = '';
  cvvCartao = '';
  isCartao = false;
  isPix = false;
  bandeiraCartao = '';
  pagamentoId: number | null = null;
  pagamentoStatus = '';
  pagamentoMensagem = '';
  private mp: any = null;
  private mercadoPagoPublicKeyAcademia = '';
  private academiasPagamento: any[] = [];
  private pollingHandle: any = null;

  constructor(
    private modalService: BsModalService,
    private cd: ChangeDetectorRef,
    private spinner: NgxSpinnerService,
    private toastr: ToastrService,
    private matriculasService: MatriculasService,
    private alunoService: AlunosService,
    private planoService: PlanosService,
    private pgService: PagamentosService,
    private academiasService: AcademiasService,
  ) {}

  ngOnInit(): void {
    this.dataMatricula = new Date().toISOString().split('T')[0];
    this.carregarConfiguracaoPagamentoAcademia();
    this.carregarMatriculas();
  }

  ngOnDestroy(): void {
    this.pararPollingStatus();
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
    this.pixPayload = '';
    this.isGerandoPix = false;
    this.marcarComoPagoManual = false;
    this.numeroCartao = '';
    this.nomeCartao = '';
    this.cpfCartao = '';
    this.validadeCartao = '';
    this.cvvCartao = '';
    this.isCartao = false;
    this.isPix = false;
    this.bandeiraCartao = '';
    this.pagamentoId = null;
    this.pagamentoStatus = '';
    this.pagamentoMensagem = '';
    this.pararPollingStatus();
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
      email: matricula.email ?? '',
      academiaId: matricula.academiaId ?? 0,
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
    this.nomeCartao = aluno.nome || '';
    this.cpfCartao = aluno.cpf || '';
    this.configurarMercadoPagoParaAcademia(aluno.academiaId);
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
    this.isPix = this.isFormaPix(this.formaPagamentoSelecionada?.nome);
    this.isCartao = this.isFormaCartao(this.formaPagamentoSelecionada?.nome);
    this.showQrCode = false;
    this.qrCodePix = '';
    this.pixPayload = '';
    this.mensagemPix = '';
    this.numeroCartao = '';
    this.nomeCartao = this.alunoSelecionado?.nome || '';
    this.cpfCartao = this.alunoSelecionado?.cpf || '';
    this.validadeCartao = '';
    this.cvvCartao = '';
    this.pagamentoId = null;
    this.pagamentoPixId = null;
    this.pagamentoStatus = '';
    this.pagamentoMensagem = '';
    this.pararPollingStatus();
  }

  onCartaoInput(event: any): void {
    let valor = event.target.value.replace(/\D/g, '').substring(0, 16);
    valor = valor.replace(/(\d{4})(?=\d)/g, '$1 ');
    this.numeroCartao = valor;
    this.detectarBandeira(valor.replace(/\s/g, ''));
  }

  detectarBandeira(numero: string): void {
    if (/^4/.test(numero)) this.bandeiraCartao = 'visa';
    else if (/^(5[1-5]|2[2-7])/.test(numero)) this.bandeiraCartao = 'master';
    else if (/^3[47]/.test(numero)) this.bandeiraCartao = 'amex';
    else if (/^6(?:011|5)/.test(numero)) this.bandeiraCartao = 'discover';
    else this.bandeiraCartao = '';
  }

  gerarCodePix(payload: string): void {
    QRCode.toDataURL(payload)
      .then((url) => {
        setTimeout(() => {
          this.qrCodePix = url;
          this.showQrCode = true;
          this.isGerandoPix = false;
          this.cd.detectChanges();
        }, 0);
      })
      .catch(() => {
        this.qrCodePix = '';
        this.showQrCode = false;
        this.isGerandoPix = false;
        this.toastr.error('Nao foi possivel gerar o QR Code do PIX.');
      });
  }

  gerarPix(): void {
    if (this.isGerandoPix) {
      return;
    }

    if (!this.alunoSelecionado || !this.planoSelecionado || !this.formaPagamentoId || !this.editarId) {
      this.toastr.warning('Salve a matricula antes de gerar o PIX');
      return;
    }

    this.isGerandoPix = true;
    this.qrCodePix = '';
    this.showQrCode = false;
    this.pixPayload = '';
    this.pagamentoStatus = '';
    this.pagamentoMensagem = '';
    this.cd.detectChanges();

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
          this.pagamentoId = res.pagamentoId;
          this.pagamentoStatus = res.status;
          this.pagamentoMensagem = res.mensagem;
          this.verificacaoAutomaticaPix = res.verificacaoAutomaticaDisponivel;
          this.mensagemPix = res.mensagem;
          this.pixPayload = res.payload || '';

          if (res.qrCodeBase64) {
            setTimeout(() => {
              this.qrCodePix = `data:image/png;base64,${res.qrCodeBase64}`;
              this.showQrCode = true;
              this.isGerandoPix = false;
              this.cd.detectChanges();
            }, 0);
          } else if (res.payload) {
            this.gerarCodePix(res.payload);
          } else {
            this.isGerandoPix = false;
          }

          if (res.status === 'Pago') {
            this.toastr.success('Pagamento PIX confirmado.');
            this.carregarMatriculas();
          } else {
            this.toastr.info(res.mensagem || 'PIX gerado. Aguarde a confirmacao do pagamento.');
            this.iniciarPollingStatus();
          }
        },
        error: (err) => {
          const message = err?.error?.message || 'Erro ao gerar cobranca PIX.';
          this.toastr.error(message);
          this.pagamentoStatus = 'Erro';
          this.pagamentoMensagem = message;
          this.isGerandoPix = false;
        },
      });
  }

  verificarPagamentoPix(): void {
    if (!this.pagamentoPixId) {
      return;
    }

    this.pgService.consultarStatusPagamentoAtualizado(this.pagamentoPixId).subscribe({
      next: (res) => {
        this.pagamentoStatus = res.status;
        this.pagamentoMensagem = this.getMensagemStatus(res.status);
        this.toastr.info(`Status atualizado: ${res.status}`);
        if (res.status === 'Pago' || res.status === 'Recusado') {
          this.pararPollingStatus();
        }
        this.carregarMatriculas();
      },
      error: () => this.toastr.error('Erro ao consultar status do PIX'),
    });
  }

  excluirMatricula(matricula: Matriculas): void {
    const aluno = matricula.alunoNome || 'esta matricula';
    if (!confirm(`Deseja excluir a matricula de ${aluno}?`)) {
      return;
    }

    this.spinner.show();
    this.matriculasService.excluirMatricula(matricula.id).subscribe({
      next: () => {
        this.spinner.hide();
        this.toastr.success('Matricula excluida com sucesso.');
        this.carregarMatriculas();
      },
      error: (err) => {
        this.spinner.hide();
        const message = err?.error?.message || 'Erro ao excluir matricula.';
        this.toastr.error(message);
      },
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

        if (this.isFormaPix(this.formaPagamentoSelecionada?.nome)) {
          this.gerarPix();
        } else if (this.isCartao) {
          this.toastr.info('Matricula salva. Confira os dados do cartao e clique em Pagar com cartao.');
        } else {
          this.resetForm();
          this.modalRef?.hide();
        }

        this.carregarMatriculas();
      },
      error: (err) => {
        this.spinner.hide();
        const message = err?.error?.message || 'Erro ao salvar matricula';
        this.toastr.error(message, 'Erro');
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

  async pagarCartao(): Promise<void> {
    if (!this.alunoSelecionado || !this.planoSelecionado || !this.formaPagamentoId || !this.editarId) {
      this.toastr.warning('Salve a matricula antes de efetuar o pagamento no cartao.');
      return;
    }

    if (!this.mp) {
      this.toastr.error('Pagamento por cartao indisponivel. Configure a chave publica do Mercado Pago na academia.');
      return;
    }

    if (!this.numeroCartao || !this.nomeCartao || !this.cpfCartao || !this.validadeCartao || !this.cvvCartao) {
      this.toastr.warning('Preencha todos os dados do cartao.');
      return;
    }

    const cardToken = await this.gerarTokenCartao();
    if (!cardToken) {
      return;
    }

    this.spinner.show();
    this.pagamentoId = null;
    this.pagamentoStatus = '';
    this.pagamentoMensagem = '';
    this.pararPollingStatus();

    this.pgService.pagarCartao({
      alunoId: this.alunoSelecionado.id,
      matriculaId: this.editarId,
      planoId: this.planoId,
      formaPagamentoId: this.formaPagamentoId,
      valor: this.planoSelecionado.valor,
      parcelas: Number(this.formaPagamentoSelecionada?.parcelas || 1),
      payerEmail: this.alunoSelecionado.email || 'pagador@aluno.local',
      cardToken,
      paymentMethodId: this.detectarBandeiraMercadoPago(this.numeroCartao),
    }).subscribe({
      next: (res) => {
        this.spinner.hide();
        this.pagamentoId = res.pagamentoId;
        this.pagamentoStatus = res.status;
        this.pagamentoMensagem = res.mensagem || this.getMensagemStatus(res.status);

        if (res.status === 'Pago') {
          this.toastr.success('Pagamento com cartao aprovado.');
          this.limparDadosCartao();
          this.carregarMatriculas();
        } else if (res.status === 'Recusado') {
          this.toastr.error(this.pagamentoMensagem);
        } else {
          this.toastr.info(this.pagamentoMensagem);
          this.iniciarPollingStatus();
        }
      },
      error: (err) => {
        this.spinner.hide();
        const message = err?.error?.message || 'Nao foi possivel processar o pagamento no cartao.';
        this.toastr.error(message);
        this.pagamentoStatus = 'Erro';
        this.pagamentoMensagem = message;
      },
    });
  }

  copiarPix(): void {
    if (!this.pixPayload) {
      return;
    }

    navigator.clipboard.writeText(this.pixPayload);
    this.toastr.success('Codigo PIX copiado. Cole no aplicativo do banco.');
  }

  private async inicializarMercadoPago(): Promise<void> {
    const publicKey = this.mercadoPagoPublicKeyAcademia || environment.mercadoPagoPublicKey;
    if (!publicKey || publicKey.includes('__CONFIGURE_VIA_')) {
      return;
    }

    await loadMercadoPago();
    this.mp = new (window as any).MercadoPago(publicKey, { locale: 'pt-BR' });
  }

  private carregarConfiguracaoPagamentoAcademia(): void {
    this.academiasService.getAcademias().subscribe({
      next: (academias) => {
        this.academiasPagamento = academias ?? [];
        this.mercadoPagoPublicKeyAcademia = this.academiasPagamento.find((a) => !!a.mercadoPagoPublicKey)?.mercadoPagoPublicKey || '';
        this.inicializarMercadoPago();
      },
      error: () => this.inicializarMercadoPago(),
    });
  }

  private configurarMercadoPagoParaAcademia(academiaId?: number): void {
    if (!academiaId) {
      return;
    }

    const publicKey = this.academiasPagamento.find((a) => a.id === academiaId)?.mercadoPagoPublicKey || '';
    if (publicKey && publicKey !== this.mercadoPagoPublicKeyAcademia) {
      this.mercadoPagoPublicKeyAcademia = publicKey;
      this.inicializarMercadoPago();
    }
  }

  private async gerarTokenCartao(): Promise<string | null> {
    try {
      const validade = (this.validadeCartao || '').split('/');
      if (validade.length !== 2) {
        this.toastr.warning('Validade do cartao deve estar no formato MM/AA.');
        return null;
      }

      const cpfNumerico = String(this.cpfCartao || this.alunoSelecionado?.cpf || '').replace(/\D/g, '');
      if (cpfNumerico.length !== 11) {
        this.toastr.warning('Informe um CPF valido para tokenizar o cartao.');
        return null;
      }

      const token = await this.mp.createCardToken({
        cardNumber: this.numeroCartao.replace(/\s/g, ''),
        cardholderName: this.nomeCartao,
        cardExpirationMonth: validade[0],
        cardExpirationYear: `20${validade[1]}`,
        securityCode: this.cvvCartao,
        identificationType: 'CPF',
        identificationNumber: cpfNumerico,
      });

      return token?.id || null;
    } catch (error: any) {
      this.toastr.error(error?.message || 'Nao foi possivel tokenizar o cartao.');
      return null;
    }
  }

  private detectarBandeiraMercadoPago(numeroCartao: string): string {
    const numero = (numeroCartao || '').replace(/\D/g, '');

    if (/^4/.test(numero)) return 'visa';
    if (/^(5[1-5]|2[2-7])/.test(numero)) return 'master';
    if (/^3[47]/.test(numero)) return 'amex';
    if (/^((4011(78|79))|(431274)|(438935)|(451416)|(457393)|(45763[12])|(504175)|(5067)|(509)|(627780)|(636297)|(636368))/.test(numero)) return 'elo';
    if (/^(606282|3841)/.test(numero)) return 'hipercard';

    return 'visa';
  }

  private iniciarPollingStatus(): void {
    this.pararPollingStatus();
    this.pollingHandle = setInterval(() => {
      if (!this.pagamentoId) {
        return;
      }

      this.pgService.consultarStatusPagamentoAtualizado(this.pagamentoId).subscribe({
        next: (res) => {
          this.pagamentoStatus = res.status;
          this.pagamentoMensagem = this.getMensagemStatus(res.status);

          if (res.status === 'Pago' || res.status === 'Recusado') {
            this.pararPollingStatus();
            this.carregarMatriculas();
          }
        },
        error: () => this.pararPollingStatus(),
      });
    }, 5000);
  }

  private pararPollingStatus(): void {
    if (this.pollingHandle) {
      clearInterval(this.pollingHandle);
      this.pollingHandle = null;
    }
  }

  private limparDadosCartao(): void {
    this.numeroCartao = '';
    this.validadeCartao = '';
    this.cvvCartao = '';
    this.bandeiraCartao = '';
  }

  private getMensagemStatus(status: string): string {
    if (status === 'Pago') return 'Pagamento aprovado com sucesso.';
    if (status === 'Recusado') return 'Pagamento recusado pelo emissor.';
    if (status === 'EmAnalise') return 'Pagamento em analise. Aguarde confirmacao.';
    if (status === 'Erro') return this.pagamentoMensagem || 'Nao foi possivel processar o pagamento.';
    return 'Pagamento pendente. Aguardando confirmacao.';
  }

  private isFormaPix(nome?: string): boolean {
    const nomeNormalizado = this.normalizarTexto(nome);
    return nomeNormalizado === 'pix' || nomeNormalizado.includes('pix');
  }

  private isFormaCartao(nome?: string): boolean {
    const nomeNormalizado = this.normalizarTexto(nome);
    return nomeNormalizado.includes('credito') ||
      nomeNormalizado.includes('cartao') ||
      nomeNormalizado.includes('debito');
  }

  private normalizarTexto(valor?: string): string {
    return (valor || '')
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLowerCase()
      .trim();
  }
}
