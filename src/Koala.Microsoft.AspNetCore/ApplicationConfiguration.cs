using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore
{
    public class ApplicationConfiguration
    {
        public Guid AppIdentifier { get; set; }

        public IList<IList<string>> AppApiPageGroups { get; set; }

        public int AppPort { get; set; }

        public string[] AppAllowedOrigins { get; set; }

        public string[] AppArgs { get; set; }

        public IList<string> AppAppendAlwaysHiddenControllerNames { get; set; }

        public IList<string> AppAppendReleaseBuildHiddenControllerNames { get; set; }

        public IList<string> AppAppendHiddenControllerNames { get; set; }

        public IList<string> AppExceptHiddenControllerNames { get; set; }

        public IList<string> AppAppendExemptedRequestAuthorizationPaths { get; set; }

        public IList<string> AppExceptExemptedRequestAuthorizationPaths { get; set; }

        public bool? AddBearerTokenSecurityDefinition { get; set; }

        public bool? HeaderXIdEnabled { get; set; }

        public bool? HeaderAccessTokenHeaderEnabled { get; set; }

        public bool? HeaderAsCountryCodeEnabled { get; set; }

        public bool? ForceGarbageCollectBeforeEveryApiCall { get; set; }

        public bool? ForceGarbageCollectAfterEveryApiCall { get; set; }

        public bool? LogApiCallExecuting { get; set; }

        public bool? LogApiCallExecuted { get; set; }

        public IList<string> HeadersCustom { get; set; }

        public bool? PrependCustomSwaggerEndpoints { get; set; }

        public IList<SwaggerEndpoint> CustomSwaggerEndpoints { get; set; }

        public Func<string> DefaultBearerSecurityTokenResolver { get; set; }
    }

    public class SwaggerEndpoint
    {
        public string Title { get; set; }

        public string SwaggerJsonUrl { get; set; }
    }
}