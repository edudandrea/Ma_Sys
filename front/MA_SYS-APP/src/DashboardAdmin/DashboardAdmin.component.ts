import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { DashboardService} from '../Services/Dashboard/Dashboard.service';
import Chart from 'chart.js/auto';

@Component({
  selector: 'app-DashboardAdmin',
  templateUrl: './DashboardAdmin.component.html',
  styleUrls: ['./DashboardAdmin.component.css'],
  imports: [CommonModule, FormsModule],
})
export class DashboardAdminComponent implements OnInit {
  dashboard: any;

  constructor(private spinner: NgxSpinnerService,
              private cd: ChangeDetectorRef,
              private toastr: ToastrService,
              private dashService: DashboardService) {}

  ngOnInit() {
    this.loadDashboard();
  }

  ngAfterViewInit() {

  new Chart("graficoAlunos", {
    type: 'line',
    data: {
      labels: ['Jan', 'Fev', 'Mar'],
      datasets: [{
        label: 'Alunos',
        data: [10, 20, 35]
      }]
    }
  });

  new Chart("graficoPlanos", {
    type: 'doughnut',
    data: {
      labels: ['Mensal', 'Anual'],
      datasets: [{
        data: [60, 40]
      }]
    }
  });
}

  loadDashboard() {
    this.spinner.show();
    this.dashService.getDashboard().subscribe({
      next: (res) => {
        console.log('Dashboard recebido:', res);
        this.spinner.hide();
        this.dashboard = res;

          this.cd.markForCheck();
      },

      error: (err) => {
        console.error(err);
        this.toastr.error('Erro ao carregar Dashboard', 'Erro');
      },
    });
  }
}
