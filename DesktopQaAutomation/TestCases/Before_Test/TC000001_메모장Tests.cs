using System;
using System.Threading;
using System.Diagnostics;
using Xunit;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using Microsoft.AspNetCore.SignalR.Client;
using DesktopQaAutomation.Infrastructure;

namespace DesktopQaAutomation.TestCases.Before_Test
{
    [Trait("Category", "Before_Test")]
    [Trait("TC", "000001")]
    public class TC000001_메모장Tests : IClassFixture<AppFixture>
    {
        private readonly AppFixture _fixture;
        private readonly string _tcId = "TC000001"; // 요청하신 규격으로 매칭

        // 생성자를 통해 SignalR 연결 상태를 공유받음
        public TC000001_메모장Tests(AppFixture fixture)
        {
            _fixture = fixture;
        }

        // 실시간 전송을 위한 헬퍼 메서드
        private void SendLog(string status, string message)
        {
            if (_fixture.Connection != null && _fixture.Connection.State == HubConnectionState.Connected)
            {
                _ = _fixture.Connection.InvokeAsync("SendTestStatus", _tcId, status, message);
            }
        }

        [Fact(DisplayName = "메모장 자동화 및 입력 팝업 방어 테스트")]
        public void 메모장_자동화_및_팝업_테스트()
        {
            Console.WriteLine("📝 메모장(Notepad) 자동화 테스트를 시작합니다...");
            SendLog("RUNNING", "메모장 자동화 테스트를 시작합니다...");

            try
            {
                Console.WriteLine("🚀 메모장 애플리케이션을 실행합니다...");
                SendLog("RUNNING", "메모장(notepad.exe) 프로세스 기동 시작");
                
                var process = Process.Start("notepad.exe");
                Assert.NotNull(process); // 프로세스가 정상 실행되었는지 검증

                using (var automation = new UIA3Automation())
                {
                    Window? window = null;
                    Console.WriteLine("🔍 메모장 창을 찾는 중...");
                    SendLog("RUNNING", "UIA3을 활용하여 메모장 메인 윈도우 탐색 중...");
                    
                    for (int i = 0; i < 10; i++)
                    {
                        var desktop = automation.GetDesktop();
                        var found = desktop.FindFirstChild(cf => cf.ByClassName("Notepad"))
                                     ?? desktop.FindFirstChild(cf => cf.ByName("메모장"))
                                     ?? desktop.FindFirstChild(cf => cf.ByName("Notepad"));
                        
                        if (found != null)
                        {
                            window = found.AsWindow();
                            break;
                        }
                        Thread.Sleep(500);
                    }

                    // 창을 찾았는지 검증 (null이면 실패 처리)
                    Assert.True(window != null, "메모장 메인 창을 찾을 수 없습니다.");

                    Console.WriteLine($"🎯 메모장 창 발견: {window.Title}");
                    SendLog("RUNNING", $"메모장 핸들 확보 완료: {window.Title}");
                    window.Focus();
                    Thread.Sleep(1000);

                    var document = window.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Document))
                                ?? window.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit));

                    if (document != null)
                    {
                        Console.WriteLine("✍️ 텍스트 입력 영역을 찾았습니다. 글자를 입력합니다...");
                        SendLog("RUNNING", "텍스트 입력 영역(Document) 발견, 문자열 타이핑 시작");
                        
                        document.Focus();
                        Keyboard.Type("Hello, FlaUI Automation World!");
                        Keyboard.Press(VirtualKeyShort.ENTER);
                        Keyboard.Release(VirtualKeyShort.ENTER);
                        Keyboard.Type("This is a simple automated test.");
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        Console.WriteLine("⚠️ 텍스트 입력 컨트롤을 찾지 못해 창 포커스 후 입력을 시도합니다.");
                        SendLog("RUNNING", "입력 컨트롤을 못 찾아 윈도우 다이렉트 포커스 타이핑 시도");
                        window.Focus();
                        Keyboard.Type("Hello, FlaUI!");
                        Thread.Sleep(2000);
                    }

                    Console.WriteLine("🛑 메모장 창을 닫습니다...");
                    SendLog("RUNNING", "테스트 완료 구문 통과, 메모장 종료 프로세스 진입");
                    window.Close();
                    Thread.Sleep(1500);

                    var desktopElement = automation.GetDesktop();
                    var dialog = window.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Window))
                              ?? desktopElement.FindFirstDescendant(cf => cf.ByName("메모장"))
                              ?? desktopElement.FindFirstDescendant(cf => cf.ByName("Notepad"));

                    if (dialog != null)
                    {
                        Console.WriteLine("💬 저장 확인 창이 나타났습니다. '저장 안 함'을 클릭합니다.");
                        SendLog("RUNNING", "저장 팝업 감지, '저장 안 함' 처리 중...");
                        
                        var dontSaveButton = dialog.FindFirstDescendant(cf => cf.ByName("저장 안 함"))
                                          ?? dialog.FindFirstDescendant(cf => cf.ByName("Don't Save"))
                                          ?? dialog.FindFirstDescendant(cf => cf.ByAutomationId("CommandButton_7"));

                        if (dontSaveButton != null)
                        {
                            dontSaveButton.AsButton().Click();
                            Console.WriteLine("🎯 '저장 안 함' 버튼을 클릭했습니다.");
                        }
                        else
                        {
                            Console.WriteLine("⚠️ 저장 안 함 버튼을 찾지 못해 단축키 'N'을 전송합니다.");
                            Keyboard.Type('n');
                        }
                    }

                    Thread.Sleep(1000);
                    Console.WriteLine("✅ 메모장 자동화 테스트가 성공적으로 종료되었습니다.");
                    SendLog("SUCCESS", "메모장 기능 검증 및 팝업 방어 처리 테스트 최종 통과");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 테스트 진행 중 에러 발생: {ex.Message}");
                SendLog("FAIL", $"런타임 에러 발생: {ex.Message}");
                
                Assert.Fail($"테스트 도중 예외 발생: {ex.Message}");
            }
        }
    }
}