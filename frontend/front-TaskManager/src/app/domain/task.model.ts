export type TaskStatus = 0 | 1 | 2 | 3;

export const TASK_STATUS = {
  Pending: 0,
  InProgress: 1,
  Done: 2,
  Canceled: 3,
} as const;

export const TASK_STATUS_LABEL: Record<TaskStatus, string> = {
  0: 'Pendente',
  1: 'Em andamento',
  2: 'Concluída',
  3: 'Cancelada',
};

export const TASK_STATUS_OPTIONS: { value: TaskStatus; label: string }[] = [
  { value: 0, label: 'Pendente' },
  { value: 1, label: 'Em andamento' },
  { value: 2, label: 'Concluída' },
  { value: 3, label: 'Cancelada' },
];

export interface Task {
  id: string;
  title: string;
  description: string | null;
  status: TaskStatus;
  isDeleted: boolean;
  deletedAtUtc: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CreateTaskPayload {
  title: string;
  description: string | null;
}

export interface UpdateTaskPayload {
  title: string;
  description: string | null;
  status: TaskStatus;
}