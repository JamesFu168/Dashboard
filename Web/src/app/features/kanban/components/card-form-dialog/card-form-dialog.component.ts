import { Component, EventEmitter, Input, OnInit, Output, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { CardScope, KanbanCard } from '../../models/kanban.models';
import { KanbanStateService } from '../../services/kanban-state.service';

@Component({
  selector: 'app-card-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './card-form-dialog.component.html',
  styleUrl: './card-form-dialog.component.css'
})
export class CardFormDialogComponent implements OnInit {
  @Input() card: KanbanCard | null = null;
  @Output() closed = new EventEmitter<void>();

  protected readonly CardScope = CardScope;

  private readonly fb = inject(FormBuilder);
  private readonly state = inject(KanbanStateService);

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    title: ['', Validators.required],
    description: [''],
    scope: [CardScope.Personal, Validators.required],
    dueDate: [''],
    devOpsUrl: ['']
  });

  ngOnInit(): void {
    if (this.card) {
      this.form.patchValue({
        title: this.card.title,
        description: this.card.description ?? '',
        scope: this.card.scope,
        dueDate: this.card.dueDate ?? '',
        devOpsUrl: this.card.devOpsUrl ?? ''
      });
    }
  }

  submit(): void {
    if (this.form.invalid || this.submitting()) {
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    const { title, description, scope, dueDate, devOpsUrl } = this.form.getRawValue();
    const trimmedTitle = title.trim();
    const trimmedDescription = description.trim();
    const trimmedDevOpsUrl = devOpsUrl.trim();
    const request$ = this.card
      ? this.state.updateCard(this.card, trimmedTitle, trimmedDescription, scope, dueDate || null, trimmedDevOpsUrl || null)
      : this.state.createCard(trimmedTitle, trimmedDescription, scope, dueDate || null, trimmedDevOpsUrl || null);

    request$
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => this.closed.emit(),
        error: () => this.errorMessage.set(this.card ? '更新卡片失敗，請再試一次。' : '新增卡片失敗，請再試一次。')
      });
  }

  cancel(): void {
    this.closed.emit();
  }
}
