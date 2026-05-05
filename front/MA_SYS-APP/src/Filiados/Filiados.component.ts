import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, TemplateRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { Observable, of, switchMap } from 'rxjs';
import { environment } from '../app/environments/environment';
import { Filiado, FiliadosService, PagamentoFiliado } from '../Services/Filiados/Filiados.service';

@Component({
  selector: 'app-Filiados',
  templateUrl: './Filiados.component.html',
  styleUrls: ['./Filiados.component.css'],
  imports: [CommonModule, FormsModule],
})
export class FiliadosComponent implements OnInit {
  modalRef?: BsModalRef;
  filiados: Filiado[] = [];
  filiadoEmEdicao: Filiado | null = null;
  editarId: number | null = null;
  nome = '';
  cidade = '';
  estado = '';
  email = '';
  telefone = '';
  responsavel = '';
  redeSocial = '';
  logoUrl = '';
  logoPreviewUrl = '';
  logoArquivo: File | null = null;
  cobrancaSelecionada: Filiado | null = null;
  cobrancas: PagamentoFiliado[] = [];
  cobrancaValor = 199.9;
  cobrancaDataVencimento = new Date().toISOString().slice(0, 10);
  cobrancaDescricao = '';
  pixPayload = '';
  pixQrCodeBase64 = '';
  linkPagamentoPublico = '';

  constructor(
    private filiadosService: FiliadosService,
    private modalService: BsModalService,
    private toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private cd: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.definirLinkPagamentoPublico();
    this.carregarFiliados();
  }

  getInicial(nome?: string): string {
    return nome ? nome.charAt(0).toUpperCase() : '?';
  }

  carregarFiliados() {
    this.spinner.show();
    this.filiadosService.getFiliados().subscribe({
      next: (res) => {
        this.spinner.hide();
        this.filiados = (res ?? []).map((f) => ({
          ...f,
          logoUrl: this.resolveLogoUrl(f.logoUrl),
          menuAberto: false,
        }));
        this.cd.markForCheck();
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Erro ao carregar filiados.');
      },
    });
  }

  openModalFiliado(template: TemplateRef<any>) {
    this.resetForm();
    this.modalRef = this.modalService.show(template, {
      class: 'modal-xl modal-dialog-centered',
    });
  }

  editarFiliado(template: TemplateRef<any>, filiado: Filiado) {
    this.resetForm();
    this.filiadoEmEdicao = filiado;
    this.editarId = filiado.id;
    this.nome = filiado.nome || '';
    this.cidade = filiado.cidade || '';
    this.estado = filiado.estado || '';
    this.email = filiado.email || '';
    this.telefone = filiado.telefone || '';
    this.responsavel = filiado.responsavel || '';
    this.redeSocial = filiado.redeSocial || '';
    this.logoUrl = filiado.logoUrl || '';
    this.logoPreviewUrl = filiado.logoUrl || '';
    this.modalRef = this.modalService.show(template, {
      class: 'modal-xl modal-dialog-centered',
    });
  }

  salvar() {
    if (!this.nome.trim()) {
      this.toastr.warning('Informe o nome do filiado.');
      return;
    }

    this.spinner.show();
    this.uploadLogoSeNecessario().pipe(
      switchMap((logoUrl) => {
        const payload = {
          id: this.editarId ?? undefined,
          nome: this.nome,
          cidade: this.cidade,
          estado: this.estado,
          email: this.email,
          telefone: this.telefone,
          responsavel: this.responsavel,
          redeSocial: this.redeSocial,
          logoUrl,
        };

        return this.editarId
          ? this.filiadosService.atualizarFiliado(payload)
          : this.filiadosService.novoFiliado(payload);
      }),
    ).subscribe({
      next: () => {
        this.spinner.hide();
        this.toastr.success(this.editarId ? 'Filiado atualizado.' : 'Filiado cadastrado.');
        this.fecharModal();
        this.carregarFiliados();
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Nao foi possivel salvar o filiado.');
      },
    });
  }

  toggleStatus(filiado: Filiado) {
    const ativo = !filiado.ativo;
    this.filiadosService.atualizarStatus(filiado.id, ativo).subscribe({
      next: () => {
        filiado.ativo = ativo;
        this.toastr.success(`Filiado ${ativo ? 'ativado' : 'desativado'}.`);
      },
      error: () => this.toastr.error('Erro ao atualizar status.'),
    });
  }

  excluir(filiado: Filiado) {
    if (!confirm('Deseja realmente excluir este filiado?')) return;
    this.filiadosService.excluirFiliado(filiado.id).subscribe({
      next: () => {
        this.toastr.success('Filiado excluido.');
        this.carregarFiliados();
      },
      error: () => this.toastr.error('Nao foi possivel excluir o filiado.'),
    });
  }

