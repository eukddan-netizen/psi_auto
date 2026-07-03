import React, { useMemo, useState } from 'react';
import { useSignalR } from './hooks/useSignalR';
import { CategorySection } from './components/CategorySection';
import { groupLogs } from './hooks/groupLogs';

function App() {
  const { logs, isConnected, clearLogs } = useSignalR("http://localhost:5000/testLogHub");
  const [search, setSearch] = useState("");

  const grouped = useMemo(() => groupLogs(logs), [logs]);

  const successCount = logs.filter(l => l.status === 'SUCCESS').length;
  const failCount = logs.filter(l => l.status === 'FAIL').length;

  const filteredCategories = useMemo(() => {
    if (!search.trim()) return [...grouped.entries()];
    const q = search.toLowerCase();
    return [...grouped.entries()]
      .map(([category, tcMap]) => {
        const filteredTcs = new Map(
          [...tcMap.entries()].filter(([tcId]) => tcId.toLowerCase().includes(q))
        );
        return [category, filteredTcs];
      })
      .filter(([, tcMap]) => tcMap.size > 0);
  }, [grouped, search]);

  return (
    <div className="min-h-screen bg-base-300 p-4 md:p-8 flex flex-col items-center">
      <div className="w-full max-w-5xl flex flex-col gap-6">

        <header className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-base-100 p-6 rounded-2xl shadow-lg">
          <div>
            <h1 className="text-2xl font-black tracking-tight flex items-center gap-2">
              <span>🤖</span> PSI 자동화 테스트
            </h1>
            <p className="text-xs text-base-content/60 mt-1">실시간 테스트 모니터링 시스템</p>
          </div>
          <div className="flex items-center gap-2">
            {isConnected ? (
              <span className="badge badge-success gap-1.5 py-3 font-semibold text-black">서버 연결됨</span>
            ) : (
              <span className="badge badge-error gap-1.5 py-3 font-semibold">연결 끊김</span>
            )}
            <button onClick={clearLogs} className="btn btn-sm btn-outline btn-warning">
              로그 비우기
            </button>
          </div>
        </header>

        <section className="stats shadow w-full bg-base-100">
          <div className="stat">
            <div className="stat-title">전체 TC 수</div>
            <div className="stat-value text-primary font-mono">
              {[...grouped.values()].reduce((sum, tcMap) => sum + tcMap.size, 0)}
            </div>
          </div>
          <div className="stat">
            <div className="stat-title text-success">SUCCESS 로그</div>
            <div className="stat-value text-success font-mono">{successCount}</div>
          </div>
          <div className="stat">
            <div className="stat-title text-error">FAIL 로그</div>
            <div className="stat-value text-error font-mono">{failCount}</div>
          </div>
        </section>

        <input
          type="text"
          placeholder="TC_ID로 검색 (예: TC000042)"
          className="input input-bordered w-full"
          value={search}
          onChange={e => setSearch(e.target.value)}
        />

        <main className="flex flex-col gap-4">
          {filteredCategories.length === 0 ? (
            <div className="bg-base-100 rounded-2xl shadow-lg h-64 flex flex-col justify-center items-center gap-3 text-base-content/40">
              <span className="loading loading-infinity loading-lg text-primary"></span>
              <p className="text-sm">테스트 에이전트로부터 실시간 신호를 대기하고 있습니다...</p>
            </div>
          ) : (
            filteredCategories.map(([category, tcMap]) => (
              <CategorySection key={category} category={category} tcMap={tcMap} />
            ))
          )}
        </main>

      </div>
    </div>
  );
}

export default App;