PSI 자동화 테스트 시스템 가이드Windows 데스크톱 UI 자동화(FlaUI + xUnit) 테스트 결과를 React 모니터링 대시보드에 실시간(SignalR)으로 시각화하는 시스템입니다. 
모든 프로세스는 테스트 PC 단일 환경(localhost)에서 구동됩니다.  

1. 환경 요구사항Git (저장소 복제용).NET 8.0 SDK (서버 구동 및 xUnit 테스트 실행용)  Node.js (LTS 버전) (대시보드 UI를 수정하고 빌드할 때만 필요)

2. 저장소 복제 (Git Clone)터미널(PowerShell 또는 CMD)을 열고 프로젝트를 내려받을 디렉토리로 이동한 뒤 저장소를 복제합니다.Bash# 원하는 작업 폴더로 이동 (예: D 드라이브 루트)
cd d:\

# 깃 저장소 복제
git clone <저장소_URL> psi_auto

# 프로젝트 루트 폴더로 이동
cd psi_auto

3. 구동 순서 및 실행 방법1단계: 통합 서버 구동 (대시보드 + SignalR 허브)백엔드 폴더로 이동하여 ASP.NET Core 통합 서버를 실행합니다.  Bashcd MyAutomationDashboard/server
dotnet run
서버 실행 후 웹 브라우저를 열고 http://localhost:5000에 접속하여 대시보드 화면을 띄워둡니다. (5000번 포트에서 UI 웹 화면과 실시간 소켓 통신을 동시에 처리합니다.)2단계: 자동화 테스트 실행새 터미널 창을 열고 테스트 에이전트 폴더로 이동합니다.  Bashcd DesktopQaAutomation
실행 목적에 맞는 명령어를 입력합니다:Bash# A. 전체 테스트 케이스 실행
dotnet test

# B. 특정 카테고리만 묶어서 실행
dotnet test --filter "Category=Before_Test"   # 사전 테스트 케이스 전체
dotnet test --filter "Category=After_Test"    # 사후 테스트 케이스 전체

# C. 특정 단일 테스트 케이스만 지정 실행 (예: TC000001 메모장 테스트)
dotnet test --filter "FullyQualifiedName~TC000001"

4. UI 코드 수정 시 빌드 방법 (선택 사항)React 소스 코드(dashboard/src)를 수정한 경우에만 아래 명령어로 빌드를 진행합니다. 빌드 결과물은 서버 폴더(server/wwwroot)로 자동 반영됩니다.  Bashcd MyAutomationDashboard/dashboard
npm install        # 최초 1회 또는 패키지 추가 시에만 실행
npm run build      # 빌드 수행
빌드 완료 후 1단계의 통합 서버(dotnet run)를 재시작하거나 브라우저를 새로고침하면 변경된 화면이 즉시 적용됩니다.5. 주요 변경 및 주의사항ngrok 및 외부 터널링 완전 제거: 로컬 단일 환경으로 통합되어 ngrok 터널링 개통 및 외부 고정 도메인 유지 작업이 일체 불필요합니다.  통신 URL: 테스트 러너(AppFixture.cs)의 SignalR 연결 주소는 http://localhost:5000/testLogHub로 고정되어 있습니다.순차 실행 강제: UI 자동화 조작 충돌(키보드/마우스 제어)을 방지하기 위해 병렬 실행이 비활성화되어 있으므로 순차적으로 테스트가 수행됩니다.  
