import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router, RouterModule, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-Layout',
  templateUrl: './Layout.component.html',
  styleUrls: ['./Layout.component.css'],
  imports: [CommonModule, RouterOutlet, RouterModule],
})

export class LayoutComponent implements OnInit {
  sidebarCollapsed = false;

  constructor(private router: Router) {}

  ngOnInit() {}

  toggleSidebar() {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }

  onLogout(): void {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }

  isAdmin(): boolean {
    const role = localStorage.getItem('role');
    return role === 'Admin';
  }

}
