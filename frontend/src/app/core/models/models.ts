export enum TaskPriority {
  Low = 0,
  Medium = 1,
  High = 2
}

export interface User {
  id: number;
  email: string;
  userName: string;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: User;
}

export interface Category {
  id: number;
  name: string;
  color: string;
  tasksCount: number;
}

export interface TaskItem {
  id: number;
  title: string;
  description?: string | null;
  isCompleted: boolean;
  dueDate?: string | null;
  priority: TaskPriority;
  createdAt: string;
  updatedAt: string;
  categoryId?: number | null;
  categoryName?: string | null;
  categoryColor?: string | null;
}

export interface CreateTaskRequest {
  title: string;
  description?: string | null;
  dueDate?: string | null;
  priority: TaskPriority;
  categoryId?: number | null;
}

export interface UpdateTaskRequest extends CreateTaskRequest {
  isCompleted: boolean;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface TaskQueryParams {
  page?: number;
  pageSize?: number;
  search?: string;
  categoryId?: number | null;
  isCompleted?: boolean | null;
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
}
