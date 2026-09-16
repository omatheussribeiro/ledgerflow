import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Account, ApiResponse, Category, Dashboard, PagedResult, Transaction, TransactionType } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = `${environment.apiUrl}api`;
  private readonly http = inject(HttpClient);

  getDashboard(year?: number, month?: number): Observable<Dashboard> {
    let params = new HttpParams();
    if (year) params = params.set('year', year);
    if (month) params = params.set('month', month);
    return this.http
      .get<ApiResponse<Dashboard>>(`${this.baseUrl}/dashboard`, { params })
      .pipe(map((response) => response.data));
  }
  getAccounts(): Observable<Account[]> {
    return this.http
      .get<ApiResponse<Account[]>>(`${this.baseUrl}/accounts`)
      .pipe(map((response) => response.data));
  }
  createAccount(body: { name: string; type: number; initialBalance: number }): Observable<Account> {
    return this.http
      .post<ApiResponse<Account>>(`${this.baseUrl}/accounts`, body)
      .pipe(map((response) => response.data));
  }
  getCategories(): Observable<Category[]> {
    return this.http
      .get<ApiResponse<Category[]>>(`${this.baseUrl}/categories`)
      .pipe(map((response) => response.data));
  }
  createCategory(body: { name: string; type: TransactionType; color: string }): Observable<Category> {
    return this.http
      .post<ApiResponse<Category>>(`${this.baseUrl}/categories`, body)
      .pipe(map((response) => response.data));
  }
  getTransactions(filters: Record<string, string | number | undefined> = {}): Observable<PagedResult<Transaction>> {
    let params = new HttpParams();
    Object.entries(filters).forEach(([key, value]) => { if (value !== undefined && value !== '') params = params.set(key, value); });
    return this.http
      .get<ApiResponse<PagedResult<Transaction>>>(`${this.baseUrl}/transactions`, { params })
      .pipe(map((response) => response.data));
  }
  createTransaction(body: object): Observable<Transaction> {
    return this.http
      .post<ApiResponse<Transaction>>(`${this.baseUrl}/transactions`, body)
      .pipe(map((response) => response.data));
  }
}
