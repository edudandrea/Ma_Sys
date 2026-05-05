import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Component, HostListener, inject, OnInit, PLATFORM_ID, TemplateRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule, RouterOutlet } from '@angular/router';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { AcademiasService } from '../Services/AcademiaService/Academias.service';
import { UserService } from '../Services/UsuarioService/User.service';
import { ThemeOption, ThemeService } from '../Services/Theme/theme.service';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../app/environments/environment';

@Component({
  selector: 'app-Layout',
  templateUrl: './Layout.component.html',
  styleUrls: ['./Layout.component.css'],
  imports: [CommonModule, FormsModule, RouterOutlet, RouterModule],
})
export class LayoutComponent implements OnInit {
  sidebarCollapsed = false;
  isCompactViewport = false;
  userName = '';
  userEmail = '';
  academiaNome = '';
  academiaLogoUrl = '';
  financeiroOpen = false;
  cadastroOpen = false;
  currentTheme = 'system';
  readonly themeOptions: ThemeOption[];
  currentRole = '';
  profileMenuOpen = false;
  modalRef?: BsModalRef;
  currentAcademiaId: number | null = null;
  currentFederacaoId: number | null = null;
  currentUserId = 0;

  perfilForm = {
    userName: '',
    email: '',
  };

  senhaForm = {
    novaSenha: '',
    confirmarNovaSenha: '',
  };

  private readonly platformId = inject(PLATFORM_ID);

