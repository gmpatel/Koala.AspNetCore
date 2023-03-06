using Microsoft.AspNetCore.Constants;
using Microsoft.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class XSwaggerController : ControllerBase
    {
        [HttpGet, Route("/Swagger")]
        public async Task<RedirectResult> RedirectToSwaggerIndex()
        {
            var result = default(RedirectResult);

            await Task.Run(() =>
            {
                result = Redirect($"/{SetConstants.ApiPageUrlPrefix}/index.html");
            });

            return result;
        }

        [HttpGet, Route("/Swagger/Json/{version}")]
        public async Task<RedirectResult> RedirectToSwaggerJson([FromRoute] string version = "v1")
        {
            var result = default(RedirectResult);

            await Task.Run(() =>
            {
                result = Redirect($"/{SetConstants.ApiPageUrlPrefix}/{version}/swagger.json");
            });

            return result;
        }
        
        [HttpGet("/Swagger/SpecialToken")]
        public IActionResult GetProdToken([FromQuery] string accessToken = default)
        {
            accessToken.ValidateSpecialToken(true);
            return Ok(new { AspNetCoreExtensions.SpecialToken });
        }
    }
}