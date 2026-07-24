import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, finalize, tap } from 'rxjs';
import { CardScope, CardStatus, CardTask, CreateCardRequest, CreateTaskRequest, KanbanCard, UpdateCardRequest } from '../models/kanban.models';
import { KanbanService } from './kanban.service';

@Injectable({ providedIn: 'root' })
export class KanbanStateService {
  private readonly api = inject(KanbanService);
  private readonly cardsSignal = signal<KanbanCard[]>([]);
  private readonly loadingSignal = signal(false);
  private readonly viewModeSignal = signal<'personal' | 'organization'>('personal');
  private readonly trashSignal = signal<KanbanCard[]>([]);

  readonly cards = this.cardsSignal.asReadonly();
  readonly loading = this.loadingSignal.asReadonly();
  readonly viewMode = this.viewModeSignal.asReadonly();
  readonly trash = this.trashSignal.asReadonly();
  readonly columns = computed(() => [
    this.createColumn(CardStatus.Plan, '規劃'),
    this.createColumn(CardStatus.ToDo, '待辦'),
    this.createColumn(CardStatus.Doing, '進行中'),
    this.createColumn(CardStatus.Done, '完成')
  ]);

  load(viewMode = this.viewModeSignal()): void {
    this.viewModeSignal.set(viewMode);
    this.loadingSignal.set(true);
    this.api.getCards(viewMode)
      .pipe(finalize(() => this.loadingSignal.set(false)))
      .subscribe((cards) => this.cardsSignal.set(cards));
  }

  createCard(title: string, description: string | null, scope: CardScope, dueDate: string | null, devOpsUrl: string | null): Observable<KanbanCard> {
    const planCards = this.cardsSignal().filter((card) => card.status === CardStatus.Plan);
    const request: CreateCardRequest = {
      title,
      description,
      scope,
      departmentId: null,
      dueDate,
      sequenceOrder: (planCards.length + 1) * 100,
      devOpsUrl
    };

    return this.api.createCard(request).pipe(
      tap((card) => this.upsertCard(card))
    );
  }

  updateCard(card: KanbanCard, title: string, description: string | null, scope: CardScope, dueDate: string | null, devOpsUrl: string | null): Observable<KanbanCard> {
    const request: UpdateCardRequest = {
      title,
      description,
      scope,
      departmentId: null,
      dueDate,
      devOpsUrl,
      updatedAt: card.updatedAt
    };

    return this.api.updateCard(card.id, request).pipe(
      tap((updated) => this.upsertCard(updated))
    );
  }

  deleteCard(card: KanbanCard): Observable<void> {
    return this.api.deleteCard(card.id).pipe(
      tap(() => this.removeCard(card.id))
    );
  }

  loadTrash(): void {
    this.api.getTrash().subscribe((cards) => this.trashSignal.set(cards));
  }

  restoreCard(card: KanbanCard): Observable<KanbanCard> {
    return this.api.restoreCard(card.id).pipe(
      tap((restored) => {
        this.trashSignal.update((cards) => cards.filter((item) => item.id !== card.id));
        this.upsertCard(restored);
      })
    );
  }

  permanentlyDeleteCard(cardId: string): Observable<void> {
    return this.api.permanentlyDeleteCard(cardId).pipe(
      tap(() => this.trashSignal.update((cards) => cards.filter((item) => item.id !== cardId)))
    );
  }

  moveCard(card: KanbanCard, status: CardStatus, sequenceOrder: number): void {
    this.api.moveCard(card.id, { status, sequenceOrder, updatedAt: card.updatedAt })
      .subscribe((updated) => this.upsertCard(updated));
  }

  toggleTask(task: CardTask): void {
    this.api.toggleTask(task.id).subscribe((updatedTask) => {
      this.cardsSignal.update((cards) => cards.map((card) => ({
        ...card,
        tasks: card.tasks.map((item) => item.id === updatedTask.id ? updatedTask : item)
      })));
    });
  }

  createTask(card: KanbanCard, title: string, dueDate: string | null, assigneeId: number | null): Observable<CardTask> {
    const request: CreateTaskRequest = {
      title,
      assigneeId,
      sequenceOrder: (card.tasks.length + 1) * 100,
      dueDate,
      devOpsUrl: null
    };

    return this.api.createTask(card.id, request).pipe(
      tap((task) => {
        this.cardsSignal.update((cards) => cards.map((item) => item.id === card.id
          ? { ...item, tasks: [...item.tasks, task] }
          : item));
      })
    );
  }

  assignTask(card: KanbanCard, task: CardTask, assigneeId: number | null): Observable<CardTask> {
    return this.api.assignTask(task.id, { assigneeId, updatedAt: task.updatedAt }).pipe(
      tap((updatedTask) => {
        this.cardsSignal.update((cards) => cards.map((item) => item.id === card.id
          ? { ...item, tasks: item.tasks.map((existing) => existing.id === updatedTask.id ? updatedTask : existing) }
          : item));
      })
    );
  }

  upsertCard(card: KanbanCard): void {
    this.cardsSignal.update((cards) => {
      const existing = cards.some((item) => item.id === card.id);
      return existing
        ? cards.map((item) => item.id === card.id ? card : item)
        : [...cards, card];
    });
  }

  removeCard(cardId: string): void {
    this.cardsSignal.update((cards) => cards.filter((card) => card.id !== cardId));
  }

  private createColumn(status: CardStatus, title: string) {
    return {
      status,
      title,
      cards: this.cardsSignal()
        .filter((card) => card.status === status)
        .sort((left, right) => left.sequenceOrder - right.sequenceOrder)
    };
  }
}
