import React, { useState, useRef, useEffect, useMemo } from 'react';
import { Search, ChevronDown, X } from 'lucide-react';

interface Option {
  value: string;
  label: string;
  subLabel?: string; // e.g. Identity Number
}

interface SearchableSelectProps {
  options: Option[];
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  required?: boolean;
}

const SearchableSelect: React.FC<SearchableSelectProps> = ({
  options,
  value,
  onChange,
  placeholder = "Seçiniz...",
  required = false
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const containerRef = useRef<HTMLDivElement>(null);

  const selectedOption = useMemo(() => 
    options.find(opt => opt.value === value), 
  [options, value]);

  const filteredOptions = useMemo(() => {
    if (!searchTerm) return options;
    const lowerSearch = searchTerm.toLowerCase();
    return options.filter(opt => 
      opt.label.toLowerCase().includes(lowerSearch) || 
      (opt.subLabel && opt.subLabel.toLowerCase().includes(lowerSearch))
    );
  }, [options, searchTerm]);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleSelect = (val: string) => {
    onChange(val);
    setIsOpen(false);
    setSearchTerm('');
  };

  return (
    <div ref={containerRef} className="searchable-select-container" style={{ position: 'relative', width: '100%' }}>
      <input 
        type="hidden" 
        value={value} 
        required={required} 
        onChange={() => {}} // React controlled input warning fix
      />
      
      <div 
        className={`searchable-select-input-wrapper ${isOpen ? 'active' : ''}`}
        onClick={() => setIsOpen(!isOpen)}
        style={{
          display: 'flex',
          alignItems: 'center',
          padding: '8px 12px',
          border: '1px solid var(--border-color)',
          borderRadius: '6px',
          backgroundColor: 'white',
          cursor: 'pointer',
          transition: 'all 0.2s ease',
          boxShadow: isOpen ? '0 0 0 2px rgba(5, 150, 105, 0.2)' : 'none',
          borderColor: isOpen ? 'var(--primary)' : 'var(--border-color)',
          minHeight: '40px'
        }}
      >
        <div style={{ flex: 1, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
          {selectedOption ? (
            <span style={{ color: 'var(--text-main)', fontWeight: 500 }}>{selectedOption.label}</span>
          ) : (
            <span style={{ color: 'var(--text-muted)' }}>{placeholder}</span>
          )}
        </div>
        <ChevronDown size={18} style={{ color: 'var(--text-muted)', marginLeft: '8px', transform: isOpen ? 'rotate(180deg)' : 'none', transition: 'transform 0.2s' }} />
      </div>

      {isOpen && (
        <div 
          className="searchable-select-dropdown"
          style={{
            position: 'absolute',
            top: 'calc(100% + 4px)',
            left: 0,
            right: 0,
            backgroundColor: 'white',
            border: '1px solid var(--border-color)',
            borderRadius: '8px',
            boxShadow: 'var(--shadow-lg)',
            zIndex: 1010,
            overflow: 'hidden',
            animation: 'dropdownIn 0.2s ease-out'
          }}
        >
          <div style={{ padding: '8px', borderBottom: '1px solid var(--border-color)', display: 'flex', alignItems: 'center', gap: '8px', backgroundColor: '#f9fafb' }}>
            <Search size={16} style={{ color: 'var(--text-muted)' }} />
            <input
              autoFocus
              className="search-input"
              placeholder="İsim veya TCKN ile ara..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              onClick={(e) => e.stopPropagation()}
              style={{
                border: 'none',
                outline: 'none',
                width: '100%',
                fontSize: '0.9rem',
                padding: '4px 0',
                backgroundColor: 'transparent'
              }}
            />
            {searchTerm && (
              <X 
                size={16} 
                style={{ cursor: 'pointer', color: 'var(--text-muted)' }} 
                onClick={(e) => { e.stopPropagation(); setSearchTerm(''); }} 
              />
            )}
          </div>
          
          <div style={{ maxHeight: '250px', overflowY: 'auto', scrollbarWidth: 'thin' }}>
            {filteredOptions.length > 0 ? (
              filteredOptions.map(opt => (
                <div
                  key={opt.value}
                  className={`option-item ${value === opt.value ? 'selected' : ''}`}
                  onClick={(e) => { e.stopPropagation(); handleSelect(opt.value); }}
                  style={{
                    padding: '10px 12px',
                    cursor: 'pointer',
                    transition: 'all 0.2s',
                    backgroundColor: value === opt.value ? 'var(--success-bg)' : 'transparent',
                    borderLeft: value === opt.value ? '3px solid var(--primary)' : '3px solid transparent'
                  }}
                  onMouseEnter={(e) => {
                    if (value !== opt.value) e.currentTarget.style.backgroundColor = 'var(--table-row-hover)';
                  }}
                  onMouseLeave={(e) => {
                    if (value !== opt.value) e.currentTarget.style.backgroundColor = 'transparent';
                  }}
                >
                  <div style={{ fontWeight: 500, color: value === opt.value ? 'var(--success-text)' : 'var(--text-main)' }}>{opt.label}</div>
                  {opt.subLabel && <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', marginTop: '2px' }}>{opt.subLabel}</div>}
                </div>
              ))
            ) : (
              <div style={{ padding: '24px 12px', textAlign: 'center', color: 'var(--text-muted)', fontSize: '0.9rem' }}>
                <div style={{ marginBottom: '4px' }}>🔍</div>
                Müşteri bulunamadı
              </div>
            )}
          </div>
        </div>
      )}

      <style>{`
        @keyframes dropdownIn {
          from { opacity: 0; transform: translateY(-8px); }
          to { opacity: 1; transform: translateY(0); }
        }
        .search-input::placeholder { color: var(--text-muted); opacity: 0.6; }
        /* Custom scrollbar for better premium look */
        .searchable-select-dropdown div::-webkit-scrollbar {
          width: 6px;
        }
        .searchable-select-dropdown div::-webkit-scrollbar-track {
          background: #f1f1f1;
        }
        .searchable-select-dropdown div::-webkit-scrollbar-thumb {
          background: #cbd5e1;
          border-radius: 3px;
        }
        .searchable-select-dropdown div::-webkit-scrollbar-thumb:hover {
          background: #94a3b8;
        }
      `}</style>
    </div>
  );
};

export default SearchableSelect;
