using System.Net;
using ArchiSharp.Core.Exceptions;
using ArchiSharp.Core.Models.Responses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class XErrorController : AbstractController<XErrorController>
    {
        [Route("{stackTrace}")]
        public ErrorResponse Error(bool stackTrace)
        {
            var status = HttpStatusCode.InternalServerError;
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context?.Error;

            if (exception is HttpStatusException httpException)
            {
                status = httpException.Status;
            }

            Response.StatusCode = (int)status;
            Response.Headers.Add("X-StackTrace-Enabled", StackTraceOverride == null ? $"{stackTrace}" : $"{stackTrace} [{StackTraceOverride}]");

            return new ErrorResponse(exception, status, StackTraceOverride ?? stackTrace);
        }

        public static bool? StackTraceOverride { get; set; }
    }
}