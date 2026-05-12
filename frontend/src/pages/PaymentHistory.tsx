import { useEffect, useState, useMemo } from 'react';
import apiClient from '../services/apiClient';
import type { SubscriptionPaymentSummaryDto } from '../types';
import { FeedbackAlert } from '../components/FeedbackAlert';
import { useTableSorting } from '../hooks/useTableSorting';
import { Pagination } from '../components/Pagination';
import { SortableHeader } from '../components/SortableHeader';

const PaymentHistory = () => {
  const [history, setHistory] = useState<SubscriptionPaymentSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchHistory = async () => {
      try {
        setLoading(true);
        const response = await apiClient.get<SubscriptionPaymentSummaryDto[]>('/summary/payment-history');
        setHistory(response.data);
      } catch (err: any) {
        setError(err.message || 'Ödeme geçmişi yüklenemedi.');
      } finally {
        setLoading(false);
      }
    };
    fetchHistory();
  }, []);

  // Flatten the grouped data into a single array of payments
  const allPayments = useMemo(() => history.flatMap(sub =>
    sub.payments?.map(payment => ({
      ...payment,
      subscriptionTypeName: sub.subscriptionTypeName,
      isActive: sub.isActive
    })) ?? []
  ), [history]);

  const {
    paginatedData,
    currentPage,
    pageSize,
    totalPages,
    sortConfig,
    handleSort,
    handlePageChange,
    handlePageSizeChange,
    totalItems
  } = useTableSorting(allPayments, 'paymentDate', 'desc');

  return (
    <div>
      <h2 style={{ marginBottom: '20px' }}>Tüm Ödeme Geçmişi</h2>
      {error && <FeedbackAlert type="error" message={error} onClose={() => setError(null)} />}

      {loading ? (
        <p>Ödeme geçmişi yükleniyor...</p>
      ) : (
        <div className="fintech-table-container">
        <table className="fintech-table">
          <thead>
            <tr>
              <SortableHeader label="Ödeme Tarihi" sortKey="paymentDate" currentSortKey={sortConfig.key as string} currentDirection={sortConfig.direction} onSort={handleSort} />
              <SortableHeader label="Dönem" sortKey="period" currentSortKey={sortConfig.key as string} currentDirection={sortConfig.direction} onSort={handleSort} />
              <th>Abonelik No</th>
              <SortableHeader label="Kurum" sortKey="serviceProviderName" currentSortKey={sortConfig.key as string} currentDirection={sortConfig.direction} onSort={handleSort} />
              <SortableHeader label="Tutar" sortKey="amount" currentSortKey={sortConfig.key as string} currentDirection={sortConfig.direction} onSort={handleSort} />
              <th>Durum</th>
            </tr>
          </thead>
          <tbody>
            {paginatedData.map(p => (
              <tr key={p.id}>
                <td>{new Date(p.paymentDate).toLocaleString('tr-TR')}</td>
                <td>{p.period}</td>
                <td>{p.subscriptionNumber}</td>
                <td>{p.serviceProviderName}</td>
                <td style={{ fontWeight: 'bold' }}>
                  {p.amount.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })}
                </td>
                <td>
                  <span className={`status-pill ${p.isSuccessful ? 'status-success' : 'status-error'}`}>
                    {p.isSuccessful ? 'Başarılı' : 'Başarısız'}
                  </span>
                </td>
              </tr>
            ))}
            {paginatedData.length === 0 && (
              <tr>
                <td colSpan={6} style={{ padding: '20px', textAlign: 'center', color: 'var(--text-muted)' }}>Henüz ödeme geçmişi bulunmamaktadır.</td>
              </tr>
            )}
          </tbody>
        </table>
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          pageSize={pageSize}
          totalItems={totalItems}
          pageSizeOptions={[5, 10, 20, 50]}
          onPageChange={handlePageChange}
          onPageSizeChange={handlePageSizeChange}
        />
      </div>)}
    </div>
  );
};

export default PaymentHistory;
