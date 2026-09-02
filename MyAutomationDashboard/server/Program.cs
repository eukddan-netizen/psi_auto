using server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// SignalR 서비스 등록
builder.Services.AddSignalR();

var app = builder.Build();

// React 빌드 결과물(wwwroot/) 정적 파일 서빙
app.UseDefaultFiles();  // index.html을 기본 문서로 지정
app.UseStaticFiles();   // wwwroot 폴더를 정적 파일 루트로 사용

// SignalR Hub 엔드포인트
app.MapHub<TestLogHub>("/testLogHub");

// SPA Fallback: API 경로 외의 모든 요청을 index.html로 처리 (React Router 대비)
app.MapFallbackToFile("index.html");

app.Run("http://0.0.0.0:5000");