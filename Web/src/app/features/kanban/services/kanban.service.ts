import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CardTask, CreateCardRequest, CreateTaskRequest, KanbanCard, MoveCardRequest, UpdateCardRequest } from '../models/kanban.models';

@Injectable({ providedIn: 'root' })
export class KanbanService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = `${environment.apiBaseUrl}/api/v1`;

  getCards(viewMode: 'personal' | 'organization'): Observable<KanbanCard[]> {
    return this.http.get<KanbanCard[]>(`${this.apiBaseUrl}/cards`, {
      params: { viewMode }
    });
  }

  createCard(request: CreateCardRequest): Observable<KanbanCard> {
    return this.http.post<KanbanCard>(`${this.apiBaseUrl}/cards`, request);
  }

  updateCard(cardId: string, request: UpdateCardRequest): Observable<KanbanCard> {
    return this.http.patch<KanbanCard>(`${this.apiBaseUrl}/cards/${cardId}`, request);
  }

  moveCard(cardId: string, request: MoveCardRequest): Observable<KanbanCard> {
    return this.http.put<KanbanCard>(`${this.apiBaseUrl}/cards/${cardId}/status`, request);
  }

  toggleTask(taskId: string): Observable<CardTask> {
    return this.http.patch<CardTask>(`${this.apiBaseUrl}/tasks/${taskId}/toggle`, {});
  }

  createTask(cardId: string, request: CreateTaskRequest): Observable<CardTask> {
    return this.http.post<CardTask>(`${this.apiBaseUrl}/cards/${cardId}/tasks`, request);
  }
}
