using Microsoft.AspNetCore.Mvc;

namespace SOF1022_NHOM1_SD2003_Ver2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShareController : ControllerBase
    {
        // GET: api/Share/links
        [HttpGet("links")]
        public IActionResult GetShareLinks(string url)
        {
            var encodedUrl = System.Net.WebUtility.UrlEncode(url);
            
            return Ok(new
            {
                success = true,
                links = new
                {
                    facebook = $"https://www.facebook.com/sharer/sharer.php?u={encodedUrl}",
                    twitter = $"https://twitter.com/intent/tweet?url={encodedUrl}",
                    zalo = $"https://zalo.me/share?url={encodedUrl}",
                    linkedin = $"https://www.linkedin.com/sharing/share-offsite/?url={encodedUrl}",
                    email = $"mailto:?subject=Check out this book&body=I found this interesting book: {url}"
                }
            });
        }
    }
}
