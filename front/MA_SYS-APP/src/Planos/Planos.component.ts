import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, TemplateRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Planos, PlanosService } from '../Services/Planos/Planos.service';
import { ToastrService } from 'ngx-toastr';

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
  toitalAlunos: number = 0;

  constructor(
    private modalService: BsModalService,
    private planosService: PlanosService,
    private toastr: ToastrService,
    private cd: ChangeDetectorRef,
  ) {}

  ngOnInit() {}

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

  novoPlano() {
   
  }

  excluirPlano(plano: number): void{

  }
}
