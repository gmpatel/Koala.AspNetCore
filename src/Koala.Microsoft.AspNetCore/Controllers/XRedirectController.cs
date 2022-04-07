using System.Threading.Tasks;
using Microsoft.AspNetCore.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class XRedirectController : Controller
    {
        [Route(""), HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<RedirectResult> RedirectToIndexHtml()
        {
            var result = default(RedirectResult);

            await Task.Run(() => { result = Redirect($"/index.html"); });

            return result;
        }

        [Route("doc"), HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<RedirectResult> RedirectToSwaggerUiV1()
        {
            var result = default(RedirectResult);

            await Task.Run(() =>
            {
                result = Redirect($"/{SetConstants.ApiPageUrlPrefix}"); /* /index.html?urls.primaryName={Constants.ApiPageV1Version} */
            });

            return result;
        }

        [Route("yougood"), HttpGet] [Route("health"), HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult RedirectToYouGoodHealthApi()
        {
            return Ok("Ok");
        }
    }
}