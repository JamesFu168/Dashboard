import { CdkDragDrop, DragDropModule } from '@angular/cdk/drag-drop';
import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthStateService } from '../../../auth/services/auth-state.service';
import { CardFormDialogComponent } from '../../components/card-form-dialog/card-form-dialog.component';
import { CreateTaskDialogComponent } from '../../components/create-task-dialog/create-task-dialog.component';
import { TrashDialogComponent } from '../../components/trash-dialog/trash-dialog.component';
import { CardScope, CardStatus, CardTask, KanbanCard, UserSummary } from '../../models/kanban.models';
import { KanbanRealtimeService } from '../../services/kanban-realtime.service';
import { KanbanService } from '../../services/kanban.service';
import { KanbanStateService } from '../../services/kanban-state.service';

@Component({
  selector: 'app-kanban-board-page',
  standalone: true,
  imports: [CommonModule, DragDropModule, CreateTaskDialogComponent, CardFormDialogComponent, TrashDialogComponent],
  templateUrl: './kanban-board-page.component.html',
  styleUrl: './kanban-board-page.component.css'
})
export class KanbanBoardPageComponent implements OnInit {
  protected readonly state = inject(KanbanStateService);
  protected readonly auth = inject(AuthStateService);
  private readonly realtime = inject(KanbanRealtimeService);
  private readonly api = inject(KanbanService);
  private readonly router = inject(Router);

  protected readonly CardScope = CardScope;

  protected readonly expandedStatus = signal<CardStatus | null>(null);
  protected readonly creatingTaskFor = signal<KanbanCard | null>(null);
  protected readonly creatingCard = signal(false);
  protected readonly editingCard = signal<KanbanCard | null>(null);
  protected readonly showTrash = signal(false);
  protected readonly departmentUsers = signal<UserSummary[]>([]);

  ngOnInit(): void {
    this.state.load('personal');
    void this.realtime.connect(this.auth.user()?.departmentId ?? 1);
    this.api.getUsers(this.auth.user()?.departmentId).subscribe((users) => this.departmentUsers.set(users));
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

  openCreateCard(): void {
    this.creatingCard.set(true);
  }

  closeCreateCard(): void {
    this.creatingCard.set(false);
  }

  openEditCard(card: KanbanCard): void {
    this.editingCard.set(card);
  }

  closeEditCard(): void {
    this.editingCard.set(null);
  }

  deleteCard(card: KanbanCard): void {
    if (!confirm(`確定要刪除卡片「${card.title}」嗎？可以之後從垃圾桶還原。`)) {
      return;
    }

    this.state.deleteCard(card).subscribe();
  }

  openTrash(): void {
    this.showTrash.set(true);
  }

  closeTrash(): void {
    this.showTrash.set(false);
  }

  assignTask(card: KanbanCard, task: CardTask, assigneeId: string): void {
    this.state.assignTask(card, task, assigneeId ? Number(assigneeId) : null).subscribe();
  }
}
