import { useEffect, useState } from 'react';
import apiClient from '../services/apiClient';
import type { SubscriptionDto, CustomerDto, PaymentHistoryDto } from '../types';
import { FeedbackAlert } from '../components/FeedbackAlert';

const subscriptionTypes = ['Elektrik', 'Su', 'Doğalgaz', 'İnternet', 'CepTelefonu', 'Televizyon', 'Sigorta', 'KrediKartı', 'Kira', 'Aidat', 'Diğer'];

const Subscriptions = () => {
  const [subscriptions, setSubscriptions] = useState<SubscriptionDto[]>([]);
  const [customers, setCustomers] = useState<CustomerDto[]>([]);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isEditMode, setIsEditMode] = useState(false);

  const [formData, setFormData] = useState({
    id: '',
    customerId: '',
    subscriptionType: 'Elektrik',
    serviceProviderName: '',
    subscriptionNumber: '',
    isActive: true
  });

  // History Modal State
  const [isHistoryModalOpen, setIsHistoryModalOpen] = useState(false);
  const [selectedSubscription, setSelectedSubscription] = useState<SubscriptionDto | null>(null);
  const [paymentHistory, setPaymentHistory] = useState<PaymentHistoryDto[]>([]);
  const [historyLoading, setHistoryLoading] = useState(false);

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setLoading(true);
      const [subsRes, custRes] = await Promise.all([
        apiClient.get<SubscriptionDto[]>('/subscriptions'),
        apiClient.get<CustomerDto[]>('/customers')
      ]);
      setSubscriptions(subsRes.data);
      setCustomers(custRes.data);
    } catch (err: any) {
      setError(err.message || 'Veriler yüklenemedi');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Bu aboneliği silmek istediğinize emin misiniz?')) return;

    try {
      await apiClient.delete(`/subscriptions/${id}`);
      setSuccess('Abonelik başarıyla silindi.');
      fetchData();
    } catch (err: any) {
      setError(err.message || 'Abonelik silinirken bir hata oluştu');
    }
  };

  const openCreateModal = () => {
    setIsEditMode(false);
    setFormData({ id: '', customerId: '', subscriptionType: 'Elektrik', serviceProviderName: '', subscriptionNumber: '', isActive: true });
    setIsModalOpen(true);
  };

  const openEditModal = (sub: SubscriptionDto) => {
    setIsEditMode(true);
    setFormData({
      id: sub.id,
      customerId: sub.customerId,
      subscriptionType: sub.subscriptionType,
      serviceProviderName: sub.serviceProviderName,
      subscriptionNumber: sub.subscriptionNumber,
      isActive: sub.isActive
    });
    setIsModalOpen(true);
  };

  const handleHistory = async (sub: SubscriptionDto) => {
    setSelectedSubscription(sub);
    setIsHistoryModalOpen(true);
    setHistoryLoading(true);
    try {
      const response = await apiClient.get<PaymentHistoryDto[]>(`/payments/subscription/${sub.id}`);
      // Sort history date descending
      const sorted = response.data.sort((a, b) => new Date(b.paymentDate).getTime() - new Date(a.paymentDate).getTime());
      setPaymentHistory(sorted);
    } catch (err: any) {
      setError(err.message || 'Ödeme geçmişi yüklenemedi');
    } finally {
      setHistoryLoading(false);
    }
  };

  const handleSubmit = async (e: React.SubmitEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);

    try {
      if (isEditMode) {
        await apiClient.put(`/subscriptions/${formData.id}`, formData);
        setSuccess('Abonelik güncellendi.');
      } else {
        await apiClient.post('/subscriptions', formData);
        setSuccess('Abonelik başarıyla oluşturuldu.');
      }
      setIsModalOpen(false);
      fetchData();
    } catch (err: any) {
      setError(err.message || 'İşlem başarısız oldu');
    }
  };

  const getCustomerName = (id: string) => {
    const c = customers.find(x => x.id === id);
    return c ? `${c.firstName} ${c.lastName}` : 'Bilinmeyen Müşteri';
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <h2>Abonelik Yönetimi</h2>
        <button
          onClick={openCreateModal}
          style={{ padding: '8px 16px', backgroundColor: '#0056b3', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}
        >
          + Abonelik Ekle
        </button>
      </div>

      {error && <FeedbackAlert type="error" message={error} onClose={() => setError(null)} />}
      {success && <FeedbackAlert type="success" message={success} onClose={() => setSuccess(null)} />}

      {loading ? (
        <p>Abonelikler yükleniyor...</p>
      ) : (
        <div className="fintech-table-container">
          <table className="fintech-table">
            <thead>
              <tr>
                <th>Müşteri</th>
                <th>Tip</th>
                <th>Kurum</th>
                <th>Abonelik No</th>
                <th>Güncel Borç</th>
                <th>Son Ödeme Tarihi</th>
                <th>Durum</th>
                <th>İşlemler</th>
              </tr>
            </thead>
            <tbody>
              {subscriptions.map(s => (
                <tr key={s.id}>
                  <td>{getCustomerName(s.customerId)}</td>
                  <td>{s.subscriptionType}</td>
                  <td>{s.serviceProviderName}</td>
                  <td>{s.subscriptionNumber}</td>
                  <td style={{ fontWeight: 'bold', color: s.currentDebtAmount > 0 ? 'var(--error-text)' : 'var(--success-text)' }}>
                    {s.currentDebtAmount.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })}
                  </td>
                  <td>{new Date(s.nextDueDate).toLocaleDateString('tr-TR')}</td>
                  <td>
                    <span className={`status-pill ${s.isActive ? 'status-success' : 'status-error'}`}>
                      {s.isActive ? 'Aktif' : 'Pasif'}
                    </span>
                  </td>
                  <td style={{ display: 'flex', gap: '5px' }}>
                    <button
                      onClick={() => handleHistory(s)}
                      style={{ padding: '6px 12px', backgroundColor: 'var(--primary)', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontSize: '0.85rem' }}
                    >
                      Geçmiş
                    </button>
                    <button
                      onClick={() => openEditModal(s)}
                      style={{ padding: '6px 12px', backgroundColor: '#eab308', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontSize: '0.85rem' }}
                    >
                      Düzenle
                    </button>
                    <button
                      onClick={() => handleDelete(s.id)}
                      style={{ padding: '6px 12px', backgroundColor: '#dc3545', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontSize: '0.85rem' }}
                    >
                      Sil
                    </button>
                  </td>
                </tr>
              ))}
              {subscriptions.length === 0 && (
                <tr>
                  <td colSpan={8} style={{ padding: '20px', textAlign: 'center', color: 'var(--text-muted)' }}>Hiç abonelik bulunamadı.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* Create/Edit Modal */}
      {isModalOpen && (
        <div style={{ position: 'fixed', top: 0, left: 0, right: 0, bottom: 0, backgroundColor: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div style={{ backgroundColor: 'white', padding: '30px', borderRadius: '8px', width: '400px' }}>
            <h3>{isEditMode ? 'Abonelik Düzenle' : 'Yeni Abonelik Ekle'}</h3>
            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>

              {!isEditMode && (
                <div>
                  <label style={{ display: 'block', marginBottom: '5px' }}>Müşteri</label>
                  <select required value={formData.customerId} onChange={e => setFormData({ ...formData, customerId: e.target.value })} style={{ width: '100%', padding: '8px' }}>
                    <option value="">Bir müşteri seçin...</option>
                    {customers.map(c => (
                      <option key={c.id} value={c.id}>{c.firstName} {c.lastName} ({c.identityNumber})</option>
                    ))}
                  </select>
                </div>
              )}

              <div>
                <label style={{ display: 'block', marginBottom: '5px' }}>Tipi</label>
                <select required value={formData.subscriptionType} onChange={e => setFormData({ ...formData, subscriptionType: e.target.value })} style={{ width: '100%', padding: '8px' }}>
                  {subscriptionTypes.map(type => (
                    <option key={type} value={type}>{type}</option>
                  ))}
                </select>
              </div>

              <div>
                <label style={{ display: 'block', marginBottom: '5px' }}>Kurum Adı</label>
                <input required value={formData.serviceProviderName} onChange={e => setFormData({ ...formData, serviceProviderName: e.target.value })} style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }} />
              </div>

              <div>
                <label style={{ display: 'block', marginBottom: '5px' }}>Abonelik No (Örn: Abone Numarası)</label>
                <input required value={formData.subscriptionNumber} onChange={e => setFormData({ ...formData, subscriptionNumber: e.target.value })} style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }} />
              </div>

              {isEditMode && (
                <div>
                  <label style={{ display: 'flex', alignItems: 'center', gap: '8px', cursor: 'pointer' }}>
                    <input type="checkbox" checked={formData.isActive} onChange={e => setFormData({ ...formData, isActive: e.target.checked })} />
                    Aktif
                  </label>
                </div>
              )}

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px', marginTop: '10px' }}>
                <button type="button" onClick={() => setIsModalOpen(false)} style={{ padding: '8px 16px', cursor: 'pointer' }}>İptal</button>
                <button type="submit" style={{ padding: '8px 16px', backgroundColor: '#0056b3', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>Kaydet</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* History Modal */}
      {isHistoryModalOpen && selectedSubscription && (
        <div style={{ position: 'fixed', top: 0, left: 0, right: 0, bottom: 0, backgroundColor: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div style={{ backgroundColor: 'white', padding: '30px', borderRadius: '8px', width: '600px', maxHeight: '80vh', overflowY: 'auto' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
              <h3 style={{ margin: 0 }}>Ödeme Geçmişi: {selectedSubscription.subscriptionNumber}</h3>
              <button onClick={() => { setIsHistoryModalOpen(false); setSelectedSubscription(null); setPaymentHistory([]); }} style={{ border: 'none', background: 'none', fontSize: '1.5rem', cursor: 'pointer' }}>&times;</button>
            </div>

            {historyLoading ? (
              <p>Ödeme geçmişi yükleniyor...</p>
            ) : paymentHistory.length === 0 ? (
              <div style={{ padding: '20px', backgroundColor: '#e9ecef', color: '#495057', borderRadius: '4px', textAlign: 'center' }}>
                Bu abonelik için hiç ödeme kaydı bulunamadı.
              </div>
            ) : (
              <table className="fintech-table" style={{ marginTop: '10px' }}>
                <thead>
                  <tr>
                    <th>Tarih</th>
                    <th>Dönem</th>
                    <th style={{ textAlign: 'right' }}>Tutar</th>
                    <th style={{ textAlign: 'center' }}>Durum</th>
                  </tr>
                </thead>
                <tbody>
                  {paymentHistory.map(p => (
                    <tr key={p.id}>
                      <td>{new Date(p.paymentDate).toLocaleString('tr-TR')}</td>
                      <td>{p.period}</td>
                      <td style={{ textAlign: 'right', fontWeight: 'bold' }}>
                        {p.amount.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })}
                      </td>
                      <td style={{ textAlign: 'center' }}>
                        <span className={`status-pill ${p.isSuccessful ? 'status-success' : 'status-error'}`}>
                          {p.isSuccessful ? 'Başarılı' : 'Başarısız'}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}

            <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: '20px' }}>
              <button onClick={() => { setIsHistoryModalOpen(false); setSelectedSubscription(null); setPaymentHistory([]); }} style={{ padding: '8px 16px', backgroundColor: '#6c757d', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                Kapat
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Subscriptions;
