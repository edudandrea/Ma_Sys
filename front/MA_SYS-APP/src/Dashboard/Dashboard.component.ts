import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, NgZone, OnInit, TemplateRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgxSpinnerService } from 'ngx-spinner';
import { DashboardService } from '../Services/Dashboard/Dashboard.service';
import {
  PagamentoAcademia,
  PagamentoAcademiaPixResponse,
  PagamentoAcademiaStatusResponse,
  PagamentosAcademiasService,
} from '../Services/PagamentosAcademias/PagamentosAcademias.service';
import Chart from 'chart.js/auto';
import { ToastrService } from 'ngx-toastr';
import { AfterViewInit, OnDestroy } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { loadMercadoPago } from '@mercadopago/sdk-js';
import * as QRCode from 'qrcode';
import { environment } from '../app/environments/environment';

@Component({
  selector: 'app-Dashboard',
  templateUrl: './Dashboard.component.html',
  styleUrls: ['./Dashboard.component.css'],
  imports: [CommonModule, FormsModule],
})
export class DashboardComponent implements OnInit, AfterViewInit, OnDestroy {
  dashboard: any;
  role = '';
  modalRef?: BsModalRef;
  cobrancasAcademia: PagamentoAcademia[] = [];
  cobrancaSelecionada: PagamentoAcademia | null = null;
  metodoPagamentoSelecionado: 'pix' | 'cartao' | '' = '';
  qrCodePix = '';
  pixPayload = '';
  pagamentoMensagem = '';
  pagamentoStatus = '';
  pagamentoVerificacaoAutomatica = false;
  pixGerado = false;
  cartao = {
    numero: '',
    nome: '',
    validade: '',
    cvv: '',
    cpf: '',
  };
  private mp: any = null;
  private mpPublicKey = '';
  private themeObserver?: MutationObserver;
  private statusPollingHandle: any = null;

  constructor(
    private spinner: NgxSpinnerService,
    private cd: ChangeDetectorRef,
    private toastr: ToastrService,
    private dashService: DashboardService,
    private pagamentosAcademiasService: PagamentosAcademiasService,
    private modalService: BsModalService,
    private ngZone: NgZone,
  ) {}

  ngOnInit() {
    if (typeof window !== 'undefined') {
      const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
      this.role = usuario.role || '';
    }

    this.loadDashboard();
    this.carregarCobrancasAcademia();
  }

  ngAfterViewInit() {
    if (typeof document === 'undefined') {
      return;
    }

    this.themeObserver = new MutationObserver((mutations) => {
      const themeChanged = mutations.some(
        (mutation) => mutation.type === 'attributes' && mutation.attributeName === 'data-theme',
      );

      if (themeChanged && this.dashboard) {
        this.renderCharts();
      }
    });

    this.themeObserver.observe(document.body, {
      attributes: true,
      attributeFilter: ['data-theme'],
    });
  }

  ngOnDestroy() {
    this.themeObserver?.disconnect();
    this.pararPollingStatus();
  }

