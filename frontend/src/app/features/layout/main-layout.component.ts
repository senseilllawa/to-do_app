import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="min-h-screen flex flex-col">
      <header class="bg-white border-b border-slate-200">
        <div class="max-w-6xl mx-auto px-4 py-3 flex items-center justify-between">
          <div class="flex items-center gap-6">
            <span class="text-xl font-bold text-primary-600">TodoApp</span>
            <nav class="flex gap-1">
              <a routerLink="/tasks" routerLinkActive="bg-slate-100"
                 class="px-3 py-1.5 rounded-md text-sm font-medium hover:bg-slate-100">Tasks</a>
              <a routerLink="/categories" routerLinkActive="bg-slate-100"
                 class="px-3 py-1.5 rounded-md text-sm font-medium hover:bg-slate-100">Categories</a>
            </nav>
          </div>
          <div class="flex items-center gap-3">
            <span class="text-sm text-slate-600">{{ auth.user()?.userName }}</span>
            <button (click)="auth.logout()" class="btn-secondary text-sm">Logout</button>
          </div>
        </div>
      </header>
      <main class="flex-1 max-w-6xl mx-auto w-full px-4 py-6">
        <router-outlet />
      </main>
    </div>
  `
})
export class MainLayoutComponent {
  auth = inject(AuthService);
}
