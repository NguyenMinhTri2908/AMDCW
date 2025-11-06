using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Interfaces
{
    // Đây là "hợp đồng" cho Service của chúng ta
    public interface IUrlShorteningService
    {
        // Nhận vào link gốc, trả về link đã rút gọn
        Task<ShortenedUrl?> CreateShortUrlAsync(string originalUrl);

        // Nhận vào mã ngắn, trả về link gốc
        Task<string?> GetOriginalUrlByCodeAsync(string shortCode);
    }
}