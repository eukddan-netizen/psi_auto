using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using DesktopQaAutomation.Infrastructure;

namespace DesktopQaAutomation.TestCases._05_로그인_암호_안전성
{
    [Trait("Category", "05_로그인_암호_안전성")]
    [Trait("TC", "31_0001")]
    public class TC_31_0001 : IClassFixture<AppFixture>
    {
        private readonly AppFixture _fixture;
        private readonly string _tcId = "TC_31_0001";
        private readonly string _category = "05_로그인_암호_안전성";
        private readonly string _exePath = @"C:\Program Files (x86)\topaegis\PRSv5\taKeeper.exe";

        public TC_31_0001(AppFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(DisplayName = "로그인 암호 안전성 - taKeeper 프로그램 실행 및 UI 트리 분석 테스트")]
        public async Task taKeeper_프로그램_실행_테스트()
        {
            Console.WriteLine($"📝 {_tcId} 로그인 암호 안전성 테스트를 시작합니다...");

            using var driver = new SecurityAppDriver(_exePath, _fixture.Connection, _category, _tcId);

            bool isAdmin = SecurityAppDriver.IsAdministrator();
            if (!isAdmin)
            {
                Console.WriteLine("⚠️ 경고: 현재 터미널이 일반 사용자 권한으로 실행 중입니다.");
                Console.WriteLine("💡 관리자 권한 터미널 실행을 권장합니다.");
            }

            // 1. 보안점검 프로그램 실행 및 메인 윈도우 바인딩
            bool isLaunched = await driver.LaunchAndBindAsync(timeoutSeconds: 15);
            Assert.True(isLaunched, "보안점검 프로그램 기동 실패");

            // 2. [UI 구조 분석] 현재 보안점검 프로그램 메인 화면의 모든 UI 요소 트리를 콘솔에 덤프
            Console.WriteLine("🔍 보안점검 프로그램 UI 트리 구조를 분석합니다...");
            string uiTreeDump = driver.DumpUiTree(maxDepth: 5);
            Assert.NotEmpty(uiTreeDump);

            // 3. 테스트 결과 전달
            Console.WriteLine("✅ taKeeper 프로그램 실행 및 UI 트리 구조 파악 완료");
            driver.SendLog("SUCCESS", "taKeeper 프로그램 실행 및 UI 분석 완료");
        }
    }
}
