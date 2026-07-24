import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../../environments/environment';
import { KanbanStateService } from './kanban-state.service';
import { KanbanCard } from '../models/kanban.models';

/**
 * SignalR 即時同步服務 (KanbanRealtimeService)。
 * 負責連線後端 KanbanHub，監聽 CardCreated, CardUpdated, CardMoved, CardDeleted, TaskUpdated 等廣播事件。
 */
@Injectable({ providedIn: 'root' })
export class KanbanRealtimeService {
  private readonly state = inject(KanbanStateService);
  private connection: signalR.HubConnection | null = null;

  /**
   * 建立 SignalR 連線並加入指定部門廣播群組。
   * @param departmentId 部門識別碼 (預設為 1)
   */
  async connect(departmentId = 1): Promise<void> {
    if (this.connection) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(environment.signalRHubUrl)
      .withAutomaticReconnect()
      .build();

    // 監聽卡片與細項建立、更新、移動事件
    for (const eventName of ['CardCreated', 'CardUpdated', 'CardMoved', 'TaskUpdated']) {
      this.connection.on(eventName, (card: KanbanCard) => this.state.upsertCard(card));
    }

    // 監聽卡片軟刪除事件
    this.connection.on('CardDeleted', (card: KanbanCard) => this.state.removeCard(card.id));

    await this.connection.start();
    await this.connection.invoke('JoinDepartmentGroup', departmentId);
  }
}
