import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, HostListener, OnInit, TemplateRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Pagamentos, PagamentosService } from '../Services/PagamentosService/Pagamentos.service';
import { ToastrService } from 'ngx-toastr';
import { NgxSpinnerService } from 'ngx-spinner';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { PixResponse } from '../Model/pix-response.model';
import { Academias, AcademiasService } from '../Services/AcademiaService/Academias.service';

@Component({
  selector: 'app-Pagamentos',
  templateUrl: './Pagamentos.component.html',
  styleUrls: ['./Pagamentos.component.css'],
  imports: [CommonModule, FormsModule],
})
export class PagamentosComponent implements OnInit {
  modalRef?: BsModalRef;
  menuAberto: boolean = false;
  pagamentos: any[] = [];
  nome: string = '';
  ativo: boolean = true;
  taxa: number = 0;
  parcelas: number = 0;
  dias: number = 0;
  pixPayload: string = '';
  valor: number = 0;
  academias: Academias[] = [];
  academiaId = 0;
  role = '';

  novoFormaPgto = {
    nome: '',
    ativo: true,
    taxa: 0,
    parcelas: 0,
    dias: 0,
  };

  editarId: number | null = null;

  constructor(
    private pgService: PagamentosService,
    private toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private cd: ChangeDetectorRef,
    private modalService: BsModalService,
    private academiasService: AcademiasService,
  ) {}

  ngOnInit() {
    const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
    this.role = usuario.role || '';
    this.academiaId = usuario.academiaId || 0;
    if (this.role !== 'Academia') {
      this.carregarAcademias();
    }
    this.carregarFormaPgtos();
  }

  get isAcademia(): boolean {
    return this.role === 'Academia';
  }

  @HostListener('document:click', ['$event'])
  fecharMenu(event: any) {
    const clicouMenu = event.target.closest('.card-menu');

    if (!clicouMenu) {
      this.pagamentos.forEach((m: any) => (m.menuAberto = false));
    }
  }

  gerarQrCodePixRemovido(payload: string) {
    this.toastr.info('A geracao manual de PIX foi removida. Use as cobrancas via Mercado Pago.');
  }

  gerarQrCodePix(payload: string) {
    this.gerarQrCodePixRemovido(payload);
  }

  gerarPix() {
    if (true) {
      this.toastr.info('A geracao manual de PIX foi removida. Use as cobrancas via Mercado Pago.');
      return;
    }

    if (!this.valor || this.valor <= 0) {
      this.toastr.warning('Informe um valor válido');
      return;
    }

    this.spinner.show();

    this.pgService.gerarPix(this.valor).subscribe({
      next: (res: PixResponse) => {
        this.spinner.hide();

        this.pixPayload = res.payload;

        this.gerarQrCodePix(res.payload);

        this.toastr.success('Pix gerado com sucesso!');
      },

      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao gerar Pix');
      },
    });
  }

  getInicial(nome: string): string {
    return nome ? nome.charAt(0).toUpperCase() : '?';
  }

  toggleMenu(modalidade: any, event: Event) {
    event.stopPropagation();

    this.pagamentos.forEach((m: any) => {
      if (m !== modalidade) {
        m.menuAberto = false;
      }
    });

    modalidade.menuAberto = !modalidade.menuAberto;

    this.pagamentos = [...this.pagamentos];
  }

  toggleStatus(pgtos: Pagamentos) {
    const novoStatus = !pgtos.ativo;

    this.pgService.atualizarStatus(pgtos.id, novoStatus).subscribe({
      next: () => {
        pgtos.ativo = novoStatus;

        this.pagamentos = [...this.pagamentos];

        this.cd.markForCheck();
        this.carregarFormaPgtos();

        this.toastr.success(`Pagamento ${novoStatus ? 'ativado' : 'desativado'}`);
      },

      error: () => {
        this.toastr.error('Erro ao atualizar pagamento');
      },
    });
  }

  // ---------- MODAL NOVA FORMA DE PAGAMENTO ----------

  openModalNovoPgto(template: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template, {
      class: 'modal-md modal-dialog-centered',
    });
  }

  //---------- GETS ----------

  carregarFormaPgtos() {
    this.spinner.show();

    this.pgService.getFormaPagamentos().subscribe({
      next: (res) => {
        this.spinner.hide();

        console.log('Forma de pagamento recebidas:', res);

        this.pagamentos = res.map((p) => ({
          ...p,
          menuAberto: false,
        }));
        this.cd.markForCheck();
      },

      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao carregar forma de pagamento', 'Erro');
      },
    });
  }

  carregarAcademias() {
    this.academiasService.getAcademias().subscribe({
      next: (res) => {
        this.academias = res;
      },
      error: () => {
        this.toastr.error('Erro ao carregar academias.', 'Erro');
      },
    });
  }

  // ---------- POST ----------
  cadastrarNovaFormaPgto() {
    this.spinner.show();

    const formaPgto = {
      nome: this.nome,
      ativo: this.ativo,
      taxa: this.taxa,
      parcelas: this.parcelas,
      dias: this.dias,
      academiaId: this.academiaId,
    };

    console.group('📤 NOVA FORMA DE PAGAMENTO');
    console.log(JSON.stringify(formaPgto, null, 2));
    console.groupEnd();

    this.pgService.novaFormaPgto(formaPgto).subscribe({
      next: (res) => {
        this.spinner.hide();
        this.toastr.success('Forma de pagamento cadastrada!', 'Sucesso');

        this.novoFormaPgto.nome = '';

        this.modalRef?.hide();
        this.carregarFormaPgtos();
      },
      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao salvar Forma de Pagamento', 'Erro');
      },
    });
  }

  // ---------- PUT ----------

  editarFormaPagamento(pg: Pagamentos) {
    this.editarId = pg.id;
    this.nome = pg.nome;
    this.taxa = pg.taxa;
    this.parcelas = pg.parcelas;
    this.dias = pg.dias;
    pg.menuAberto = false;
  }

  cancelarEdicao() {
    this.editarId = null;
    this.pagamentos.forEach(p => p.menuAberto = false);
  }

  salvarEdicao(pg: Pagamentos) {
    const payload = {
     id: pg.id,
     nome: this.nome,
     taxa: this.taxa,
     parcelas: this.parcelas,
     dias: this.dias,
    };

    this.pgService.atualizarFormaPagamento(payload).subscribe({
      next: () => {
        pg.nome = this.nome;

        this.editarId = null;

        this.carregarFormaPgtos();

        this.toastr.success('Forma de pagamento atualizada');
      },
    });
  }
}
