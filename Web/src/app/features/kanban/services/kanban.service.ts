import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AssignTaskRequest, CardTask, CreateCardRequest, CreateTaskRequest, KanbanCard, MoveCardRequest, UpdateCardRequest, UserSummary } from '../models/kanban.models';

/**
 * 看板系統 HTTP API 串接服務 (KanbanService)。
 * 提供卡片 CRUD、狀態拖移、軟刪除/回收桶還原、自動結案、細項 Task 與成員清單 API 存取。
 */
@Injectable({ providedIn: 'root' })
export class KanbanService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = `${environment.apiBaseUrl}/api/v1`;

  /**
   * 取得指定視角可存取的卡片列表 (預設排除 AutoClosed 與 IsDeleted 卡片)。
   * @param viewMode 'personal' (個人視角) 或 'organization' (組織視角)
   */
  getCards(viewMode: 'personal' | 'organization'): Observable<KanbanCard[]> {
    return this.http.get<KanbanCard[]>(`${this.apiBaseUrl}/cards`, {
      params: { viewMode }
    });
  }

  /**
   * 建立新的看板卡片。
   */
  createCard(request: CreateCardRequest): Observable<KanbanCard> {
    return this.http.post<KanbanCard>(`${this.apiBaseUrl}/cards`, request);
  }

  /**
   * 更新卡片內容 (標題、描述、範疇、到期日、DevOps 連結)。
   */
  updateCard(cardId: string, request: UpdateCardRequest): Observable<KanbanCard> {
    return this.http.patch<KanbanCard>(`${this.apiBaseUrl}/cards/${cardId}`, request);
  }

  /**
   * 移動卡片位置 (Status) 與排序 SequenceOrder。
   */
  moveCard(cardId: string, request: MoveCardRequest): Observable<KanbanCard> {
    return this.http.put<KanbanCard>(`${this.apiBaseUrl}/cards/${cardId}/status`, request);
  }

  /**
   * 切換/勾選細項 Task 完成狀態。
   */
  toggleTask(taskId: string): Observable<CardTask> {
    return this.http.patch<CardTask>(`${this.apiBaseUrl}/tasks/${taskId}/toggle`, {});
  }

  /**
   * 於指定卡片新增細項 Task。
   */
  createTask(cardId: string, request: CreateTaskRequest): Observable<CardTask> {
    return this.http.post<CardTask>(`${this.apiBaseUrl}/cards/${cardId}/tasks`, request);
  }

  /**
   * 變更 Task 的被指派人員。
   */
  assignTask(taskId: string, request: AssignTaskRequest): Observable<CardTask> {
    return this.http.put<CardTask>(`${this.apiBaseUrl}/tasks/${taskId}/assign`, request);
  }

  /**
   * 軟刪除卡片 (移至回收桶)。
   */
  deleteCard(cardId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiBaseUrl}/cards/${cardId}`);
  }

  /**
   * 批次將上個月以前完成 (Done) 之卡片自動更新為結案 (AutoClosed)。
   */
  autoClosePreviousMonthCards(): Observable<KanbanCard[]> {
    return this.http.post<KanbanCard[]>(`${this.apiBaseUrl}/cards/auto-close-previous-month`, {});
  }

  /**
   * 取得已結案 (AutoClosed) 卡片列表。
   */
  getClosedCards(): Observable<KanbanCard[]> {
    return this.http.get<KanbanCard[]>(`${this.apiBaseUrl}/cards/closed`);
  }

  /**
   * 取得回收桶中的軟刪除卡片列表。
   */
  getTrash(): Observable<KanbanCard[]> {
    return this.http.get<KanbanCard[]>(`${this.apiBaseUrl}/cards/trash`);
  }

  /**
   * 從回收桶還原卡片。
   */
  restoreCard(cardId: string): Observable<KanbanCard> {
    return this.http.post<KanbanCard>(`${this.apiBaseUrl}/cards/${cardId}/restore`, {});
  }

  /**
   * 從回收桶永久刪除卡片。
   */
  permanentlyDeleteCard(cardId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiBaseUrl}/cards/${cardId}/permanent`);
  }

  /**
   * 取得系統使用者摘要列表 (可選擇依部門 ID 過濾)。
   */
  getUsers(departmentId?: number | null): Observable<UserSummary[]> {
    const params = departmentId ? { departmentId: String(departmentId) } : undefined;
    return this.http.get<UserSummary[]>(`${this.apiBaseUrl}/users`, { params });
  }
}
