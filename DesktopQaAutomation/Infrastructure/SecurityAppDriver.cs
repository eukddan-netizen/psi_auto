using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using Microsoft.AspNetCore.SignalR.Client;

namespace DesktopQaAutomation.Infrastructure
{
    /// <summary>
    /// 보안점검 프로그램(taKeeper.exe 등)의 실행, UI 구조 탐색, 화면 요소 조작 및 검증을 담당하는 공통 드라이버 클래스
    /// </summary>
    public class SecurityAppDriver : IDisposable
    {
        private readonly string _exePath;
        private readonly HubConnection? _hubConnection;
        private readonly string _category;
        private readonly string _tcId;

        public UIA3Automation Automation { get; }
        public Process? Process { get; private set; }
        public Window? MainWindow { get; private set; }

        public SecurityAppDriver(
            string exePath = @"C:\Program Files (x86)\topaegis\PRSv5\taKeeper.exe",
            HubConnection? hubConnection = null,
            string category = "COMMON",
            string tcId = "COMMON")
        {
            _exePath = exePath;
            _hubConnection = hubConnection;
            _category = category;
            _tcId = tcId;
            Automation = new UIA3Automation();
        }

        /// <summary>
        /// 대시보드로 실시간 로그를 전송합니다.
        /// </summary>
        public void SendLog(string status, string message)
        {
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                _ = _hubConnection.InvokeAsync("SendTestStatus", _category, _tcId, status, message);
            }
        }

