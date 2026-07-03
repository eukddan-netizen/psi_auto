import React, { useState } from 'react';
import { TcCard } from './TcCard';

export function CategorySection({ category, tcMap }) {
  const [open, setOpen] = useState(true);

  const tcEntries = [...tcMap.entries()].sort(([a], [b]) => a.localeCompare(b));
  const successCount = tcEntries.filter(([, d]) => d.latest.status === 'SUCCESS').length;
  const failCount = tcEntries.filter(([, d]) => d.latest.status === 'FAIL').length;
  const runningCount = tcEntries.filter(([, d]) => d.latest.status === 'RUNNING').length;

  return (
    <section className="bg-base-100 rounded-2xl shadow-lg overflow-hidden">
      <button
        onClick={() => setOpen(!open)}
        className="w-full flex items-center justify-between px-5 py-3 bg-base-200 hover:bg-base-300/60 transition-colors"
      >
        <div className="flex items-center gap-3">
          <span className="font-black text-base">{category}</span>
          <span className="text-xs text-base-content/50">({tcEntries.length}개 TC)</span>
        </div>
        <div className="flex items-center gap-2 text-xs font-mono">
          {runningCount > 0 && <span className="badge badge-info badge-sm">RUN {runningCount}</span>}
          <span className="badge badge-success badge-sm text-black">{successCount}</span>
          <span className="badge badge-error badge-sm">{failCount}</span>
          <span>{open ? '▲' : '▼'}</span>
        </div>
      </button>

      {open && (
        <div className="p-3 flex flex-col gap-2">
          {tcEntries.map(([tcId, data]) => (
            <TcCard key={tcId} tcId={tcId} data={data} />
          ))}
        </div>
      )}
    </section>
  );
}