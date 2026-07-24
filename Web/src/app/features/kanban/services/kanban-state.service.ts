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

  readonly cards = this.cardsSignal.asReadonly();
  readonly loading = this.loadingSignal.asReadonly();
  readonly viewMode = this.viewModeSignal.asReadonly();
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

  createCard(title: string, description: string | null, scope: CardScope, dueDate: string | null): Observable<KanbanCard> {
    const planCards = this.cardsSignal().filter((card) => card.status === CardStatus.Plan);
    const request: CreateCardRequest = {
      title,
      description,
      scope,
      departmentId: null,
      dueDate,
      sequenceOrder: (planCards.length + 1) * 100,
      devOpsUrl: null
    };

    return this.api.createCard(request).pipe(
      tap((card) => this.upsertCard(card))
    );
  }

  updateCard(card: KanbanCard, title: string, description: string | null, scope: CardScope, dueDate: string | null): Observable<KanbanCard> {
    const request: UpdateCardRequest = {
      title,
      description,
      scope,
      departmentId: null,
      dueDate,
      devOpsUrl: null,
      updatedAt: card.updatedAt
    };

    return this.api.updateCard(card.id, request).pipe(
      tap((updated) => this.upsertCard(updated))
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

  createTask(card: KanbanCard, title: string, dueDate: string | null): Observable<CardTask> {
    const request: CreateTaskRequest = {
      title,
      assigneeId: null,
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
