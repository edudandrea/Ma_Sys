import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, TemplateRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import {
  MensalidadeSistema,
  MensalidadesSistemaService,
} from '../Services/MensalidadesSistema/MensalidadesSistema.service';

@Component({
  selector: 'app-MensalidadesSistema',
  templateUrl: './MensalidadesSistema.component.html',
  styleUrls: ['./MensalidadesSistema.component.css'],
  imports: [CommonModule, FormsModule],
})
export class MensalidadesSistemaComponent implements OnInit {
  modalRef?: BsModalRef;
  mensalidades: (MensalidadeSistema & { menuAberto?: boolean })[] = [];
  editarId: number | null = null;
  role = '';
  valor = 0;
  prazoPagamentoDias = 0;
  mesesUso = 0;
  descricao = '';
  ativo = true;
  aceitaPix = true;
  aceitaCartao = true;
  mercadoPagoPublicKey = '';
  mercadoPagoAccessToken = '';

  constructor(
    private mensalidadesService: MensalidadesSistemaService,
    private toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private cd: ChangeDetectorRef,
    private modalService: BsModalService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
    this.role = usuario.role || '';

    if (!this.canManageMensalidades()) {
      this.toastr.warning('Tela disponivel apenas para administradores do sistema.');
      this.router.navigate(['/dashboard']);
      return;
    }

    this.carregarMensalidades();
  }

  get tituloPagina(): string {
    return this.role === 'SuperAdmin' ? 'Mensalidades do Sistema' : 'Mensalidades das suas academias';
  }

  openModalNovaMensalidade(template: TemplateRef<any>) {
    this.resetForm();
    this.modalRef = this.modalService.show(template, {
      class: 'modal-md modal-dialog-centered',
    });
  }

  toggleStatus(mensalidade: MensalidadeSistema) {
    const novoStatus = !mensalidade.ativo;

    this.mensalidadesService.atualizarStatus(mensalidade.id, novoStatus).subscribe({
      next: () => {
        mensalidade.ativo = novoStatus;
        this.mensalidades = [...this.mensalidades];
        this.cd.markForCheck();
        this.toastr.success(`Mensalidade ${novoStatus ? 'ativada' : 'desativada'}.`);
      },
      error: () => {
        this.toastr.error('Erro ao atualizar status da mensalidade.');
      },
    });
  }

  carregarMensalidades() {
    this.spinner.show();

    this.mensalidadesService.listar().subscribe({
      next: (res) => {
        this.spinner.hide();
        this.mensalidades = (res || []).map((item) => ({
          ...item,
          menuAberto: false,
          ativo: !!item.ativo,
          aceitaPix: !!item.aceitaPix,
          aceitaCartao: !!item.aceitaCartao,
        }));
        this.cd.detectChanges();
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Erro ao carregar mensalidades do sistema.');
      },
    });
  }

  criarMensalidade() {
    if (!this.formValido()) {
      return;
    }

    this.spinner.show();

    this.mensalidadesService.criar({
      valor: this.valor,
      prazoPagamentoDias: this.prazoPagamentoDias,
      mesesUso: this.mesesUso,
      descricao: this.descricao,
      aceitaPix: this.aceitaPix,
      aceitaCartao: this.aceitaCartao,
      mercadoPagoPublicKey: this.mercadoPagoPublicKey,
      mercadoPagoAccessToken: this.mercadoPagoAccessToken,
    }).subscribe({
      next: () => {
        this.spinner.hide();
        this.modalRef?.hide();
        this.toastr.success('Mensalidade cadastrada com sucesso.');
        this.resetForm();
        this.carregarMensalidades();
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Erro ao cadastrar mensalidade.');
      },
    });
  }

  editarMensalidade(mensalidade: MensalidadeSistema) {
    this.editarId = mensalidade.id;
    this.valor = mensalidade.valor;
    this.prazoPagamentoDias = mensalidade.prazoPagamentoDias;
    this.mesesUso = mensalidade.mesesUso;
    this.descricao = mensalidade.descricao || '';
    this.ativo = mensalidade.ativo;
    this.aceitaPix = mensalidade.aceitaPix;
    this.aceitaCartao = mensalidade.aceitaCartao;
    this.mercadoPagoPublicKey = mensalidade.mercadoPagoPublicKey || '';
    this.mercadoPagoAccessToken = '';
  }

  salvarEdicao(mensalidade: MensalidadeSistema) {
    if (!this.formValido()) {
      return;
    }

    this.mensalidadesService.atualizar({
      id: mensalidade.id,
      valor: this.valor,
      prazoPagamentoDias: this.prazoPagamentoDias,
      mesesUso: this.mesesUso,
      descricao: this.descricao,
      ativo: this.ativo,
      aceitaPix: this.aceitaPix,
      aceitaCartao: this.aceitaCartao,
      mercadoPagoPublicKey: this.mercadoPagoPublicKey,
      mercadoPagoAccessToken: this.mercadoPagoAccessToken,
    }).subscribe({
      next: () => {
        mensalidade.valor = this.valor;
        mensalidade.prazoPagamentoDias = this.prazoPagamentoDias;
        mensalidade.mesesUso = this.mesesUso;
        mensalidade.descricao = this.descricao;
        mensalidade.ativo = this.ativo;
        mensalidade.aceitaPix = this.aceitaPix;
        mensalidade.aceitaCartao = this.aceitaCartao;
        mensalidade.mercadoPagoPublicKey = this.mercadoPagoPublicKey;
        this.editarId = null;
        this.toastr.success('Mensalidade atualizada com sucesso.');
        this.carregarMensalidades();
      },
      error: () => {
        this.toastr.error('Erro ao atualizar mensalidade.');
      },
    });
  }

  cancelarEdicao() {
    this.editarId = null;
    this.resetForm();
  }

  formatarValor(valor: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(valor || 0);
  }

  private canManageMensalidades(): boolean {
    return this.role === 'Admin' || this.role === 'SuperAdmin';
  }

  private formValido(): boolean {
    if (this.valor <= 0) {
      this.toastr.warning('Informe um valor maior que zero.');
      return false;
    }

    if (this.prazoPagamentoDias <= 0) {
      this.toastr.warning('Informe o prazo de pagamento em dias.');
      return false;
    }

    if (this.mesesUso <= 0) {
      this.toastr.warning('Informe os meses de uso.');
      return false;
    }

    if (!this.aceitaPix && !this.aceitaCartao) {
      this.toastr.warning('Habilite PIX, cartao ou ambos para a cobranca.');
      return false;
    }

    if (this.aceitaCartao && !this.mercadoPagoPublicKey.trim()) {
      this.toastr.warning('Informe a chave publica do Mercado Pago para pagamento com cartao.');
      return false;
    }

    if (!this.editarId && !this.mercadoPagoAccessToken.trim()) {
      this.toastr.warning('Informe o Access Token do Mercado Pago para receber a cobranca.');
      return false;
    }

    return true;
  }

  private resetForm() {
    this.valor = 0;
    this.prazoPagamentoDias = 0;
    this.mesesUso = 0;
    this.descricao = '';
    this.ativo = true;
    this.aceitaPix = true;
    this.aceitaCartao = true;
    this.mercadoPagoPublicKey = '';
    this.mercadoPagoAccessToken = '';
  }
}
