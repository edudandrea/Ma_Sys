import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, NgZone, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { NgxSpinnerService } from 'ngx-spinner';
import * as QRCode from 'qrcode';
import { loadMercadoPago } from '@mercadopago/sdk-js';
import { environment } from '../app/environments/environment';
import { FiliadosService, PagamentoFiliado } from '../Services/Filiados/Filiados.service';

declare global {
  interface Window {
    MercadoPago: any;
  }
}

@Component({
  selector: 'app-pagamento-filiados',
  standalone: true,
  templateUrl: './PagamentoFiliados.component.html',
  styleUrls: ['./PagamentoFiliados.component.css'],
  imports: [CommonModule, FormsModule],
})
export class PagamentoFiliadosComponent implements OnInit, OnDestroy {
  federacaoId = 0;
  federacaoNome = 'Federacao';
  federacaoLogoUrl = '';
  emailBusca = '';
  telefoneBusca = '';
  filiado: any = null;
  cobrancas: PagamentoFiliado[] = [];
  formasPagamento: any[] = [];
  cobrancaSelecionadaId = 0;
  cobrancaSelecionada: PagamentoFiliado | null = null;
  formaPagamentoId = '';
  formaPagamentoSelecionada: any = null;
  isPix = false;
  isCartao = false;
  isGerandoPix = false;
  showQrCode = false;
  qrCodePix = '';
  pixPayload = '';
  pagamentoId: number | null = null;
  pagamentoStatus = '';
  pagamentoMensagem = '';
  verificacaoAutomaticaPix = false;
  private mp: any = null;
  private pollingHandle: any = null;

  cartao = {
    numero: '',
    nome: '',
    validade: '',
    cvv: '',
    cpf: '',
  };

  constructor(
    private route: ActivatedRoute,
    private filiadosService: FiliadosService,
    private toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private cd: ChangeDetectorRef,
    private ngZone: NgZone,
  ) {}

  ngOnInit() {
    this.federacaoId = Number(this.route.snapshot.paramMap.get('federacaoId') || 0);
    if (!this.federacaoId) {
      this.toastr.error('Link de pagamento invalido.');
      return;
    }

    this.carregarConfiguracao();
    this.carregarFormasPagamento();
  }

  ngOnDestroy() {
    this.pararPollingStatus();
  }

  pesquisarFiliado() {
    if (!this.emailBusca.trim() || !this.telefoneBusca.trim()) {
      this.toastr.warning('Informe email e telefone para pesquisar.');
      return;
    }

    this.spinner.show();
    this.filiadosService.buscarFiliadoPublico(this.federacaoId, {
      email: this.emailBusca,
      telefone: this.telefoneBusca,
    }).subscribe({
      next: (res) => {
        this.spinner.hide();
        this.filiado = res;
        this.cobrancas = res.cobrancas || [];
        this.resetPagamento();
        if (!this.cobrancas.length) {
          this.toastr.info('Nenhuma cobranca pendente encontrada.');
        }
      },
      error: (err) => {
        this.spinner.hide();
        this.filiado = null;
        this.cobrancas = [];
        this.toastr.error(err?.error?.message || 'Filiado nao encontrado.');
      },
    });
  }

  onCobrancaChange() {
    this.cobrancaSelecionada = this.cobrancas.find((c) => c.id == this.cobrancaSelecionadaId) || null;
    this.resetPagamento(false);
  }

  onFormaPagamentoChange() {
    this.formaPagamentoSelecionada = this.formasPagamento.find((f) => f.id === this.formaPagamentoId) || null;
    this.isPix = this.formaPagamentoId === 'pix';
    this.isCartao = this.formaPagamentoId === 'cartao';
    this.resetStatus();
  }

