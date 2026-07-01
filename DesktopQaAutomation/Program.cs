using System;
using System.Threading.Tasks; // async/await 사용을 위해 필요
using Microsoft.AspNetCore.SignalR.Client; // SignalR 라이브러리 연결

namespace WindowsQaAutomation
{
    class Program
    {
        // 내 PC(서버)와 연결을 유지해 줄 무전기 객체입니다.
        public static HubConnection Connection;

        // 메인 함수도 비동기(static async Task)로 변경합니다.
        static async Task Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("🤖 Windows QA Automation Test Runner");
            Console.WriteLine("========================================");
            Console.WriteLine("1. 바탕화면 우클릭 및 개인 설정 테스트");
            Console.WriteLine("2. 메모장(Notepad) 자동화 테스트");
            Console.WriteLine("========================================");
            Console.Write("실행할 테스트 번호를 입력하세요 (1-2): ");
        
            string input = Console.ReadLine() ?? "";
            Console.WriteLine();

            // 📢 [중요] 테스트를 돌리기 전에 내 PC 서버에 먼저 주파수를 맞추고 연결합니다.
            // '내_PC_실제_IP_주소' 부분에 대시보드를 띄워둔 컴퓨터의 IP를 적으셔야 합니다 (예: 192.168.0.15)
            Connection = new HubConnectionBuilder()
                .WithUrl("https://proofread-prance-paramedic.ngrok-free.dev/testLogHub")
                .WithAutomaticReconnect() // 인터넷이 잠깐 끊겨도 자동으로 재연결 시도
                .Build();

            try
            {
                Console.WriteLine("📡 내 PC 대시보드 서버와 연결을 시도합니다...");
                await Connection.StartAsync();
                Console.WriteLine("🟢 대시보드 서버 연결 성공!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 대시보드 서버 연결 실패 (방화벽이나 IP 확인 필요): {ex.Message}");
                Console.WriteLine("대시보드 전송 없이 로컬 테스트로 계속 진행합니다.");
            }

            // 사용자의 선택에 따라 테스트 메서드 실행
            if (input == "2")
            {
                // 메모장 테스트 실행 (비동기로 호출하기 위해 구조 변경)
                await Task.Run(() => NotepadTest.Run());
            }
            else
            {
                // 바탕화면 테스트 실행 (비동기로 호출하기 위해 구조 변경)
                await Task.Run(() => RunDesktopTest());
            }

            // 테스트가 다 끝나면 연결을 안전하게 닫아줍니다.
            if (Connection != null && Connection.State == HubConnectionState.Connected)
            {
                await Connection.StopAsync();
            }

            Console.WriteLine("\n아무 키나 누르면 종료됩니다...");
            if (!Console.IsInputRedirected)
            {
                Console.ReadKey();
            }
        }

        static void RunDesktopTest()
        {
            // (기존 작성하신 바탕화면 테스트 로직은 그대로 유지하시면 됩니다)
            Console.WriteLine("🚀 바탕화면 테스트는 로컬로 우선 작동합니다.");
        }
    }
}