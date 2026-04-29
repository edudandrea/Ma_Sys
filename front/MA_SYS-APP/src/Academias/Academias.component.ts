import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, HostListener, OnInit, TemplateRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { Academias, AcademiasService } from '../Services/AcademiaService/Academias.service';
import { PagamentosAcademiasService } from '../Services/PagamentosAcademias/PagamentosAcademias.service';
import { PagamentoAcademia } from '../Services/PagamentosAcademias/PagamentosAcademias.service';
import {
  MensalidadeSistema,
  MensalidadesSistemaService,
} from '../Services/MensalidadesSistema/MensalidadesSistema.service';
import { Observable, of } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { environment } from '../app/environments/environment';

@Component({
  selector: 'app-Academias',
  templateUrl: './Academias.component.html',
  styleUrls: ['./Academias.component.css'],
  imports: [CommonModule, FormsModule],
})
export class AcademiasComponent implements OnInit {
  modalRef?: BsModalRef;
  academiaEmEdicao: Academias | null = null;
  id = 0;
  nome = '';
  totalAlunos = 0;
  cidade = '';
  email = '';
  telefone = '';
  logoUrl = '';
  logoPreviewUrl = '';
  logoArquivo: File | null = null;
  redeSocial = '';
  dataCadastro = '';
  ativo = true;
  editarId: number | null = null;
  responsavel = '';
  totalProf = 0;
  linkCadastro = '';
  slug = '';
  chavePix = '';
  mercadoPagoPublicKey = '';
  mercadoPagoAccessToken = '';
  publicOrigin = '';
  academiaCobrancaSelecionada: Academias | null = null;
  cobrancasAcademia: PagamentoAcademia[] = [];
  mensalidadesSistema: MensalidadeSistema[] = [];
  mensalidadeSistemaSelecionadaId: number | null = null;
  cobrancaValor = 199.9;
  cobrancaDataVencimento = new Date().toISOString().slice(0, 10);
  cobrancaDescricao = '';

  academias: (Academias & { menuAberto?: boolean })[] = [];

  constructor(
    private modalService: BsModalService,
    private toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private acad: AcademiasService,
    private cd: ChangeDetectorRef,
    private academiaService: AcademiasService,
    private pagamentosAcademiasService: PagamentosAcademiasService,
    private mensalidadesSistemaService: MensalidadesSistemaService,
  ) {}

  ngOnInit() {
    this.publicOrigin = typeof window !== 'undefined' ? window.location.origin : '';
    this.carregarAcademias();
    this.carregarMensalidadesSistema();
  }

  getInicial(nome: string): string {
    return nome ? nome.charAt(0).toUpperCase() : '?';
  }

  getCadastroLink(slug: string): string {
    return `${this.publicOrigin}/${slug}/cadastro`;
  }

  @HostListener('document:click', ['$event'])
  fecharMenu(event: Event) {
    const target = event.target as HTMLElement;
    const clicouMenu = target.closest('.card-menu');

    if (!clicouMenu) {
      this.academias.forEach((m) => (m.menuAberto = false));
    }
  }

  toggleMenu(academia: Academias & { menuAberto?: boolean }, event: Event) {
    event.stopPropagation();

    this.academias.forEach((m) => {
      if (m !== academia) {
        m.menuAberto = false;
      }
    });

    academia.menuAberto = !academia.menuAberto;
    this.academias = [...this.academias];
  }

  toggleExpand(card: { expandido?: boolean }) {
    card.expandido = !card.expandido;
  }

  openModalNovaAcademia(template: TemplateRef<any>) {
    this.resetForm();
    this.academiaEmEdicao = null;
    this.modalRef = this.modalService.show(template, {
      class: 'modal-xl modal-dialog-centered',
    });
  }

