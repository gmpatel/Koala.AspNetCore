using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class XHealthController : AbstractController<XErrorController>
    {
        [HttpGet("YouGood")]
        public IActionResult GetYouGood()
        {
            return Ok("Ok");
        }
    }
}