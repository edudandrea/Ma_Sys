import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, HostListener, OnInit, TemplateRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { Academias, AcademiasService } from '../Services/AcademiaService/Academias.service';

@Component({
  selector: 'app-Academias',
  templateUrl: './Academias.component.html',
  styleUrls: ['./Academias.component.css'],
  imports: [CommonModule, FormsModule],
})
export class AcademiasComponent implements OnInit {
  modalRef?: BsModalRef;
  id = 0;
  nome = '';
  totalAlunos = 0;
  cidade = '';
  email = '';
  telefone = '';
  redeSocial = '';
  dataCadastro = '';
  ativo = true;
  editarId: number | null = null;
  responsavel = '';
  totalProf = 0;
  linkCadastro = '';
  slug = '';
  publicOrigin = '';

  academias: (Academias & { menuAberto?: boolean })[] = [];

  constructor(
    private modalService: BsModalService,
    private toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private acad: AcademiasService,
    private cd: ChangeDetectorRef,
    private academiaService: AcademiasService,
  ) {}

  ngOnInit() {
    this.publicOrigin = typeof window !== 'undefined' ? window.location.origin : '';
    this.carregarAcademias();
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
    this.modalRef = this.modalService.show(template, {
      class: 'modal-md modal-dialog-centered',
    });
  }

  openModalExcluir(template: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template, {
      class: 'modal-md modal-dialog-centered',
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

    const academia = {
      nome: this.nome,
      cidade: this.cidade,
      telefone: this.telefone,
      email: this.email,
      redeSocial: this.redeSocial,
      responsavel: this.responsavel,
      dataCadastro: this.dataCadastro || new Date().toISOString(),
    };

    this.academiaService.novaAcademia(academia).subscribe({
      next: (res) => {
        this.linkCadastro = this.getCadastroLink(res.slug);
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
  }

  editarAcademia(academia: Academias & { menuAberto?: boolean }) {
    this.editarId = academia.id;
    this.nome = academia.nome;
    this.cidade = academia.cidade;
    this.telefone = academia.telefone;
    this.email = academia.email;
    this.redeSocial = academia.redeSocial;
    this.responsavel = academia.responsavel;
    academia.menuAberto = false;
  }

  salvarEdicao(academia: Academias) {
    const payload = {
      id: academia.id,
      nome: this.nome,
      cidade: this.cidade,
      telefone: this.telefone,
      email: this.email,
      redeSocial: this.redeSocial,
      responsavel: this.responsavel,
      ativo: academia.ativo,
    };

    this.academiaService.atualizarAcademia(payload).subscribe({
      next: () => {
        academia.nome = this.nome;
        academia.cidade = this.cidade;
        academia.telefone = this.telefone;
        academia.email = this.email;
        academia.redeSocial = this.redeSocial;
        academia.responsavel = this.responsavel;
        this.editarId = null;
        this.carregarAcademias();
        this.toastr.success('Academia atualizada');
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
}
