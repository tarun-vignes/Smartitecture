import React from 'react';

const HandIcon = ({ size = 200, className = "" }) => {
  return (
    <svg 
      width={size} 
      height={size * 0.6} 
      viewBox="0 0 200 120" 
      fill="none" 
      xmlns="http://www.w3.org/2000/svg"
      className={className}
    >
      <defs>
        <linearGradient id="handGradientFeature" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" style={{stopColor:'#6ee7b7', stopOpacity:1}} />
          <stop offset="30%" style={{stopColor:'#4ade80', stopOpacity:1}} />
          <stop offset="70%" style={{stopColor:'#22c55e', stopOpacity:1}} />
          <stop offset="100%" style={{stopColor:'#16a34a', stopOpacity:1}} />
        </linearGradient>
      </defs>
      
      {/* Hand matching the user's image - scaled up */}
      <g transform="translate(50, 20) scale(2.5)">
        {/* Wrist/base */}
        <rect x="4" y="28" width="16" height="8" rx="2" 
              fill="url(#handGradientFeature)" stroke="#059669" strokeWidth="1"/>
        
        {/* Thumb (leftmost, angled) */}
        <ellipse cx="2" cy="20" rx="2.5" ry="6" transform="rotate(-30 2 20)" 
                 fill="url(#handGradientFeature)" stroke="#059669" strokeWidth="1"/>
        
        {/* Index finger */}
        <rect x="4" y="8" width="3" height="20" rx="1.5" 
              fill="url(#handGradientFeature)" stroke="#059669" strokeWidth="1"/>
        
        {/* Middle finger (tallest) */}
        <rect x="8" y="4" width="3" height="24" rx="1.5" 
              fill="url(#handGradientFeature)" stroke="#059669" strokeWidth="1"/>
        
        {/* Ring finger */}
        <rect x="12" y="8" width="3" height="20" rx="1.5" 
              fill="url(#handGradientFeature)" stroke="#059669" strokeWidth="1"/>
        
        {/* Pinky (shortest) */}
        <rect x="16" y="12" width="3" height="16" rx="1.5" 
              fill="url(#handGradientFeature)" stroke="#059669" strokeWidth="1"/>
        
        {/* Circuit patterns on fingers */}
        <line x1="5.5" y1="10" x2="5.5" y2="26" stroke="#10b981" strokeWidth="1.5" strokeLinecap="round"/>
        <line x1="9.5" y1="6" x2="9.5" y2="26" stroke="#10b981" strokeWidth="1.5" strokeLinecap="round"/>
        <line x1="13.5" y1="10" x2="13.5" y2="26" stroke="#10b981" strokeWidth="1.5" strokeLinecap="round"/>
        <line x1="17.5" y1="14" x2="17.5" y2="26" stroke="#10b981" strokeWidth="1.5" strokeLinecap="round"/>
        
        {/* Circuit nodes */}
        <circle cx="5.5" cy="14" r="1.2" fill="#34d399"/>
        <circle cx="9.5" cy="12" r="1.2" fill="#34d399"/>
        <circle cx="13.5" cy="14" r="1.2" fill="#34d399"/>
        <circle cx="17.5" cy="18" r="1.2" fill="#34d399"/>
        
        {/* Connecting circuit lines */}
        <path d="M5.5 14 L9.5 12 L13.5 14 L17.5 18" stroke="#6ee7b7" strokeWidth="1" fill="none"/>
        
        {/* Palm circuits */}
        <circle cx="8" cy="30" r="1" fill="#34d399"/>
        <circle cx="12" cy="30" r="1" fill="#34d399"/>
        <path d="M8 30 L12 30" stroke="#6ee7b7" strokeWidth="1" fill="none"/>
        
        {/* Thumb circuit */}
        <circle cx="2" cy="20" r="0.8" fill="#34d399"/>
        <path d="M2 20 L5.5 14" stroke="#6ee7b7" strokeWidth="0.8" fill="none"/>
      </g>
    </svg>
  );
};

export default HandIcon;
