import { CdkDragDrop, DragDropModule } from '@angular/cdk/drag-drop';
import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthStateService } from '../../../auth/services/auth-state.service';
import { CreateTaskDialogComponent } from '../../components/create-task-dialog/create-task-dialog.component';
import { CardStatus, CardTask, KanbanCard } from '../../models/kanban.models';
import { KanbanRealtimeService } from '../../services/kanban-realtime.service';
import { KanbanStateService } from '../../services/kanban-state.service';

@Component({
  selector: 'app-kanban-board-page',
  standalone: true,
  imports: [CommonModule, DragDropModule, CreateTaskDialogComponent],
  templateUrl: './kanban-board-page.component.html',
  styleUrl: './kanban-board-page.component.css'
})
export class KanbanBoardPageComponent implements OnInit {
  protected readonly state = inject(KanbanStateService);
  protected readonly auth = inject(AuthStateService);
  private readonly realtime = inject(KanbanRealtimeService);
  private readonly router = inject(Router);

  protected readonly expandedStatus = signal<CardStatus | null>(null);
  protected readonly creatingTaskFor = signal<KanbanCard | null>(null);

  ngOnInit(): void {
    this.state.load('personal');
    void this.realtime.connect(this.auth.user()?.departmentId ?? 1);
  }

  setViewMode(viewMode: 'personal' | 'organization'): void {
    this.state.load(viewMode);
  }

  toggleExpand(status: CardStatus): void {
    this.expandedStatus.update((current) => current === status ? null : status);
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

  openCreateTask(card: KanbanCard): void {
    this.creatingTaskFor.set(card);
  }

  closeCreateTask(): void {
    this.creatingTaskFor.set(null);
  }
}
