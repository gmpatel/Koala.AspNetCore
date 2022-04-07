using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog;

namespace Microsoft.AspNetCore.Middlewares
{
    public class RequestAuthorizationMiddleware
    {
        private readonly RequestDelegate next;

        public RequestAuthorizationMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            this.LogExecution(context);

            if (!Authorized(context))
            {
                await MiddlewareConstants.ResponseUnauthorized(context);
            }
            else
            {
                await next(context);
            }

            this.LogExecuted(context);
        }

        private bool Authorized(HttpContext context)
        {
            // Validate OR Authorize straight if there is no authorization require
            if (!MiddlewareConstants.DoesRequireAccessTokenHeader(httpContext: context))
            {
                return true;
            }

            // Validate OR Authorize through valid DataAPI header name
            if (context.Request.Headers.ContainsKey(SetConstants.RequestHeaderNameDataAccessToken))
            {
                return true;
            }

            // Validate OR Authorize through valid X-Access-Token header value
            var accessTokenInHeader = context.Request.Headers.ContainsKey(SetConstants.RequestHeaderNameAccessToken)
                ? context.Request.Headers[SetConstants.RequestHeaderNameAccessToken]
                : new StringValues();

            Log.Logger.Debug("RequestAuthorizationMiddleware Authorizing accessTokenInHeader {@accessTokenInHeader}, headers {@headers}", accessTokenInHeader, context.Request.Headers);

            if (accessTokenInHeader.Count > 0 && accessTokenInHeader.Any(x => x.Equals(SetConstants.RequestHeaderValueAccessToken, StringComparison.InvariantCultureIgnoreCase)))
            {
                return true;
            }

            if (context.Request.Query.TryGetValue(SetConstants.RequestQueryParamNameAccessToken, out var accessTokenInQuery))
            {
                if (accessTokenInQuery.Count > 0 && accessTokenInQuery.Any(x => x.Equals(SetConstants.RequestHeaderValueAccessToken, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return true;
                }
            }

            var authorizationInHeader = context.Request.Headers.ContainsKey(SetConstants.RequestHeaderNameAuthorization)
                ? context.Request.Headers[SetConstants.RequestHeaderNameAuthorization]
                : new StringValues();

            var hostInHeader = context.Request.Headers.ContainsKey(SetConstants.RequestHeaderNameHost)
                ? context.Request.Headers[SetConstants.RequestHeaderNameHost]
                : new StringValues();

            Log.Logger.Debug("RequestAuthorizationMiddleware Authorizing authorizationInHeader {@authorizationInHeader}, hostInHeader {@hostInHeader}", authorizationInHeader, hostInHeader);

            if (hostInHeader.Count > 0 && authorizationInHeader.Count > 0 && hostInHeader.Any(x => SetConstants.RequestHeaderValuesHost.Contains(x.ToLower())) && authorizationInHeader[0].StartsWith("Bearer", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private void LogExecution(HttpContext context)
        {
            Log.Logger.Debug("API Request [{@protocol}] {@method} {@path}{@query}", context.Request.Protocol, context.Request.Method, context.Request.Path.Value, context.Request.QueryString.Value);
        }

        private void LogExecuted(HttpContext context)
        {
            Log.Logger.Information("API Request [{@protocol}] {@method} {@path}{@query} [{@status}]", context.Request.Protocol, context.Request.Method, context.Request.Path.Value, context.Request.QueryString.Value, context.Response.StatusCode);
        }
    }
}