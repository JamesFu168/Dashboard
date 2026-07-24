import { Component, EventEmitter, Input, OnInit, Output, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { AuthStateService } from '../../../auth/services/auth-state.service';
import { CardScope, KanbanCard, UserSummary } from '../../models/kanban.models';
import { KanbanService } from '../../services/kanban.service';
import { KanbanStateService } from '../../services/kanban-state.service';

@Component({
  selector: 'app-create-task-dialog',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './create-task-dialog.component.html',
  styleUrl: './create-task-dialog.component.css'
})
export class CreateTaskDialogComponent implements OnInit {
  @Input({ required: true }) card!: KanbanCard;
  @Output() closed = new EventEmitter<void>();

  protected readonly CardScope = CardScope;

  private readonly fb = inject(FormBuilder);
  private readonly state = inject(KanbanStateService);
  private readonly api = inject(KanbanService);
  private readonly auth = inject(AuthStateService);

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly departmentUsers = signal<UserSummary[]>([]);

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

  cancel(): void {
    this.closed.emit();
  }
}
