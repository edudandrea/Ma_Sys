import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Component, HostListener, inject, OnInit, PLATFORM_ID } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule, RouterOutlet } from '@angular/router';
import { ThemeOption, ThemeService } from '../Services/Theme/theme.service';

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
  academiaNome = '';
  financeiroOpen = false;
  cadastroOpen = false;
  currentTheme = 'system';
  readonly themeOptions: ThemeOption[];

  private readonly platformId = inject(PLATFORM_ID);

  constructor(
    private router: Router,
    private themeService: ThemeService,
  ) {
    this.themeOptions = this.themeService.themes;
  }

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.getUserInfo();
      this.currentTheme = this.themeService.getCurrentTheme();
      this.syncViewportState(window.innerWidth, true);
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

  toggleFinanceiro() {
    this.financeiroOpen = !this.financeiroOpen;
  }

  toggleCadastro() {
    this.cadastroOpen = !this.cadastroOpen;
  }

  toggleSidebar() {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }

  isAdmin(): boolean {
    const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
    return usuario.role === 'Admin';
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
    this.userName = usuario.userName || 'Usuário';
    this.academiaNome = usuario.academiaNome || 'Marcial ProX';
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
}
