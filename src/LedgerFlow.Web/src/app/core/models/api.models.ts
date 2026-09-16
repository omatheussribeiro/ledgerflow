export type TransactionType = 1 | 2;
export type TransactionStatus = 1 | 2 | 3;

export interface ApiResponse<T> { success: true; message: string; data: T }
export interface ApiMessageResponse { success: true; message: string }
export interface User { id: string; name: string; email: string; role: string }
export interface AuthResponse { accessToken: string; refreshToken: string; expiresAt: string; user: User }
export interface Account { id: string; name: string; type: number; initialBalance: number; balance: number; isActive: boolean }
export interface Category { id: string; name: string; type: TransactionType; color: string }
export interface Transaction {
  id: string; accountId: string; accountName: string; categoryId: string; categoryName: string;
  categoryColor: string; type: TransactionType; description: string; amount: number;
  occurredOn: string; status: TransactionStatus; notes?: string;
}
export interface PagedResult<T> { items: T[]; page: number; pageSize: number; totalCount: number; totalPages: number }
export interface MonthlyPoint { year: number; month: number; income: number; expense: number }
export interface CategoryTotal { categoryId: string; name: string; color: string; total: number }
export interface Dashboard {
  totalBalance: number; monthlyIncome: number; monthlyExpense: number; monthlyResult: number;
  evolution: MonthlyPoint[]; expensesByCategory: CategoryTotal[]; latestTransactions: Transaction[];
}
export interface ProblemDetails {
  success?: false;
  title?: string;
  detail?: string;
  message?: string;
  code?: string;
  errors?: Record<string, string[]>;
  traceId?: string;
}
