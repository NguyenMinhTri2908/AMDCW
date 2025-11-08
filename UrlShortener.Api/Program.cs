using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UrlShortener.Application.Interfaces;
using UrlShortener.Application.Services;
using UrlShortener.Infrastructure.Persistence;
using UrlShortener.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ----- KHU VỰC 1: Đăng ký Dịch vụ (builder.Services.Add...) -----

// 1. Thêm CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:8080")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// 2. Đăng ký AppDbContext (Kết nối Database)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

// 3. Đăng ký Redis Cache (Yêu cầu điểm Merit)
builder.Services.AddStackExchangeRedisCache(options =>
{
    // Đảm bảo IP 10.25.42.67 này vẫn là IP của máy Mac (Default Gateway) nhé!
    options.Configuration = "10.25.41.217:6379";
});

// 4. Đăng ký Service (Dependency Injection)
builder.Services.AddScoped<IUrlShorteningService, UrlShorteningService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();

// 5. Các dịch vụ có sẵn của API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 6. Đăng ký Dịch vụ Rate Limit (Yêu cầu điểm cao)
// (Nó PHẢI nằm ở đây, trước khi build 'app')
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "ApiLimiter", opt =>
    {
        opt.PermitLimit = 10; // Chỉ cho phép 10 request
        opt.Window = TimeSpan.FromMinutes(1); // Trong vòng 1 phút
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ----- DÒNG NÀY CHỈ XUẤT HIỆN 1 LẦN -----
var app = builder.Build();

// ----- KHU VỰC 2: Cấu hình Pipeline (app.Use...) -----

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

// Vị trí các 'Use' RẤT QUAN TRỌNG
app.UseCors("AllowFrontend");

app.UseRateLimiter(); // Sử dụng Rate Limiter

app.UseAuthorization();

app.MapControllers();

// (Đã comment khối using 'Migrate' đi)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //dbContext.Database.Migrate();
}

app.Run();