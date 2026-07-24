import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, finalize, tap } from 'rxjs';
import { CardScope, CardStatus, CardTask, CreateCardRequest, CreateTaskRequest, KanbanCard, UpdateCardRequest } from '../models/kanban.models';
import { KanbanService } from './kanban.service';

/**
 * 看板狀態管理服務 (KanbanStateService)。
 * 利用 Angular Signals 管理當前看板卡片列表、載入狀態、視角模式 (Personal / Organization) 與回收桶列表。
 */
@Injectable({ providedIn: 'root' })
export class KanbanStateService {
  private readonly api = inject(KanbanService);
  private readonly cardsSignal = signal<KanbanCard[]>([]);
  private readonly loadingSignal = signal(false);
  private readonly viewModeSignal = signal<'personal' | 'organization'>('personal');
  private readonly trashSignal = signal<KanbanCard[]>([]);

  /** 看板卡片列表 (唯讀 Signal) */
  readonly cards = this.cardsSignal.asReadonly();
  /** 載入中狀態 (唯讀 Signal) */
  readonly loading = this.loadingSignal.asReadonly();
  /** 當前檢視視角 (唯讀 Signal) */
  readonly viewMode = this.viewModeSignal.asReadonly();
  /** 回收桶軟刪除卡片列表 (唯讀 Signal) */
  readonly trash = this.trashSignal.asReadonly();

  /** 依卡片 Status (Plan, ToDo, Doing, Done) 分類之 4 個 Column (Computed Signal) */
  readonly columns = computed(() => [
    this.createColumn(CardStatus.Plan, '規劃'),
    this.createColumn(CardStatus.ToDo, '待辦'),
    this.createColumn(CardStatus.Doing, '進行中'),
    this.createColumn(CardStatus.Done, '完成')
  ]);

  /**
   * 載入指定視角的看板卡片資料。
   * @param viewMode 'personal' | 'organization'
   */
  load(viewMode = this.viewModeSignal()): void {
    this.viewModeSignal.set(viewMode);
    this.loadingSignal.set(true);
    this.api.getCards(viewMode)
      .pipe(finalize(() => this.loadingSignal.set(false)))
      .subscribe((cards) => this.cardsSignal.set(cards));
  }

  /**
   * 建立卡片並更新本地 Signals 狀態。
   */
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

  /**
   * 更新卡片內容並更新本地 Signals 狀態。
   */
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

  /**
   * 軟刪除卡片並從本地看板 Signals 中移除該卡片。
   */
  deleteCard(card: KanbanCard): Observable<void> {
    return this.api.deleteCard(card.id).pipe(
      tap(() => this.removeCard(card.id))
    );
  }

  /**
   * 載入回收桶軟刪除卡片列表。
   */
  loadTrash(): void {
    this.api.getTrash().subscribe((cards) => this.trashSignal.set(cards));
  }

  /**
   * 還原軟刪除卡片，將其從回收桶移除並加回看板 Signals。
   */
  restoreCard(card: KanbanCard): Observable<KanbanCard> {
    return this.api.restoreCard(card.id).pipe(
      tap((restored) => {
        this.trashSignal.update((cards) => cards.filter((item) => item.id !== card.id));
        this.upsertCard(restored);
      })
    );
  }

  /**
   * 永久刪除卡片並從回收桶 Signals 移除。
   */
  permanentlyDeleteCard(cardId: string): Observable<void> {
    return this.api.permanentlyDeleteCard(cardId).pipe(
      tap(() => this.trashSignal.update((cards) => cards.filter((item) => item.id !== cardId)))
    );
  }

  /**
   * 移動卡片位置 (Status) 或排序 SequenceOrder。
   */
  moveCard(card: KanbanCard, status: CardStatus, sequenceOrder: number): void {
    this.api.moveCard(card.id, { status, sequenceOrder, updatedAt: card.updatedAt })
      .subscribe((updated) => this.upsertCard(updated));
  }

  /**
   * 切換細項 Task 勾選完成狀態。
   */
  toggleTask(task: CardTask): void {
    this.api.toggleTask(task.id).subscribe((updatedTask) => {
      this.cardsSignal.update((cards) => cards.map((card) => ({
        ...card,
        tasks: card.tasks.map((item) => item.id === updatedTask.id ? updatedTask : item)
      })));
    });
  }

  /**
   * 新增細項 Task 至卡片中。
   */
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

  /**
   * 變更細項 Task 之被指派人員。
   */
  assignTask(card: KanbanCard, task: CardTask, assigneeId: number | null): Observable<CardTask> {
    return this.api.assignTask(task.id, { assigneeId, updatedAt: task.updatedAt }).pipe(
      tap((updatedTask) => {
        this.cardsSignal.update((cards) => cards.map((item) => item.id === card.id
          ? { ...item, tasks: item.tasks.map((existing) => existing.id === updatedTask.id ? updatedTask : existing) }
          : item));
      })
    );
  }

  /**
   * 新增或更新卡片至本地 Signals。
   */
  upsertCard(card: KanbanCard): void {
    this.cardsSignal.update((cards) => {
      const existing = cards.some((item) => item.id === card.id);
      return existing
        ? cards.map((item) => item.id === card.id ? card : item)
        : [...cards, card];
    });
  }

  /**
   * 從本地 Signals 移除指定卡片。
   */
  removeCard(cardId: string): void {
    this.cardsSignal.update((cards) => cards.filter((card) => card.id !== cardId));
  }

  /**
   * 輔助建立 Column 物件，包含按 SequenceOrder 排序之卡片。
   */
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