  gerarPix() {
    if (!this.cobrancaSelecionada || !this.filiado) {
      this.toastr.warning('Selecione uma cobranca.');
      return;
    }

    this.spinner.show();
    this.isGerandoPix = true;
    this.resetStatus();

    this.filiadosService.gerarPixPublico({
      federacaoId: this.federacaoId,
      filiadoId: this.filiado.filiadoId,
      pagamentoId: this.cobrancaSelecionada.id,
      valor: this.cobrancaSelecionada.valor,
      nome: this.filiado.nome,
      email: this.filiado.email || this.emailBusca,
      cidade: this.filiado.cidade || 'Cidade',
    }).subscribe({
      next: (res) => {
        this.spinner.hide();
        this.pagamentoId = res.pagamentoId;
        this.pagamentoStatus = res.status;
        this.pagamentoMensagem = res.mensagem;
        this.verificacaoAutomaticaPix = res.verificacaoAutomaticaDisponivel;
        this.pixPayload = res.payload || '';

        if (res.payload) {
          this.gerarQrCode(res.payload);
        } else if (res.qrCodeBase64) {
          this.qrCodePix = `data:image/png;base64,${res.qrCodeBase64}`;
          this.showQrCode = true;
          this.isGerandoPix = false;
        } else {
          this.isGerandoPix = false;
          this.toastr.error('O Mercado Pago nao retornou um codigo PIX valido.');
        }

        if (res.status === 'Pago') {
          this.toastr.success('Pagamento aprovado.');
          this.pesquisarFiliado();
        } else {
          this.iniciarPollingStatus();
        }
        this.cd.detectChanges();
      },
      error: (err) => {
        this.spinner.hide();
        this.isGerandoPix = false;
        this.toastr.error(err?.error?.message || 'Nao foi possivel gerar o PIX.');
      },
    });
  }

  async pagarCartao() {
    if (!this.cobrancaSelecionada || !this.filiado) {
      this.toastr.warning('Selecione uma cobranca.');
      return;
    }

    if (!this.cartao.numero || !this.cartao.nome || !this.cartao.validade || !this.cartao.cvv || !this.cartao.cpf) {
      this.toastr.warning('Preencha os dados do cartao.');
      return;
    }

    if (!this.mp) {
      this.toastr.error('Pagamento por cartao indisponivel. Configure a chave publica do Mercado Pago.');
      return;
    }

    const cardToken = await this.gerarTokenCartao();
    if (!cardToken) {
      return;
    }

    this.spinner.show();
    this.resetStatus();

    this.filiadosService.pagarCartaoPublico({
      federacaoId: this.federacaoId,
      filiadoId: this.filiado.filiadoId,
      pagamentoId: this.cobrancaSelecionada.id,
      valor: this.cobrancaSelecionada.valor,
      nome: this.filiado.nome,
      email: this.filiado.email || this.emailBusca,
      cidade: this.filiado.cidade || 'Cidade',
      parcelas: Number(this.formaPagamentoSelecionada?.parcelas || 1),
      payerEmail: this.filiado.email || this.emailBusca,
      payerCpf: this.cartao.cpf,
      cardToken,
      paymentMethodId: this.detectarBandeiraMercadoPago(this.cartao.numero),
    }).subscribe({
      next: (res) => {
        this.spinner.hide();
        this.pagamentoId = res.pagamentoId;
        this.pagamentoStatus = res.status;
        this.pagamentoMensagem = res.mensagem || '';

        if (res.status === 'Pago') {
          this.toastr.success('Pagamento aprovado.');
          this.pesquisarFiliado();
        } else if (res.status === 'Recusado') {
          this.toastr.error('Pagamento recusado.');
        } else {
          this.iniciarPollingStatus();
        }
      },
      error: (err) => {
        this.spinner.hide();
        this.pagamentoStatus = 'Erro';
        this.pagamentoMensagem = err?.error?.message || 'Nao foi possivel processar o cartao.';
        this.toastr.error(this.pagamentoMensagem);
      },
    });
  }

  copiarPix() {
    if (!this.pixPayload) return;
    navigator.clipboard.writeText(this.pixPayload);
    this.toastr.success('Codigo PIX copiado.');
  }

