// logs는 최신순(newest-first)으로 들어옴을 전제로 함
export function groupLogs(logs) {
  const categories = new Map();

  for (const log of logs) {
    if (!categories.has(log.category)) {
      categories.set(log.category, new Map());
    }
    const tcMap = categories.get(log.category);

    if (!tcMap.has(log.testCaseId)) {
      // 최초로 만나는 로그 = 가장 최신 상태 (newest-first이므로)
      tcMap.set(log.testCaseId, { latest: log, history: [] });
    }
    tcMap.get(log.testCaseId).history.push(log);
  }

  return categories;
}