  constructor(
    private router: Router,
    private themeService: ThemeService,
    private modalService: BsModalService,
    private toastr: ToastrService,
    private userService: UserService,
    private academiasService: AcademiasService,
  ) {
    this.themeOptions = this.themeService.themes;
  }

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.getUserInfo();
      this.currentTheme = this.themeService.getCurrentTheme();
      this.syncViewportState(window.innerWidth, true);
      this.carregarDadosAcademia();
    }
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: Event) {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    const target = event.target as Window;
    this.syncViewportState(target.innerWidth);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event) {
    const target = event.target as HTMLElement;
    if (!target.closest('.profile-dropdown')) {
      this.profileMenuOpen = false;
    }
  }

  toggleFinanceiro() {
    this.financeiroOpen = !this.financeiroOpen;
  }

  toggleCadastro() {
    this.cadastroOpen = !this.cadastroOpen;
  }

  toggleSidebar() {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }

  toggleProfileMenu(event?: Event) {
    event?.stopPropagation();
    this.profileMenuOpen = !this.profileMenuOpen;
  }

  isAdmin(): boolean {
    return this.currentRole === 'Admin';
  }

  isSuperAdmin(): boolean {
    return this.currentRole === 'SuperAdmin';
  }

  isAcademia(): boolean {
    return this.currentRole === 'Academia';
  }

  isFederacao(): boolean {
    return this.currentRole === 'Federacao';
  }

  canViewAcademias(): boolean {
    return this.isAdmin() || this.isSuperAdmin();
  }

  canViewFederacoes(): boolean {
    return this.isAdmin() || this.isSuperAdmin();
  }

  canViewFiliados(): boolean {
    return this.isAdmin() || this.isSuperAdmin() || this.isFederacao();
  }

  canViewUsuarios(): boolean {
    return this.isAdmin() || this.isSuperAdmin();
  }

  canViewCadastrosAcademia(): boolean {
    return this.isAcademia() || this.isFederacao() || this.isSuperAdmin();
  }

  canViewFinanceiroSistema(): boolean {
    return this.isAdmin() || this.isSuperAdmin();
  }

  canViewFinanceiroAcademia(): boolean {
    return this.isAcademia() || this.isFederacao() || this.isSuperAdmin();
  }

  openPerfilModal(template: TemplateRef<any>, event?: Event) {
    event?.stopPropagation();
    this.profileMenuOpen = false;
    this.perfilForm = {
      userName: this.userName,
      email: this.userEmail,
    };
    this.modalRef = this.modalService.show(template, {
      class: 'modal-md modal-dialog-centered',
    });
  }

  openSenhaModal(template: TemplateRef<any>, event?: Event) {
    event?.stopPropagation();
    this.profileMenuOpen = false;
    this.senhaForm = {
      novaSenha: '',
      confirmarNovaSenha: '',
    };
    this.modalRef = this.modalService.show(template, {
      class: 'modal-md modal-dialog-centered',
    });
  }

  salvarPerfil() {
    this.userService.atualizarUsuario({
      userId: this.currentUserId,
      userName: this.perfilForm.userName,
      email: this.perfilForm.email,
    }).subscribe({
      next: () => {
        this.userName = this.perfilForm.userName;
        this.userEmail = this.perfilForm.email;
        this.persistUsuario();
        this.modalRef?.hide();
        this.toastr.success('Perfil atualizado com sucesso.');
      },
      error: () => {
        this.toastr.error('Não foi possível atualizar o perfil.');
      },
    });
  }

  trocarSenha() {
    if (!this.senhaForm.novaSenha || this.senhaForm.novaSenha !== this.senhaForm.confirmarNovaSenha) {
      this.toastr.warning('As senhas informadas não conferem.');
      return;
    }

    this.userService.atualizarUsuario({
      userId: this.currentUserId,
      password: this.senhaForm.novaSenha,
    }).subscribe({
      next: () => {
        this.modalRef?.hide();
        this.toastr.success('Senha atualizada com sucesso.');
      },
      error: () => {
        this.toastr.error('Não foi possível alterar a senha.');
      },
    });
  }

  changeTheme(theme: string) {
    this.currentTheme = theme;
    this.themeService.applyTheme(theme);
  }

  logout() {
    localStorage.clear();
    this.router.navigate(['/login']);
  }

  getUserInfo() {
    const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
    this.currentUserId = usuario.id || 0;
    this.userName = usuario.userName || 'Usuario';
    this.userEmail = usuario.email || '';
    this.academiaNome = usuario.academiaNome || 'Marcial ProX';
    this.academiaLogoUrl = this.resolveLogoUrl(usuario.academiaLogoUrl);
    if (usuario.role === 'Federacao') {
      this.academiaNome = usuario.federacaoNome || this.academiaNome;
      this.academiaLogoUrl = this.resolveLogoUrl(usuario.federacaoLogoUrl) || this.academiaLogoUrl;
    }
    this.currentAcademiaId = usuario.academiaId || null;
    this.currentFederacaoId = usuario.federacaoId || null;
    this.currentRole = usuario.role || '';
  }

  private carregarDadosAcademia() {
    if (!this.currentAcademiaId) {
      return;
    }

    this.academiasService.getAcademiaById(this.currentAcademiaId).subscribe({
      next: (academia) => {
        this.academiaNome = academia.nome || this.academiaNome;
        this.academiaLogoUrl = this.resolveLogoUrl(academia.logoUrl);
        this.persistUsuario();
      },
    });
  }

  private persistUsuario() {
    const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
    localStorage.setItem('usuario', JSON.stringify({
      ...usuario,
      id: this.currentUserId,
      userName: this.userName,
      email: this.userEmail,
      academiaNome: this.academiaNome,
      academiaLogoUrl: this.academiaLogoUrl,
      academiaId: this.currentAcademiaId,
      federacaoId: this.currentFederacaoId,
      role: this.currentRole,
    }));
  }

  private syncViewportState(width: number, firstLoad = false) {
    const compact = width < 1024;
    this.isCompactViewport = compact;

    if (compact) {
      this.sidebarCollapsed = true;
      return;
    }

    if (firstLoad) {
      this.sidebarCollapsed = false;
    }
  }

  private resolveLogoUrl(logoUrl?: string | null): string {
    if (!logoUrl) {
      return '';
    }

    if (logoUrl.startsWith('http://') || logoUrl.startsWith('https://')) {
      return logoUrl;
    }

    if (logoUrl.startsWith('/api/')) {
      return logoUrl;
    }

    if (logoUrl.startsWith('/uploads/academias/')) {
      const fileName = logoUrl.split('/').pop();
      return fileName ? `${environment.apiUrl}/Academias/logo/${fileName}` : logoUrl;
    }

    return logoUrl;
  }
}
