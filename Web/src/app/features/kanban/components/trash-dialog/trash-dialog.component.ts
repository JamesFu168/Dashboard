import { CommonModule } from '@angular/common';
import { Component, EventEmitter, OnInit, Output, inject } from '@angular/core';
import { CardScope } from '../../models/kanban.models';
import { KanbanStateService } from '../../services/kanban-state.service';

@Component({
  selector: 'app-trash-dialog',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './trash-dialog.component.html',
  styleUrl: './trash-dialog.component.css'
})
export class TrashDialogComponent implements OnInit {
  @Output() closed = new EventEmitter<void>();

  protected readonly CardScope = CardScope;
  protected readonly state = inject(KanbanStateService);

  ngOnInit(): void {
    this.state.loadTrash();
  }

  restore(cardId: string): void {
    const card = this.state.trash().find((item) => item.id === cardId);
    if (card) {
      this.state.restoreCard(card).subscribe();
    }
  }

  permanentlyDelete(cardId: string): void {
    if (!confirm('確定要永久刪除這張卡片嗎？此動作無法復原。')) {
      return;
    }

    this.state.permanentlyDeleteCard(cardId).subscribe();
  }

  close(): void {
    this.closed.emit();
  }
}
