import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize, forkJoin } from 'rxjs';
import { ApiService } from '../../core/api/api.service';
import {
  Account,
  Category,
  PagedResult,
  Transaction,
  TransactionStatus,
  TransactionType,
} from '../../core/models/api.models';

@Component({
  selector: 'app-transactions',
  imports: [ReactiveFormsModule, CurrencyPipe, DatePipe],
  templateUrl: './transactions.component.html',
  styleUrl: './transactions.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TransactionsComponent {
  private readonly api = inject(ApiService);
  private page = 1;

  readonly accounts = signal<Account[]>([]);
  readonly categories = signal<Category[]>([]);
  readonly result = signal<PagedResult<Transaction>>({
    items: [],
    page: 1,
    pageSize: 20,
    totalCount: 0,
    totalPages: 0,
  });
  readonly showForm = signal(false);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly form = new FormGroup({
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(160)],
    }),
    type: new FormControl<TransactionType>(2, { nonNullable: true }),
    amount: new FormControl<number | null>(null, [Validators.required, Validators.min(0.01)]),
    accountId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    categoryId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    occurredOn: new FormControl(new Date().toISOString().slice(0, 10), {
      nonNullable: true,
      validators: [Validators.required],
    }),
    status: new FormControl<TransactionStatus>(2, { nonNullable: true }),
    notes: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(500)],
    }),
  });
  readonly filters = new FormGroup({
    search: new FormControl('', { nonNullable: true }),
    from: new FormControl('', { nonNullable: true }),
    to: new FormControl('', { nonNullable: true }),
    type: new FormControl('', { nonNullable: true }),
  });
  readonly availableCategories = () =>
    this.categories().filter((item) => item.type === this.form.controls.type.value);

  constructor() {
    forkJoin({
      accounts: this.api.getAccounts(),
      categories: this.api.getCategories(),
    }).subscribe(({ accounts, categories }) => {
      this.accounts.set(accounts);
      this.categories.set(categories);
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const filters = this.filters.getRawValue();
    this.api.getTransactions({ ...filters, page: this.page, pageSize: 20 }).subscribe({
      next: (data) => {
        this.result.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load transactions.');
        this.loading.set(false);
      },
    });
  }

  goTo(page: number): void {
    this.page = page;
    this.load();
  }

  create(): void {
    if (this.form.invalid) return;

    this.saving.set(true);
    this.error.set('');
    this.api
      .createTransaction(this.form.getRawValue())
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.showForm.set(false);
          this.form.reset({
            description: '',
            type: 2,
            amount: null,
            accountId: '',
            categoryId: '',
            occurredOn: new Date().toISOString().slice(0, 10),
            status: 2,
            notes: '',
          });
          this.page = 1;
          this.load();
        },
        error: () =>
          this.error.set(
            'Could not record this transaction. Check its account, category and values.',
          ),
      });
  }
}
