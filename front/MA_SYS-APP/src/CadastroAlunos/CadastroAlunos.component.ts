import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BsModalService } from 'ngx-bootstrap/modal';
import { AlunosService } from '../Services/AlunosService/Alunosservice';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute } from '@angular/router';
import { NgxSpinnerService } from 'ngx-spinner';
import { CadastroAlunosService } from '../Services/CadastroAlunos/CadastroAlunos.service';
import { CadastroAlunosContext } from '../Services/CadastroAlunos/CadastroAlunos.service';
import * as QRCode from 'qrcode';
import { PagamentosService } from '../Services/PagamentosService/Pagamentos.service';
import { loadMercadoPago } from '@mercadopago/sdk-js';
import { environment } from '../app/environments/environment';
import { AcademiasService } from '../Services/AcademiaService/Academias.service';

declare global {
  interface Window {
    MercadoPago: any;
  }
}

@Component({
  selector: 'app-CadastroAlunos',
  standalone: true,
  templateUrl: './CadastroAlunos.component.html',
  styleUrls: ['./CadastroAlunos.component.css'],
  imports: [CommonModule, FormsModule],
})
export class CadastroAlunosComponent implements OnInit, OnDestroy {
  cpfBusca: string = '';
  cpf: string = '';
  endereco: string = '';
  bairro: string = '';
  cidade: string = '';
  estado: string = '';
  cep: string = '';
  obs: string = '';
  emailBusca: string = '';
  nome: string = '';
  telefone: string = '';
  email: string = '';
  graduacao: string = '';
  redeSocial: string = '';
  dataNascimento: string = '';
  dataCadastro: string = '';
  mostrarFormulario: boolean = false;

  planoId: number = 0;
  planos: any[] = [];
  planoSelecionado: any = null;

  formaPagamentoId: number = 0;
  formasPagamento: any[] = [];
  formaPagamentoSelecionada: any = null;

  isPix: boolean = false;
  showQrCode: boolean = false;
  qrCodePix: string = '';
  pixPayload: string = '';
  verificacaoAutomaticaPix = false;

  aluno: any = null;
  alunoEncontrado: boolean = false;
  mostrarCadastro: boolean = false;

  numeroCartao: string = '';
  validadeCartao: string = '';
  cvvCartao: string = '';

  isCartao: boolean = false;
  bandeiraCartao: string = '';

  cartao = {
    numero: '',
    nome: '',
    validade: '',
    cvv: '',
  };
  private mp: any = null;
  private mercadoPagoPublicKeyAcademia = '';
  pagamentoStatus = '';
  pagamentoMensagem = '';
  pagamentoId: number | null = null;
  statusPollingAtivo = false;
  private pollingHandle: any = null;
  mensalidadeStatus: string = '';
  dataVencimentoMensalidade: string = '';
  diasParaVencimento: number = 0;
  alertaVencimento: boolean = false;

  constructor(
    private modalService: BsModalService,
    private alunoService: AlunosService,
    private route: ActivatedRoute,
    private toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private cadastroAlunosService: CadastroAlunosService,
    private context: CadastroAlunosContext,
    private pgService: PagamentosService,
    private academiasService: AcademiasService,
    private cd: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.context.slug = this.obterSlugDaRota();
    this.carregarConfiguracaoPagamentoAcademia();
    this.carregarFormaPgtos();
  }

  ngOnDestroy() {
    this.pararPollingStatus();
  }

  private async inicializarMercadoPago() {
    const publicKey = this.mercadoPagoPublicKeyAcademia || environment.mercadoPagoPublicKey;
    if (!publicKey || publicKey.includes('__CONFIGURE_VIA_')) {
      return;
    }

    await loadMercadoPago();
    this.mp = new window.MercadoPago(publicKey, { locale: 'pt-BR' });
  }

  private carregarConfiguracaoPagamentoAcademia() {
    if (!this.context.slug) {
      this.inicializarMercadoPago();
      return;
    }

    this.academiasService.getPagamentoConfigPublico(this.context.slug).subscribe({
      next: (config) => {
        this.mercadoPagoPublicKeyAcademia = config?.mercadoPagoPublicKey || '';
        this.inicializarMercadoPago();
      },
      error: () => {
        this.inicializarMercadoPago();
      },
    });
  }

