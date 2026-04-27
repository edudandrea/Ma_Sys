import { Injectable } from '@angular/core';

export type ThemeOption = {
  value: string;
  label: string;
};

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  private readonly storageKey = 'ma_sys_theme';
  private readonly defaultTheme = 'system';

  readonly themes: ThemeOption[] = [
    { value: 'system', label: 'Sistema' },
    { value: 'blue-light', label: 'Branco + Azul Escuro' },
    { value: 'green-gold', label: 'Verde + Dourado' },
    { value: 'red-black', label: 'Vermelho + Preto' },
    { value: 'windows', label: 'Padrao Windows' },
  ];

  initializeTheme() {
    if (typeof document === 'undefined') {
      return;
    }

    const theme = localStorage.getItem(this.storageKey) || this.defaultTheme;
    this.applyTheme(theme);
  }

  applyTheme(theme: string) {
    if (typeof document === 'undefined') {
      return;
    }

    document.body.setAttribute('data-theme', theme);
    localStorage.setItem(this.storageKey, theme);
  }

  getCurrentTheme() {
    if (typeof document === 'undefined') {
      return this.defaultTheme;
    }

    return document.body.getAttribute('data-theme') || localStorage.getItem(this.storageKey) || this.defaultTheme;
  }
}
