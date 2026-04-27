import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgxSpinnerService } from 'ngx-spinner';
import { DashboardService } from '../Services/Dashboard/Dashboard.service';
import Chart from 'chart.js/auto';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-Dashboard',
  templateUrl: './Dashboard.component.html',
  styleUrls: ['./Dashboard.component.css'],
  imports: [CommonModule, FormsModule],
})
export class DashboardComponent implements OnInit {
  dashboard: any;
  isAdmin = false;

  constructor(
    private spinner: NgxSpinnerService,
    private cd: ChangeDetectorRef,
    private toastr: ToastrService,
    private dashService: DashboardService,
  ) {}

  ngOnInit() {
    if (typeof window !== 'undefined') {
      const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
      this.isAdmin = usuario.role === 'Admin';
    }

    this.loadDashboard();
  }

  loadDashboard() {
    this.spinner.show();
    this.dashService.getDashboard().subscribe({
      next: (res) => {
        this.spinner.hide();
        this.dashboard = res;

        setTimeout(() => {
          this.renderCharts();
        }, 100);

        this.cd.markForCheck();
      },
      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao carregar o dashboard', 'Erro');
      },
    });
  }

  renderCharts() {
    const canvasAlunos = document.getElementById('graficoAlunos') as HTMLCanvasElement | null;
    const canvasPlanos = document.getElementById('graficoPlanos') as HTMLCanvasElement | null;

    if (!canvasAlunos || !canvasPlanos) {
      return;
    }

    Chart.getChart(canvasAlunos)?.destroy();
    Chart.getChart(canvasPlanos)?.destroy();

    const textColor = this.getCssVar('--text-secondary');
    const gridColor = 'rgba(148, 163, 184, 0.14)';
    const accentPrimary = this.getCssVar('--accent-primary');
    const accentStrong = this.getCssVar('--accent-primary-strong');
    const accentSecondary = this.getCssVar('--accent-secondary');

    new Chart(canvasAlunos, {
      type: 'line',
      data: {
        labels: ['Jan', 'Fev', 'Mar', 'Abr'],
        datasets: [
          {
            label: 'Alunos',
            data: this.dashboard?.alunosPorMes || [10, 20, 30, 40],
            fill: true,
            tension: 0.35,
            borderColor: accentPrimary,
            backgroundColor: 'rgba(37, 99, 235, 0.16)',
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
            labels: {
              color: textColor,
            },
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

    new Chart(canvasPlanos, {
      type: 'doughnut',
      data: {
        labels: this.dashboard?.planos?.map((p: any) => p.nome) || [],
        datasets: [
          {
            data: this.dashboard?.planos?.map((p: any) => p.totalAlunos) || [],
            backgroundColor: [accentPrimary, accentSecondary, '#f59e0b', '#22c55e', '#ef4444'],
            borderWidth: 0,
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
              color: textColor,
              padding: 18,
            },
          },
        },
      },
    });
  }

  onPeriodoChange(_: Event) {}

  private getCssVar(name: string) {
    return getComputedStyle(document.body).getPropertyValue(name).trim() || '#94a3b8';
  }
}