  pesquisarAlunos() {
    if (!this.context.slug) {
      this.toastr.error('Link de cadastro invalido. Abra novamente o link da academia.');
      return;
    }

    const cpf = String(this.cpfBusca || '').trim();
    const email = String(this.emailBusca || '').trim();

    if (!cpf || !email) {
      this.toastr.warning('Informe CPF e email para pesquisar o cadastro.');
      return;
    }

    this.spinner.show();

    this.cadastroAlunosService.getByCpfEmail(cpf, email).subscribe({
      next: (res: any) => {
        this.spinner.hide();
        this.aluno = res;
        this.nome = res.nome;
        this.telefone = res.telefone;
        this.cpf = this.cpfBusca;
        this.endereco = res.endereco;
        this.bairro = res.bairro;
        this.cidade = res.cidade;
        this.estado = res.estado;
        this.cep = res.cep;
        this.email = this.emailBusca;
        this.redeSocial = res.redeSocial;
        this.graduacao = res.graduacao;
        this.dataNascimento = res.dataNascimento;
        this.dataCadastro = res.dataCadastro;
        this.obs = res.obs;
        this.mensalidadeStatus = res.mensalidadeStatus || '';
        this.dataVencimentoMensalidade = res.dataVencimentoMensalidade || '';
        this.diasParaVencimento = Number(res.diasParaVencimento ?? 0);
        this.alertaVencimento = !!res.alertaVencimento;
        this.alunoEncontrado = true;
        this.formaPagamentoId = 0;
        this.formaPagamentoSelecionada = null;
        this.carregarFormaPgtos();
        this.cd.detectChanges();
      },
      error: (err) => {
        this.spinner.hide();

        if (err.status === 404) {
          this.toastr.warning('Aluno nao encontrado.');
          this.alunoEncontrado = false;
        } else {
          this.toastr.error('Erro ao buscar aluno.');
        }
      },
    });
  }

  private obterSlugDaRota(): string {
    const slugPorParametro = this.route.snapshot.paramMap.get('academia');
    if (slugPorParametro) {
      return slugPorParametro;
    }

    const segmentos = this.route.snapshot.url.map((segmento) => segmento.path);
    if (segmentos.length >= 2 && segmentos[1] === 'cadastro') {
      return segmentos[0] || '';
    }

    const viaPath = window?.location?.pathname?.split('/').filter(Boolean) || [];
    if (viaPath.length >= 2 && viaPath[1] === 'cadastro') {
      return viaPath[0] || '';
    }

    return '';
  }

  onPlanoChange() {
    const plano = this.planos.find((p) => p.id == this.planoId);
    this.planoSelecionado = plano;
    this.showQrCode = false;
    this.qrCodePix = '';
  }

  onFormaPagamentoChange() {
    this.formaPagamentoSelecionada = this.formasPagamento.find((f) => f.id == this.formaPagamentoId);
    const nome = this.formaPagamentoSelecionada?.nome?.toLowerCase() || '';

    this.isPix = nome === 'pix';
    this.isCartao = nome.includes('credito') || nome.includes('debito');

    this.showQrCode = false;
    this.qrCodePix = '';
    this.numeroCartao = '';
    this.validadeCartao = '';
    this.cvvCartao = '';
  }