        /// <summary>
        /// 현재 프로세스가 관리자 권한으로 실행 중인지 확인합니다.
        /// </summary>
        public static bool IsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// 보안점검 프로그램을 UAC 우회 및 관리자 권한으로 실행하고 메인 윈도우를 바인딩합니다.
        /// </summary>
        public async Task<bool> LaunchAndBindAsync(int timeoutSeconds = 15)
        {
            Console.WriteLine($"🚀 보안점검 프로그램 실행 시작: {_exePath}");
            SendLog("RUNNING", $"프로세스 실행 시도: {_exePath}");

            if (!File.Exists(_exePath))
            {
                string errorMsg = $"실행 파일이 존재하지 않습니다: {_exePath}";
                Console.WriteLine($"⚠️ {errorMsg}");
                SendLog("FAIL", errorMsg);
                return false;
            }

            var psi = new ProcessStartInfo
            {
                FileName = _exePath,
                UseShellExecute = true,
                Verb = "runas"
            };

            Process? startedProcess = null;
            Exception? startException = null;

            var startTask = Task.Run(() =>
            {
                try
                {
                    startedProcess = Process.Start(psi);
                }
                catch (Exception ex)
                {
                    startException = ex;
                }
            });

            // UAC 승인 팝업 자동 감지 및 승인 시도 (최대 10초)
            for (int i = 0; i < 20; i++)
            {
                if (startTask.IsCompleted) break;

                try
                {
                    var desktop = Automation.GetDesktop();
                    var uacWindow = desktop.FindFirstChild(cf => cf.ByName("사용자 계정 컨트롤"))
                                 ?? desktop.FindFirstChild(cf => cf.ByName("User Account Control"));

                    if (uacWindow != null)
                    {
                        Console.WriteLine("⚠️ UAC 팝업 감지됨. 승인 버튼 클릭 시도...");
                        var yesBtn = uacWindow.FindFirstDescendant(cf => cf.ByName("예"))
                                  ?? uacWindow.FindFirstDescendant(cf => cf.ByName("Yes"));

                        if (yesBtn != null)
                        {
                            yesBtn.AsButton().Click();
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
                    // UAC 데스크톱 보호 예외 무시
                }

                await Task.Delay(500);
            }

            await startTask;

            if (startException != null)
            {
                Console.WriteLine($"❌ 프로세스 실행 예외 발생: {startException.Message}");
                SendLog("FAIL", $"프로세스 실행 실패: {startException.Message}");
                return false;
            }

            Process = startedProcess;
            if (Process != null)
            {
                Console.WriteLine($"✅ 프로세스가 실행되었습니다. (PID: {Process.Id})");
                SendLog("RUNNING", $"프로세스 실행 성공 (PID: {Process.Id})");
            }

            // 메인 윈도우 탐색
            Console.WriteLine("🔍 메인 윈도우 바인딩 시도 중...");
            int loopCount = timeoutSeconds * 2;
            for (int i = 0; i < loopCount; i++)
            {
                var desktop = Automation.GetDesktop();
                var found = desktop.FindFirstChild(cf => cf.ByName("내PC보안점검"))
                             ?? desktop.FindFirstChild(cf => cf.ByName("taKeeper"))
                             ?? desktop.FindFirstChild(cf => cf.ByClassName("taKeeper"));

                if (found != null)
                {
                    MainWindow = found.AsWindow();
                    Console.WriteLine($"🎯 메인 윈도우 바인딩 성공: [{MainWindow.Title}] (Class: {MainWindow.ClassName})");
                    SendLog("RUNNING", $"메인 윈도우 연결 완료: {MainWindow.Title}");
                    return true;
                }
                await Task.Delay(500);
            }

            Console.WriteLine("⚠️ 특정 메인 윈도우 핸들을 발견하지 못했으나 프로세스는 실행 중입니다.");
            SendLog("RUNNING", "메인 윈도우 미발견 (프로세스는 실행 중)");
            return Process != null;
        }

        /// <summary>
        /// 메인 윈도우 하위의 모든 UI 요소 계층 구조를 트리 형태의 문자열로 반환하고 콘솔에 출력합니다.
        /// (Accessibility Insights 없이도 UI 구조를 한눈에 파악할 수 있는 디버깅 유틸리티)
        /// </summary>
        public string DumpUiTree(int maxDepth = 5)
        {
            if (MainWindow == null)
            {
                var desktop = Automation.GetDesktop();
                var found = desktop.FindFirstChild(cf => cf.ByName("내PC보안점검"))
                             ?? desktop.FindFirstChild(cf => cf.ByName("taKeeper"));
                if (found != null)
                {
                    MainWindow = found.AsWindow();
                }
            }

            if (MainWindow == null)
            {
                string msg = "❌ UI 트리를 덤프할 메인 윈도우가 없습니다.";
                Console.WriteLine(msg);
                return msg;
            }

            var sb = new StringBuilder();
            sb.AppendLine("==================== [ 보안점검 프로그램 UI 트리 DUMP ] ====================");
            DumpElementRecursive(MainWindow, sb, "", 0, maxDepth);
            sb.AppendLine("=========================================================================");

            string result = sb.ToString();
            Console.WriteLine(result);
            return result;
        }

        private void DumpElementRecursive(AutomationElement element, StringBuilder sb, string indent, int currentDepth, int maxDepth)
        {
            if (currentDepth > maxDepth) return;

            try
            {
                string name = string.IsNullOrEmpty(element.Name) ? "(No Name)" : element.Name;
                string autoId = string.IsNullOrEmpty(element.AutomationId) ? "(No AutoId)" : element.AutomationId;
                string controlType = element.ControlType.ToString();
                string className = string.IsNullOrEmpty(element.ClassName) ? "" : $" [{element.ClassName}]";

                sb.AppendLine($"{indent}├─ [{controlType}] Name: \"{name}\" | AutoId: \"{autoId}\"{className}");

                var children = element.FindAllChildren();
                foreach (var child in children)
                {
                    DumpElementRecursive(child, sb, indent + "│  ", currentDepth + 1, maxDepth);
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"{indent}├─ [Error reading element: {ex.Message}]");
            }
        }

        /// <summary>
        /// AutomationId로 요소 찾기
        /// </summary>
        public AutomationElement? FindByAutomationId(string automationId)
        {
            return MainWindow?.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
        }

        /// <summary>
        /// Name(표시 텍스트)으로 요소 찾기
        /// </summary>
        public AutomationElement? FindByName(string name)
        {
            return MainWindow?.FindFirstDescendant(cf => cf.ByName(name));
        }

        /// <summary>
        /// AutomationId 또는 Name으로 버튼을 찾아 클릭합니다.
        /// </summary>
        public bool ClickButton(string idOrName)
        {
            var element = FindByAutomationId(idOrName) ?? FindByName(idOrName);
            if (element != null)
            {
                var button = element.AsButton();
                button.Click();
                Console.WriteLine($"🖱️ 버튼 클릭 성공: {idOrName}");
                return true;
            }

            Console.WriteLine($"⚠️ 버튼을 찾을 수 없습니다: {idOrName}");
            return false;
        }

        public void Dispose()
        {
            Automation.Dispose();
        }
    }
}