  openModalCobrancas(template: TemplateRef<any>, filiado: Filiado) {
    this.cobrancaSelecionada = filiado;
    this.cobrancaDescricao = `Mensalidade - ${filiado.nome}`;
    this.cobrancaDataVencimento = new Date().toISOString().slice(0, 10);
    this.pixPayload = '';
    this.pixQrCodeBase64 = '';
    this.carregarCobrancas();
    this.modalRef = this.modalService.show(template, {
      class: 'modal-xl modal-dialog-centered',
    });
  }

  carregarCobrancas() {
    if (!this.cobrancaSelecionada) return;
    this.filiadosService.listarCobrancas(this.cobrancaSelecionada.id).subscribe({
      next: (res) => this.cobrancas = res ?? [],
      error: () => this.cobrancas = [],
    });
  }

  criarCobranca() {
    if (!this.cobrancaSelecionada) return;
    this.filiadosService.criarCobranca({
      filiadoId: this.cobrancaSelecionada.id,
      valor: Number(this.cobrancaValor),
      dataVencimento: this.cobrancaDataVencimento,
      descricao: this.cobrancaDescricao,
    }).subscribe({
      next: () => {
        this.toastr.success('Cobranca criada.');
        this.carregarCobrancas();
      },
      error: () => this.toastr.error('Nao foi possivel criar a cobranca.'),
    });
  }

  baixarCobranca(id: number) {
    this.filiadosService.baixarCobranca(id).subscribe({
      next: () => {
        this.toastr.success('Cobranca baixada como paga.');
        this.carregarCobrancas();
      },
      error: () => this.toastr.error('Nao foi possivel baixar a cobranca.'),
    });
  }

  gerarPix(id: number) {
    this.pixPayload = '';
    this.pixQrCodeBase64 = '';
    this.filiadosService.gerarPix(id).subscribe({
      next: (res) => {
        this.pixPayload = res.payload || '';
        this.pixQrCodeBase64 = res.qrCodeBase64 || '';
        this.toastr.success('PIX gerado pelo Mercado Pago.');
        this.carregarCobrancas();
      },
      error: (error) => this.toastr.error(error?.error?.message || 'Nao foi possivel gerar o PIX.'),
    });
  }

  copiarLinkPagamentoPublico() {
    if (!this.linkPagamentoPublico) {
      this.toastr.warning('Usuario sem federacao vinculada para gerar link publico.');
      return;
    }

    navigator.clipboard.writeText(this.linkPagamentoPublico);
    this.toastr.success('Link publico de pagamento copiado.');
  }

  onLogoSelecionada(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] || null;
    if (!file) return;
    if (!file.type.startsWith('image/')) {
      this.toastr.error('Selecione uma imagem valida.');
      input.value = '';
      return;
    }
    this.logoArquivo = file;
    this.logoPreviewUrl = URL.createObjectURL(file);
  }

  removerLogoSelecionada() {
    this.logoArquivo = null;
    this.logoUrl = '';
    this.logoPreviewUrl = '';
  }

  fecharModal() {
    this.modalRef?.hide();
    this.resetForm();
  }

  private uploadLogoSeNecessario(): Observable<string> {
    if (!this.logoArquivo) return of(this.logoUrl);
    return this.filiadosService.uploadLogo(this.logoArquivo).pipe(
      switchMap((response) => {
        this.logoUrl = response.logoUrl;
        this.logoArquivo = null;
        return of(this.resolveLogoUrl(response.logoUrl));
      }),
    );
  }

  private resolveLogoUrl(logoUrl?: string | null): string {
    if (!logoUrl) return '';
    if (logoUrl.startsWith('http://') || logoUrl.startsWith('https://') || logoUrl.startsWith('blob:')) return logoUrl;
    if (logoUrl.startsWith('/api/')) return logoUrl;
    if (logoUrl.startsWith('/uploads/filiados/')) {
      const fileName = logoUrl.split('/').pop();
      return fileName ? `${environment.apiUrl}/Filiados/logo/${fileName}` : logoUrl;
    }
    return logoUrl;
  }

  private resetForm() {
    this.filiadoEmEdicao = null;
    this.editarId = null;
    this.nome = '';
    this.cidade = '';
    this.estado = '';
    this.email = '';
    this.telefone = '';
    this.responsavel = '';
    this.redeSocial = '';
    this.logoUrl = '';
    this.logoPreviewUrl = '';
    this.logoArquivo = null;
  }

  private definirLinkPagamentoPublico() {
    const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
    const federacaoId = Number(usuario.federacaoId || 0);
    this.linkPagamentoPublico = federacaoId
      ? `${window.location.origin}/federacao/${federacaoId}/pagamento`
      : '';
  }
}
