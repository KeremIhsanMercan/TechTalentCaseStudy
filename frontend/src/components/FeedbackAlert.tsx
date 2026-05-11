import React from 'react';

interface AlertProps {
  type: 'success' | 'error' | 'info';
  message: string;
  onClose?: () => void;
}

export const FeedbackAlert: React.FC<AlertProps> = ({ type, message, onClose }) => {
  const colors = {
    success: { bg: '#d4edda', color: '#155724', border: '#c3e6cb' },
    error: { bg: '#f8d7da', color: '#721c24', border: '#f5c6cb' },
    info: { bg: '#cce5ff', color: '#004085', border: '#b8daff' },
  };

  const style = colors[type];

  return (
    <div style={{
      backgroundColor: style.bg,
      color: style.color,
      border: `1px solid ${style.border}`,
      padding: '10px 15px',
      borderRadius: '4px',
      marginBottom: '15px',
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center'
    }}>
      <span>{message}</span>
      {onClose && (
        <button 
          onClick={onClose} 
          style={{ background: 'none', border: 'none', cursor: 'pointer', fontSize: '1.2rem', color: style.color }}
        >
          &times;
        </button>
      )}
    </div>
  );
};
