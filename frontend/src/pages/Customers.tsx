import { useEffect, useState } from 'react';
import apiClient from '../services/apiClient';
import type { CustomerDto, ReminderNotificationDto } from '../types';
import { FeedbackAlert } from '../components/FeedbackAlert';

const Customers = () => {
  const [customers, setCustomers] = useState<CustomerDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    identityNumber: '',
    email: '',
    phoneNumber: ''
  });

  // Debt Check Modal State
  const [isDebtModalOpen, setIsDebtModalOpen] = useState(false);
  const [selectedCustomer, setSelectedCustomer] = useState<CustomerDto | null>(null);
  const [customerDebts, setCustomerDebts] = useState<ReminderNotificationDto[]>([]);
  const [debtLoading, setDebtLoading] = useState(false);

  useEffect(() => {
    fetchCustomers();
  }, []);

  const fetchCustomers = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get<CustomerDto[]>('/customers');
      setCustomers(response.data);
    } catch (err: any) {
      setError(err.message || 'Müşteriler yüklenemedi');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Bu müşteriyi silmek istediğinize emin misiniz?')) return;

    try {
      await apiClient.delete(`/customers/${id}`);
      setSuccess('Müşteri başarıyla silindi.');
      fetchCustomers();
    } catch (err: any) {
      setError(err.message || 'Müşteri silinirken bir hata oluştu');
    }
  };

  const handleCreate = async (e: React.SubmitEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);

    try {
      await apiClient.post('/customers', formData);
      setSuccess('Müşteri başarıyla eklendi.');
      setIsCreateModalOpen(false);
      setFormData({ firstName: '', lastName: '', identityNumber: '', email: '', phoneNumber: '' });
      fetchCustomers();
    } catch (err: any) {
      setError(err.message || 'Müşteri eklenemedi');
    }
  };

  const handleCheckDebt = async (customer: CustomerDto) => {
    setSelectedCustomer(customer);
    setIsDebtModalOpen(true);
    setDebtLoading(true);
    try {
      const response = await apiClient.get<ReminderNotificationDto[]>('/reminders/pending?thresholdDays=30');
      const filteredDebts = response.data.filter(r => r.customerId === customer.id);
      setCustomerDebts(filteredDebts);
    } catch (err: any) {
      setError(err.message || 'Borçlar sorgulanamadı');
    } finally {
      setDebtLoading(false);
    }
  };

  const totalDebtAmount = customerDebts.reduce((sum, item) => sum + item.debtAmount, 0);

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <h2>Müşteri Yönetimi</h2>
        <button
          onClick={() => setIsCreateModalOpen(true)}
          style={{ padding: '8px 16px', backgroundColor: '#0056b3', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}
        >
          + Müşteri Ekle
        </button>
      </div>

      {error && <FeedbackAlert type="error" message={error} onClose={() => setError(null)} />}
      {success && <FeedbackAlert type="success" message={success} onClose={() => setSuccess(null)} />}

      {loading ? (
        <p>Müşteriler yükleniyor...</p>
      ) : (
        <div className="fintech-table-container">
        <table className="fintech-table">
          <thead>
            <tr>
              <th>Ad Soyad</th>
              <th>TC Kimlik No</th>
              <th>E-Posta</th>
              <th>Telefon</th>
              <th>İşlemler</th>
            </tr>
          </thead>
          <tbody>
            {customers.map(c => (
              <tr key={c.id}>
                <td>{c.firstName} {c.lastName}</td>
                <td>{c.identityNumber}</td>
                <td>{c.email}</td>
                <td>{c.phoneNumber}</td>
                <td style={{ display: 'flex', gap: '5px' }}>
                  <button
                    onClick={() => handleCheckDebt(c)}
                    style={{ padding: '6px 12px', backgroundColor: 'var(--primary)', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontSize: '0.85rem' }}
                  >
                    Borç Sorgula
                  </button>
                  {/* Note: Edit button strictly omitted per business rules */}
                  <button
                    onClick={() => handleDelete(c.id)}
                    style={{ padding: '6px 12px', backgroundColor: '#dc3545', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontSize: '0.85rem' }}
                  >
                    Sil
                  </button>
                </td>
              </tr>
            ))}
            {customers.length === 0 && (
              <tr>
                <td colSpan={5} style={{ padding: '20px', textAlign: 'center', color: 'var(--text-muted)' }}>Hiç müşteri bulunamadı.</td>
              </tr>
            )}
          </tbody>
        </table>
        </div>
      )}

      {/* Create Modal */}
      {isCreateModalOpen && (
        <div style={{ position: 'fixed', top: 0, left: 0, right: 0, bottom: 0, backgroundColor: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div style={{ backgroundColor: 'white', padding: '30px', borderRadius: '8px', width: '400px' }}>
            <h3>Yeni Müşteri Ekle</h3>
            <form onSubmit={handleCreate} style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
              <div>
                <label style={{ display: 'block', marginBottom: '5px' }}>Ad</label>
                <input required value={formData.firstName} onChange={e => setFormData({ ...formData, firstName: e.target.value })} style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }} />
              </div>
              <div>
                <label style={{ display: 'block', marginBottom: '5px' }}>Soyad</label>
                <input required value={formData.lastName} onChange={e => setFormData({ ...formData, lastName: e.target.value })} style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }} />
              </div>
              <div>
                <label style={{ display: 'block', marginBottom: '5px' }}>TC Kimlik No (11 hane)</label>
                <input required pattern="\d{11}" title="Tam olarak 11 hane olmalıdır" value={formData.identityNumber} onChange={e => setFormData({ ...formData, identityNumber: e.target.value })} style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }} />
              </div>
              <div>
                <label style={{ display: 'block', marginBottom: '5px' }}>E-Posta</label>
                <input type="email" required value={formData.email} onChange={e => setFormData({ ...formData, email: e.target.value })} style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }} />
              </div>
              <div>
                <label style={{ display: 'block', marginBottom: '5px' }}>Telefon</label>
                <input required value={formData.phoneNumber} onChange={e => setFormData({ ...formData, phoneNumber: e.target.value })} style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }} />
              </div>
              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px', marginTop: '10px' }}>
                <button type="button" onClick={() => setIsCreateModalOpen(false)} style={{ padding: '8px 16px', cursor: 'pointer' }}>İptal</button>
                <button type="submit" style={{ padding: '8px 16px', backgroundColor: '#0056b3', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>Kaydet</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Debt Checking Modal */}
      {isDebtModalOpen && selectedCustomer && (
        <div style={{ position: 'fixed', top: 0, left: 0, right: 0, bottom: 0, backgroundColor: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div style={{ backgroundColor: 'white', padding: '30px', borderRadius: '8px', width: '600px', maxHeight: '80vh', overflowY: 'auto' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
              <h3 style={{ margin: 0 }}>Borç Sorgusu: {selectedCustomer.firstName} {selectedCustomer.lastName}</h3>
              <button onClick={() => { setIsDebtModalOpen(false); setSelectedCustomer(null); setCustomerDebts([]); }} style={{ border: 'none', background: 'none', fontSize: '1.5rem', cursor: 'pointer' }}>&times;</button>
            </div>

            {debtLoading ? (
              <p>Borçlar sorgulanıyor...</p>
            ) : customerDebts.length === 0 ? (
              <div style={{ padding: '20px', backgroundColor: '#d4edda', color: '#155724', borderRadius: '4px', textAlign: 'center', fontWeight: 'bold' }}>
                Bu müşterinin vadesi gelen borcu bulunmamaktadır.
              </div>
            ) : (
              <>
                <div style={{ padding: '15px', backgroundColor: '#f8d7da', color: '#721c24', borderRadius: '4px', marginBottom: '20px', fontSize: '1.2rem', textAlign: 'center' }}>
                  Toplam Borç: <strong>{totalDebtAmount.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })}</strong>
                </div>
                <table className="fintech-table" style={{ marginTop: '10px' }}>
                  <thead>
                    <tr>
                      <th>Tarih/Dönem</th>
                      <th>Abonelik Türü</th>
                      <th>Kurum</th>
                      <th style={{ textAlign: 'right' }}>Tutar</th>
                    </tr>
                  </thead>
                  <tbody>
                    {customerDebts.map(d => (
                      <tr key={d.subscriptionId}>
                        <td>{new Date(d.dueDate).toLocaleDateString('tr-TR')} ({d.period})</td>
                        <td>{d.subscriptionTypeName}</td>
                        <td>{d.serviceProviderName}</td>
                        <td style={{ textAlign: 'right', fontWeight: 'bold', color: 'var(--error-text)' }}>
                          {d.debtAmount.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </>
            )}

            <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: '20px' }}>
              <button onClick={() => { setIsDebtModalOpen(false); setSelectedCustomer(null); setCustomerDebts([]); }} style={{ padding: '8px 16px', backgroundColor: '#6c757d', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                Kapat
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Customers;
