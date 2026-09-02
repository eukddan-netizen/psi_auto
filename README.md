# PSI 자동화 테스트 시스템

Windows 데스크톱 UI 자동화(FlaUI + xUnit) 테스트 결과를 React 대시보드에 실시간(SignalR)으로 모니터링하는 시스템입니다.

모든 프로세스는 테스트 PC 로컬 단일 머신(`localhost`) 환경에서 구동됩니다.

## 1. 사전 환경 요구사항

- **Git**
- **.NET 8.0 SDK** (백엔드 서버 및 xUnit 테스트 실행용)
- **Node.js (LTS 버전)** (React 대시보드 소스 수정 및 재빌드 시에만 필요)

## 2. 저장소 복제 (Git Clone)

```bash
# 작업 디렉토리 이동 (예: D 드라이브 루트)
cd d:\

# 저장소 클론
git clone <저장소_URL> psi_auto

# 프로젝트 디렉토리로 이동
cd psi_auto
```

## 3. 구동 순서 및 실행 방법

### Step 1. 통합 서버 기동 (대시보드 + SignalR 허브)

React 빌드 파일 서빙과 실시간 SignalR 메시지 중계를 처리하는 통합 서버를 5000번 포트로 구동합니다.

```bash
cd MyAutomationDashboard/server
dotnet run
```

- 서버 기동 후 웹 브라우저에서 `http://localhost:5000`에 접속하여 대시보드 화면을 띄워둡니다.

### Step 2. 자동화 테스트 실행

새 터미널 창을 열고 테스트 에이전트 폴더로 이동하여 테스트를 실행합니다.

```bash
cd DesktopQaAutomation
```

**A. 전체 테스트 실행**

```bash
dotnet test
```

**B. 카테고리별 묶음 실행**

```bash
# 사전 테스트 케이스 전체 실행
dotnet test --filter "Category=Before_Test"

# 사후 테스트 케이스 전체 실행
dotnet test --filter "Category=After_Test"
```

**C. 단일 테스트 케이스 지정 실행**

```bash
# 특정 TC(예: TC000001 메모장 테스트)만 단독 실행
dotnet test --filter "FullyQualifiedName~TC000001"
```

## 4. UI 코드 수정 시 빌드 방법 (선택 사항)

`MyAutomationDashboard/dashboard/src` 내부의 React 컴포넌트나 스타일을 수정한 경우에만 빌드를 수행합니다.

빌드 결과물은 서버 폴더(`server/wwwroot`)로 자동 생성되어 반영됩니다.

```bash
cd MyAutomationDashboard/dashboard

# 의존성 설치 (최초 1회 또는 패키지 변경 시)
npm install

# React 프로덕션 빌드
npm run build
```

- 빌드가 끝나면 Step 1의 서버(`dotnet run`)를 재시작하거나 브라우저 화면을 새로고침합니다.

## 5. 주요 변경 및 주의사항

- **ngrok 및 외부 터널링 제거**: 로컬 단일 호스트 완결 구조로 개편되어 외부 고정 도메인 및 ngrok 터널 프로세스가 불필요합니다.
- **SignalR 엔드포인트**: 테스트 에이전트(`AppFixture.cs`)의 통신 주소는 `http://localhost:5000/testLogHub`를 바라봅니다.
- **순차 실행 강제**: 데스크톱 UI 테스트의 마우스 및 키보드 포커스 충돌을 방지하기 위해 병렬 실행이 비활성화(`DisableTestParallelization = true`)되어 순차적으로 동작합니다.
