using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;

// UI 테스트가 서로 방해하지 않도록 순차 실행 설정
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace DesktopQaAutomation.Infrastructure
{
    public class AppFixture : IAsyncLifetime
    {
        public HubConnection Connection { get; private set; } = null!;

        // 테스트가 시작되기 전에 딱 한 번 실행되는 초기화 메서드 (SignalR 연결)
        public async Task InitializeAsync()
        {
            Console.WriteLine("📡 대시보드 서버와 연결을 시도합니다...");

            Connection = new HubConnectionBuilder()
                .WithUrl("https://proofread-prance-paramedic.ngrok-free.dev/testLogHub")
                .WithAutomaticReconnect()
                .Build();

            try
            {
                await Connection.StartAsync();
                Console.WriteLine("🟢 대시보드 서버 연결 성공!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 대시보드 서버 연결 실패: {ex.Message}");
                Console.WriteLine("대시보드 전송 없이 로컬 테스트로 계속 진행합니다.");
            }
        }

        // 모든 테스트가 끝나면 안전하게 연결 해제
        public async Task DisposeAsync()
        {
            if (Connection != null && Connection.State == HubConnectionState.Connected)
            {
                await Connection.StopAsync();
                Console.WriteLine("📡 대시보드 서버 연결 종료");
            }
        }
    }
}