  gerarPix() {
    if (!this.aluno?.alunoId || !this.aluno?.matriculaId || !this.aluno?.planoId) {
      this.toastr.error('Nao foi possivel identificar a matricula para gerar o PIX.');
      return;
    }

    if (!this.formaPagamentoId) {
      this.toastr.warning('Selecione a forma de pagamento.');
      return;
    }

    const valor = this.aluno?.valor || 0;

    this.spinner.show();
    this.pagamentoStatus = '';
    this.pagamentoMensagem = '';
    this.pagamentoId = null;
    this.pararPollingStatus();

    this.pgService.gerarPagamentoPixPublico({
      slug: this.context.slug,
      alunoId: this.aluno.alunoId,
      matriculaId: this.aluno.matriculaId,
      planoId: this.aluno.planoId,
      formaPagamentoId: Number(this.formaPagamentoId),
      valor,
      nome: this.nome || 'Aluno',
      email: this.email || this.emailBusca || '',
      cidade: this.cidade || 'Cidade',
    }).subscribe((res) => {
      this.spinner.hide();
      this.pagamentoId = res.pagamentoId;
      this.pagamentoStatus = res.status;
      this.pagamentoMensagem = res.mensagem;
      this.verificacaoAutomaticaPix = res.verificacaoAutomaticaDisponivel;
      this.pixPayload = res.payload || '';

      if (res.qrCodeBase64) {
        this.qrCodePix = `data:image/png;base64,${res.qrCodeBase64}`;
        this.showQrCode = true;
      } else if (res.payload) {
        this.gerarCodePix(res.payload);
      }

      if (res.status === 'Pago') {
        this.toastr.success('Pagamento PIX confirmado com sucesso.');
      } else {
        this.toastr.info(res.mensagem);
        this.iniciarPollingStatus();
      }
    }, (err) => {
      this.spinner.hide();
      const message = err?.error?.message || 'Nao foi possivel gerar o pagamento PIX.';
      this.toastr.error(message);
    });
  }

  gerarCodePix(payload: string) {
    QRCode.toDataURL(payload).then((url) => {
      setTimeout(() => {
        this.qrCodePix = url;
        this.showQrCode = true;
        this.cd.detectChanges();
      }, 0);
    });
  }

  copiarPix() {
    if (!this.pixPayload) {
      return;
    }

    navigator.clipboard.writeText(this.pixPayload);
    this.toastr.success('Codigo PIX copiado. Cole no aplicativo do seu banco.');
  }

  carregarFormaPgtos() {
    if (!this.context.slug) {
      return;
    }

    this.spinner.show();

    this.pgService.getFormaPagamentosPublico(this.context.slug).subscribe({
      next: (res) => {
        this.spinner.hide();
        this.formasPagamento = res.map((p) => ({
          ...p,
          menuAberto: false,
        }));
        this.cd.markForCheck();
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Erro ao carregar formas de pagamento.', 'Erro');
      },
    });
  }

