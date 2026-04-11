import { Routes } from '@angular/router';
 
export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./presentation/task-list').then(m => m.TaskListComponent),
  },
  {
    path: '**',
    redirectTo: '',
  },
];
 