import { Component, OnInit, TemplateRef } from '@angular/core';
import { Modalidades, ModalidadesService } from '../Services/ModalidadeService/Modalidades.service';
import { ToastrService } from 'ngx-toastr';
import { CommonModule } from '@angular/common';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { FormsModule } from '@angular/forms';
import { NgxSpinnerService } from 'ngx-spinner';
import { HostListener } from '@angular/core';
import { ChangeDetectorRef, ChangeDetectionStrategy, NgZone } from '@angular/core';
import { Academias, AcademiasService } from '../Services/AcademiaService/Academias.service';

@Component({
  selector: 'app-Modalidades',
  standalone: true,
  templateUrl: './Modalidades.component.html',
  styleUrls: ['./Modalidades.component.css'],
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ModalidadesComponent implements OnInit {
  modalidadeId: number = 0;
  modalidades: (Modalidades & { menuAberto?: boolean; academiaNome?: string })[] = [];
  academias: (Academias)[] = [];
  ativo: boolean = true;
  nomeModalidade: string = '';
  totalAlunos: number = 0;
  totalProf: number = 0;
  academiaId: number = 0;

  academiaMap = new Map<number, string>();

  modalRef?: BsModalRef;

  editarId: number | null = null;

  novaModalidade = {
    nomeModalidade: '',
  };

  constructor(
    private modalidadesService: ModalidadesService,
    private toastr: ToastrService,
    private modalService: BsModalService,
    private spinner: NgxSpinnerService,
    private cd: ChangeDetectorRef,
    private acad: AcademiasService,
  ) {}

  ngOnInit() {
    console.log('Tela modalidades carregada');
     this.carregarAcademias(() => {
    this.carregarModalidades();
  });
      
  }

  trackByModalidade(index: number, item: Modalidades) {
    return item.id;
  }

  @HostListener('document:click', ['$event'])
  fecharMenu(event: any) {
    const clicouMenu = event.target.closest('.card-menu');

    if (!clicouMenu) {
      this.modalidades.forEach((m: any) => (m.menuAberto = false));
    }
  }

  // Alterna o status ativo/inativo da modalidade

  toggleMenu(modalidade: any, event: Event) {
    event.stopPropagation();

    this.modalidades.forEach((m: any) => {
      if (m !== modalidade) {
        m.menuAberto = false;
      }
    });

    modalidade.menuAberto = !modalidade.menuAberto;

    this.modalidades = [...this.modalidades];
  }

  toggleStatus(modalidade: Modalidades) {
    const novoStatus = !modalidade.ativo;

    this.modalidadesService.atualizarStatus(modalidade.id, novoStatus).subscribe({
      next: () => {
        modalidade.ativo = novoStatus;

        this.modalidades = [...this.modalidades];

        this.cd.markForCheck();
        this.carregarModalidades();

        this.toastr.success(`Modalidade ${novoStatus ? 'ativada' : 'desativada'}`);
      },

      error: () => {
        this.toastr.error('Erro ao atualizar modalidade');
      },
    });
  }

  //--------------------------------------------------------------

  cadastrarNovaModalidade() {
    this.spinner.show();

    const modalidade = {      
      nomeModalidade: this.novaModalidade.nomeModalidade,
    };

    console.group('📤 NOVA MODALIDADE');
    console.log(JSON.stringify(modalidade, null, 2));
    console.groupEnd();

    this.modalidadesService.novaModalidade(modalidade).subscribe({
      next: (res) => {
        this.spinner.hide();
        this.toastr.success('Modalidade cadastrada!', 'Sucesso');

        this.novaModalidade.nomeModalidade = '';

        this.modalRef?.hide();

        this.carregarModalidades();
      },
      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao salvar Modalidade', 'Erro');
      },
    });
  }

  editarModalidade(modalidade: Modalidades) {
    this.editarId = modalidade.id;
    this.nomeModalidade = modalidade.nomeModalidade;
    modalidade.menuAberto = false;
  }

  cancelarEdicao() {
    this.editarId = null;
  }

  salvarEdicao(modalidade: Modalidades) {
    const payload = {
      id: modalidade.id,
      nomeModalidade: this.nomeModalidade,
      ativo: modalidade.ativo,
    };

    this.modalidadesService.atualizarModalidade(payload).subscribe({
      next: () => {
        modalidade.nomeModalidade = this.nomeModalidade;

        this.editarId = null;

        this.carregarModalidades();

        this.toastr.success('Modalidade atualizada');
      },
    });
  }

  carregarModalidades() {
  this.spinner.show();

  this.modalidadesService.getModalidades().subscribe({
    next: (res) => {
      console.log('Modalidades recebidas:', res);

      this.spinner.hide();

      this.modalidades = res.map((m) => {

        // 🔍 DEBUG
        console.log('--------------------------------');
        console.log('Modalidade:', m.nomeModalidade);
        console.log('ID Academia:', m.academiaId);
        console.log('Map completo:', this.academiaMap);
        console.log('Nome encontrado:', this.academiaMap.get(m.academiaId));

        return {
          ...m,
          menuAberto: false,

          // ✅ CORRETO (sem erro de digitação)
          academiaNome: this.academiaMap.get(m.academiaId) || 'Sem Academia',
        };
      });

      this.cd.markForCheck();
    },

    error: (err) => {
      console.error(err);
      this.toastr.error('Erro ao carregar modalidades');
      this.spinner.hide();
    },
  });
}

  excluirModalidade(modalidadeId: number): void {
    if (confirm('Deseja realmente excluir essa modalidade?')) {
      this.spinner.show();
      this.modalidadesService.excluirModalidade(modalidadeId).subscribe({
        next: () => {
          this.toastr.success('Modalidade excluída com sucesso!', 'Sucesso');
          this.spinner.hide();
          this.carregarModalidades();
        },
        error: (err) => {
          console.error('Erro ao excluir modalidade', err);
          this.toastr.error('Erro ao excluir modalidade!', 'Erro');
          this.spinner.hide();
        },
      });
    }
  }

  openModalNovaModalidade(template: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template, {
      class: 'modal-md modal-dialog-centered',
    });
  }

  carregarAcademias(callback?: () => void) {    
    this.acad.getAcademias().subscribe({
      next: (res) => {
        console.log('Academias recebidas:', res);
       
        this.academias = res;

        this.academiaMap.clear();
        this.academias.forEach(a => this.academiaMap.set(a.id, a.nome));

        this.cd.markForCheck(); 

        if(callback) callback();
      },      

      error: (err) => {
        console.error(err);
        this.toastr.error('Erro ao carregar Academias');
      },
    });
  }  
 

  getIconeModalidade(nome: string): string {
    const icones: any = {
      Musculação: 'fa-dumbbell',
      'Jiu Jitsu': 'fa-hand-fist',
      'Muay Thai': 'fa-hand-back-fist',
      Boxe: 'fa-hand-fist',
      Crossfit: 'fa-fire',
    };

    return icones[nome] || 'fa-medal';
  }
}
