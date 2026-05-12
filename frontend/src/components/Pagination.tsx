import React from 'react';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  pageSize: number;
  totalItems: number;
  pageSizeOptions: number[];
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
}

export const Pagination: React.FC<PaginationProps> = ({
  currentPage,
  totalPages,
  pageSize,
  totalItems,
  pageSizeOptions,
  onPageChange,
  onPageSizeChange
}) => {
  return (
    <div style={{
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      padding: '15px 20px',
      backgroundColor: 'white',
      borderTop: '1px solid #eee',
      borderBottomLeftRadius: '8px',
      borderBottomRightRadius: '8px'
    }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: '15px', color: '#666', fontSize: '0.9rem' }}>
        <div>
          <label htmlFor="pageSize" style={{ marginRight: '8px' }}>Sayfa başına satır:</label>
          <select
            id="pageSize"
            value={pageSize}
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
            style={{ padding: '4px 8px', borderRadius: '4px', border: '1px solid #ddd', outline: 'none' }}
          >
            {pageSizeOptions.map(size => (
              <option key={size} value={size}>{size}</option>
            ))}
          </select>
        </div>
        <div>
          Toplam: <span style={{ fontWeight: '500', color: '#333' }}>{totalItems}</span> kayıt
        </div>
      </div>

      <div style={{ display: 'flex', alignItems: 'center', gap: '20px' }}>
        <div style={{ color: '#666', fontSize: '0.9rem' }}>
          Sayfa <span style={{ fontWeight: '500', color: '#333' }}>{currentPage}</span> / {totalPages}
        </div>
        <div style={{ display: 'flex', gap: '5px' }}>
          <button
            onClick={() => onPageChange(currentPage - 1)}
            disabled={currentPage === 1}
            style={{
              padding: '6px 12px',
              backgroundColor: currentPage === 1 ? '#f8f9fa' : 'white',
              color: currentPage === 1 ? '#adb5bd' : 'var(--primary)',
              border: '1px solid #ddd',
              borderRadius: '4px',
              cursor: currentPage === 1 ? 'not-allowed' : 'pointer',
              fontWeight: '500',
              transition: 'all 0.2s'
            }}
            onMouseOver={(e) => { if (currentPage !== 1) e.currentTarget.style.backgroundColor = '#f8f9fa'; }}
            onMouseOut={(e) => { if (currentPage !== 1) e.currentTarget.style.backgroundColor = 'white'; }}
          >
            Önceki
          </button>
          <button
            onClick={() => onPageChange(currentPage + 1)}
            disabled={currentPage === totalPages || totalPages === 0}
            style={{
              padding: '6px 12px',
              backgroundColor: currentPage === totalPages || totalPages === 0 ? '#f8f9fa' : 'white',
              color: currentPage === totalPages || totalPages === 0 ? '#adb5bd' : 'var(--primary)',
              border: '1px solid #ddd',
              borderRadius: '4px',
              cursor: currentPage === totalPages || totalPages === 0 ? 'not-allowed' : 'pointer',
              fontWeight: '500',
              transition: 'all 0.2s'
            }}
            onMouseOver={(e) => { if (currentPage !== totalPages && totalPages !== 0) e.currentTarget.style.backgroundColor = '#f8f9fa'; }}
            onMouseOut={(e) => { if (currentPage !== totalPages && totalPages !== 0) e.currentTarget.style.backgroundColor = 'white'; }}
          >
            Sonraki
          </button>
        </div>
      </div>
    </div>
  );
};
