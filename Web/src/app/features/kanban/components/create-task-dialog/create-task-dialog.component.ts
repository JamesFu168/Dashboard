import { Component, EventEmitter, Input, OnInit, Output, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { AuthStateService } from '../../../auth/services/auth-state.service';
import { CardScope, KanbanCard, UserSummary } from '../../models/kanban.models';
import { KanbanService } from '../../services/kanban.service';
import { KanbanStateService } from '../../services/kanban-state.service';

/**
 * 新增 Task 任務細項對話框元件 (CreateTaskDialogComponent)。
 * 提供標題、到期日與被指派成員選單 (僅 Organization 卡片允許指派他人)。
 */
@Component({
  selector: 'app-create-task-dialog',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './create-task-dialog.component.html',
  styleUrl: './create-task-dialog.component.css'
})
export class CreateTaskDialogComponent implements OnInit {
  /** 所屬卡片標的 */
  @Input({ required: true }) card!: KanbanCard;
  /** 對話框關閉事件發射器 */
  @Output() closed = new EventEmitter<void>();

  protected readonly CardScope = CardScope;

  private readonly fb = inject(FormBuilder);
  private readonly state = inject(KanbanStateService);
  private readonly api = inject(KanbanService);
  private readonly auth = inject(AuthStateService);

  /** 表單送出中狀態 Signal */
  protected readonly submitting = signal(false);
  /** 錯誤訊息 Signal */
  protected readonly errorMessage = signal<string | null>(null);
  /** 可供指派之部門成員列表 */
  protected readonly departmentUsers = signal<UserSummary[]>([]);

  /** 細項 Task 表單控制項 */
  protected readonly form = this.fb.nonNullable.group({
    title: ['', Validators.required],
    dueDate: [''],
    assigneeId: [this.auth.user()?.userId ?? null]
  });

  ngOnInit(): void {
    if (this.card.scope === CardScope.Organization) {
      this.api.getUsers(this.card.departmentId).subscribe((users) => this.departmentUsers.set(users));
    }
  }

  /** 送出新增 Task 表單 */
  submit(): void {
    if (this.form.invalid || this.submitting()) {
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    const { title, dueDate, assigneeId } = this.form.getRawValue();
    const effectiveAssigneeId = this.card.scope === CardScope.Organization ? assigneeId : null;
    this.state.createTask(this.card, title.trim(), dueDate || null, effectiveAssigneeId)
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => this.closed.emit(),
        error: () => this.errorMessage.set('新增任務失敗，請再試一次。')
      });
  }

  /** 取消並關閉對話框 */
  cancel(): void {
    this.closed.emit();
  }
}
