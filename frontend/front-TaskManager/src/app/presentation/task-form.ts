
import { Component, Input, Output, EventEmitter, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Task, CreateTaskPayload, UpdateTaskPayload, TASK_STATUS_OPTIONS, TaskStatus } from '../domain/task.model';

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './task-form.html',
  styleUrls: ['./task-form.scss'],
})
export class TaskFormComponent implements OnInit {
  @Input() task: Task | null = null;
  @Output() submitted = new EventEmitter<CreateTaskPayload | UpdateTaskPayload>();
  @Output() cancelled = new EventEmitter<void>();

  title = signal('');
  description = signal('');
  selectedStatus = signal<TaskStatus>(0);
  statusOptions = TASK_STATUS_OPTIONS;

  ngOnInit(): void {
    if (this.task) {
      this.title.set(this.task.title);
      this.description.set(this.task.description ?? '');
      this.selectedStatus.set(this.task.status);
    }
  }

  onSubmit(): void {
    if (!this.title().trim()) return;

    const payload = this.task
      ? {
          title: this.title().trim(),
          description: this.description().trim() || null,
          status: this.selectedStatus(),
        } satisfies UpdateTaskPayload
      : {
          title: this.title().trim(),
          description: this.description().trim() || null,
        } satisfies CreateTaskPayload;

    this.submitted.emit(payload);
  }

  onCancel(): void {
    this.cancelled.emit();
  }
}