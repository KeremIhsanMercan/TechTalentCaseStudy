import { Link, useLocation } from 'react-router-dom';
import { Home, Users, CreditCard, History } from 'lucide-react';

const Sidebar = () => {
  const location = useLocation();

  const navItems = [
    { path: '/', label: 'Özet (Dashboard)', icon: <Home size={20} /> },
    { path: '/customers', label: 'Müşteriler', icon: <Users size={20} /> },
    { path: '/subscriptions', label: 'Abonelikler', icon: <CreditCard size={20} /> },
    { path: '/payments', label: 'Ödeme Geçmişi', icon: <History size={20} /> },
  ];

  return (
    <div style={{ width: '250px', backgroundColor: 'var(--sidebar-bg)', color: 'white', padding: '30px 0', display: 'flex', flexDirection: 'column', boxShadow: 'var(--shadow-lg)', zIndex: 20 }}>
      <div style={{ padding: '0 25px', marginBottom: '40px', fontSize: '1.5rem', fontWeight: 'bold', letterSpacing: '1px', display: 'flex', alignItems: 'center', gap: '10px' }}>
        TechTalent
      </div>
      <nav style={{ display: 'flex', flexDirection: 'column', gap: '5px' }}>
        {navItems.map((item) => {
          const isActive = location.pathname === item.path;
          return (
            <Link
              key={item.path}
              to={item.path}
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '15px',
                padding: '12px 25px',
                color: isActive ? 'white' : '#94A3B8',
                textDecoration: 'none',
                backgroundColor: isActive ? 'var(--sidebar-hover)' : 'transparent',
                borderLeft: isActive ? '4px solid var(--primary)' : '4px solid transparent',
                transition: 'all 0.2s',
                fontWeight: isActive ? '600' : '400'
              }}
            >
              <span style={{ color: isActive ? 'var(--primary)' : '#94A3B8', transition: 'color 0.2s' }}>{item.icon}</span>
              {item.label}
            </Link>
          );
        })}
      </nav>
    </div>
  );
};

export default Sidebar;
