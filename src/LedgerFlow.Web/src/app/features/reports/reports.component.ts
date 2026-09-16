import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ApiService } from '../../core/api/api.service';
import { Dashboard } from '../../core/models/api.models';

@Component({
  selector: 'app-reports',
  imports: [CurrencyPipe, DecimalPipe],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReportsComponent {
  private readonly api = inject(ApiService);

  readonly data = signal<Dashboard | null>(null);
  readonly savingsRate = computed(() =>
    this.data()?.monthlyIncome
      ? (this.data()!.monthlyResult / this.data()!.monthlyIncome) * 100
      : 0,
  );
  readonly boundedSavingsRate = computed(() => Math.max(0, Math.min(100, this.savingsRate())));
  readonly averageIncome = computed(() =>
    this.average(this.data()?.evolution.map((item) => item.income) ?? []),
  );
  readonly averageExpense = computed(() =>
    this.average(this.data()?.evolution.map((item) => item.expense) ?? []),
  );

  constructor() {
    this.api.getDashboard().subscribe((data) => this.data.set(data));
  }

  categoryPercent(value: number): number {
    return this.data()?.monthlyExpense ? (value / this.data()!.monthlyExpense) * 100 : 0;
  }

  private average(values: number[]): number {
    return values.length ? values.reduce((sum, value) => sum + value, 0) / values.length : 0;
  }
}