  verificarPagamento() {
    if (!this.pagamentoId) return;

    this.filiadosService.consultarStatusPublico(this.federacaoId, this.pagamentoId).subscribe({
      next: (res) => {
        this.pagamentoStatus = res.status;
        if (res.status === 'Pago') {
          this.pagamentoMensagem = 'Pagamento aprovado com sucesso.';
          this.toastr.success('Pagamento aprovado.');
          this.pararPollingStatus();
          this.pesquisarFiliado();
        } else {
          this.pagamentoMensagem = 'Pagamento pendente. Aguardando confirmacao.';
        }
      },
      error: () => this.toastr.error('Nao foi possivel consultar o pagamento.'),
    });
  }

  private carregarConfiguracao() {
    this.filiadosService.getPagamentoConfigPublico(this.federacaoId).subscribe({
      next: (config) => {
        this.federacaoNome = config?.nome || 'Federacao';
        this.federacaoLogoUrl = config?.logoUrl || '';
        this.inicializarMercadoPago(config?.mercadoPagoPublicKey || '');
      },
      error: () => this.inicializarMercadoPago(''),
    });
  }

  private carregarFormasPagamento() {
    this.filiadosService.getFormasPagamentoPublicas(this.federacaoId).subscribe({
      next: (res) => this.formasPagamento = res || [],
      error: () => this.formasPagamento = [],
    });
  }

  private async inicializarMercadoPago(publicKey: string) {
    const chave = publicKey || environment.mercadoPagoPublicKey;
    if (!chave || chave.includes('__CONFIGURE_VIA_')) {
      return;
    }

    try {
      await loadMercadoPago();
      this.mp = new window.MercadoPago(chave, { locale: 'pt-BR' });
    } catch {
      this.mp = null;
    }
  }

  private gerarQrCode(payload: string) {
    QRCode.toDataURL(payload)
      .then((url) => {
        this.ngZone.run(() => {
          this.qrCodePix = url;
          this.showQrCode = true;
          this.isGerandoPix = false;
          this.cd.detectChanges();
        });
      })
      .catch(() => {
        this.isGerandoPix = false;
        this.toastr.error('Nao foi possivel gerar o QR Code do PIX.');
      });
  }

  private iniciarPollingStatus() {
    if (!this.pagamentoId) return;
    this.pararPollingStatus();
    this.pollingHandle = setInterval(() => this.verificarPagamento(), 5000);
  }

  private pararPollingStatus() {
    if (this.pollingHandle) {
      clearInterval(this.pollingHandle);
      this.pollingHandle = null;
    }
  }

  private async gerarTokenCartao(): Promise<string | null> {
    const validade = (this.cartao.validade || '').split('/');
    if (validade.length !== 2) {
      this.toastr.warning('Validade deve estar no formato MM/AA.');
      return null;
    }

    try {
      const token = await this.mp.createCardToken({
        cardNumber: this.cartao.numero.replace(/\s/g, ''),
        cardholderName: this.cartao.nome,
        cardExpirationMonth: validade[0],
        cardExpirationYear: `20${validade[1]}`,
        securityCode: this.cartao.cvv,
        identificationType: 'CPF',
        identificationNumber: this.cartao.cpf.replace(/\D/g, ''),
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
    if (/^(606282|3841)/.test(numero)) return 'hipercard';
    return 'visa';
  }

  private resetPagamento(limparCobranca = true) {
    if (limparCobranca) {
      this.cobrancaSelecionadaId = 0;
      this.cobrancaSelecionada = null;
    }
    this.formaPagamentoId = '';
    this.formaPagamentoSelecionada = null;
    this.isPix = false;
    this.isCartao = false;
    this.resetStatus();
  }

  private resetStatus() {
    this.showQrCode = false;
    this.qrCodePix = '';
    this.pixPayload = '';
    this.pagamentoId = null;
    this.pagamentoStatus = '';
    this.pagamentoMensagem = '';
    this.verificacaoAutomaticaPix = false;
    this.pararPollingStatus();
  }
}
