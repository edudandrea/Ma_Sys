import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import Chart from 'chart.js/auto';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { DashboardService } from '../Services/Dashboard/Dashboard.service';

@Component({
  selector: 'app-dashboard-federacao',
  standalone: true,
  templateUrl: './DashboardFederacao.component.html',
  styleUrls: ['../Dashboard/Dashboard.component.css'],
  imports: [CommonModule, FormsModule],
})
export class DashboardFederacaoComponent implements OnInit, OnDestroy {
  dashboard: any;
  private themeObserver?: MutationObserver;

  constructor(
    private spinner: NgxSpinnerService,
    private toastr: ToastrService,
    private dashboardService: DashboardService,
    private cd: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.loadDashboard();

    if (typeof document !== 'undefined') {
      this.themeObserver = new MutationObserver(() => {
        if (this.dashboard) {
          this.renderCharts();
        }
      });

      this.themeObserver.observe(document.body, {
        attributes: true,
        attributeFilter: ['data-theme'],
      });
    }
  }

  ngOnDestroy() {
    this.themeObserver?.disconnect();
  }

  loadDashboard() {
    this.spinner.show();
    this.dashboardService.getDashboardFederacao().subscribe({
      next: (res) => {
        this.spinner.hide();
        this.dashboard = res;
        setTimeout(() => this.renderCharts(), 100);
        this.cd.markForCheck();
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Erro ao carregar dashboard da federacao.');
      },
    });
  }

  onPeriodoChange(_: Event) {}

  get cobrancasPendentes(): any[] {
    return this.dashboard?.ultimasCobrancas?.filter((item: any) => item.status !== 'Pago') || [];
  }

  renderCharts() {
    const canvasFiliados = document.getElementById('graficoFiliadosFederacao') as HTMLCanvasElement | null;
    const canvasCobrancas = document.getElementById('graficoCobrancasFederacao') as HTMLCanvasElement | null;

    if (canvasFiliados) {
      Chart.getChart(canvasFiliados)?.destroy();
    }

    if (canvasCobrancas) {
      Chart.getChart(canvasCobrancas)?.destroy();
    }

    const textColor = this.getCssVar('--text-secondary');
    const legendColor = this.isDarkThemeActive() ? this.getCssVar('--text-primary') : '#1f2937';
    const gridColor = 'rgba(148, 163, 184, 0.14)';
    const accentPrimary = this.getCssVar('--accent-primary');
    const accentStrong = this.getCssVar('--accent-primary-strong');
    const accentSecondary = this.getCssVar('--accent-secondary');

    if (canvasFiliados) {
      new Chart(canvasFiliados, {
        type: 'line',
        data: {
          labels: this.dashboard?.meses?.length ? this.dashboard.meses : ['Jan', 'Fev', 'Mar', 'Abr'],
          datasets: [
            {
              label: 'Filiados',
              data: this.dashboard?.filiadosPorMes?.length ? this.dashboard.filiadosPorMes : [0, 0, 0, 0],
              fill: true,
              tension: 0.35,
              borderColor: accentPrimary,
              backgroundColor: 'rgba(20, 184, 166, 0.16)',
              pointBackgroundColor: accentStrong,
              pointBorderColor: '#ffffff',
              pointRadius: 4,
            },
          ],
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: {
              labels: { color: textColor },
            },
          },
          scales: {
            x: {
              ticks: { color: textColor },
              grid: { color: gridColor },
            },
            y: {
              ticks: { color: textColor },
              grid: { color: gridColor },
              beginAtZero: true,
            },
          },
        },
      });
    }

    if (canvasCobrancas) {
      const status = this.dashboard?.cobrancasPorStatus || [];
      new Chart(canvasCobrancas, {
        type: 'doughnut',
        data: {
          labels: status.map((item: any) => item.nome),
          datasets: [
            {
              data: status.map((item: any) => item.total),
              backgroundColor: [accentPrimary, '#f59e0b', '#ef4444', accentSecondary, '#60a5fa'],
              borderColor: this.getCssVar('--bg-panel'),
              borderWidth: 2,
            },
          ],
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: {
              position: 'bottom',
              labels: {
                color: legendColor,
                padding: 18,
              },
            },
          },
        },
      });
    }
  }

  private getCssVar(name: string) {
    return getComputedStyle(document.body).getPropertyValue(name).trim() || '#94a3b8';
  }

  private isDarkThemeActive() {
    if (typeof document === 'undefined') {
      return true;
    }

    const theme = document.body.getAttribute('data-theme') || 'system';
    return ['system', 'green-gold', 'red-black'].includes(theme);
  }
}
