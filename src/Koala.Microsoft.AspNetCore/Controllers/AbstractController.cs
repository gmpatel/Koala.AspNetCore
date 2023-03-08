using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Linq;
using System.Threading.Tasks;
using System;
using Koala.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Constants;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Exceptions;

namespace Microsoft.AspNetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class AbstractController<T> : ControllerBase
    {
        public ILogger<T> Logger { get; private set; }
        
        protected AbstractController()
        {
        }

        protected AbstractController(ILogger<T> logger) : base()
        {
            this.Logger = logger;
        }

        protected async Task<IActionResult> ReturnProxyResponse(HttpResponseMessage response, Uri proxy = default, string forwardedPath = default, HttpClientMethods? forwardedMethod = default)
        {
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content;

                if (proxy != null)
                {
                    Response.Headers.Add(SetConstants.ResponseHeaderNameForwardedToServer, proxy.ToString());

                    if (!string.IsNullOrWhiteSpace(forwardedPath))
                    {
                        Response.Headers.Add(SetConstants.ResponseHeaderNameForwardedToPath, $"{forwardedMethod}:{new Uri(proxy, forwardedPath)}");
                    }
                }

                if (content.Headers.Contains(SetConstants.ContentTypeHeaderName) && content.Headers.Contains(SetConstants.ResponseHeaderNameContentDisposition))
                {
                    var contentStream = await content.ReadAsStreamAsync();
                    var contentType = content.Headers.GetValues(SetConstants.ContentTypeHeaderName).FirstOrDefault();
                    var contentDispositionString = content.Headers.GetValues(SetConstants.ResponseHeaderNameContentDisposition).FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(contentType) && !string.IsNullOrWhiteSpace(contentDispositionString))
                    {
                        var contentDisposition = new ContentDisposition(contentDispositionString);

                        if (contentDisposition.DispositionType.Equals("attachment", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return File(contentStream, contentType, contentDisposition.FileName);
                        }
                    }
                }
                else if (content.Headers.Contains(SetConstants.ContentTypeHeaderName) && content.Headers.GetValues(SetConstants.ContentTypeHeaderName).First().Equals(SetConstants.ContentTypeHeaderValueJson))
                {
                    return Content(await content.ReadAsStringAsync(), SetConstants.ContentTypeHeaderValueJson);
                }
            }

            throw new HttpStatusException(response.StatusCode, await response.Content.ReadAsStringAsync());
        }
    }
}