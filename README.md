내 PC (서버 & 대시보드)

Node.js (LTS 버전)

.NET 8.0 SDK

ngrok 실행 파일 및 무료 고정 도메인 (ngrok 가입 필요)

테스트 PC (자동화 러너)

.NET 8.0 SDK 또는 실행 환경

ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ

1) SignalR 백엔드 서버 실행
server 폴더로 이동합니다.

터미널에서 아래 명령어를 입력하여 서버를 구동합니다. (기본 5000번 포트 바인딩)

Bash
cd server
dotnet run

ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ

dashboard 폴더로 이동합니다.

의존성 패키지를 설치한 뒤 대시보드를 구동합니다. (5173 포트 고정)

Bash
cd dashboard
npm install
npm run dev
웹 브라우저를 열고 http://localhost:5173 접속 상태를 확인합니다.

ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ

ngrok.exe 파일이 있는 폴더에서 터미널을 열고 아래 명령어를 실행합니다. (테스트 진행 동안 이 창을 절대 닫지 마세요)

ngrok http 5000 --url=proofread-prance-paramedic.ngrok-free.dev

ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ

1) 접속 주소 수정
DesktopQaAutomation/Program.cs 파일을 열고, HubConnectionBuilder 부분의 URL을 2단계에서 활성화한 ngrok 고정 주소로 변경합니다.

// Program.cs 파일 내부
Connection = new HubConnectionBuilder()
    .WithUrl("https://proofread-prance-paramedic.ngrok-free.dev/testLogHub") // 발급받은 ngrok 주소 기입
    .WithAutomaticReconnect()
    .Build();

2) FlaUI 자동화 프로그램 실행
   cd DesktopQaAutomation
    dotnet run
