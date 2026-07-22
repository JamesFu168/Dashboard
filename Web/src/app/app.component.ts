import { Component } from '@angular/core';
import { KanbanBoardPageComponent } from './features/kanban/pages/kanban-board-page/kanban-board-page.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [KanbanBoardPageComponent],
  template: '<app-kanban-board-page />'
})
export class AppComponent {}
