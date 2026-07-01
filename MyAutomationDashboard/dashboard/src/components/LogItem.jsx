import React from 'react';

export function LogItem({ log }) {
  // 상태별 뱃지 컬러 지정
  const getStatusBadgeClass = (status) => {
    switch (status) {
      case 'SUCCESS': return 'badge-success text-black';
      case 'FAIL': return 'badge-error';
      case 'RUNNING': return 'badge-info animate-pulse';
      default: return 'badge-ghost';
    }
  };

  return (
    <div className="flex flex-col md:flex-row md:items-center justify-between gap-2 py-3 border-b border-base-300 hover:bg-base-300/40 transition-colors px-2">
      <div className="flex items-center gap-3">
        {/* 시간대 */}
        <span className="text-xs text-base-content/50 font-mono">{log.time}</span>
        
        {/* 상태 배지 */}
        <span className={`badge badge-sm font-bold tracking-wider ${getStatusBadgeClass(log.status)}`}>
          {log.status}
        </span>
        
        {/* 테스트 케이스 ID */}
        <span className="font-mono font-bold text-secondary text-sm">{log.testCaseId}</span>
      </div>
      
      {/* 로그 메시지 */}
      <div className="text-sm text-base-content/80 flex-1 md:text-right font-sans break-all">
        {log.message}
      </div>
    </div>
  );
}