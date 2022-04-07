using System.Linq;
using Microsoft.AspNetCore.Constants;
using Koala.Core;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.AspNetCore.Filters
{
    public class SwaggerPathPrefixFilter : IDocumentFilter
    {
        private readonly string prefix;

        public SwaggerPathPrefixFilter()
        {
            var gatewayId = EnvVarNames.GATEWAY_IDENTIFIER.GetEnvVarValue();
            this.prefix = gatewayId;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (!string.IsNullOrEmpty(prefix) && !this.IsDebugMode())
            {
                var paths = swaggerDoc.Paths.Keys.ToList();

                foreach (var path in paths)
                {
                    var pathToChange = swaggerDoc.Paths[path];

                    swaggerDoc.Paths.Remove(path);
                    swaggerDoc.Paths.Add($"/{prefix}{path}", pathToChange);
                }
            }
        }
    }
}