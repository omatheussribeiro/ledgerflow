import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { ApiService } from '../../core/api/api.service';
import { Category, TransactionType } from '../../core/models/api.models';

@Component({
  selector: 'app-categories',
  imports: [ReactiveFormsModule],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoriesComponent {
  private readonly api = inject(ApiService);

  readonly categories = signal<Category[]>([]);
  readonly expenses = () => this.categories().filter((item) => item.type === 2);
  readonly income = () => this.categories().filter((item) => item.type === 1);
  readonly showForm = signal(false);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(80)],
    }),
    type: new FormControl<TransactionType>(2, { nonNullable: true }),
    color: new FormControl('#18b887', {
      nonNullable: true,
      validators: [Validators.pattern(/^#[0-9a-fA-F]{6}$/)],
    }),
  });

  constructor() {
    this.api.getCategories().subscribe({
      next: (items) => this.categories.set(items),
      error: () => this.error.set('Unable to load categories.'),
    });
  }

  create(): void {
    if (this.form.invalid) return;

    this.saving.set(true);
    this.api
      .createCategory(this.form.getRawValue())
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (item) => {
          this.categories.update((items) => [...items, item]);
          this.form.reset({ name: '', type: 2, color: '#18b887' });
          this.showForm.set(false);
        },
        error: () => this.error.set('Could not save this category. Check for a duplicate name.'),
      });
  }
}
