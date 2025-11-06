using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Interfaces
{
    // Đây là "bản hợp đồng" cho DbContext
    public interface IAppDbContext
    {
        // Nó phải có 1 bảng ShortenedUrls
        DbSet<ShortenedUrl> ShortenedUrls { get; set; }

        // Và nó phải có khả năng lưu thay đổi
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}