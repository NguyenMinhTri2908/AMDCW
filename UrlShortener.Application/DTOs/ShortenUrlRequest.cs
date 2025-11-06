using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Application.DTOs
{
    // Đây là model cho request mà React sẽ gửi lên
    public class ShortenUrlRequest
    {
        [Required(ErrorMessage = "URL là bắt buộc")]
        [Url(ErrorMessage = "URL không hợp lệ")] // Tự động kiểm tra link
        public string Url { get; set; } = string.Empty;
    }
}