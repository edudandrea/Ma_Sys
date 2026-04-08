import { Component, OnInit } from '@angular/core';
import { AuthService } from '../Services/Auth/Auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { response } from 'express';

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

  constructor(
    private auth: AuthService,
    private router: Router,
    private toastr: ToastrService,
  ) {}

  ngOnInit() {}

  entrar() {
    console.log('📤 informações enviadas');

    this.auth.login(this.login, this.password).subscribe({
      next: (response) => {
        this.toastr.success('Login realizado com sucesso', 'Sucesso');

        const token = response.token;
        localStorage.setItem('token', token);

        const decodedToken = this.decodeToken(token);

        const role = decodedToken.role;

        localStorage.setItem('role', role);

        if (role === 'Admin') {
          console.log('Redirecionando para /dashboard');
          this.router.navigate(['/dashboard']);
        } else if (role === 'Academia') {
          console.log('Redirecionando para /alunos');
          this.router.navigate(['/alunos']);
        } else {
          console.log('Role não encontrado, redirecionando para /login');
          this.router.navigate(['/login']);
        }
      },
      error: () => {
        this.toastr.error('Usuário ou senha inválidos', 'Erro');
      },
    });
  }

  decodeToken(token: string): any {
    const payload = token.split('.')[1]; // Pega o payload do token
    const decodedPayload = atob(payload); // Decodifica o payload
    return JSON.parse(decodedPayload); // Retorna o payload decodificado
  }
}
