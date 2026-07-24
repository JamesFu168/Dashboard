import { CommonModule } from '@angular/common';
import { Component, EventEmitter, OnInit, Output, inject } from '@angular/core';
import { CardScope } from '../../models/kanban.models';
import { KanbanStateService } from '../../services/kanban-state.service';

/**
 * 回收桶對話框元件 (TrashDialogComponent)。
 * 展現已被軟刪除的卡片列表，並提供「還原卡片」與「永久刪除」功能。
 */
@Component({
  selector: 'app-trash-dialog',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './trash-dialog.component.html',
  styleUrl: './trash-dialog.component.css'
})
export class TrashDialogComponent implements OnInit {
  /** 對話框關閉事件發射器 */
  @Output() closed = new EventEmitter<void>();

  protected readonly CardScope = CardScope;
  protected readonly state = inject(KanbanStateService);

  ngOnInit(): void {
    // 開啟對話框時載入當前使用者的回收桶清單
    this.state.loadTrash();
  }

  /** 還原指定之軟刪除卡片 */
  restore(cardId: string): void {
    const card = this.state.trash().find((item) => item.id === cardId);
    if (card) {
      this.state.restoreCard(card).subscribe();
    }
  }

  /** 永久從資料庫刪除指定卡片 */
  permanentlyDelete(cardId: string): void {
    if (!confirm('確定要永久刪除這張卡片嗎？此動作無法復原。')) {
      return;
    }

    this.state.permanentlyDeleteCard(cardId).subscribe();
  }

  /** 關閉對話框 */
  close(): void {
    this.closed.emit();
  }
}
