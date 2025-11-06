using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.DTOs;
using UrlShortener.Application.Interfaces;
using Microsoft.AspNetCore.RateLimiting;

namespace UrlShortener.Api.Controllers
{
    [EnableRateLimiting("ApiLimiter")]
    [ApiController]
    public class ShortenerController : ControllerBase
    {
        private readonly IUrlShorteningService _urlService;

        // Tiêm (Inject) Service "não" vào
        public ShortenerController(IUrlShorteningService urlService)
        {
            _urlService = urlService;
        }

        // Endpoint [POST] /api/shorten: Dùng để TẠO link
        [HttpPost("api/shorten")]
        public async Task<IActionResult> ShortenUrl([FromBody] ShortenUrlRequest request)
        {
            // Tự động kiểm tra validation (từ DTO)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var shortenedUrl = await _urlService.CreateShortUrlAsync(request.Url);

            if (shortenedUrl == null)
            {
                return BadRequest("Không thể tạo link rút gọn.");
            }

            // Trả về kết quả cho React
            var result = new
            {
                // Link trả về sẽ có dạng: https://localhost:7123/aB1cD9e
                shortUrl = $"{Request.Scheme}://{Request.Host}/{shortenedUrl.ShortCode}"
            };
            return Ok(result);
        }

        // Endpoint [GET] /{shortCode}: Dùng để CHUYỂN HƯỚNG (Redirect)
        [HttpGet("{shortCode}")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RedirectToOriginalUrl(string shortCode)
        {
            var originalUrl = await _urlService.GetOriginalUrlByCodeAsync(shortCode);

            if (originalUrl == null)
            {
                // Nếu không tìm thấy, trả về 404
                return NotFound("Không tìm thấy link này.");
            }

            // Nếu tìm thấy, chuyển hướng trình duyệt (Redirect 302 - Found)
            return Redirect(originalUrl);
        }
    }
}