import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { RouterModule, RouterOutlet } from "@angular/router";
import { ThemeService } from '../Services/Theme/theme.service';
import { NgxSpinnerModule } from 'ngx-spinner';
import { AuthService } from '../Services/Auth/Auth.service';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterModule, NgxSpinnerModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})

export class App implements OnInit {
  constructor(private themeService: ThemeService, private auth: AuthService) {}

  ngOnInit(): void {
    this.themeService.initializeTheme();
    this.auth.initializeIdleTimeout();
  }

  protected readonly title = signal('Marcial ProX'); 
 
}
