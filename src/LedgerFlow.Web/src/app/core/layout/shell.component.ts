import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthStore } from '../auth/auth.store';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShellComponent {
  readonly auth = inject(AuthStore);
  private readonly router = inject(Router);

  readonly navOpen = signal(false);
  readonly navItems = [
    { path: '/dashboard', label: 'Dashboard', icon: '⌂' },
    { path: '/transactions', label: 'Transactions', icon: '⇄' },
    { path: '/accounts', label: 'Accounts', icon: '▣' },
    { path: '/categories', label: 'Categories', icon: '◈' },
    { path: '/reports', label: 'Reports', icon: '⌁' },
  ];

  firstName = () => this.auth.user()?.name.split(' ')[0] ?? 'there';
  initials = () =>
    this.auth.user()?.name.split(' ').slice(0, 2).map((part) => part[0]).join('').toUpperCase() ?? 'LF';

  signOut(): void {
    this.auth.logout();
    void this.router.navigate(['/login']);
  }
}
