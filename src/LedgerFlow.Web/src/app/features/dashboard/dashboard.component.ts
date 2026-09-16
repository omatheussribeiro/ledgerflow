import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { Dashboard } from '../../core/models/api.models';

@Component({
  selector: 'app-dashboard',
  imports: [CurrencyPipe, DatePipe, DecimalPipe, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent {
  private readonly api = inject(ApiService);

  readonly data = signal<Dashboard | null>(null);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly maxChart = computed(() =>
    Math.max(1, ...(this.data()?.evolution ?? []).flatMap((item) => [item.income, item.expense])),
  );
  readonly expenseShare = computed(() =>
    this.data()?.monthlyIncome
      ? (this.data()!.monthlyExpense / this.data()!.monthlyIncome) * 100
      : 0,
  );

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.api.getDashboard().subscribe({
      next: (data) => {
        this.data.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Check that the API and database are available.');
        this.loading.set(false);
      },
    });
  }

  barHeight(value: number): number {
    return Math.max(3, (value / this.maxChart()) * 100);
  }

  monthName(month: number): string {
    return new Intl.DateTimeFormat('en', { month: 'short' }).format(
      new Date(2026, month - 1, 1),
    );
  }

  categoryPercent(value: number): number {
    return this.data()?.monthlyExpense ? (value / this.data()!.monthlyExpense) * 100 : 0;
  }
}
