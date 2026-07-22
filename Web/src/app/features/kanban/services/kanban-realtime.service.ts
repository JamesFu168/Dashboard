import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../../environments/environment';
import { KanbanStateService } from './kanban-state.service';
import { KanbanCard } from '../models/kanban.models';

@Injectable({ providedIn: 'root' })
export class KanbanRealtimeService {
  private readonly state = inject(KanbanStateService);
  private connection: signalR.HubConnection | null = null;

  async connect(departmentId = 1): Promise<void> {
    if (this.connection) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(environment.signalRHubUrl)
      .withAutomaticReconnect()
      .build();

    for (const eventName of ['CardCreated', 'CardUpdated', 'CardMoved', 'TaskUpdated']) {
      this.connection.on(eventName, (card: KanbanCard) => this.state.upsertCard(card));
    }

    this.connection.on('CardDeleted', (card: KanbanCard) => this.state.removeCard(card.id));

    await this.connection.start();
    await this.connection.invoke('JoinDepartmentGroup', departmentId);
  }
}
