using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Middlewares;
using Koala.Core;
using Microsoft.AspNetCore.Constants;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.AspNetCore.Filters
{
    public class RequestHeadersRequireSwaggerFilter : IOperationFilter
    {
        public static bool HeaderXIdEnabled { get; set; } = true;

        public static bool HeaderAccessTokenHeaderEnabled { get; set; } = true;

        public static bool HeaderAsCountryCodeEnabled { get; set; } = false;

        public static IList<string> HeadersCustom { get; set; } = new List<string>
        {
        };

        public static string PersistXIdFileName { get; set; } = "xid";

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (MiddlewareConstants.DoesRequireAccessTokenHeader(operationFilterContext: context))
            {
                if (HeaderAccessTokenHeaderEnabled)
                {
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = SetConstants.RequestHeaderNameAccessToken,
                        In = ParameterLocation.Header,
                        Required = true,
                        Example = new OpenApiString(
                            new { }.IsDebugMode()
                                ? SetConstants.RequestHeaderValueAccessToken
                                : SetConstants.RequestHeaderValueAccessToken.Substring(0, SetConstants.RequestHeaderValueAccessToken.Length - 3)
                        )
                    });
                }
            }


            if (HeaderAsCountryCodeEnabled)
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = SetConstants.RequestHeaderNameAsCountryCode,
                    In = ParameterLocation.Header,
                    Required = false,
                    Example = new OpenApiString(
                        SetConstants.RequestHeaderValueAsCountryCode
                    )
                });
            }

            if (HeaderXIdEnabled)
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = SetConstants.RequestHeaderNameId,
                    In = ParameterLocation.Header,
                    Required = false,
                    Example = !MiddlewareConstants.DoesRequireXIdHeaderValueFilled(operationFilterContext: context)
                        ? new OpenApiString(string.Empty)
                        : GetXIdValue()
                });
            }

            if (HeadersCustom != null && HeadersCustom.Any())
            {
                foreach (var customHeader in HeadersCustom)
                {
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = customHeader,
                        In = ParameterLocation.Header,
                        Required = false,
                        Example = new OpenApiString(string.Empty)
                    });
                }
            }
        }

        private static readonly object XIdNewLock = new object();

        private static string XId;

        private static OpenApiString GetXIdValue()
        {
            if (string.IsNullOrWhiteSpace(XId))
            {
                lock (XIdNewLock)
                {
                    if (string.IsNullOrWhiteSpace(XId))
                    {
                        var sample = $"{PersistXIdFileName}.sist".GetFromFile<dynamic>(() => new
                        {
                            sessionId = string.Empty.GenerateUniqueString()
                        });

                        XId = sample.sessionId;
                    }
                }
            }

            return new OpenApiString(XId);
        }
    }
}