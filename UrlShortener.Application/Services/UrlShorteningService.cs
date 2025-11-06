using Microsoft.Extensions.Caching.Distributed; // Gói này để dùng Redis
using System;
using System.Text;
using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Application.Services
{
    // Đây là Service triển khai logic
    public class UrlShorteningService : IUrlShorteningService
    {
        private readonly IAppDbContext _context;
        private readonly IDistributedCache _cache; // Biến để dùng Redis

        // Tiêm (Inject) DbContext và Redis Cache vào
        public UrlShorteningService(IAppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<ShortenedUrl?> CreateShortUrlAsync(string originalUrl)
        {
            // 1. Kiểm tra xem URL này đã rút gọn chưa
            var existing = await _context.ShortenedUrls
                .FirstOrDefaultAsync(x => x.OriginalUrl == originalUrl);

            if (existing != null)
            {
                // Nếu có rồi, trả về luôn
                return existing;
            }

            // 2. Tạo mã ngắn (short code)
            string shortCode;
            do
            {
                shortCode = GenerateShortCode(7); // Tạo mã 7 ký tự
            }
            // 3. Phải kiểm tra đảm bảo code là duy nhất
            while (await _context.ShortenedUrls.AnyAsync(x => x.ShortCode == shortCode));

            // 4. Lưu vào DB
            var newUrl = new ShortenedUrl
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                CreatedAt = DateTime.UtcNow
            };

            _context.ShortenedUrls.Add(newUrl);
            await _context.SaveChangesAsync();

            return newUrl;
        }

        // HÀM NÀY ĐỂ ĂN ĐIỂM MERIT (DÙNG CACHING)
        public async Task<string?> GetOriginalUrlByCodeAsync(string shortCode)
        {
            // 1. Thử lấy từ Cache (Redis) trước
            string? originalUrl = await _cache.GetStringAsync(shortCode);

            if (string.IsNullOrEmpty(originalUrl))
            {
                // 2. Nếu Cache không có, lấy từ DB
                var url = await _context.ShortenedUrls
                    .FirstOrDefaultAsync(x => x.ShortCode == shortCode);

                if (url == null)
                {
                    return null;
                }

                originalUrl = url.OriginalUrl;

                // 3. Lưu vào Cache cho lần sau (ví dụ: 1 giờ)
                await _cache.SetStringAsync(shortCode, originalUrl, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                });
            }

            return originalUrl;
        }

        // HÀM ĐÃ SỬA:
        private string GenerateShortCode(int length)
        {
            // Đây là bộ ký tự Base62: Chỉ chữ cái (hoa, thường) và số. Không có dấu.
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var code = new char[length];
            for (int i = 0; i < code.Length; i++)
            {
                code[i] = chars[random.Next(chars.Length)];
            }
            return new string(code);
        }
    }
}