  async pagarCartao() {
    if (!this.aluno?.alunoId || !this.aluno?.matriculaId || !this.aluno?.planoId) {
      this.toastr.error('Nao foi possivel identificar a matricula para o pagamento.');
      return;
    }

    if (!this.formaPagamentoId) {
      this.toastr.warning('Selecione a forma de pagamento.');
      return;
    }

    if (!this.cartao.numero || !this.cartao.nome || !this.cartao.validade || !this.cartao.cvv) {
      this.toastr.warning('Preencha os dados do cartao.');
      return;
    }

    if (!this.mp) {
      this.toastr.error('Pagamento indisponivel. Configure a chave publica do Mercado Pago.');
      return;
    }

    const cardToken = await this.gerarTokenCartao();
    if (!cardToken) {
      return;
    }

    const paymentMethodId = this.detectarBandeiraMercadoPago(this.cartao.numero);

    const payload = {
      slug: this.context.slug,
      alunoId: this.aluno.alunoId,
      matriculaId: this.aluno.matriculaId,
      planoId: this.aluno.planoId,
      formaPagamentoId: Number(this.formaPagamentoId),
      valor: this.aluno?.valor,
      parcelas: Number(this.formaPagamentoSelecionada?.parcelas || 1),
      payerEmail: this.email || this.emailBusca || '',
      cardToken,
      paymentMethodId,
    };

    this.spinner.show();
    this.pagamentoStatus = '';
    this.pagamentoMensagem = '';
    this.pagamentoId = null;
    this.pararPollingStatus();

    this.pgService.pagarCartaoPublico(payload).subscribe({
      next: (res) => {
        this.spinner.hide();
        this.pagamentoId = res.pagamentoId;
        this.pagamentoStatus = res.status;
        this.pagamentoMensagem = res.mensagem;

        if (res.status === 'Pago') {
          this.toastr.success('Pagamento realizado com sucesso!');
        } else if (res.status === 'Recusado') {
          this.toastr.error('Pagamento recusado.');
        } else {
          this.toastr.info(res.mensagem);
          this.iniciarPollingStatus();
        }

        this.cartao = { numero: '', nome: '', validade: '', cvv: '' };
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

  private iniciarPollingStatus() {
    if (!this.pagamentoId || !this.context.slug) {
      return;
    }

    this.statusPollingAtivo = true;
    this.pollingHandle = setInterval(() => {
      if (!this.pagamentoId) {
        return;
      }

      this.pgService.consultarStatusPagamentoPublico(this.pagamentoId, this.context.slug).subscribe({
        next: (res) => {
          this.pagamentoStatus = res.status;

          if (res.status === 'Pago') {
            this.pagamentoMensagem = 'Pagamento aprovado com sucesso.';
            this.toastr.success('Pagamento aprovado.');
            this.pararPollingStatus();
          } else if (res.status === 'Recusado') {
            this.pagamentoMensagem = 'Pagamento recusado pelo emissor.';
            this.toastr.error('Pagamento recusado.');
            this.pararPollingStatus();
          } else if (res.status === 'EmAnalise') {
            this.pagamentoMensagem = 'Pagamento em analise. Aguarde confirmacao.';
          } else {
            this.pagamentoMensagem = 'Pagamento pendente. Aguardando confirmacao.';
          }
        },
        error: () => {
          this.pararPollingStatus();
        },
      });
    }, 5000);
  }

  verificarPagamentoPix() {
    if (!this.pagamentoId) {
      this.toastr.warning('Nenhum pagamento PIX em acompanhamento.');
      return;
    }

    this.pgService.consultarStatusPagamentoPublico(this.pagamentoId, this.context.slug).subscribe({
      next: (res) => {
        this.pagamentoStatus = res.status;

        if (res.status === 'Pago') {
          this.pagamentoMensagem = 'Pagamento aprovado com sucesso.';
          this.toastr.success('Pagamento aprovado.');
          this.pararPollingStatus();
        } else if (res.status === 'Recusado') {
          this.pagamentoMensagem = 'Pagamento recusado pelo emissor.';
          this.toastr.error('Pagamento recusado.');
          this.pararPollingStatus();
        } else if (res.status === 'EmAnalise') {
          this.pagamentoMensagem = 'Pagamento em analise. Aguarde confirmacao.';
        } else {
          this.pagamentoMensagem = 'Pagamento pendente. Aguardando confirmacao.';
        }
      },
      error: () => {
        this.toastr.error('Nao foi possivel consultar o status do PIX.');
      },
    });
  }

  private pararPollingStatus() {
    this.statusPollingAtivo = false;
    if (this.pollingHandle) {
      clearInterval(this.pollingHandle);
      this.pollingHandle = null;
    }
  }

  private async gerarTokenCartao(): Promise<string | null> {
    try {
      const validade = (this.cartao.validade || '').split('/');
      if (validade.length !== 2) {
        this.toastr.warning('Validade do cartao deve estar no formato MM/AA.');
        return null;
      }

      const cpfNumerico = String(this.cpfBusca || this.cpf || '').replace(/\D/g, '');
      if (cpfNumerico.length !== 11) {
        this.toastr.warning('Informe um CPF valido para tokenizar o cartao.');
        return null;
      }

      const token = await this.mp.createCardToken({
        cardNumber: this.cartao.numero.replace(/\s/g, ''),
        cardholderName: this.cartao.nome,
        cardExpirationMonth: validade[0],
        cardExpirationYear: `20${validade[1]}`,
        securityCode: this.cartao.cvv,
        identificationType: 'CPF',
        identificationNumber: cpfNumerico,
      });

      return token?.id || null;
    } catch (error: any) {
      const message = error?.message || 'Nao foi possivel tokenizar o cartao.';
      this.toastr.error(message);
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
}
