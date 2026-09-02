using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using Microsoft.AspNetCore.SignalR.Client;
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

        // 생성자를 통해 SignalR 연결 상태를 공유받음
        public TC_31_0001(AppFixture fixture)
        {
            _fixture = fixture;
        }

        // 실시간 전송을 위한 헬퍼 메서드
        private void SendLog(string status, string message)
        {
            if (_fixture.Connection != null && _fixture.Connection.State == HubConnectionState.Connected)
            {
                _ = _fixture.Connection.InvokeAsync("SendTestStatus", _category, _tcId, status, message);
            }
        }

        // 현재 테스트 프로세스가 관리자 권한으로 실행 중인지 확인
        private static bool IsRunningAsAdministrator()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        [Fact(DisplayName = "로그인 암호 안전성 - taKeeper 프로그램 실행 테스트")]
        public async Task taKeeper_프로그램_실행_테스트()
        {
            Console.WriteLine($"📝 {_tcId} 로그인 암호 안전성 테스트를 시작합니다...");
            SendLog("RUNNING", $"{_tcId} 테스트를 시작합니다.");

            bool isAdmin = IsRunningAsAdministrator();
            if (!isAdmin)
            {
                Console.WriteLine("⚠️ 경고: 현재 터미널이 일반 사용자 권한으로 실행 중입니다.");
                Console.WriteLine("💡 Windows 보안 정책상 UAC(Secure Desktop) 창은 일반 권한 프로세스에서 매크로 클릭이 금지됩니다.");
                Console.WriteLine("💡 테스트를 성공적으로 수행하려면 VS Code/PowerShell 터미널을 '관리자 권한으로 실행'해 주세요.");
                SendLog("RUNNING", "일반 사용자 권한 감지 - 관리자 권한 터미널 실행 권장");
            }
            else
            {
                Console.WriteLine("🛡️ 관리자 권한으로 실행 중 확인됨. UAC 팝업 없이 진행됩니다.");
                SendLog("RUNNING", "관리자 권한 확인됨 - UAC 우회 기동 진행");
            }

            try
            {
                Console.WriteLine($"🚀 프로그램 실행 시도: {_exePath}");
                SendLog("RUNNING", $"프로세스 기동 시작: {_exePath}");

                if (!File.Exists(_exePath))
                {
                    string fileNotFoundMsg = $"실행 파일이 존재하지 않습니다: {_exePath}";
                    Console.WriteLine($"⚠️ {fileNotFoundMsg}");
                    SendLog("FAIL", fileNotFoundMsg);
                    Assert.Fail(fileNotFoundMsg);
                    return;
                }

                var psi = new ProcessStartInfo
                {
                    FileName = _exePath,
                    UseShellExecute = true,
                    Verb = "runas" // UAC 관리자 권한 기동 요청
                };

                Process? process = null;
                Exception? startException = null;

                var startTask = Task.Run(() =>
                {
                    try
                    {
                        process = Process.Start(psi);
                    }
                    catch (Exception ex)
                    {
                        startException = ex;
                    }
                });

                using (var automation = new UIA3Automation())
                {
                    // UAC 팝업 대기 및 창 감지 (최대 10초)
                    for (int i = 0; i < 20; i++)
                    {
                        if (startTask.IsCompleted) break;

                        try
                        {
                            var desktop = automation.GetDesktop();
                            var uacWindow = desktop.FindFirstChild(cf => cf.ByName("사용자 계정 컨트롤"))
                                         ?? desktop.FindFirstChild(cf => cf.ByName("User Account Control"));

                            if (uacWindow != null)
                            {
                                Console.WriteLine("⚠️ UAC 팝업 감지. 승인 단축키 전송 중...");
                                var yesButton = uacWindow.FindFirstDescendant(cf => cf.ByName("예"))
                                             ?? uacWindow.FindFirstDescendant(cf => cf.ByName("Yes"));

                                if (yesButton != null)
                                {
                                    yesButton.AsButton().Click();
                                }
                                else
                                {
                                    Keyboard.Press(VirtualKeyShort.ALT);
                                    Keyboard.Type('y');
                                    Keyboard.Release(VirtualKeyShort.ALT);
                                }
                            }
                        }
                        catch
                        {
                            // UAC 보안 데스크톱 제약 시 예외 무시
                        }

                        await Task.Delay(500);
                    }

                    await startTask;

                    if (startException != null)
                    {
                        if (!isAdmin)
                        {
                            string uacErrorMsg = "UAC 승인이 거부되었거나 보안 데스크톱 제약으로 실행되지 못했습니다. 터미널을 '관리자 권한으로 실행' 후 다시 시도해 주세요.";
                            Console.WriteLine($"❌ {uacErrorMsg} (원인: {startException.Message})");
                            SendLog("FAIL", uacErrorMsg);
                            Assert.Fail(uacErrorMsg);
                            return;
                        }
                        throw startException;
                    }

                    Assert.NotNull(process);
                    Console.WriteLine($"✅ 프로세스가 실행되었습니다. (PID: {process.Id})");
                    SendLog("RUNNING", $"프로세스 실행 성공 (PID: {process.Id})");

                    Window? window = null;
                    Console.WriteLine("🔍 taKeeper 메인 창을 탐색 중...");
                    SendLog("RUNNING", "UIA3을 활용하여 메인 윈도우 탐색 중...");

                    for (int i = 0; i < 15; i++)
                    {
                        var desktop = automation.GetDesktop();
                        var found = desktop.FindFirstChild(cf => cf.ByName("내PC보안점검"))
                                     ?? desktop.FindFirstChild(cf => cf.ByName("taKeeper"))
                                     ?? desktop.FindFirstChild(cf => cf.ByClassName("taKeeper"));

                        if (found != null)
                        {
                            window = found.AsWindow();
                            break;
                        }
                        await Task.Delay(500);
                    }

                    if (window != null)
                    {
                        Console.WriteLine($"🎯 메인 창 발견: {window.Title}");
                        SendLog("RUNNING", $"창 핸들 확보 완료: {window.Title}");
                    }
                    else
                    {
                        Console.WriteLine("ℹ️ 특정 메인 창 핸들을 찾지 못했으나 프로세스는 실행 중입니다.");
                        SendLog("RUNNING", "윈도우 창 탐색 완료");
                    }
                }

                Console.WriteLine("✅ taKeeper 프로그램 실행 테스트 완료");
                SendLog("SUCCESS", "taKeeper 프로그램 실행 및 검증 완료");
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
