import React from 'react';
import { useSignalR } from './hooks/useSignalR';
import { LogItem } from './components/LogItem';

function App() {
  // 커스텀 훅에서 필요한 데이터와 함수만 쏙 빼옵니다.
  const { logs, isConnected, clearLogs } = useSignalR("http://localhost:5000/testLogHub");

  // 통계 계산
  const successCount = logs.filter(l => l.status === 'SUCCESS').length;
  const failCount = logs.filter(l => l.status === 'FAIL').length;

  return (
    <div className="min-h-screen bg-base-300 p-4 md:p-8 flex flex-col items-center">
      <div className="w-full max-w-5xl flex flex-col gap-6">
        
        {/* 상단 헤더 영역 */}
        <header className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-base-100 p-6 rounded-2xl shadow-lg">
          <div>
            <h1 className="text-2xl font-black tracking-tight flex items-center gap-2">
              <span>🤖</span> PSI 자동화 테스트
            </h1>
            <p className="text-xs text-base-content/60 mt-1">실시간 테스트 모니터링 시스템</p>
          </div>
          
          {/* 서버 연결 상태 표시 기믹 */}
          <div className="flex items-center gap-2">
            {isConnected ? (
              <span className="badge badge-success gap-1.5 py-3 font-semibold text-black">
                <span className="h-2 w-2 rounded-full bg-green-700 animate-ping"></span>
                서버 연결됨
              </span>
            ) : (
              <span className="badge badge-error gap-1.5 py-3 font-semibold">
                <span className="h-2 w-2 rounded-full bg-red-200"></span>
                연결 끊김
              </span>
            )}
            <button onClick={clearLogs} className="btn btn-sm btn-outline btn-warning">
              로그 비우기
            </button>
          </div>
        </header>

        {/* 상단 통계 현황판 (DaisyUI Stats) */}
        <section className="stats shadow w-full bg-base-100">
          <div className="stat">
            <div className="stat-title">전체 수신 로그</div>
            <div className="stat-value text-primary font-mono">{logs.length}</div>
            <div className="stat-desc">현재 세션 기준</div>
          </div>
          <div className="stat">
            <div className="stat-title text-success">SUCCESS</div>
            <div className="stat-value text-success font-mono">{successCount}</div>
            <div className="stat-desc">성공적으로 패스된 항목</div>
          </div>
          <div className="stat">
            <div className="stat-title text-error">FAIL</div>
            <div className="stat-value text-error font-mono">{failCount}</div>
            <div className="stat-desc">확인이 필요한 실패 항목</div>
          </div>
        </section>

        {/* 메인 콘솔 모니터 영역 (DaisyUI Mockup Window 스타일) */}
        <main className="mockup-window border border-base-300 bg-base-100 shadow-2xl">
          <div className="bg-base-200 px-4 py-2 text-xs font-mono text-base-content/40 border-b border-base-300 flex justify-between">
            <span>LIVE_STREAM_CONSOLE</span>
            <span>PORT: 5173</span>
          </div>
          
          <div className="h-[550px] overflow-y-auto p-4 font-mono divide-y divide-base-300">
            {logs.length === 0 ? (
              <div className="h-full flex flex-col justify-center items-center gap-3 text-base-content/40">
                <span className="loading loading-infinity loading-lg text-primary"></span>
                <p className="text-sm font-sans tracking-wide">테스트 에이전트로부터 실시간 신호를 대기하고 있습니다...</p>
              </div>
            ) : (
              logs.map((log, index) => <LogItem key={index} log={log} />)
            )}
          </div>
        </main>

      </div>
    </div>
  );
}

export default App;