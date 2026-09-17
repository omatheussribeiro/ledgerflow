import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ApiService } from './api.service';

describe('ApiService', () => {
  let service: ApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ApiService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('requests the dashboard from the configured API with its optional period', () => {
    service.getDashboard(2026, 9).subscribe();

    const request = http.expectOne('http://localhost:5080/api/dashboard?year=2026&month=9');
    expect(request.request.method).toBe('GET');
    request.flush({
      success: true,
      message: 'Dashboard retrieved successfully.',
      data: {
        totalBalance: 0,
        monthlyIncome: 0,
        monthlyExpense: 0,
        monthlyResult: 0,
        evolution: [],
        expensesByCategory: [],
        latestTransactions: [],
      },
    });
  });

  it('omits empty transaction filters and sends pagination values', () => {
    service.getTransactions({ search: '', type: undefined, page: 2, pageSize: 10 }).subscribe();

    const request = http.expectOne(
      (candidate) =>
        candidate.url === 'http://localhost:5080/api/transactions' &&
        candidate.params.get('page') === '2' &&
        candidate.params.get('pageSize') === '10' &&
        !candidate.params.has('search') &&
        !candidate.params.has('type'),
    );
    expect(request.request.method).toBe('GET');
    request.flush({
      success: true,
      message: 'Transactions retrieved successfully.',
      data: { items: [], page: 2, pageSize: 10, totalCount: 0, totalPages: 0 },
    });
  });

  it('posts a new account to the configured API', () => {
    const account = { name: 'Checking', type: 1, initialBalance: 500 };
    service.createAccount(account).subscribe();

    const request = http.expectOne('http://localhost:5080/api/accounts');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(account);
    request.flush({
      success: true,
      message: 'Account created successfully.',
      data: { id: 'account-id', ...account, balance: 500, isActive: true },
    });
  });

  it('updates and deletes an account using its resource URL', () => {
    const account = { name: 'Updated checking', type: 1, initialBalance: 750 };
    service.updateAccount('account-id', account).subscribe();

    const update = http.expectOne('http://localhost:5080/api/accounts/account-id');
    expect(update.request.method).toBe('PUT');
    expect(update.request.body).toEqual(account);
    update.flush({
      success: true,
      message: 'Account updated successfully.',
      data: { id: 'account-id', ...account, balance: 750, isActive: true },
    });

    service.deleteAccount('account-id').subscribe();
    const deletion = http.expectOne('http://localhost:5080/api/accounts/account-id');
    expect(deletion.request.method).toBe('DELETE');
    deletion.flush(null);
  });

  it('updates and deletes a category using its resource URL', () => {
    const category = { name: 'Education', type: 2 as const, color: '#123456' };
    service.updateCategory('category-id', category).subscribe();

    const update = http.expectOne('http://localhost:5080/api/categories/category-id');
    expect(update.request.method).toBe('PUT');
    expect(update.request.body).toEqual(category);
    update.flush({
      success: true,
      message: 'Category updated successfully.',
      data: { id: 'category-id', ...category },
    });

    service.deleteCategory('category-id').subscribe();
    const deletion = http.expectOne('http://localhost:5080/api/categories/category-id');
    expect(deletion.request.method).toBe('DELETE');
    deletion.flush(null);
  });

  it('updates and deletes a transaction using its resource URL', () => {
    const transaction = {
      accountId: 'account-id',
      categoryId: 'category-id',
      type: 2,
      description: 'Updated expense',
      amount: 25,
      occurredOn: '2026-09-16',
      status: 2,
      notes: '',
    };
    service.updateTransaction('transaction-id', transaction).subscribe();

    const update = http.expectOne('http://localhost:5080/api/transactions/transaction-id');
    expect(update.request.method).toBe('PUT');
    expect(update.request.body).toEqual(transaction);
    update.flush({
      success: true,
      message: 'Transaction updated successfully.',
      data: {
        id: 'transaction-id',
        ...transaction,
        accountName: 'Checking',
        categoryName: 'Education',
        categoryColor: '#123456',
      },
    });

    service.deleteTransaction('transaction-id').subscribe();
    const deletion = http.expectOne('http://localhost:5080/api/transactions/transaction-id');
    expect(deletion.request.method).toBe('DELETE');
    deletion.flush(null);
  });
});
