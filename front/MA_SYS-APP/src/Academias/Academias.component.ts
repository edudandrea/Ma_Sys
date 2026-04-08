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
  id: number = 0;
  nome: string = '';
  totalAlunos: number = 0;
  cidade: string = '';
  email: string = '';
  telefone: string = '';
  redeSocial: string = '';
  dataCadastro: string = '';
  ativo: boolean = true;
  editarId: number | null = null;
  responsavel: string = '';
  totalProf: number = 0

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
    this.carregarAcademias();
  }

  getInicial(nome: string): string {
    return nome ? nome.charAt(0).toUpperCase() : '?';
  }

  @HostListener('document:click', ['$event'])
  fecharMenu(event: any) {
    const clicouMenu = event.target.closest('.card-menu');

    if (!clicouMenu) {
      this.academias.forEach((m: any) => (m.menuAberto = false));
    }
  }

  toggleMenu(academia: any, event: Event) {
    event.stopPropagation();

    this.academias.forEach((m: any) => {
      if (m !== academia) {
        m.menuAberto = false;
      }
    });

    academia.menuAberto = !academia.menuAberto;

    this.academias = [...this.academias];
  }

  toggleExpand(card: any) {
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
        console.log('Academias recebidas:', res);
        this.spinner.hide();
        this.academias = res.map((m) => ({
          ...m,
          menuAberto: false,
        }));

        this.cd.markForCheck(); // força Angular atualizar
      },

      error: (err) => {
        console.error(err);
        this.toastr.error('Erro ao carregar Academias');
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

    console.group('📤 NOVO ACADEMIA');
    console.log(JSON.stringify(academia, null, 2));
    console.groupEnd();

    this.academiaService.novaAcademia(academia).subscribe({
      next: () => {
        this.spinner.hide();
        this.toastr.success('Academia cadastrado!', 'Sucesso');

        this.carregarAcademias();
        this.fecharModal();
      },
      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao salvar Academia', 'Erro');
      },
    });
  }

  excluirAcademia(academiaId: number): void {    
      this.spinner.show();
      this.academiaService.excluirAcademia(academiaId).subscribe({
        next: () => {
          this.toastr.success('Academia excluída com sucesso!', 'Sucesso');
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
        this.toastr.error('Erro ao atualizar a Academia');
      },
    });
  }

  fecharModal() {
    this.modalRef?.hide();
  }
}
