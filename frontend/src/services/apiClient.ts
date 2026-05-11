import axios, { AxiosError, type AxiosResponse } from 'axios';
import type { ApiErrorResponse } from '../types';

// Configure standard backend URL matching the .NET WebAPI default port (e.g. 5165)
const apiClient = axios.create({
  baseURL: 'http://localhost:5165/api',
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000, // 10 seconds timeout
});

// Request Interceptor: Currently passes request through, but ready for Auth Tokens if needed.
apiClient.interceptors.request.use(
  (config) => {
    // e.g. config.headers.Authorization = `Bearer ${token}`
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response Interceptor: Globally catches standard HTTP errors from ExceptionHandlingMiddleware
apiClient.interceptors.response.use(
  (response: AxiosResponse) => {
    return response;
  },
  (error: AxiosError<ApiErrorResponse>) => {
    // Network errors (no response)
    if (!error.response) {
      console.error('Network Error:', error.message);
      return Promise.reject({
        message: 'Ağ hatası: Sunucuya ulaşılamıyor.',
      });
    }

    const { status, data } = error.response;
    const backendMessage = data?.message || 'Beklenmeyen bir hata oluştu.';

    // Specifically handle the Idempotency 409 Conflict rejection
    if (status === 409) {
      console.warn('Idempotency Blocked:', data);
      return Promise.reject({
        status,
        message: 'Bu dönem için başarılı bir ödeme zaten mevcut (Çifte Ödeme Koruması).',
        details: data?.details,
      });
    }

    // Standard mappings
    if (status === 400) {
      return Promise.reject({
        status,
        message: 'Girdi doğrulama hatası (Validation Failed).',
        details: data?.details, // Contains array of { field, error } from FluentValidation
      });
    }

    if (status === 404) {
      return Promise.reject({
        status,
        message: backendMessage, // E.g., 'Customer/Subscription not found'
      });
    }

    if (status === 500) {
      return Promise.reject({
        status,
        message: 'Sunucu tarafında kritik bir hata oluştu (500).',
      });
    }

    // Default fallback
    return Promise.reject({
      status,
      message: backendMessage,
      details: data?.details,
    });
  }
);

export default apiClient;
