import { useEffect, useState } from 'react';
import apiClient from '../services/apiClient';
import type { SubscriptionPaymentSummaryDto } from '../types';
import { FeedbackAlert } from '../components/FeedbackAlert';

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

  // Flatten the grouped data into a single array of payments and sort by date descending
  const allPayments = history.flatMap(sub =>
    sub.payments?.map(payment => ({
      ...payment,
      subscriptionTypeName: sub.subscriptionTypeName,
      isActive: sub.isActive
    })) ?? []
  ).sort((a, b) => new Date(b.paymentDate).getTime() - new Date(a.paymentDate).getTime());

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
              <th>Ödeme Tarihi</th>
              <th>Dönem</th>
              <th>Abonelik No</th>
              <th>Kurum</th>
              <th>Tutar</th>
              <th>Durum</th>
            </tr>
          </thead>
          <tbody>
            {allPayments.map(p => (
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
            {allPayments.length === 0 && (
              <tr>
                <td colSpan={6} style={{ padding: '20px', textAlign: 'center', color: 'var(--text-muted)' }}>Henüz ödeme geçmişi bulunmamaktadır.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>)}
    </div>
  );
};

export default PaymentHistory;
