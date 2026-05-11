import React from 'react';
import Sidebar from './Sidebar';

const Layout = ({ children }: { children: React.ReactNode }) => {
  return (
    <div style={{ display: 'flex', width: '100%', minHeight: '100vh', backgroundColor: 'var(--bg-color)' }}>
      <Sidebar />
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column', width: 'calc(100% - 250px)' }}>
        <header style={{ backgroundColor: 'white', padding: '20px 30px', boxShadow: 'var(--shadow-sm)', display: 'flex', justifyContent: 'space-between', alignItems: 'center', zIndex: 10 }}>
          <div style={{ fontSize: '1.25rem', fontWeight: 'bold', color: 'var(--primary)', display: 'flex', alignItems: 'center', gap: '10px' }}>
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><polygon points="12 2 22 8.5 22 15.5 12 22 2 15.5 2 8.5 12 2"/><line x1="12" y1="22" x2="12" y2="15.5"/><polyline points="22 8.5 12 15.5 2 8.5"/><polyline points="2 15.5 12 8.5 22 15.5"/><line x1="12" y1="2" x2="12" y2="8.5"/></svg>
            Architecht
          </div>
          <div style={{ fontWeight: '500', color: 'var(--text-muted)' }}>Admin Console</div>
        </header>
        <main style={{ flex: 1, padding: '40px', overflowY: 'auto' }}>
          {children}
        </main>
      </div>
    </div>
  );
};

export default Layout;
