using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Interfaces; // Sẽ báo lỗi đỏ, kệ nó, bước sau sẽ hết
using UrlShortener.Application.Services; // Sẽ báo lỗi đỏ, kệ nó, bước sau sẽ hết
using UrlShortener.Infrastructure.Persistence;
using UrlShortener.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Thêm CORS (RẤT QUAN TRỌNG)
// Cho phép React UI của bạn gọi được API này
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            // Cho phép địa chỉ của React App (mặc định là 3000)
            policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:8080")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// 2. Đăng ký AppDbContext (Kết nối Database)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
// Đăng ký map IAppDbContext -> AppDbContext
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

// 3. Đăng ký Redis Cache (Yêu cầu điểm Merit)
builder.Services.AddStackExchangeRedisCache(options =>
{
    
    options.Configuration = "10.25.42.67:6379";
});

// 4. Đăng ký Service (Dependency Injection)
// Chúng ta sẽ tạo 2 file IUrlShorteningService và UrlShorteningService ở bước tiếp theo
builder.Services.AddScoped<IUrlShorteningService, UrlShorteningService>();

// ----- Các dịch vụ có sẵn của API -----
builder.Services.AddControllers(); // Thêm dòng này nếu bạn muốn dùng Controller (chúng ta sẽ dùng sau)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// -------------------------------------

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

// 5. Sử dụng CORS (Phải nằm trước UseAuthorization)
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers(); // Thêm dòng này để kích hoạt Controller

// 6. Chạy migration tự động khi app khởi động (cho đơn giản)
// Điều này giúp tự tạo database "UrlShortenerDb" trong Docker
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //dbContext.Database.Migrate();
}

app.Run();