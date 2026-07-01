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
      .then(() => {
        console.log("🟢 SignalR 연결 성공");
        setIsConnected(true);
      })
      .catch(err => {
        console.error("🔴 연결 실패: ", err);
        setIsConnected(false);
      });

    connection.on("ReceiveStatus", (testCaseId, status, message) => {
      const newLog = {
        testCaseId,
        status: status.toUpperCase(),
        message,
        time: new Date().toLocaleTimeString()
      };
      setLogs(prevLogs => [newLog, ...prevLogs]);
    });

    // 재연결 이벤트 감지
    connection.onreconnecting(() => setIsConnected(false));
    connection.onreconnected(() => setIsConnected(true));

    return () => {
      connection.stop();
    };
  }, [hubUrl]);

  // 로그 초기화 기능도 함께 제공
  const clearLogs = () => setLogs([]);

  return { logs, isConnected, clearLogs };
}