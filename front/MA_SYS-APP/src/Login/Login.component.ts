import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../Services/Auth/Auth.service';
import { UserService } from '../Services/UsuarioService/User.service';

@Component({
  selector: 'app-Login',
  templateUrl: './Login.component.html',
  styleUrls: ['./Login.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule],
})
export class LoginComponent implements OnInit {
  login = '';
  password = '';
  bootstrapUserName = '';
  bootstrapLogin = '';
  bootstrapEmail = '';
  bootstrapPassword = '';
  bootstrapPasswordConfirm = '';
  requiresBootstrap = false;
  checkingBootstrap = true;
  creatingBootstrap = false;
  private bootstrapCheckTimeoutId: ReturnType<typeof setTimeout> | null = null;

  constructor(
    private auth: AuthService,
    private router: Router,
    private toastr: ToastrService,
    private userService: UserService,
  ) {}

  ngOnInit() {
    this.checkBootstrapStatus();
  }

  entrar() {
    this.auth.login(this.login, this.password).subscribe({
      next: (response) => {
        this.toastr.success('Login realizado com sucesso', 'Sucesso');

        const token = response.token;
        localStorage.setItem('token', token);
        localStorage.setItem('role', response.role);
        localStorage.setItem('usuario', JSON.stringify(response.usuario));

        const decodedToken = this.decodeToken(token);
        const role = decodedToken.role;

        localStorage.setItem('role', role);
        this.router.navigate(['/dashboard']);
      },
      error: () => {
        this.toastr.error('Usuario ou senha invalidos', 'Erro');
      },
    });
  }

  criarPrimeiroAcesso() {
    if (!this.requiresBootstrap || this.creatingBootstrap) {
      return;
    }

    if (
      !this.bootstrapUserName.trim() ||
      !this.bootstrapLogin.trim() ||
      !this.bootstrapEmail.trim() ||
      !this.bootstrapPassword ||
      !this.bootstrapPasswordConfirm
    ) {
      this.toastr.warning('Preencha todos os campos para criar o primeiro acesso.');
      return;
    }

    if (this.bootstrapPassword !== this.bootstrapPasswordConfirm) {
      this.toastr.warning('As senhas nao coincidem.');
      return;
    }

    this.creatingBootstrap = true;

    this.userService
      .novoUsuario({
        userName: this.bootstrapUserName.trim(),
        login: this.bootstrapLogin.trim(),
        email: this.bootstrapEmail.trim(),
        password: this.bootstrapPassword,
      })
      .subscribe({
        next: () => {
          this.toastr.success('Super admin criado com sucesso. Entre com o novo acesso.');
          this.login = this.bootstrapLogin.trim();
          this.password = '';
          this.requiresBootstrap = false;
          this.resetBootstrapForm();
          this.creatingBootstrap = false;
        },
        error: (error) => {
          const message =
            error?.error && typeof error.error === 'string'
              ? error.error
              : 'Nao foi possivel criar o primeiro usuario.';
          this.toastr.error(message, 'Erro');
          this.creatingBootstrap = false;
          this.checkBootstrapStatus();
        },
      });
  }

  decodeToken(token: string): any {
    const payload = token.split('.')[1];
    const decodedPayload = atob(payload);
    return JSON.parse(decodedPayload);
  }

  private checkBootstrapStatus() {
    this.checkingBootstrap = true;
    this.clearBootstrapCheckTimeout();

    this.bootstrapCheckTimeoutId = setTimeout(() => {
      this.requiresBootstrap = false;
      this.checkingBootstrap = false;
    }, 4000);

    this.userService.getBootstrapStatus().subscribe({
      next: (response) => {
        this.clearBootstrapCheckTimeout();
        this.requiresBootstrap = !!response?.requiresBootstrap;
        this.checkingBootstrap = false;
      },
      error: () => {
        this.clearBootstrapCheckTimeout();
        this.requiresBootstrap = false;
        this.checkingBootstrap = false;
      },
    });
  }

  private clearBootstrapCheckTimeout() {
    if (this.bootstrapCheckTimeoutId) {
      clearTimeout(this.bootstrapCheckTimeoutId);
      this.bootstrapCheckTimeoutId = null;
    }
  }

  private resetBootstrapForm() {
    this.bootstrapUserName = '';
    this.bootstrapLogin = '';
    this.bootstrapEmail = '';
    this.bootstrapPassword = '';
    this.bootstrapPasswordConfirm = '';
  }
}
