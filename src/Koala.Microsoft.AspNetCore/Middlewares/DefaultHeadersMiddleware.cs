using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Constants;
using Koala.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Middlewares
{
    public class DefaultHeadersMiddleware
    {
        private readonly RequestDelegate next;
        private readonly Stopwatch watch;

        private static readonly Func<string> funcGetNewRequestId = () => string.Empty.GenerateUniqueString();
        private static readonly Func<string> funcGetNewRequestCorrelationId = () => Guid.NewGuid().ToString();

        public DefaultHeadersMiddleware(RequestDelegate next)
        {
            this.next = next;
            this.watch = new Stopwatch();
        }

        public async Task Invoke(HttpContext context)
        {
            watch.Restart();

            var requestId = GetRequestHeaderValue(
                context,
                SetConstants.RequestHeaderNameId,
                funcGetNewRequestId
            );

            var requestCorrelationId = GetRequestHeaderValue(
                context,
                SetConstants.RequestHeaderNameCorrelationId,
                funcGetNewRequestCorrelationId
            );

            context.Response.OnStarting(state =>
            {
                var httpContext = (HttpContext)state;
                var relativePath = MiddlewareConstants.GetRelativePath(httpContext);

                if (!httpContext.Response.Headers.ContainsKey(SetConstants.ContentTypeHeaderName))
                {
                    httpContext.Response.Headers.Add(
                        SetConstants.ContentTypeHeaderName, new[] { SetConstants.ContentTypeHeaderValueJson }
                    );
                }

                if (!httpContext.Response.Headers.ContainsKey(SetConstants.ResponseHeaderNameId))
                {
                    httpContext.Response.Headers.Add(
                        SetConstants.ResponseHeaderNameId, new[] { requestId }
                    );
                }

                if (!httpContext.Response.Headers.ContainsKey(SetConstants.ResponseHeaderNameCorrelationId))
                {
                    httpContext.Response.Headers.Add(
                        SetConstants.ResponseHeaderNameCorrelationId, new[] { requestCorrelationId }
                    );
                }

                if (!httpContext.Response.Headers.ContainsKey(SetConstants.ResponseHeaderNameHostIpAddresses))
                {
                    httpContext.Response.Headers.Add(
                        SetConstants.ResponseHeaderNameHostIpAddresses, new[] { $"{ this.GetContainerIPsString() }" }
                    );
                }

                if (!httpContext.Response.Headers.ContainsKey(SetConstants.ResponseHeaderNameHostNames))
                {
                    httpContext.Response.Headers.Add(
                        SetConstants.ResponseHeaderNameHostNames, new[] { $"{ this.GetHostNamesString() }" }
                    );
                }

                watch.Stop();

                if (!httpContext.Response.Headers.ContainsKey(SetConstants.ResponseHeaderNameProcessingTime))
                {
                    httpContext.Response.Headers.Add(
                        SetConstants.ResponseHeaderNameProcessingTime, new[] { $"{watch.ElapsedMilliseconds} ms" }
                    );
                }

                if (!httpContext.Response.Headers.ContainsKey(SetConstants.ResponseHeaderNameAccessControlExposeHeaders))
                {
                    var headerValues = new List<string>
                    {
                        SetConstants.ResponseHeaderNameId,
                        SetConstants.ResponseHeaderNameCorrelationId,
                        SetConstants.ResponseHeaderNameProcessingTime,
                        SetConstants.ResponseHeaderNameHostIpAddresses,
                        SetConstants.ResponseHeaderNameHostNames,
                    };

                    httpContext.Response.Headers.Add(
                        SetConstants.ResponseHeaderNameAccessControlExposeHeaders, new[] { $"{string.Join(", ", headerValues) }" }
                    );
                }

                if (!httpContext.Response.Headers.ContainsKey(SetConstants.ResponseHeaderNameAccessControlAllowOrigin))
                {
                    var headerValues = new List<string>();

                    if (httpContext.Request.Headers.ContainsKey(SetConstants.RequestHeaderNameOrigin))
                    {
                        var headerValue = httpContext.Request.Headers[SetConstants.RequestHeaderNameOrigin].FirstOrDefault();

                        if (!string.IsNullOrWhiteSpace(headerValue))
                        {
                            headerValues.Add(headerValue);
                        }
                    }

                    if (headerValues.Count > 0)
                    {
                        httpContext.Response.Headers.Add(
                            SetConstants.ResponseHeaderNameAccessControlAllowOrigin, new[] { headerValues.First() }
                        );
                    }
                }

                return Task.CompletedTask;

            }, context);

            var logger = context.RequestServices.GetRequiredService<ILogger<DefaultHeadersMiddleware>>();

            using (logger.BeginScope("{@X-Id} {@X-Correlation-Id}", requestId, requestCorrelationId))
            {
                await next(context);
            }
        }

        private string GetRequestHeaderValue(HttpContext context, string headerName, Func<string> funcDefaultValue = null)
        {
            var headerValue = context.Request.Headers.ContainsKey(headerName) && context.Request.Headers[headerName].Count > 0
                ? context.Request.Headers[headerName].FirstOrDefault()
                : default;

            if (string.IsNullOrWhiteSpace(headerValue) && funcDefaultValue != null)
            {
                headerValue = funcDefaultValue();
                context.Request.Headers.Remove(headerName);
                context.Request.Headers.Add(headerName, headerValue);
            }

            context.Items[headerName] = headerValue;

            return headerValue;
        }
    }
}