import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TaskService } from '../../../core/services/task.service';
import { CategoryService } from '../../../core/services/category.service';
import { TaskItem, Category, PagedResult, TaskPriority } from '../../../core/models/models';
import { TaskFormComponent } from '../task-form/task-form.component';
import { debounceTime, Subject } from 'rxjs';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, FormsModule, TaskFormComponent],
  template: `
    <div class="space-y-4">
      <div class="flex items-center justify-between">
        <h1 class="text-2xl font-bold">My Tasks</h1>
        <button class="btn-primary" (click)="openForm()">+ New task</button>
      </div>

      <!-- Filters & sorting -->
      <div class="card p-4 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-3">
        <input class="input md:col-span-2 lg:col-span-1" placeholder="Search by title or description..."
               [(ngModel)]="search" (ngModelChange)="onSearchChange()" />
        <select class="input" [(ngModel)]="categoryId" (ngModelChange)="reload()">
          <option [ngValue]="null">All categories</option>
          @for (c of categories(); track c.id) {
            <option [ngValue]="c.id">{{ c.name }}</option>
          }
        </select>
        <select class="input" [(ngModel)]="status" (ngModelChange)="reload()">
          <option [ngValue]="null">All statuses</option>
          <option [ngValue]="false">Active</option>
          <option [ngValue]="true">Completed</option>
        </select>
        <div class="flex gap-2">
          <select class="input flex-1" [(ngModel)]="sortBy" (ngModelChange)="reload()">
            <option value="createdAt">Date created</option>
            <option value="dueDate">Due date</option>
            <option value="priority">Priority</option>
            <option value="title">Title</option>
          </select>
          <button class="btn-secondary px-3" (click)="toggleSortDir()" title="Toggle sort direction">
            {{ sortDir === 'asc' ? '↑' : '↓' }}
          </button>
        </div>
      </div>

      @if (loading()) {
        <div class="text-center py-8 text-slate-500">Loading...</div>
      } @else if (result() !== null) {
        @if (result(); as r) {
          @if (r.items.length === 0) {
            <div class="card p-8 text-center text-slate-500">No tasks found.</div>
          } @else {
            <div class="space-y-2">
              @for (t of r.items; track t.id) {
                <div class="card p-4 flex items-start gap-3">
                  <input type="checkbox" [checked]="t.isCompleted" (change)="toggle(t)"
                         class="mt-1 h-5 w-5 cursor-pointer" />
                  <div class="flex-1 min-w-0">
                    <div class="flex items-center gap-2 flex-wrap">
                      <h3 class="font-medium"
                          [class.line-through]="t.isCompleted"
                          [class.text-slate-400]="t.isCompleted">{{ t.title }}</h3>
                      @if (t.categoryName) {
                        <span class="text-xs px-2 py-0.5 rounded-full text-white"
                              [style.background]="t.categoryColor">{{ t.categoryName }}</span>
                      }
                      <span class="text-xs px-2 py-0.5 rounded-full"
                            [ngClass]="priorityClass(t.priority)">{{ priorityLabel(t.priority) }}</span>
                    </div>
                    @if (t.description) {
                      <p class="text-sm text-slate-600 mt-1 whitespace-pre-line">{{ t.description }}</p>
                    }
                    @if (t.dueDate) {
                      <p class="text-xs text-slate-500 mt-1">Due: {{ t.dueDate | date:'medium' }}</p>
                    }
                  </div>
                  <div class="flex gap-2">
                    <button class="btn-secondary text-xs" (click)="openForm(t)">Edit</button>
                    <button class="btn-danger text-xs" (click)="remove(t)">Delete</button>
                  </div>
                </div>
              }
            </div>

            <div class="flex items-center justify-between mt-4">
              <span class="text-sm text-slate-600">
                Page {{ r.page }} of {{ r.totalPages }} ({{ r.totalItems }} total)
              </span>
              <div class="flex gap-2">
                <button class="btn-secondary" [disabled]="!r.hasPrevious" (click)="goTo(r.page - 1)">← Prev</button>
                <button class="btn-secondary" [disabled]="!r.hasNext" (click)="goTo(r.page + 1)">Next →</button>
              </div>
            </div>
          }
        }
      }

      @if (formOpen()) {
        <app-task-form [task]="editingTask()" [categories]="categories()"
                       (saved)="onSaved()" (closed)="formOpen.set(false)" />
      }
    </div>
  `
})
export class TaskListComponent implements OnInit {
  private tasks = inject(TaskService);
  private cats = inject(CategoryService);

  loading = signal(false);
  result = signal<PagedResult<TaskItem> | null>(null);
  categories = signal<Category[]>([]);
  formOpen = signal(false);
  editingTask = signal<TaskItem | null>(null);

  page = 1;
  pageSize = 10;
  search = '';
  categoryId: number | null = null;
  status: boolean | null = null;
  sortBy = 'createdAt';
  sortDir: 'asc' | 'desc' = 'desc';

  private searchSub = new Subject<void>();

  ngOnInit(): void {
    this.cats.getAll().subscribe(c => this.categories.set(c));
    this.searchSub.pipe(debounceTime(300)).subscribe(() => { this.page = 1; this.reload(); });
    this.reload();
  }

  onSearchChange(): void { this.searchSub.next(); }

  toggleSortDir(): void {
    this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.tasks.getTasks({
      page: this.page,
      pageSize: this.pageSize,
      search: this.search || undefined,
      categoryId: this.categoryId,
      isCompleted: this.status,
      sortBy: this.sortBy,
      sortDir: this.sortDir
    }).subscribe({
      next: r => { this.result.set(r); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  goTo(p: number): void { this.page = p; this.reload(); }
  toggle(t: TaskItem): void { this.tasks.toggleComplete(t).subscribe(() => this.reload()); }
  remove(t: TaskItem): void {
    if (!confirm(`Delete "${t.title}"?`)) return;
    this.tasks.delete(t.id).subscribe(() => this.reload());
  }
  openForm(t?: TaskItem): void { this.editingTask.set(t ?? null); this.formOpen.set(true); }
  onSaved(): void { this.formOpen.set(false); this.reload(); }

  priorityLabel(p: TaskPriority): string { return ['Low', 'Medium', 'High'][p]; }
  priorityClass(p: TaskPriority): string {
    return ['bg-slate-100 text-slate-700', 'bg-yellow-100 text-yellow-800', 'bg-red-100 text-red-800'][p];
  }
}
