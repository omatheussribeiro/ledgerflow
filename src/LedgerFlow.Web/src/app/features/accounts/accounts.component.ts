import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { ApiService } from '../../core/api/api.service';
import { Account } from '../../core/models/api.models';

@Component({
  selector: 'app-accounts',
  imports: [ReactiveFormsModule, CurrencyPipe],
  templateUrl: './accounts.component.html',
  styleUrl: './accounts.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountsComponent {
  private readonly api = inject(ApiService);

  readonly accounts = signal<Account[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly showForm = signal(false);
  readonly error = signal('');
  readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    type: new FormControl(1, { nonNullable: true }),
    initialBalance: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.api.getAccounts().subscribe({
      next: (data) => {
        this.accounts.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load accounts.');
        this.loading.set(false);
      },
    });
  }

  create(): void {
    if (this.form.invalid) return;

    this.saving.set(true);
    this.api
      .createAccount(this.form.getRawValue())
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (account) => {
          this.accounts.update((items) => [...items, account]);
          this.form.reset({ name: '', type: 1, initialBalance: 0 });
          this.showForm.set(false);
        },
        error: () => this.error.set('Could not save this account. Check for a duplicate name.'),
      });
  }

  typeName(type: number): string {
    return ['', 'Checking', 'Savings', 'Cash', 'Investment'][type] ?? 'Account';
  }

  icon(type: number): string {
    return ['', '▤', '◇', '●', '↗'][type] ?? '▣';
  }
}
