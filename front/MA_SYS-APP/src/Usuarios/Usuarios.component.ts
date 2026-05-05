import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { UserService } from '../Services/UsuarioService/User.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { AcademiasService } from '../Services/AcademiaService/Academias.service';
import { FiliadosService } from '../Services/Filiados/Filiados.service';
import { FederacoesService } from '../Services/Federacoes/Federacoes.service';

@Component({
  selector: 'app-Usuarios',
  templateUrl: './Usuarios.component.html',
  styleUrls: ['./Usuarios.component.css'],
  imports: [CommonModule, FormsModule],
})
export class UsuariosComponent implements OnInit {
  modalRef?: BsModalRef;

  userId = 0;
  userName = '';
  login = '';
  email = '';
  senha = '';
  confirmarSenha = '';
  academiaId = '';
  federacaoId = '';
  role = '';
  academias: any[] = [];
  usuarios: any[] = [];
  editando = false;
  currentRole = '';
  filiados: any[] = [];
  federacoes: any[] = [];

  @ViewChild('modalUsuario') modalUsuario!: TemplateRef<any>;

  constructor(
    private modalService: BsModalService,
    private toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private service: UserService,
    private cd: ChangeDetectorRef,
    private acad: AcademiasService,
    private filiadosService: FiliadosService,
    private federacoesService: FederacoesService,
  ) {}

  ngOnInit() {
    const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
    this.currentRole = usuario.role || '';
    this.carregarAcademias();
    this.carregarFiliados();
    this.carregarFederacoes();
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
    if (!this.editando) {
      this.resetForm();
    }

    this.modalRef = this.modalService.show(template, {
      class: 'modal-md modal-dialog-centered',
    });
  }

  closeModals() {
    this.modalRef?.hide();
    this.resetForm();
  }

  getInicial(nome: string): string {
    return nome ? nome.charAt(0).toUpperCase() : '?';
  }

  carregarAcademias() {
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
        this.toastr.error('Erro ao carregar academias.');
      },
    });
  }

  carregarFiliados() {
    this.filiadosService.getFiliados().subscribe({
      next: (res) => {
        this.filiados = res ?? [];
        this.cd.markForCheck();
      },
      error: () => {
        this.filiados = [];
      },
    });
  }

  carregarFederacoes() {
    this.federacoesService.listar().subscribe({
      next: (res) => {
        this.federacoes = res ?? [];
        this.cd.markForCheck();
      },
      error: () => {
        this.federacoes = [];
      },
    });
  }

  loadUsers() {
    this.spinner.show();
    this.service.getUsuarios().subscribe({
      next: (res) => {
        this.spinner.hide();
        this.usuarios = res.map((m) => ({
          ...m,
          menuAberto: false,
        }));

        this.cd.markForCheck();
      },
      error: (err) => {
        console.error(err);
        this.toastr.error('Erro ao carregar usuários.');
      },
    });
  }

  salvar() {
    if (!this.editando && this.senha !== this.confirmarSenha) {
      this.toastr.warning('As senhas não coincidem.');
      return;
    }

    if (this.role === 'Academia' && !this.academiaId) {
      this.toastr.warning('Selecione a academia.');
      return;
    }

    if (this.role === 'Federacao' && !this.federacaoId) {
      this.toastr.warning('Selecione a federação.');
      return;
    }

    const payload: any = {
      userId: this.userId,
      userName: this.userName,
      login: this.login,
      email: this.email,
      academiaId: this.role === 'Academia'
        ? parseInt(this.academiaId, 10)
        : undefined,
      federacaoId: this.role === 'Federacao'
        ? parseInt(this.federacaoId, 10)
        : undefined,
      role: this.role,
    };

    if (!this.editando) {
      payload.password = this.senha;
    } else if (this.senha) {
      payload.password = this.senha;
    }

    const request$ = this.editando
      ? this.service.atualizarUsuario(payload)
      : this.service.novoUsuario(payload);

    request$.subscribe({
      next: () => {
        this.toastr.success(this.editando ? 'Usuário atualizado com sucesso!' : 'Usuário criado com sucesso!');
        this.closeModals();
        this.loadUsers();
      },
      error: (error) => {
        console.error('Erro ao salvar usuário:', error);
        this.toastr.error('Ocorreu um erro ao salvar o usuário. Tente novamente.');
      },
    });
  }

  editar(user: any) {
    this.userId = user.userId ?? user.id;
    this.editando = true;
    this.userName = user.userName;
    this.login = user.login;
    this.email = user.email || '';
    this.role = user.role;
    this.academiaId = user.academiaId ? String(user.academiaId) : '';
    this.federacaoId = user.federacaoId ? String(user.federacaoId) : '';
    this.senha = '';
    this.confirmarSenha = '';

    this.openModals(this.modalUsuario);
  }

  getVinculo(user: any): string {
    if (user.role === 'Federacao') {
      return user.federacaoNome || this.federacoes.find((f) => f.id === user.federacaoId)?.nome || 'Federação';
    }

    return user.academiaNome || 'Global';
  }

  deleteUsuario(userId: number): void {
    if (confirm('Deseja realmente excluir este usuário?')) {
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

  cancelar() {
    this.closeModals();
  }

  private resetForm() {
    this.userId = 0;
    this.userName = '';
    this.login = '';
    this.email = '';
    this.senha = '';
    this.confirmarSenha = '';
    this.academiaId = '';
    this.federacaoId = '';
    this.role = '';
    this.editando = false;
  }
}
