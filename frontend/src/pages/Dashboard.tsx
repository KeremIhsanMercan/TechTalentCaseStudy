import { useEffect, useState } from 'react';
import apiClient from '../services/apiClient';
import type { ReminderNotificationDto, SubscriptionDto } from '../types';
import { FeedbackAlert } from '../components/FeedbackAlert';

const Dashboard = () => {
  const [reminders, setReminders] = useState<ReminderNotificationDto[]>([]);
  const [activeCount, setActiveCount] = useState<number>(0);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [processingId, setProcessingId] = useState<string | null>(null);

  const fetchDashboardData = async (showLoading: boolean = true) => {
    try {
      setLoading(showLoading);
      setError(null);
      // Fetch reminders with a wide threshold to catch all current period unpaid active subs
      const [remindersRes, activeSubsRes] = await Promise.all([
        apiClient.get<ReminderNotificationDto[]>('/reminders/pending?thresholdDays=30'),
        apiClient.get<SubscriptionDto[]>('/summary/active-subscriptions')
      ]);

      setReminders(remindersRes.data);
      setActiveCount(activeSubsRes.data.length);
    } catch (err: any) {
      setError(err.message || 'Özet verileri yüklenemedi');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDashboardData(true);
  }, []);

  const handlePayNow = async (reminder: ReminderNotificationDto) => {
    setProcessingId(reminder.subscriptionId);
    setError(null);
    setSuccess(null);

    try {
      // ZERO MANUAL ENTRY: Send the exact amount dynamically from the reminder
      const response = await apiClient.post('/payments/process', {
        subscriptionId: reminder.subscriptionId,
        amount: reminder.debtAmount
      });

      setSuccess(`Ödeme başarılı! İşlem No: ${response.data.transactionId}`);
      // Refresh to clear the paid reminder
      fetchDashboardData(false);
    } catch (err: any) {
      setError(err.message || 'Ödeme işlemi başarısız oldu');
    } finally {
      setProcessingId(null);
    }
  };

  return (
    <div>
      <h2 style={{ marginBottom: '20px' }}>Özet (Dashboard)</h2>

      {error && <FeedbackAlert type="error" message={error} onClose={() => setError(null)} />}
      {success && <FeedbackAlert type="success" message={success} onClose={() => setSuccess(null)} />}

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '20px', marginBottom: '40px' }}>
        <div style={{ backgroundColor: 'white', padding: '25px', borderRadius: '12px', boxShadow: '0 4px 6px rgba(0,0,0,0.05)', border: '1px solid #eee' }}>
          <h3 style={{ margin: '0 0 10px 0', color: '#666', fontSize: '0.9rem', textTransform: 'uppercase', letterSpacing: '0.05em' }}>Aktif Abonelikler</h3>
          <p style={{ margin: 0, fontSize: '2rem', fontWeight: 'bold', color: '#333' }}>
            {loading ? '-' : activeCount}
          </p>
        </div>
        <div style={{ backgroundColor: 'white', padding: '25px', borderRadius: '12px', boxShadow: '0 4px 6px rgba(0,0,0,0.05)', border: '1px solid #eee' }}>
          <h3 style={{ margin: '0 0 10px 0', color: '#666', fontSize: '0.9rem', textTransform: 'uppercase', letterSpacing: '0.05em' }}>Bekleyen Ödemeler</h3>
          <p style={{ margin: 0, fontSize: '2rem', fontWeight: 'bold', color: '#dc3545' }}>
            {loading ? '-' : reminders.length}
          </p>
        </div>
      </div>

      <h3 style={{ marginBottom: '15px' }}>Bekleyen Borçlar ve Hatırlatmalar</h3>

      {loading ? (
        <p>Hatırlatmalar yükleniyor...</p>
      ) : reminders.length === 0 ? (
        <div style={{ backgroundColor: 'white', padding: '30px', borderRadius: '12px', textAlign: 'center', color: '#28a745', boxShadow: '0 4px 6px rgba(0,0,0,0.05)', border: '1px solid #eee' }}>
          Bu dönem için tüm abonelikleriniz ödenmiştir. Tebrikler!
        </div>
      ) : (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(350px, 1fr))', gap: '20px' }}>
          {reminders.map(reminder => (
            <div key={reminder.subscriptionId} style={{ backgroundColor: 'white', borderRadius: '12px', boxShadow: '0 4px 6px rgba(0,0,0,0.05)', overflow: 'hidden', border: '1px solid #eee', display: 'flex', flexDirection: 'column' }}>
              <div style={{ backgroundColor: '#f8f9fa', padding: '15px 20px', borderBottom: '1px solid #eee' }}>
                <h4 style={{ margin: 0, color: '#333', fontSize: '1.1rem' }}>{reminder.serviceProviderName}</h4>
                <p style={{ margin: '5px 0 0 0', color: '#666', fontSize: '0.85rem' }}>{reminder.subscriptionTypeName}</p>
              </div>

              <div style={{ padding: '20px', flex: 1 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '15px' }}>
                  <span style={{ color: '#666' }}>Son Ödeme</span>
                  <span style={{ fontWeight: '500' }}>{reminder.daysUntilDue} gün kaldı</span>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '20px', paddingBottom: '15px', borderBottom: '1px dashed #eee' }}>
                  <span style={{ color: '#666' }}>Borç Tutarı</span>
                  <span style={{ fontWeight: 'bold', fontSize: '1.2rem', color: '#dc3545' }}>
                    {reminder.debtAmount.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })}
                  </span>
                </div>

                <p style={{ fontSize: '0.9rem', color: '#856404', backgroundColor: '#fff3cd', padding: '10px', borderRadius: '6px', margin: '0 0 20px 0' }}>
                  "{reminder.notificationMessage}"
                </p>
              </div>

              <div style={{ padding: '15px 20px', backgroundColor: '#fdfdfd', borderTop: '1px solid #eee' }}>
                <button
                  onClick={() => handlePayNow(reminder)}
                  disabled={processingId === reminder.subscriptionId}
                  style={{
                    width: '100%',
                    padding: '12px',
                    backgroundColor: processingId === reminder.subscriptionId ? '#6c757d' : '#28a745',
                    color: 'white',
                    border: 'none',
                    borderRadius: '6px',
                    cursor: processingId === reminder.subscriptionId ? 'not-allowed' : 'pointer',
                    fontWeight: 'bold',
                    transition: 'background-color 0.2s'
                  }}
                  onMouseOver={(e) => { if (processingId !== reminder.subscriptionId) e.currentTarget.style.backgroundColor = '#218838'; }}
                  onMouseOut={(e) => { if (processingId !== reminder.subscriptionId) e.currentTarget.style.backgroundColor = '#28a745'; }}
                >
                  {processingId === reminder.subscriptionId ? 'İşleniyor...' : 'Ödeme Yap'}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Dashboard;
