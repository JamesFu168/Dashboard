import { Routes } from '@angular/router';
import { authGuard } from './features/auth/guards/auth.guard';
import { LoginPageComponent } from './features/auth/pages/login-page/login-page.component';
import { KanbanBoardPageComponent } from './features/kanban/pages/kanban-board-page/kanban-board-page.component';

export const routes: Routes = [
  { path: 'login', component: LoginPageComponent },
  { path: '', component: KanbanBoardPageComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '' }
];
