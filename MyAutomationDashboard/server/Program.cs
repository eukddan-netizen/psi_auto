using server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. SignalR 서비스 등록
builder.Services.AddSignalR();

// 2. React(포트 5173 등)가 접근할 수 있도록 CORS 정책 허용
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite React 기본 포트
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // SignalR 필수 설정
    });
});

var app = builder.Build();

app.UseCors("CorsPolicy");

// 3. SignalR 주소(엔드포인트) 매핑
app.MapHub<TestLogHub>("/testLogHub");

// 로컬 테스트를 위해 고정 포트(5000) 지정하여 서버 실행
app.Run("http://0.0.0.0:5000");