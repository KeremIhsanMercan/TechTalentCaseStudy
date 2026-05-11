// Customer DTOs
export interface CustomerDto {
  id: string;
  firstName: string;
  lastName: string;
  identityNumber: string;
  email: string;
  phoneNumber: string;
  createdDate?: string;
}

// Subscription DTOs
export interface SubscriptionDto {
  id: string;
  customerId: string;
  customerFullName?: string;
  subscriptionType: string;
  subscriptionTypeName?: string;
  serviceProviderName: string;
  subscriptionNumber: string;
  isActive: boolean;
  currentDebtAmount: number;
  nextDueDate: string;
  createdDate?: string;
}

// Debt Checking / Reminders
export interface DebtQueryResultDto {
  debtAmount: number;
  dueDate: string;
  period: string;
}

export interface ReminderNotificationDto {
  subscriptionId: string;
  customerId: string;
  customerFullName: string;
  customerEmail: string;
  subscriptionTypeName: string;
  serviceProviderName: string;
  subscriptionNumber: string;
  debtAmount: number;
  dueDate: string;
  daysUntilDue: number;
  period: string;
  notificationMessage: string;
}

// Payments
export interface ProcessPaymentDto {
  subscriptionId: string;
  amount: number;
}

export interface PaymentResultDto {
  isSuccessful: boolean;
  transactionId: string;
  processedAt: string;
}

// Global API Error Response format from our ExceptionHandlingMiddleware
export interface ApiErrorResponse {
  error: string;
  message: string;
  details?: Array<{ field: string; error: string }> | null;
}

// Summary & History
export interface PaymentHistoryDto {
  id: string;
  subscriptionId: string;
  subscriptionNumber: string;
  serviceProviderName: string;
  amount: number;
  paymentDate: string;
  period: string;
  isSuccessful: boolean;
  createdDate: string;
}

export interface SubscriptionPaymentSummaryDto {
  subscriptionId: string;
  subscriptionTypeName: string;
  serviceProviderName: string;
  subscriptionNumber: string;
  isActive: boolean;
  payments: PaymentHistoryDto[];
}
