import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, TemplateRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { UserService } from '../Services/UsuarioService/User.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { AcademiasService } from '../Services/AcademiaService/Academias.service';

@Component({
  selector: 'app-Usuarios',
  templateUrl: './Usuarios.component.html',
  styleUrls: ['./Usuarios.component.css'],
  imports: [CommonModule, FormsModule],
})
export class UsuariosComponent implements OnInit {
  modalRef?: BsModalRef;

  userId: number = 0;
  userName: string = '';
  login: string = '';
  senha: string = '';
  confirmarSenha: string = '';
  academiaId: string = '';
  role: string = '';
  academias: any[] = [];
  usuarios: any[] = [];
  editando: boolean = false;

  modalUsuario!: TemplateRef<any>;

  constructor(
    private modalService: BsModalService,
    private toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private service: UserService,
    private cd: ChangeDetectorRef,
    private acad: AcademiasService
  ) {}

  ngOnInit() {
    this.carregarAcademias();
    this.loadUsers();
  }

  toggleMenu(usuario: any) {
    this.usuarios.forEach((m: any) => {
      if (m !== usuario) {
        m.menuAberto = false;
      }
    });

    usuario.menuAberto = !usuario.menuAberto;

    this.usuarios = [...this.usuarios];
  }

  openModals(template: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template, {
      class: 'modal-md modal-dialog-centered',
    });
  }

  closeModals() {
    this.modalRef?.hide();
  }

  getInicial(nome: string): string {    
    return nome ? nome.charAt(0).toUpperCase() : '?';
  }

  carregarAcademias() {
    this.acad.getAcademias().subscribe({
      next: (res) => {
        console.log('Academias recebidas:', res);
        this.spinner.hide();
        this.academias = res.map((m) => ({
          ...m,
          menuAberto: false,
        }));

        this.cd.markForCheck(); 
      },

      error: (err) => {
        console.error(err);
        this.toastr.error('Erro ao carregar Academias');
      },
    });
  }

  loadUsers() {
    this.spinner.show();
    this.service.getUsuarios().subscribe({
      next: (res) => {
        console.log('Usuarios recebidas:', res);
        this.spinner.hide();
        this.usuarios = res.map((m) => ({
          ...m,
          menuAberto: false,
        }));

        this.cd.markForCheck(); 
      },

      error: (err) => {
        console.error(err);
        this.toastr.error('Erro ao carregar Usuários');
      },
    });
  }

  salvar() {
    if (this.senha !== this.confirmarSenha) {
      this.toastr.warning('As senhas não coincidem.');
      return;
    }

    const payload = {
      userId: 0,
      userName: this.userName,
      login: this.login,
      password: this.senha,
      academiaId: this.role == 'Admin' ? 0 : parseInt(this.academiaId),
      role: this.role,
    };

    this.service.novoUsuario(payload).subscribe({
      next: () => {
        this.toastr.success('Usuário criado com sucesso!');
        this.closeModals();
        this.loadUsers();
      },
      error: (error) => {
        console.error('Erro ao criar usuário:', error);
        this.toastr.error('Ocorreu um erro ao criar o usuário. Por favor, tente novamente.');
      },
    });
  }

  editar(user: any) {
    this.userId = user.id;
    this.editando = true;
    this.userName = user.userName;
    this.login = user.login;
    this.role = user.role;
    this.academiaId = user.academiaId || '';

    this.openModals(this.modalUsuario);
  }

  deleteUsuario(userId: number): void {
    if (confirm('Deseja realmente excluir esse usuário?')) {
      this.spinner.show();
      this.service.deleteUsuario(userId).subscribe({
        next: () => {
          this.toastr.success('Usuário excluído com sucesso!', 'Sucesso');
          this.spinner.hide();
          this.loadUsers();
        },
        error: (err) => {
          console.error('Erro ao excluir usuário', err);
          this.toastr.error('Erro ao excluir usuário!', 'Erro');
          this.spinner.hide();
        },
      });
    }
  }

  cancelar() {}
}
