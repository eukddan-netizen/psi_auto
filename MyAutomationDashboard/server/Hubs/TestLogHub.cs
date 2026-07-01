using Microsoft.AspNetCore.SignalR;

namespace server.Hubs
{
    public class TestLogHub : Hub
    {
        // 테스트 PC가 호출할 메서드
        public async Task SendTestStatus(string testCaseId, string status, string message)
        {
            // 대시보드(React)를 포함한 연결된 모든 클라이언트에 실시간 브로드캐스팅
            await Clients.All.SendAsync("ReceiveStatus", testCaseId, status, message);
        }
    }
}