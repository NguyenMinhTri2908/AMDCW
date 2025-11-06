using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Entities;
using UrlShortener.Application.Interfaces;

namespace UrlShortener.Infrastructure.Persistence
{
    // Đây là file Context của Entity Framework
    public class AppDbContext : DbContext, IAppDbContext
    {
        // Hàm khởi tạo này là bắt buộc
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Đăng ký bảng (Entity) ShortenedUrls
        public DbSet<ShortenedUrl> ShortenedUrls { get; set; }

        // Cấu hình Fluent API (đáp ứng yêu cầu điểm cao)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ShortenedUrl>(builder =>
            {
                // Đánh Index cho cột ShortCode để tìm kiếm siêu nhanh
                builder.HasIndex(s => s.ShortCode).IsUnique();

                // Đặt độ dài tối đa cho ShortCode
                builder.Property(s => s.ShortCode).HasMaxLength(10);

                // Đặt độ dài tối đa cho Link gốc
                builder.Property(s => s.OriginalUrl).HasMaxLength(2048);
            });
        }
    }
}