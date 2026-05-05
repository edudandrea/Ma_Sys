import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, TemplateRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Planos, PlanosService } from '../Services/Planos/Planos.service';
import { ToastrService } from 'ngx-toastr';
import { NgxSpinnerService } from 'ngx-spinner';
import { Academias, AcademiasService } from '../Services/AcademiaService/Academias.service';

@Component({
  selector: 'app-Planos',
  templateUrl: './Planos.component.html',
  styleUrls: ['./Planos.component.css'],
  imports: [CommonModule, FormsModule],
})
export class PlanosComponent implements OnInit {
  modalRef?: BsModalRef;
  planos: (Planos & { menuAberto?: boolean; academiaNome?: string })[] = []; // Substitua 'any' pelo tipo correto do seu plano
  id: number = 0;
  academiaId: number = 0;
  nome: string = '';
  valor: number = 0;
  ativo: boolean = true;
  editarId: number | null = null;
  duracaoMeses: number = 0;
  totalAlunos: number = 0;
  academias: Academias[] = [];
  role = '';

  planoMap = new Map<number, string>();

  constructor(
    private modalService: BsModalService,
    private planosService: PlanosService,
    private toastr: ToastrService,
    private cd: ChangeDetectorRef,
    private spinner: NgxSpinnerService,
    private academiasService: AcademiasService,
  ) {}

  ngOnInit() {
    const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
    this.role = usuario.role || '';
    this.academiaId = usuario.academiaId || 0;
    if (!this.isAcademia && !this.isFederacao) {
      this.carregarAcademias();
    }
    this.carregarPlanos();
  }

  get isAcademia(): boolean {
    return this.role === 'Academia';
  }

  get isFederacao(): boolean {
    return this.role === 'Federacao';
  }

  getInicial(nome: string): string {
    return nome ? nome.charAt(0).toUpperCase() : '?';
  }

  openModalNovoPlano(template: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template, {
      class: 'modal-md modal-dialog-centered',
    });
  }

  toggleStatus(planos: Planos) {
    const novoStatus = !planos.ativo;

    this.planosService.atualizarStatus(planos.id, novoStatus).subscribe({
      next: () => {
        planos.ativo = novoStatus;

        this.planos = [...this.planos];

        this.cd.markForCheck();
        this.carregarPlanos();

        this.toastr.success(`Plano ${novoStatus ? 'ativado' : 'desativado'}`);
      },

      error: () => {
        this.toastr.error('Erro ao atualizar plano');
      },
    });
  }

  toggleMenu(modalidade: any, event: Event) {
    event.stopPropagation();

    this.planos.forEach((m: any) => {
      if (m !== modalidade) {
        m.menuAberto = false;
      }
    });

    modalidade.menuAberto = !modalidade.menuAberto;

    this.planos = [...this.planos];
  }

  carregarPlanos() {
    this.spinner.show();

    this.planosService.getPlanos().subscribe({
      next: (res) => {
        this.spinner.hide();

        if (!res || res.length === 0) {
          this.toastr.warning('Nenhum plano cadastrado', 'Atenção');
          this.planos = [];
          console.log('RES COMPLETO:', res);
          return;
        }

        console.log('Planos recebidos:', res);

        this.planos = res.map((p) => {
          return {
            ...p,
            menuAberto: false,
            ativo: !!p.ativo,
          };
        });
        this.cd.detectChanges();
      },

      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao carregar plano', 'Erro');
      },
    });
  }

  carregarAcademias() {
    this.academiasService.getAcademias().subscribe({
      next: (res) => {
        this.academias = res;
      },
      error: () => {
        this.toastr.error('Erro ao carregar academias.');
      },
    });
  }

  novoPlano() {
    this.spinner.show();

    const plano: Partial<Planos> = {
      nome: this.nome,
      valor: this.valor,
      duracaoMeses: this.duracaoMeses,
    };

    if (!this.isFederacao) {
      plano.academiaId = this.academiaId;
    }

    console.group('NOVO PLANO');
    console.log(JSON.stringify(plano, null, 2));
    console.groupEnd();

    this.planosService.novoPlano(plano).subscribe({
      next: (res) => {
        this.spinner.hide();
        this.toastr.success('Plano cadastrado!', 'Sucesso');

        setTimeout(() => {
          this.nome = '';
        }, 500);

        this.modalRef?.hide();

        this.carregarPlanos();
      },
      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao salvar plano', 'Erro');
      },
    });
  }

  editarPlano(plano: Planos) {
    this.editarId = plano.id;
    this.nome = plano.nome;
    this.valor = plano.valor;
    this.duracaoMeses = plano.duracaoMeses;
    plano.menuAberto = false;
  }

  salvarEdicao(plano: Planos) {
    const payload = {
      id: plano.id,
      nome: this.nome,
      valor: this.valor,
      duracaoMeses: this.duracaoMeses,
    };

    this.planosService.atualizarPlano(payload).subscribe({
      next: () => {
        plano.nome = this.nome;
        plano.valor = this.valor;
        plano.duracaoMeses = this.duracaoMeses;

        this.editarId = null;

        this.carregarPlanos();

        this.toastr.success('Plano atualizado');
      },
    });
  }

  cancelarEdicao() {
    this.editarId = null;
    this.planos.forEach((p) => (p.menuAberto = false));
  }

  excluirPlano(plano: number): void {}
}
