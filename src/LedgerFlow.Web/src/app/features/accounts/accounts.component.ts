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
  readonly deletingId = signal<string | null>(null);
  readonly editingId = signal<string | null>(null);
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

  openCreate(): void {
    this.editingId.set(null);
    this.form.reset({ name: '', type: 1, initialBalance: 0 });
    this.showForm.set(true);
  }

  edit(account: Account): void {
    this.editingId.set(account.id);
    this.form.reset({
      name: account.name,
      type: account.type,
      initialBalance: account.initialBalance,
    });
    this.showForm.set(true);
  }

  cancel(): void {
    this.editingId.set(null);
    this.showForm.set(false);
  }

  save(): void {
    if (this.form.invalid) return;

    this.saving.set(true);
    this.error.set('');
    const id = this.editingId();
    const request = id
      ? this.api.updateAccount(id, this.form.getRawValue())
      : this.api.createAccount(this.form.getRawValue());
    request
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (account) => {
          this.accounts.update((items) =>
            id ? items.map((item) => (item.id === id ? account : item)) : [...items, account],
          );
          this.cancel();
        },
        error: () => this.error.set('Could not save this account. Check for a duplicate name.'),
      });
  }

  remove(account: Account): void {
    if (!window.confirm(`Delete account "${account.name}"? Its history will be preserved.`)) return;

    this.deletingId.set(account.id);
    this.error.set('');
    this.api
      .deleteAccount(account.id)
      .pipe(finalize(() => this.deletingId.set(null)))
      .subscribe({
        next: () => {
          this.accounts.update((items) => items.filter((item) => item.id !== account.id));
          if (this.editingId() === account.id) this.cancel();
        },
        error: () => this.error.set('Could not delete this account.'),
      });
  }

  typeName(type: number): string {
    return ['', 'Checking', 'Savings', 'Cash', 'Investment'][type] ?? 'Account';
  }

  icon(type: number): string {
    return ['', '▤', '◇', '●', '↗'][type] ?? '▣';
  }
}
