import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, Validators, FormBuilder } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthStateService } from '../../services/auth-state.service';

/**
 * 使用者登入頁面元件 (LoginPageComponent)。
 * 提供 Email 與密碼輸入表單驗證與登入流程觸發。
 */
@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.css'
})
export class LoginPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authState = inject(AuthStateService);
  private readonly router = inject(Router);

  /** 登入表單送出中狀態 Signal */
  protected readonly submitting = signal(false);
  /** 錯誤訊息 Signal */
  protected readonly errorMessage = signal<string | null>(null);

  /** 登入表單控制項 (包含 Email 與 Password 驗證) */
  protected readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  /** 送出登入請求 */
  submit(): void {
    if (this.form.invalid || this.submitting()) {
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    const { email, password } = this.form.getRawValue();
    this.authState.login(email, password)
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => void this.router.navigateByUrl('/'),
        error: () => this.errorMessage.set('Email 或密碼錯誤，請再試一次。')
      });
  }
}
