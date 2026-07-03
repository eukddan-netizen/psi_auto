import { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

export function useSignalR(hubUrl) {
  const [logs, setLogs] = useState([]);
  const [isConnected, setIsConnected] = useState(false);

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .build();

    connection.start()
      .then(() => setIsConnected(true))
      .catch(err => {
        console.error("🔴 연결 실패: ", err);
        setIsConnected(false);
      });

    connection.on("ReceiveStatus", (category, testCaseId, status, message) => {
      const newLog = {
        id: `${testCaseId}-${Date.now()}-${Math.random().toString(36).slice(2, 6)}`,
        category: category || "Uncategorized",
        testCaseId,
        status: status.toUpperCase(),
        message,
        time: new Date().toLocaleTimeString()
      };
      setLogs(prevLogs => [newLog, ...prevLogs]);
    });

    connection.onreconnecting(() => setIsConnected(false));
    connection.onreconnected(() => setIsConnected(true));

    return () => connection.stop();
  }, [hubUrl]);

  const clearLogs = () => setLogs([]);

  return { logs, isConnected, clearLogs };
}