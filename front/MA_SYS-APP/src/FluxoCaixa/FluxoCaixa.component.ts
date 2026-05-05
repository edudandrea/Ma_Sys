import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, TemplateRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { Academias, AcademiasService } from '../Services/AcademiaService/Academias.service';
import {
  FluxoCaixaMovimento,
  FluxoCaixaResponse,
  FluxoCaixaService,
} from '../Services/FluxoCaixa/FluxoCaixa.service';

@Component({
  selector: 'app-FluxoCaixa',
  templateUrl: './FluxoCaixa.component.html',
  styleUrls: ['./FluxoCaixa.component.css'],
  imports: [CommonModule, FormsModule],
})
export class FluxoCaixaComponent implements OnInit {
  modalRef?: BsModalRef;
  role = '';
  academias: Academias[] = [];
  academiaIdSelecionada: number | null = null;
  resumo: FluxoCaixaResponse['resumo'] = {
    totalEntradas: 0,
    totalSaidas: 0,
    saldo: 0,
    totalMovimentos: 0,
  };
  movimentos: FluxoCaixaMovimento[] = [];

  novoLancamento = {
    academiaId: null as number | null,
    valor: 0,
    data: new Date().toISOString().slice(0, 10),
    tipo: 'Saida',
    categoria: '',
    descricao: '',
  };

  constructor(
    private fluxoCaixaService: FluxoCaixaService,
    private academiasService: AcademiasService,
    private spinner: NgxSpinnerService,
    private toastr: ToastrService,
    private cd: ChangeDetectorRef,
    private modalService: BsModalService,
  ) {}

  ngOnInit(): void {
    const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
    this.role = usuario.role || '';
    this.academiaIdSelecionada = usuario.role === 'Academia' ? (usuario.academiaId ?? null) : null;
    this.novoLancamento.academiaId = this.academiaIdSelecionada;

    if (!this.isAcademia && !this.isFederacao) {
      this.carregarAcademias();
    }

    this.carregarFluxo();
  }

  get isAcademia(): boolean {
    return this.role === 'Academia';
  }

  get isFederacao(): boolean {
    return this.role === 'Federacao';
  }

  get podeLancarManual(): boolean {
    return true;
  }

  openModalNovoLancamento(template: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template, {
      class: 'modal-lg modal-dialog-centered',
    });
  }

  carregarAcademias() {
    this.academiasService.getAcademias().subscribe({
      next: (res) => {
        this.academias = res;
        this.cd.markForCheck();
      },
      error: () => {
        this.toastr.error('Nao foi possivel carregar as academias.');
      },
    });
  }

  carregarFluxo() {
    this.spinner.show();
    this.fluxoCaixaService.listar(this.academiaIdSelecionada ?? undefined).subscribe({
      next: (res) => {
        this.resumo = res.resumo;
        this.movimentos = res.movimentos;
        this.spinner.hide();
        this.cd.markForCheck();
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Erro ao carregar o fluxo de caixa.');
      },
    });
  }

  salvarLancamento() {
    const payload = {
      ...this.novoLancamento,
      academiaId: this.isFederacao
        ? null
        : this.isAcademia
          ? this.academiaIdSelecionada
          : this.novoLancamento.academiaId,
    };

    this.fluxoCaixaService.lancar(payload).subscribe({
      next: () => {
        this.toastr.success('Lancamento registrado com sucesso.');
        this.modalRef?.hide();
        this.novoLancamento = {
          academiaId: this.academiaIdSelecionada,
          valor: 0,
          data: new Date().toISOString().slice(0, 10),
          tipo: 'Saida',
          categoria: '',
          descricao: '',
        };
        this.carregarFluxo();
      },
      error: () => {
        this.toastr.error('Nao foi possivel salvar o lancamento.');
      },
    });
  }
}
