import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  TaskItem,
  CreateTaskRequest,
  UpdateTaskRequest,
  PagedResult,
  TaskQueryParams
} from '../models/models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/tasks`;

  getTasks(params: TaskQueryParams): Observable<PagedResult<TaskItem>> {
    let httpParams = new HttpParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== '') {
        httpParams = httpParams.set(k, String(v));
      }
    });
    return this.http.get<PagedResult<TaskItem>>(this.base, { params: httpParams });
  }

  getById(id: number): Observable<TaskItem> {
    return this.http.get<TaskItem>(`${this.base}/${id}`);
  }

  create(dto: CreateTaskRequest): Observable<TaskItem> {
    return this.http.post<TaskItem>(this.base, dto);
  }

  update(id: number, dto: UpdateTaskRequest): Observable<TaskItem> {
    return this.http.put<TaskItem>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  toggleComplete(task: TaskItem): Observable<TaskItem> {
    return this.update(task.id, {
      title: task.title,
      description: task.description,
      dueDate: task.dueDate,
      priority: task.priority,
      categoryId: task.categoryId,
      isCompleted: !task.isCompleted
    });
  }
}
