import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CategoryService } from '../../core/services/category.service';
import { Category } from '../../core/models/models';

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="space-y-4">
      <h1 class="text-2xl font-bold">Categories</h1>

      <div class="card p-4">
        <form [formGroup]="form" (ngSubmit)="submit()" class="flex gap-3 items-start">
          <div class="flex-1">
            <label class="label">Name *</label>
            <input class="input" formControlName="name" placeholder="e.g. Work"
                   [class.border-red-500]="f.name.invalid && f.name.touched" />
            @if (f.name.touched && f.name.errors?.['required']) {
              <p class="text-xs text-red-600 mt-1">Name is required.</p>
            } @else if (f.name.touched && f.name.errors?.['maxlength']) {
              <p class="text-xs text-red-600 mt-1">Name must be at most 50 characters.</p>
            }
          </div>
          <div>
            <label class="label">Color</label>
            <input type="color" formControlName="color" class="h-10 w-16 rounded border border-slate-300" />
          </div>
          <div class="pt-6 flex gap-2">
            <button type="submit" class="btn-primary" [disabled]="form.invalid">
              {{ editingId() ? 'Update' : 'Add' }}
            </button>
            @if (editingId()) {
              <button type="button" class="btn-secondary" (click)="cancelEdit()">Cancel</button>
            }
          </div>
        </form>
        @if (error()) { <p class="text-sm text-red-600 mt-2">{{ error() }}</p> }
      </div>

      <div class="card divide-y divide-slate-200">
        @for (c of items(); track c.id) {
          <div class="p-4 flex items-center gap-3">
            <span class="h-6 w-6 rounded-full border border-slate-200 flex-shrink-0"
                  [style.background]="c.color"></span>
            <div class="flex-1">
              <p class="font-medium">{{ c.name }}</p>
              <p class="text-xs text-slate-500">{{ c.tasksCount }} task(s)</p>
            </div>
            <button class="btn-secondary text-xs" (click)="edit(c)">Edit</button>
            <button class="btn-danger text-xs" (click)="remove(c)">Delete</button>
          </div>
        } @empty {
          <div class="p-8 text-center text-slate-500">No categories yet.</div>
        }
      </div>
    </div>
  `
})
export class CategoryListComponent implements OnInit {
  private fb = inject(FormBuilder);
  private cats = inject(CategoryService);

  items = signal<Category[]>([]);
  editingId = signal<number | null>(null);
  error = signal<string | null>(null);

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
    color: ['#6366f1', Validators.required]
  });

  get f() { return this.form.controls; }

  ngOnInit(): void { this.load(); }

  load(): void { this.cats.getAll().subscribe(c => this.items.set(c)); }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.error.set(null);
    const dto = this.form.getRawValue();
    const id = this.editingId();
    const op$ = id ? this.cats.update(id, dto) : this.cats.create(dto);
    op$.subscribe({
      next: () => {
        this.form.reset({ name: '', color: '#6366f1' });
        this.editingId.set(null);
        this.load();
      },
      error: (err) => this.error.set(err.error?.message ?? 'Save failed')
    });
  }

  edit(c: Category): void {
    this.editingId.set(c.id);
    this.form.setValue({ name: c.name, color: c.color });
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.form.reset({ name: '', color: '#6366f1' });
  }

  remove(c: Category): void {
    if (!confirm(`Delete category "${c.name}"? Tasks will become uncategorized.`)) return;
    this.cats.delete(c.id).subscribe(() => this.load());
  }
}
