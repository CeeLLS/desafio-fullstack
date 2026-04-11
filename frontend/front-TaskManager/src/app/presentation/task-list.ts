import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TaskService } from '../application/task.service';
import { TaskFormComponent } from './task-form';
import {
  Task,
  TaskStatus,
  TASK_STATUS_OPTIONS,
  CreateTaskPayload,
  UpdateTaskPayload,
} from '../domain/task.model';

const COLUMNS: { status: TaskStatus; label: string; color: string; bg: string }[] = [
  { status: 0, label: 'Pendente',     color: '#92400e', bg: '#fef3c7' },
  { status: 1, label: 'Em andamento', color: '#1e40af', bg: '#dbeafe' },
  { status: 2, label: 'Concluída',    color: '#14532d', bg: '#dcfce7' },
  { status: 3, label: 'Cancelada',    color: '#6b7280', bg: '#f3f4f6' },
];

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, FormsModule, TaskFormComponent],
  templateUrl: './task-list.html',
  styleUrls: ['./task-list.scss'],
})
export class TaskListComponent implements OnInit {
  readonly taskService = inject(TaskService);
  readonly columns = COLUMNS;
  readonly statusOptions = TASK_STATUS_OPTIONS;

  showForm = signal(false);
  showDeleted = signal(false);
  editingTask = signal<Task | null>(null);

  ngOnInit(): void {
    this.taskService.loadAll();
  }

  tasksForStatus(status: TaskStatus): Task[] {
    return this.taskService.activeTasks().filter(t => t.status === status);
  }

  onStatusChange(task: Task, newStatus: TaskStatus): void {
    this.taskService.update(task.id, {
      title: task.title,
      description: task.description,
      status: Number(newStatus) as TaskStatus,
    });
  }

  openCreate(): void {
    this.editingTask.set(null);
    this.showForm.set(true);
  }

  openEdit(task: Task): void {
    this.editingTask.set(task);
    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.editingTask.set(null);
  }

  onFormSubmit(payload: CreateTaskPayload | UpdateTaskPayload): void {
    const editing = this.editingTask();
    if (editing) {
      this.taskService.update(editing.id, payload as UpdateTaskPayload, () => this.closeForm());
    } else {
      this.taskService.create(payload as CreateTaskPayload, () => this.closeForm());
    }
  }

  restore(task: Task): void {
    this.taskService.restore(task.id);
  }

  confirmDelete(task: Task): void {
    if (confirm(`Remover "${task.title}"?\nFicará disponível para restaurar.`)) {
      this.taskService.delete(task.id);
    }
  }
}
