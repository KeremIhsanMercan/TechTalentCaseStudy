import React from 'react';
import type { SortDirection } from '../hooks/useTableSorting';

interface SortableHeaderProps {
  label: string;
  sortKey: string;
  currentSortKey: string;
  currentDirection: SortDirection;
  onSort: (key: any) => void;
  align?: 'left' | 'center' | 'right';
  as?: 'th' | 'div';
}

export const SortableHeader: React.FC<SortableHeaderProps> = ({
  label,
  sortKey,
  currentSortKey,
  currentDirection,
  onSort,
  align = 'left',
  as = 'th'
}) => {
  const isActive = currentSortKey === sortKey;

  const Component = as;

  return (
    <Component
      onClick={() => onSort(sortKey)}
      style={{
        cursor: 'pointer',
        userSelect: 'none',
        textAlign: align,
        transition: 'background-color 0.2s',
        position: 'relative'
      }}
      title={`${label} alanına göre sırala`}
      onMouseOver={(e) => e.currentTarget.style.backgroundColor = 'rgba(0,0,0,0.02)'}
      onMouseOut={(e) => e.currentTarget.style.backgroundColor = 'transparent'}
    >
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: align === 'right' ? 'flex-end' : align === 'center' ? 'center' : 'flex-start', gap: '5px' }}>
        <span>{label}</span>
        <span style={{ display: 'inline-flex', flexDirection: 'column', fontSize: '0.6rem', color: isActive ? 'var(--primary)' : '#ccc' }}>
          <span style={{ opacity: isActive && currentDirection === 'asc' ? 1 : 0.3 }}>▲</span>
          <span style={{ opacity: isActive && currentDirection === 'desc' ? 1 : 0.3, marginTop: '-2px' }}>▼</span>
        </span>
      </div>
    </Component>
  );
};
