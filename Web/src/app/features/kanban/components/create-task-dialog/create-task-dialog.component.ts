import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { KanbanCard } from '../../models/kanban.models';
import { KanbanStateService } from '../../services/kanban-state.service';

@Component({
  selector: 'app-create-task-dialog',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './create-task-dialog.component.html',
  styleUrl: './create-task-dialog.component.css'
})
export class CreateTaskDialogComponent {
  @Input({ required: true }) card!: KanbanCard;
  @Output() closed = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);
  private readonly state = inject(KanbanStateService);

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    title: ['', Validators.required],
    dueDate: ['']
  });

  submit(): void {
    if (this.form.invalid || this.submitting()) {
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    const { title, dueDate } = this.form.getRawValue();
    this.state.createTask(this.card, title.trim(), dueDate || null)
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
