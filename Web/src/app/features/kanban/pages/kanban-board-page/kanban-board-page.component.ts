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

/**
 * 主看板頁面元件 (KanbanBoardPageComponent)。
 * 展現 Plan、ToDo、Doing、Done 四個狀態欄位、視角切換、卡片拖拉 Drag-Drop、回收桶彈窗與 SignalR 即時同步。
 */
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

  /** 展開欄位全螢幕檢視狀態 */
  protected readonly expandedStatus = signal<CardStatus | null>(null);
  /** 欲新增 Task 的卡片標的 (非 null 時開啟 Task 彈窗) */
  protected readonly creatingTaskFor = signal<KanbanCard | null>(null);
  /** 是否顯示新增卡片彈窗 */
  protected readonly creatingCard = signal(false);
  /** 欲編輯的卡片標的 (非 null 時開啟編輯彈窗) */
  protected readonly editingCard = signal<KanbanCard | null>(null);
  /** 是否開啟回收桶彈窗 */
  protected readonly showTrash = signal(false);
  /** 當前部門成員清單 (用於 Task 指派選單) */
  protected readonly departmentUsers = signal<UserSummary[]>([]);

  ngOnInit(): void {
    // 預設載入個人視角卡片
    this.state.load('personal');
    // 連線 SignalR 即時同步 Hub
    void this.realtime.connect(this.auth.user()?.departmentId ?? 1);
    // 取得當前部門成員清單
    this.api.getUsers(this.auth.user()?.departmentId).subscribe((users) => this.departmentUsers.set(users));
  }

  /** 切換「個人視角」與「組織視角」 */
  setViewMode(viewMode: 'personal' | 'organization'): void {
    this.state.load(viewMode);
  }

  /** 切換欄位展開/折疊 */
  toggleExpand(status: CardStatus): void {
    this.expandedStatus.update((current) => current === status ? null : status);
  }

  /** 檢查當前登入者是否為該卡片的 Owner */
  isOwner(card: KanbanCard): boolean {
    return card.ownerId === this.auth.user()?.userId;
  }

  /** 檢查當前登入者是否具備勾選/切換該 Task 完成狀態的權限 (Owner 或 Task Assignee) */
  canToggleTask(card: KanbanCard, task: CardTask): boolean {
    return this.isOwner(card) || task.assigneeId === this.auth.user()?.userId;
  }

  /** 登出系統 */
  logout(): void {
    this.auth.logout();
    void this.router.navigateByUrl('/login');
  }

  /** 處理 CDK Drag & Drop 拖放卡片事件 */
  drop(event: CdkDragDrop<KanbanCard[]>, status: CardStatus): void {
    const card = event.item.data as KanbanCard;
    if (!this.isOwner(card)) {
      return;
    }

    const sequenceOrder = (event.currentIndex + 1) * 100;
    this.state.moveCard(card, status, sequenceOrder);
  }

  /** 切換 Task 勾選完成狀態 */
  toggleTask(task: CardTask): void {
    this.state.toggleTask(task);
  }

  /** 開啟新增 Task 對話框 */
  openCreateTask(card: KanbanCard): void {
    this.creatingTaskFor.set(card);
  }

  /** 關閉新增 Task 對話框 */
  closeCreateTask(): void {
    this.creatingTaskFor.set(null);
  }

  /** 開啟新增卡片對話框 */
  openCreateCard(): void {
    this.creatingCard.set(true);
  }

  /** 關閉新增卡片對話框 */
  closeCreateCard(): void {
    this.creatingCard.set(false);
  }

  /** 開啟編輯卡片對話框 */
  openEditCard(card: KanbanCard): void {
    this.editingCard.set(card);
  }

  /** 關閉編輯卡片對話框 */
  closeEditCard(): void {
    this.editingCard.set(null);
  }

  /** 軟刪除指定卡片 (移至回收桶) */
  deleteCard(card: KanbanCard): void {
    if (!confirm(`確定要刪除卡片「${card.title}」嗎？可以之後從垃圾桶還原。`)) {
      return;
    }

    this.state.deleteCard(card).subscribe();
  }

  /** 開啟回收桶對話框 */
  openTrash(): void {
    this.showTrash.set(true);
  }

  /** 關閉回收桶對話框 */
  closeTrash(): void {
    this.showTrash.set(false);
  }

  /** 快速變更 Task 指派人員 */
  assignTask(card: KanbanCard, task: CardTask, assigneeId: string): void {
    this.state.assignTask(card, task, assigneeId ? Number(assigneeId) : null).subscribe();
  }
}
