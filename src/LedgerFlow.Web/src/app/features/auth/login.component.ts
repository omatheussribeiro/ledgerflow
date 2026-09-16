import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthStore } from '../../core/auth/auth.store';
import { ProblemDetails } from '../../core/models/api.models';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);

  readonly registerMode = signal(false);
  readonly busy = signal(false);
  readonly error = signal('');
  readonly form = new FormGroup({
    name: new FormControl('', { nonNullable: true }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(10)],
    }),
  });

  constructor() {
    if (this.auth.isAuthenticated()) void this.router.navigate(['/dashboard']);
  }

  toggleMode(): void {
    this.registerMode.update((value) => !value);
    this.error.set('');

    const name = this.form.controls.name;
    if (this.registerMode()) name.addValidators([Validators.required, Validators.minLength(2)]);
    else name.clearValidators();
    name.updateValueAndValidity();
  }

  submit(): void {
    if (this.form.invalid) return;

    this.busy.set(true);
    this.error.set('');
    const { name, email, password } = this.form.getRawValue();
    const request = this.registerMode()
      ? this.auth.register(name, email, password)
      : this.auth.login(email, password);

    request.pipe(finalize(() => this.busy.set(false))).subscribe({
      next: () => void this.router.navigate(['/dashboard']),
      error: (response: HttpErrorResponse) =>
        this.error.set(
          (response.error as ProblemDetails)?.detail ??
            (response.error as ProblemDetails)?.message ??
            'We could not sign you in. Please try again.',
        ),
    });
  }
}
