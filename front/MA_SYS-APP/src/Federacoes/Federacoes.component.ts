import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, TemplateRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { ToastrService } from 'ngx-toastr';
import { Observable, of, switchMap } from 'rxjs';
import { environment } from '../app/environments/environment';
import { Federacao, FederacoesService } from '../Services/Federacoes/Federacoes.service';

@Component({
  selector: 'app-Federacoes',
  templateUrl: './Federacoes.component.html',
  styleUrls: ['./Federacoes.component.css'],
  imports: [CommonModule, FormsModule],
})
export class FederacoesComponent implements OnInit {
  modalRef?: BsModalRef;
  federacoes: Federacao[] = [];
  editandoId: number | null = null;
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
  mercadoPagoPublicKey = '';
  mercadoPagoAccessToken = '';

  constructor(
    private service: FederacoesService,
    private modalService: BsModalService,
    private toastr: ToastrService,
    private cd: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.carregar();
  }

  getInicial(nome?: string): string {
    return nome ? nome.charAt(0).toUpperCase() : '?';
  }

  carregar() {
    this.service.listar().subscribe({
      next: (res) => {
        this.federacoes = (res ?? []).map((item) => ({
          ...item,
          logoUrl: this.resolveLogoUrl(item.logoUrl),
        }));
        this.cd.markForCheck();
      },
      error: () => this.toastr.error('Erro ao carregar federacoes.'),
    });
  }

  abrirModal(template: TemplateRef<any>) {
    this.resetForm();
    this.modalRef = this.modalService.show(template, {
      class: 'modal-xl modal-dialog-centered',
    });
  }

  editar(template: TemplateRef<any>, federacao: Federacao) {
    this.resetForm();
    this.editandoId = federacao.id;
    this.nome = federacao.nome || '';
    this.cidade = federacao.cidade || '';
    this.estado = federacao.estado || '';
    this.email = federacao.email || '';
    this.telefone = federacao.telefone || '';
    this.responsavel = federacao.responsavel || '';
    this.redeSocial = federacao.redeSocial || '';
    this.logoUrl = federacao.logoUrl || '';
    this.logoPreviewUrl = federacao.logoUrl || '';
    this.mercadoPagoAccessToken = federacao.mercadoPagoAccessToken || '';
    this.mercadoPagoPublicKey = federacao.mercadoPagoPublicKey || '';
    this.modalRef = this.modalService.show(template, {
      class: 'modal-xl modal-dialog-centered',
    });
  }

  salvar() {
    if (!this.nome.trim()) {
      this.toastr.warning('Informe o nome da federacao.');
      return;
    }

    this.uploadLogoSeNecessario().pipe(
      switchMap((logoUrl) => {
        const payload = {
          id: this.editandoId ?? undefined,
          nome: this.nome,
          cidade: this.cidade,
          estado: this.estado,
          email: this.email,
          telefone: this.telefone,
          responsavel: this.responsavel,
          redeSocial: this.redeSocial,
          mercadoPagoAccessToken: this.mercadoPagoAccessToken,
          mercadoPagoPublicKey: this.mercadoPagoPublicKey,
          logoUrl,
        };

        return this.editandoId ? this.service.atualizar(payload) : this.service.criar(payload);
      }),
    ).subscribe({
      next: () => {
        this.toastr.success(this.editandoId ? 'Federacao atualizada.' : 'Federacao cadastrada.');
        this.modalRef?.hide();
        this.resetForm();
        this.carregar();
      },
      error: () => this.toastr.error('Nao foi possivel salvar a federacao.'),
    });
  }

  toggleStatus(federacao: Federacao) {
    const ativo = !federacao.ativo;
    this.service.atualizarStatus(federacao.id, ativo).subscribe({
      next: () => {
        federacao.ativo = ativo;
        this.toastr.success(`Federacao ${ativo ? 'ativada' : 'desativada'}.`);
      },
      error: () => this.toastr.error('Nao foi possivel alterar o status.'),
    });
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

  private uploadLogoSeNecessario(): Observable<string> {
    if (!this.logoArquivo) return of(this.logoUrl);
    return this.service.uploadLogo(this.logoArquivo).pipe(
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
    if (logoUrl.startsWith('/uploads/federacoes/')) {
      const fileName = logoUrl.split('/').pop();
      return fileName ? `${environment.apiUrl}/Federacoes/logo/${fileName}` : logoUrl;
    }
    return logoUrl;
  }

  private resetForm() {
    this.editandoId = null;
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
    this.mercadoPagoAccessToken = '';
    this.mercadoPagoPublicKey = '';
  }
}
