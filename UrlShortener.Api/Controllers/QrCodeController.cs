using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Interfaces;

namespace UrlShortener.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QrCodeController : ControllerBase
    {
        private readonly IQrCodeService _qrService;

        public QrCodeController(IQrCodeService qrService)
        {
            _qrService = qrService;
        }

        [HttpGet]
        public IActionResult Generate(string url)
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest("Missing parameter: url");

            byte[] qrBytes = _qrService.GenerateQrCode(url);

            return File(qrBytes, "image/png");
        }
    }
}
