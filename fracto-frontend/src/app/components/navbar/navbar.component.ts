
import { Component } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent {
  constructor(public auth: AuthService, private router: Router) {}

  //  this getter is what your template should bind to
  getUsername(): string {
    return this.auth.getUsername() || 'Account';
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
