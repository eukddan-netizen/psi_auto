import React, { useState } from 'react';
import { LogItem } from './LogItem';

const statusBadge = (status) => {
  switch (status) {
    case 'SUCCESS': return 'badge-success text-black';
    case 'FAIL': return 'badge-error';
    case 'RUNNING': return 'badge-info animate-pulse';
    default: return 'badge-ghost';
  }
};

export function TcCard({ tcId, data }) {
  const [open, setOpen] = useState(false);
  const { latest, history } = data;

  return (
    <div className="border border-base-300 rounded-lg bg-base-100">
      <button
        onClick={() => setOpen(!open)}
        className="w-full flex items-center justify-between gap-3 px-3 py-2 hover:bg-base-300/40 transition-colors"
      >
        <div className="flex items-center gap-3 min-w-0">
          <span className={`badge badge-sm font-bold ${statusBadge(latest.status)}`}>
            {latest.status}
          </span>
          <span className="font-mono font-bold text-secondary text-sm">{tcId}</span>
          <span className="text-sm text-base-content/70 truncate">{latest.message}</span>
        </div>
        <div className="flex items-center gap-2 shrink-0">
          <span className="text-xs text-base-content/40 font-mono">{history.length}건</span>
          <span className="text-xs">{open ? '▲' : '▼'}</span>
        </div>
      </button>

      {open && (
        <div className="border-t border-base-300 divide-y divide-base-300 max-h-72 overflow-y-auto">
          {history.map(log => <LogItem key={log.id} log={log} />)}
        </div>
      )}
    </div>
  );
}