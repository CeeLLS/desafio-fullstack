import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Task, CreateTaskPayload, UpdateTaskPayload } from '../domain/task.model';

const API_BASE = 'http://localhost:5000/api';

@Injectable({ providedIn: 'root' })
export class TaskRepository {
  private readonly http = inject(HttpClient);

  getAll(): Observable<Task[]> {
    return this.http.get<Task[]>(`${API_BASE}/tasks`);
  }

  getById(id: string): Observable<Task> {
    return this.http.get<Task>(`${API_BASE}/tasks/${id}`);
  }

  create(payload: CreateTaskPayload): Observable<Task> {
    return this.http.post<Task>(`${API_BASE}/tasks`, payload);
  }

  update(id: string, payload: UpdateTaskPayload): Observable<void> {
    return this.http.put<void>(`${API_BASE}/tasks/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${API_BASE}/tasks/${id}`);
  }

  restore(id: string): Observable<void> {
    return this.http.patch<void>(`${API_BASE}/tasks/${id}/restore`, {});
  }
}