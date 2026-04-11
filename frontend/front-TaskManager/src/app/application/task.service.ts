import { Injectable, inject, signal, computed } from '@angular/core';
import { Task, CreateTaskPayload, UpdateTaskPayload, TaskStatus } from '../domain/task.model';
import { TaskRepository } from '../data/task.repository';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly repository = inject(TaskRepository);

  private readonly _allTasks = signal<Task[]>([]);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);

  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();

  readonly activeTasks = computed(() => this._allTasks().filter(t => !t.isDeleted));
  readonly deletedTasks = computed(() => this._allTasks().filter(t => t.isDeleted));
  readonly tasks = this.activeTasks; 

  loadAll(): void {
    this._loading.set(true);
    this._error.set(null);

    this.repository.getAll().subscribe({
      next: tasks => {
        this._allTasks.set(tasks);
        this._loading.set(false);
      },
      error: err => {
        this._error.set('Erro ao carregar tarefas. Verifique se o servidor está rodando.');
        this._loading.set(false);
        console.error(err);
      },
    });
  }

  create(payload: CreateTaskPayload, onSuccess?: () => void): void {
    this._loading.set(true);
    this._error.set(null);

    this.repository.create(payload).subscribe({
      next: task => {
        this._allTasks.update(all => [task, ...all]);
        this._loading.set(false);
        onSuccess?.();
      },
      error: err => {
        this._error.set('Erro ao criar tarefa.');
        this._loading.set(false);
        console.error(err);
      },
    });
  }

  update(id: string, payload: UpdateTaskPayload, onSuccess?: () => void): void {
    this._loading.set(true);
    this._error.set(null);

    this.repository.update(id, payload).subscribe({
      next: () => {
        this._allTasks.update(all =>
          all.map(t =>
            t.id === id
              ? { ...t, title: payload.title, description: payload.description, status: payload.status }
              : t
          )
        );
        this._loading.set(false);
        onSuccess?.();
      },
      error: err => {
        this._error.set('Erro ao atualizar tarefa.');
        this._loading.set(false);
        console.error(err);
      },
    });
  }

  delete(id: string): void {
    this._loading.set(true);
    this._error.set(null);

    this.repository.delete(id).subscribe({
      next: () => {
        const now = new Date().toISOString();
        this._allTasks.update(all =>
          all.map(t => t.id === id ? { ...t, isDeleted: true, deletedAtUtc: now } : t)
        );
        this._loading.set(false);
      },
      error: err => {
        this._error.set('Erro ao remover tarefa.');
        this._loading.set(false);
        console.error(err);
      },
    });
  }

  restore(id: string): void {
    this._loading.set(true);
    this._error.set(null);

    this.repository.restore(id).subscribe({
      next: () => {
        this._allTasks.update(all =>
          all.map(t => t.id === id ? { ...t, isDeleted: false, deletedAtUtc: null } : t)
        );
        this._loading.set(false);
      },
      error: err => {
        this._error.set('Erro ao restaurar tarefa.');
        this._loading.set(false);
        console.error(err);
      },
    });
  }
}