  openModalExcluir(template: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template, {
      class: 'modal-md modal-dialog-centered',
    });
  }

  openModalCobrancas(template: TemplateRef<any>, academia: Academias & { menuAberto?: boolean }) {
    academia.menuAberto = false;
    this.academiaCobrancaSelecionada = academia;
    this.mensalidadeSistemaSelecionadaId = null;
    this.cobrancaDescricao = `Mensalidade do sistema - ${academia.nome}`;
    this.cobrancaDataVencimento = new Date().toISOString().slice(0, 10);
    this.cobrancaValor = 199.9;
    this.carregarCobrancasAcademia();
    this.modalRef = this.modalService.show(template, {
      class: 'modal-xl modal-dialog-centered',
    });
  }

  confirmarCancelarEdicao() {
    this.fecharModal();
  }

  carregarAcademias() {
    this.spinner.show();
    this.acad.getAcademias().subscribe({
      next: (res) => {
        this.spinner.hide();
        this.academias = res.map((m) => ({
          ...m,
          logoUrl: this.resolveLogoUrl(m.logoUrl),
          menuAberto: false,
        }));

        this.cd.markForCheck();
      },
      error: (err) => {
        console.error(err);
        this.spinner.hide();
        this.toastr.error('Erro ao carregar academias');
      },
    });
  }

  salvarNovaAcademia() {
    this.spinner.show();
    this.uploadLogoSeNecessario().pipe(
      switchMap((logoUrl) => this.academiaService.novaAcademia({
        nome: this.nome,
        cidade: this.cidade,
        telefone: this.telefone,
        logoUrl,
        email: this.email,
        redeSocial: this.redeSocial,
        responsavel: this.responsavel,
        chavePix: this.chavePix,
        mercadoPagoPublicKey: this.mercadoPagoPublicKey,
        mercadoPagoAccessToken: this.mercadoPagoAccessToken,
        dataCadastro: this.dataCadastro || new Date().toISOString(),
      })),
    ).subscribe({
      next: () => {
        this.spinner.hide();
        this.toastr.success('Academia cadastrada!', 'Sucesso');
        this.carregarAcademias();
        this.fecharModal();
      },
      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao salvar academia', 'Erro');
      },
    });
  }

  excluirAcademia(academiaId: number): void {
    this.spinner.show();
    this.academiaService.excluirAcademia(academiaId).subscribe({
      next: () => {
        this.toastr.success('Academia excluida com sucesso!', 'Sucesso');
        this.spinner.hide();
        this.carregarAcademias();
      },
      error: (err) => {
        console.error('Erro ao excluir a academia', err);
        this.toastr.error('Erro ao excluir a academia!', 'Erro');
        this.spinner.hide();
      },
    });
  }

  toggleStatus(academia: Academias) {
    const novoStatus = !academia.ativo;

    this.academiaService.atualizarStatus(academia.id, novoStatus).subscribe({
      next: () => {
        academia.ativo = novoStatus;
        this.academias = [...this.academias];
        this.cd.markForCheck();
        this.carregarAcademias();
        this.toastr.success(`Academia ${novoStatus ? 'ativada' : 'desativada'}`);
      },
      error: () => {
        this.toastr.error('Erro ao atualizar a academia');
      },
    });
  }

  fecharModal() {
    this.modalRef?.hide();
    this.resetForm();
  }

  editarAcademia(template: TemplateRef<any>, academia: Academias & { menuAberto?: boolean }) {
    this.resetForm();
    this.academiaEmEdicao = academia;
    this.editarId = academia.id;
    this.nome = academia.nome;
    this.cidade = academia.cidade;
    this.telefone = academia.telefone;
    this.email = academia.email;
    this.redeSocial = academia.redeSocial;
    this.responsavel = academia.responsavel;
    this.chavePix = academia.chavePix || '';
    this.logoUrl = this.resolveLogoUrl(academia.logoUrl);
    this.logoPreviewUrl = this.resolveLogoUrl(academia.logoUrl);
    this.mercadoPagoPublicKey = academia.mercadoPagoPublicKey || '';
    this.mercadoPagoAccessToken = academia.mercadoPagoAccessToken || '';
    academia.menuAberto = false;
    this.modalRef = this.modalService.show(template, {
      class: 'modal-xl modal-dialog-centered',
    });
  }

  cancelarEdicao() {
    this.academiaEmEdicao = null;
    this.editarId = null;
    this.resetForm();
  }

  salvarEdicao() {
    if (!this.academiaEmEdicao) {
      this.toastr.error('Nenhuma academia selecionada para edicao');
      return;
    }

    const academia = this.academiaEmEdicao;
    this.spinner.show();
    this.uploadLogoSeNecessario().pipe(
      switchMap((logoUrl) => this.academiaService.atualizarAcademia({
        id: academia.id,
        nome: this.nome,
        cidade: this.cidade,
        telefone: this.telefone,
        logoUrl,
        email: this.email,
        redeSocial: this.redeSocial,
        responsavel: this.responsavel,
        chavePix: this.chavePix,
        mercadoPagoPublicKey: this.mercadoPagoPublicKey,
        mercadoPagoAccessToken: this.mercadoPagoAccessToken,
        ativo: academia.ativo,
      })),
    ).subscribe({
      next: () => {
        this.spinner.hide();
        academia.nome = this.nome;
        academia.cidade = this.cidade;
        academia.telefone = this.telefone;
        academia.logoUrl = this.resolveLogoUrl(this.logoUrl);
        academia.email = this.email;
        academia.redeSocial = this.redeSocial;
        academia.responsavel = this.responsavel;
        academia.chavePix = this.chavePix;
        academia.mercadoPagoPublicKey = this.mercadoPagoPublicKey;
        academia.mercadoPagoAccessToken = this.mercadoPagoAccessToken;

        const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
        if (usuario?.academiaId === academia.id) {
          localStorage.setItem('usuario', JSON.stringify({
            ...usuario,
            academiaNome: this.nome,
            academiaLogoUrl: this.resolveLogoUrl(this.logoUrl),
          }));
        }

        this.editarId = null;
        this.academiaEmEdicao = null;
        this.carregarAcademias();
        this.fecharModal();
        this.toastr.success('Academia atualizada');
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Erro ao atualizar academia');
      },
    });
  }

  copiarLink(slug: string) {
    const link = this.getCadastroLink(slug);
    navigator.clipboard.writeText(link).then(
      () => {
        this.toastr.success('Link copiado para a area de transferencia!');
      },
      () => {
        this.toastr.error('Erro ao copiar o link.');
      },
    );
  }

  criarCobrancaSistema(academia: Academias & { menuAberto?: boolean }) {
    if (!academia || academia.id <= 0) return;
    const valor = Number(this.cobrancaValor);
    if (!Number.isFinite(valor) || valor <= 0) {
      this.toastr.error('Valor invalido para cobranca.');
      return;
    }

    this.pagamentosAcademiasService.criarCobranca({
      academiaId: academia.id,
      mensalidadeSistemaId: this.mensalidadeSistemaSelecionadaId,
      valor,
      dataVencimento: this.cobrancaDataVencimento,
      descricao: this.cobrancaDescricao || `Mensalidade do sistema - ${academia.nome}`,
    }).subscribe({
      next: () => {
        this.toastr.success('Cobranca criada com sucesso.');
        this.carregarCobrancasAcademia();
      },
      error: () => this.toastr.error('Nao foi possivel criar a cobranca.'),
    });
  }

  onMensalidadeSistemaChange() {
    const mensalidade = this.mensalidadesSistema.find((item) => item.id === this.mensalidadeSistemaSelecionadaId);
    if (!mensalidade || !this.academiaCobrancaSelecionada) {
      return;
    }

    this.cobrancaValor = Number(mensalidade.valor);
    this.cobrancaDescricao = mensalidade.descricao?.trim()
      ? mensalidade.descricao
      : `Mensalidade do sistema - ${this.academiaCobrancaSelecionada.nome} (${mensalidade.mesesUso} ${mensalidade.mesesUso === 1 ? 'mes' : 'meses'})`;

    const dataBase = new Date();
    dataBase.setDate(dataBase.getDate() + Number(mensalidade.prazoPagamentoDias || 0));
    this.cobrancaDataVencimento = dataBase.toISOString().slice(0, 10);
  }

  carregarCobrancasAcademia() {
    if (!this.academiaCobrancaSelecionada) return;
    this.pagamentosAcademiasService.listarPorAcademia(this.academiaCobrancaSelecionada.id).subscribe({
      next: (res) => {
        this.cobrancasAcademia = res;
      },
      error: () => {
        this.cobrancasAcademia = [];
        this.toastr.error('Nao foi possivel carregar historico de cobrancas.');
      },
    });
  }

  baixarCobranca(cobrancaId: number) {
    this.pagamentosAcademiasService.baixar(cobrancaId).subscribe({
      next: () => {
        this.toastr.success('Cobranca baixada como paga.');
        this.carregarCobrancasAcademia();
      },
      error: () => this.toastr.error('Nao foi possivel baixar a cobranca.'),
    });
  }

  onLogoSelecionada(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] || null;

    if (!file) {
      return;
    }

    if (!file.type.startsWith('image/')) {
      this.toastr.error('Selecione um arquivo de imagem valido.');
      input.value = '';
      return;
    }

    if (this.logoPreviewUrl.startsWith('blob:')) {
      URL.revokeObjectURL(this.logoPreviewUrl);
    }

    this.logoArquivo = file;
    this.logoPreviewUrl = URL.createObjectURL(file);
  }

  removerLogoSelecionada() {
    if (this.logoPreviewUrl.startsWith('blob:')) {
      URL.revokeObjectURL(this.logoPreviewUrl);
    }

    this.logoArquivo = null;
    this.logoPreviewUrl = '';
    this.logoUrl = '';
  }

  private uploadLogoSeNecessario(): Observable<string> {
    if (!this.logoArquivo) {
      return of(this.logoUrl);
    }

    return this.academiaService.uploadLogo(this.logoArquivo).pipe(
      switchMap((response) => {
        this.logoUrl = response.logoUrl;
        this.logoPreviewUrl = this.resolveLogoUrl(response.logoUrl);
        this.logoArquivo = null;
        return of(this.resolveLogoUrl(response.logoUrl));
      }),
    );
  }

  private resolveLogoUrl(logoUrl?: string | null): string {
    if (!logoUrl) {
      return '';
    }

    if (logoUrl.startsWith('http://') || logoUrl.startsWith('https://') || logoUrl.startsWith('blob:')) {
      return logoUrl;
    }

    if (logoUrl.startsWith('/api/')) {
      return logoUrl;
    }

    if (logoUrl.startsWith('/uploads/academias/')) {
      const fileName = logoUrl.split('/').pop();
      return fileName ? `${environment.apiUrl}/Academias/logo/${fileName}` : logoUrl;
    }

    return logoUrl;
  }

  private carregarMensalidadesSistema() {
    this.mensalidadesSistemaService.listar().subscribe({
      next: (res) => {
        this.mensalidadesSistema = (res ?? []).filter((item) => item.ativo);
      },
      error: () => {
        this.mensalidadesSistema = [];
      },
    });
  }

  private resetForm() {
    if (this.logoPreviewUrl.startsWith('blob:')) {
      URL.revokeObjectURL(this.logoPreviewUrl);
    }

    this.id = 0;
    this.nome = '';
    this.totalAlunos = 0;
    this.cidade = '';
    this.email = '';
    this.telefone = '';
    this.logoUrl = '';
    this.logoPreviewUrl = '';
    this.logoArquivo = null;
    this.redeSocial = '';
    this.dataCadastro = '';
    this.ativo = true;
    this.editarId = null;
    this.academiaEmEdicao = null;
    this.responsavel = '';
    this.totalProf = 0;
    this.linkCadastro = '';
    this.slug = '';
    this.chavePix = '';
    this.mercadoPagoPublicKey = '';
    this.mercadoPagoAccessToken = '';
  }
}
