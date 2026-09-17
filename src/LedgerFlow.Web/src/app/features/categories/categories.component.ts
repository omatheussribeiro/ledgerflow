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
  readonly deletingId = signal<string | null>(null);
  readonly editingId = signal<string | null>(null);
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

  openCreate(): void {
    this.editingId.set(null);
    this.form.reset({ name: '', type: 2, color: '#18b887' });
    this.showForm.set(true);
  }

  edit(category: Category): void {
    this.editingId.set(category.id);
    this.form.reset({ name: category.name, type: category.type, color: category.color });
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
      ? this.api.updateCategory(id, this.form.getRawValue())
      : this.api.createCategory(this.form.getRawValue());
    request
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (item) => {
          this.categories.update((items) =>
            id ? items.map((current) => (current.id === id ? item : current)) : [...items, item],
          );
          this.cancel();
        },
        error: () => this.error.set('Could not save this category. Check for a duplicate name.'),
      });
  }

  remove(category: Category): void {
    if (!window.confirm(`Delete category "${category.name}"? Its history will be preserved.`)) return;

    this.deletingId.set(category.id);
    this.error.set('');
    this.api
      .deleteCategory(category.id)
      .pipe(finalize(() => this.deletingId.set(null)))
      .subscribe({
        next: () => {
          this.categories.update((items) => items.filter((item) => item.id !== category.id));
          if (this.editingId() === category.id) this.cancel();
        },
        error: () => this.error.set('Could not delete this category.'),
      });
  }
}
