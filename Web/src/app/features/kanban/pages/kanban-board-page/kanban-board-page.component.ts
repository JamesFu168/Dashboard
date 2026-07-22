import { CdkDragDrop, DragDropModule } from '@angular/cdk/drag-drop';
import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthStateService } from '../../../auth/services/auth-state.service';
import { CardStatus, CardTask, KanbanCard } from '../../models/kanban.models';
import { KanbanRealtimeService } from '../../services/kanban-realtime.service';
import { KanbanStateService } from '../../services/kanban-state.service';

@Component({
  selector: 'app-kanban-board-page',
  standalone: true,
  imports: [CommonModule, DragDropModule],
  templateUrl: './kanban-board-page.component.html',
  styleUrl: './kanban-board-page.component.css'
})
export class KanbanBoardPageComponent implements OnInit {
  protected readonly state = inject(KanbanStateService);
  protected readonly auth = inject(AuthStateService);
  private readonly realtime = inject(KanbanRealtimeService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.state.load('personal');
    void this.realtime.connect(this.auth.user()?.departmentId ?? 1);
  }

  setViewMode(viewMode: 'personal' | 'organization'): void {
    this.state.load(viewMode);
  }

  isOwner(card: KanbanCard): boolean {
    return card.ownerId === this.auth.user()?.userId;
  }

  canToggleTask(card: KanbanCard, task: CardTask): boolean {
    return this.isOwner(card) || task.assigneeId === this.auth.user()?.userId;
  }

  logout(): void {
    this.auth.logout();
    void this.router.navigateByUrl('/login');
  }

  drop(event: CdkDragDrop<KanbanCard[]>, status: CardStatus): void {
    const card = event.item.data as KanbanCard;
    if (!this.isOwner(card)) {
      return;
    }

    const sequenceOrder = (event.currentIndex + 1) * 100;
    this.state.moveCard(card, status, sequenceOrder);
  }

  toggleTask(task: CardTask): void {
    this.state.toggleTask(task);
  }
}
