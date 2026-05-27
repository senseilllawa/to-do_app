import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center px-4">
      <div class="card w-full max-w-md p-8">
        <h1 class="text-2xl font-bold mb-6 text-center">Sign in to TodoApp</h1>
        <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-4">
          <div>
            <label class="label">Email</label>
            <input type="email" formControlName="email" class="input"
                   [class.border-red-500]="f.email.invalid && f.email.touched"
                   autocomplete="email" />
            @if (f.email.touched && f.email.errors?.['required']) {
              <p class="text-xs text-red-600 mt-1">Email is required.</p>
            } @else if (f.email.touched && f.email.errors?.['email']) {
              <p class="text-xs text-red-600 mt-1">Enter a valid email address.</p>
            }
          </div>
          <div>
            <label class="label">Password</label>
            <input type="password" formControlName="password" class="input"
                   [class.border-red-500]="f.password.invalid && f.password.touched"
                   autocomplete="current-password" />
            @if (f.password.touched && f.password.errors?.['required']) {
              <p class="text-xs text-red-600 mt-1">Password is required.</p>
            } @else if (f.password.touched && f.password.errors?.['minlength']) {
              <p class="text-xs text-red-600 mt-1">Password must be at least 6 characters.</p>
            }
          </div>
          @if (error()) { <p class="text-sm text-red-600">{{ error() }}</p> }
          <button type="submit" [disabled]="form.invalid || loading()" class="btn-primary w-full">
            {{ loading() ? 'Signing in...' : 'Sign in' }}
          </button>
        </form>
        <p class="text-sm text-center mt-4 text-slate-600">
          No account? <a routerLink="/register" class="text-primary-600 hover:underline">Register</a>
        </p>
        <p class="text-xs text-center mt-3 text-slate-400">
          Demo: <code>demo&#64;todo.app</code> / <code>demo1234</code>
        </p>
      </div>
    </div>
  `
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  loading = signal(false);
  error = signal<string | null>(null);

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  get f() { return this.form.controls; }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.loading.set(true);
    this.error.set(null);
    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => this.router.navigate(['/tasks']),
      error: (err) => { this.error.set(err.error?.message ?? 'Login failed'); this.loading.set(false); },
      complete: () => this.loading.set(false)
    });
  }
}
