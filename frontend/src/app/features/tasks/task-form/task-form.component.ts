import { Component, EventEmitter, Input, Output, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TaskService } from '../../../core/services/task.service';
import { Category, TaskItem, TaskPriority } from '../../../core/models/models';

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4" (click)="closed.emit()">
      <div class="card w-full max-w-lg p-6" (click)="$event.stopPropagation()">
        <h2 class="text-xl font-bold mb-4">{{ task ? 'Edit task' : 'New task' }}</h2>
        <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-3">
          <div>
            <label class="label">Title *</label>
            <input class="input" formControlName="title"
                   [class.border-red-500]="f.title.invalid && f.title.touched" />
            @if (f.title.touched && f.title.errors?.['required']) {
              <p class="text-xs text-red-600 mt-1">Title is required.</p>
            } @else if (f.title.touched && f.title.errors?.['maxlength']) {
              <p class="text-xs text-red-600 mt-1">Title must be at most 200 characters.</p>
            }
          </div>
          <div>
            <label class="label">Description</label>
            <textarea class="input" rows="3" formControlName="description"
                      [class.border-red-500]="f.description.invalid && f.description.touched"></textarea>
            @if (f.description.touched && f.description.errors?.['maxlength']) {
              <p class="text-xs text-red-600 mt-1">Description must be at most 2000 characters.</p>
            }
          </div>
          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="label">Due date</label>
              <input type="datetime-local" class="input" formControlName="dueDate" />
            </div>
            <div>
              <label class="label">Priority</label>
              <select class="input" formControlName="priority">
                <option [ngValue]="0">Low</option>
                <option [ngValue]="1">Medium</option>
                <option [ngValue]="2">High</option>
              </select>
            </div>
          </div>
          <div>
            <label class="label">Category</label>
            <select class="input" formControlName="categoryId">
              <option [ngValue]="null">— None —</option>
              @for (c of categories; track c.id) {
                <option [ngValue]="c.id">{{ c.name }}</option>
              }
            </select>
          </div>
          @if (task) {
            <label class="flex items-center gap-2 text-sm">
              <input type="checkbox" formControlName="isCompleted" /> Completed
            </label>
          }
          @if (error()) { <p class="text-sm text-red-600">{{ error() }}</p> }
          <div class="flex justify-end gap-2 pt-2">
            <button type="button" class="btn-secondary" (click)="closed.emit()">Cancel</button>
            <button type="submit" class="btn-primary" [disabled]="form.invalid || saving()">
              {{ saving() ? 'Saving...' : 'Save' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  `
})
export class TaskFormComponent implements OnInit {
  @Input() task: TaskItem | null = null;
  @Input() categories: Category[] = [];
  @Output() saved = new EventEmitter<void>();
  @Output() closed = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private tasks = inject(TaskService);

  saving = signal(false);
  error = signal<string | null>(null);

  form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', Validators.maxLength(2000)],
    dueDate: [''],
    priority: [TaskPriority.Medium as number],
    categoryId: [null as number | null],
    isCompleted: [false]
  });

  get f() { return this.form.controls; }

  ngOnInit(): void {
    if (this.task) {
      this.form.patchValue({
        title: this.task.title,
        description: this.task.description ?? '',
        dueDate: this.task.dueDate ? this.task.dueDate.substring(0, 16) : '',
        priority: this.task.priority,
        categoryId: this.task.categoryId ?? null,
        isCompleted: this.task.isCompleted
      });
    }
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.saving.set(true);
    this.error.set(null);
    const v = this.form.getRawValue();
    const dto = {
      title: v.title,
      description: v.description || null,
      dueDate: v.dueDate ? new Date(v.dueDate).toISOString() : null,
      priority: v.priority,
      categoryId: v.categoryId,
      isCompleted: v.isCompleted
    };

    const op$ = this.task
      ? this.tasks.update(this.task.id, dto)
      : this.tasks.create(dto);

    op$.subscribe({
      next: () => { this.saving.set(false); this.saved.emit(); },
      error: (err) => { this.error.set(err.error?.message ?? 'Save failed'); this.saving.set(false); }
    });
  }
}