  loadDashboard() {
    this.spinner.show();
    this.dashService.getDashboard().subscribe({
      next: (res) => {
        this.spinner.hide();
        this.dashboard = res;

        setTimeout(() => {
          this.renderCharts();
        }, 100);

        this.cd.markForCheck();
      },
      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao carregar o dashboard', 'Erro');
      },
    });
  }

  carregarCobrancasAcademia() {
    this.pagamentosAcademiasService.listar().subscribe({
      next: (res) => {
        this.cobrancasAcademia = res ?? [];
        this.cd.markForCheck();
      },
      error: () => {
        this.cobrancasAcademia = [];
        if (this.isAcademia) {
          this.toastr.error('Nao foi possivel carregar as cobrancas da academia.');
        }
        this.cd.markForCheck();
      },
    });
  }

  openPagamentoModal(template: TemplateRef<any>, cobranca: PagamentoAcademia) {
    this.cobrancaSelecionada = cobranca;
    this.metodoPagamentoSelecionado = '';
    this.resetPagamentoState();
    this.modalRef = this.modalService.show(template, {
      class: 'modal-lg modal-dialog-centered',
    });
  }

  fecharModalPagamento() {
    this.pararPollingStatus();
    this.modalRef?.hide();
  }

  selecionarMetodoPagamento(metodo: 'pix' | 'cartao') {
    if (metodo === 'pix' && !this.cobrancaSelecionada?.aceitaPix) {
      this.toastr.warning('PIX nao esta habilitado para esta cobranca.');
      return;
    }

    if (metodo === 'cartao' && !this.cobrancaSelecionada?.aceitaCartao) {
      this.toastr.warning('Cartao de credito nao esta habilitado para esta cobranca.');
      return;
    }

    this.metodoPagamentoSelecionado = metodo;
    this.resetPagamentoState();
  }

  pagarCobrancaPix() {
    if (!this.cobrancaSelecionada) {
      return;
    }

    this.spinner.show();
    this.pagamentosAcademiasService.gerarPix(this.cobrancaSelecionada.id).subscribe({
      next: (res: PagamentoAcademiaPixResponse) => {
        this.spinner.hide();
        this.pagamentoStatus = res.status;
        this.pagamentoMensagem = res.mensagem;
        this.pixPayload = (res.payload || '').trim();
        this.pixGerado = true;
        this.qrCodePix = this.normalizarQrCodeBase64(res.qrCodeBase64);

        if (!this.qrCodePix && this.pixPayload) {
          this.gerarQrCodePix(this.pixPayload);
        }

        this.pagamentoVerificacaoAutomatica = res.verificacaoAutomaticaDisponivel;
        this.cd.detectChanges();

        if (res.status === 'Pago') {
          this.toastr.success('Pagamento PIX confirmado com sucesso.');
          this.finalizarPagamentoConfirmado('PIX');
          return;
        }

        if (!this.qrCodePix && !this.pixPayload) {
          this.toastr.warning('PIX gerado, mas o gateway nao retornou QR Code nem codigo copia e cola.');
        } else {
          this.toastr.info('PIX gerado. Aguarde a confirmacao do pagamento.');
        }

        this.iniciarPollingStatus();
      },
      error: async (err) => {
        this.spinner.hide();
        const message = await this.extrairMensagemErroHttp(err, 'Nao foi possivel gerar o PIX da cobranca.');
        this.toastr.error(message);
      },
    });
  }

  async pagarCobrancaCartao() {
    if (!this.cobrancaSelecionada) {
      return;
    }

    await this.inicializarMercadoPago(this.cobrancaSelecionada.mercadoPagoPublicKey);

    if (!this.mp) {
      this.toastr.error('Pagamento por cartao indisponivel. Configure a chave publica do Mercado Pago.');
      return;
    }

    if (!this.cartao.numero || !this.cartao.nome || !this.cartao.validade || !this.cartao.cvv || !this.cartao.cpf) {
      this.toastr.warning('Preencha todos os dados do cartao.');
      return;
    }

    const cardToken = await this.gerarTokenCartao();
    if (!cardToken) {
      return;
    }

    const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
    const email = usuario?.email || 'financeiro@academia.local';
    const paymentMethodId = this.detectarBandeiraMercadoPago(this.cartao.numero);

    this.spinner.show();
    this.pagamentosAcademiasService.pagarComCartao(this.cobrancaSelecionada.id, {
      payerEmail: email,
      cardToken,
      paymentMethodId,
      parcelas: 1,
    }).subscribe({
      next: (res: PagamentoAcademiaStatusResponse) => {
        this.spinner.hide();
        this.pagamentoStatus = res.status;
        this.pagamentoMensagem = this.getMensagemStatus(res.status, 'cartao');

        if (res.status === 'Pago') {
          this.toastr.success('Pagamento com cartao aprovado.');
          this.finalizarPagamentoConfirmado('Cartao');
          return;
        }

        if (res.status === 'Recusado') {
          this.toastr.error('Pagamento com cartao recusado.');
          return;
        }

        this.toastr.info(this.pagamentoMensagem);
        this.iniciarPollingStatus();
      },
      error: async (err) => {
        this.spinner.hide();
        const message = await this.extrairMensagemErroHttp(err, 'Nao foi possivel processar o pagamento no cartao.');
        this.toastr.error(message);
      },
    });
  }

  verificarStatusCobranca() {
    if (!this.cobrancaSelecionada) {
      return;
    }

    this.pagamentosAcademiasService.consultarStatusAtualizado(this.cobrancaSelecionada.id).subscribe({
      next: (res) => {
        this.pagamentoStatus = res.status;
        this.pagamentoMensagem = this.getMensagemStatus(res.status, this.metodoPagamentoSelecionado);

        if (res.status === 'Pago') {
          this.toastr.success('Pagamento confirmado com sucesso.');
          this.finalizarPagamentoConfirmado(res.formaPagamentoNome || 'Pagamento');
          return;
        }

        if (res.status === 'Recusado') {
          this.toastr.error('Pagamento recusado.');
          this.pararPollingStatus();
        }
      },
      error: () => {
        this.toastr.error('Nao foi possivel consultar o status do pagamento.');
      },
    });
  }

  copiarPix() {
    if (!this.pixPayload) {
      this.toastr.warning('Codigo PIX ainda nao disponivel para copiar.');
      return;
    }

    navigator.clipboard.writeText(this.pixPayload);
    this.toastr.success('Codigo PIX copiado para a area de transferencia.');
  }

  get cobrancasPendentesAcademia(): PagamentoAcademia[] {
    return this.cobrancasAcademia.filter((item) => item.status !== 'Pago');
  }

  get totalCobrancasPendentesAcademia(): number {
    return this.cobrancasPendentesAcademia.length;
  }

  get cobrancasRecentesAdmin(): PagamentoAcademia[] {
    return this.cobrancasAcademia.slice(0, 8);
  }

  get cobrancaPermitePix(): boolean {
    return this.cobrancaSelecionada?.aceitaPix !== false;
  }

  get cobrancaPermiteCartao(): boolean {
    return this.cobrancaSelecionada?.aceitaCartao !== false;
  }

  renderCharts() {
    const canvasAlunos = document.getElementById('graficoAlunos') as HTMLCanvasElement | null;
    const canvasPlanos = document.getElementById('graficoPlanos') as HTMLCanvasElement | null;

    if (canvasAlunos) {
      Chart.getChart(canvasAlunos)?.destroy();
    }
    if (canvasPlanos) {
      Chart.getChart(canvasPlanos)?.destroy();
    }

    const textColor = this.getCssVar('--text-secondary');
    const legendColor = this.isDarkThemeActive()
      ? this.getCssVar('--text-primary')
      : '#1f2937';
    const gridColor = 'rgba(148, 163, 184, 0.14)';
    const accentPrimary = this.getCssVar('--accent-primary');
    const accentStrong = this.getCssVar('--accent-primary-strong');
    const accentSecondary = this.getCssVar('--accent-secondary');
    const planosData = this.dashboard?.planos?.map((p: any) => p.totalAlunos) || [];
    const isDarkTheme = this.isDarkThemeActive();
    const planoPalette = isDarkTheme
      ? [
          '#60a5fa',
          '#fbbf24',
          '#4ade80',
          '#f87171',
          '#c084fc',
          '#2dd4bf',
          '#f472b6',
          '#fde047',
          accentPrimary,
          accentSecondary,
        ]
      : [
          '#1d4ed8',
          '#b45309',
          '#166534',
          '#b91c1c',
          '#6d28d9',
          '#0f766e',
          '#be185d',
          '#a16207',
          accentPrimary,
          accentSecondary,
        ];

    if (canvasAlunos) {
      new Chart(canvasAlunos, {
        type: 'line',
        data: {
          labels: ['Jan', 'Fev', 'Mar', 'Abr'],
          datasets: [
            {
              label: 'Alunos',
              data: this.dashboard?.alunosPorMes || [10, 20, 30, 40],
              fill: true,
              tension: 0.35,
              borderColor: accentPrimary,
              backgroundColor: 'rgba(37, 99, 235, 0.16)',
              pointBackgroundColor: accentStrong,
              pointBorderColor: '#ffffff',
              pointRadius: 4,
            },
          ],
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: {
              labels: {
                color: textColor,
              },
            },
          },
          scales: {
            x: {
              ticks: { color: textColor },
              grid: { color: gridColor },
            },
            y: {
              ticks: { color: textColor },
              grid: { color: gridColor },
              beginAtZero: true,
            },
          },
        },
      });
    }

    if (canvasPlanos) {
      new Chart(canvasPlanos, {
        type: 'doughnut',
        data: {
          labels: this.dashboard?.planos?.map((p: any) => p.nome) || [],
          datasets: [
            {
              data: planosData,
              backgroundColor: planosData.map((_: number, index: number) => planoPalette[index % planoPalette.length]),
              borderColor: this.getCssVar('--bg-panel'),
              borderWidth: 2,
            },
          ],
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: {
              position: 'bottom',
              labels: {
                color: legendColor,
                padding: 18,
              },
            },
          },
        },
      });
    }
  }

  onPeriodoChange(_: Event) {}

  get isAdmin(): boolean {
    return this.role === 'Admin';
  }

  get isSuperAdmin(): boolean {
    return this.role === 'SuperAdmin';
  }

  get isAcademia(): boolean {
    return this.role === 'Academia';
  }

  private getCssVar(name: string) {
    return getComputedStyle(document.body).getPropertyValue(name).trim() || '#94a3b8';
  }

  private isDarkThemeActive() {
    if (typeof document === 'undefined') {
      return true;
    }

    const theme = this.getCurrentTheme();
    return ['system', 'green-gold', 'red-black'].includes(theme);
  }

  private getCurrentTheme() {
    if (typeof document === 'undefined') {
      return 'system';
    }

    return document.body.getAttribute('data-theme') || 'system';
  }

  private async inicializarMercadoPago(publicKeyOverride?: string | null) {
    const publicKey = publicKeyOverride || environment.mercadoPagoPublicKey;
    if (!publicKey || publicKey.includes('__CONFIGURE_VIA_')) {
      this.mp = null;
      this.mpPublicKey = '';
      return;
    }

    if (this.mp && this.mpPublicKey === publicKey) {
      return;
    }

    await loadMercadoPago();
    this.mp = new (window as any).MercadoPago(publicKey, { locale: 'pt-BR' });
    this.mpPublicKey = publicKey;
  }

  private gerarQrCodePix(payload: string) {
    QRCode.toDataURL(payload)
      .then((url) => {
        this.ngZone.run(() => {
          this.qrCodePix = url;
          this.cd.detectChanges();
        });
      })
      .catch(() => {
        this.ngZone.run(() => {
          this.qrCodePix = '';
          this.toastr.error('Nao foi possivel gerar o QR Code do PIX.');
          this.cd.detectChanges();
        });
      });
  }

  private normalizarQrCodeBase64(qrCodeBase64?: string | null): string {
    const valor = (qrCodeBase64 || '').trim();
    if (!valor) {
      return '';
    }

    if (valor.startsWith('data:image/')) {
      return valor;
    }

    return `data:image/png;base64,${valor}`;
  }

  private async extrairMensagemErroHttp(error: any, fallback: string): Promise<string> {
    const body = error?.error;

    if (typeof body === 'string') {
      return body || fallback;
    }

    if (body?.message) {
      return body.message;
    }

    if (body instanceof Blob) {
      const text = await body.text();
      if (!text) {
        return fallback;
      }

      try {
        const parsed = JSON.parse(text);
        return parsed?.message || text || fallback;
      } catch {
        return text;
      }
    }

    return error?.message || fallback;
  }

  private async gerarTokenCartao(): Promise<string | null> {
    try {
      const validade = (this.cartao.validade || '').split('/');
      if (validade.length !== 2) {
        this.toastr.warning('Validade do cartao deve estar no formato MM/AA.');
        return null;
      }

      const cpfNumerico = String(this.cartao.cpf || '').replace(/\D/g, '');
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

  private iniciarPollingStatus() {
    this.pararPollingStatus();
    this.statusPollingHandle = setInterval(() => this.verificarStatusCobranca(), 5000);
  }

  private pararPollingStatus() {
    if (this.statusPollingHandle) {
      clearInterval(this.statusPollingHandle);
      this.statusPollingHandle = null;
    }
  }

  private finalizarPagamentoConfirmado(formaPagamentoNome: string) {
    this.pararPollingStatus();

    if (this.cobrancaSelecionada) {
      this.cobrancaSelecionada.status = 'Pago';
      this.cobrancaSelecionada.formaPagamentoNome = formaPagamentoNome;
      this.cobrancaSelecionada.dataPagamento = new Date().toISOString();
    }

    this.modalRef?.hide();
    this.carregarCobrancasAcademia();
    this.loadDashboard();
  }

  private resetPagamentoState() {
    this.pararPollingStatus();
    this.qrCodePix = '';
    this.pixPayload = '';
    this.pixGerado = false;
    this.pagamentoMensagem = '';
    this.pagamentoStatus = '';
    this.pagamentoVerificacaoAutomatica = false;
    this.cartao = {
      numero: '',
      nome: '',
      validade: '',
      cvv: '',
      cpf: '',
    };
  }

  private getMensagemStatus(status: string, metodo: string) {
    if (status === 'Pago') {
      return `Pagamento ${metodo || 'da cobranca'} confirmado com sucesso.`;
    }

    if (status === 'EmAnalise') {
      return 'Pagamento em analise. Aguarde a confirmacao.';
    }

    if (status === 'Pendente') {
      return 'Pagamento pendente. Aguarde a confirmacao.';
    }

    return 'Pagamento recusado.';
  }
}
