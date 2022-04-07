namespace Microsoft.AspNetCore.Constants
{
    public class SetConstants
    {
        public const string DefaultAppSettingsFileName = "appsettings.json";

        public const string ContentTypeHeaderName = "Content-Type";
        public const string ContentTypeHeaderValueJson = "application/json";

        public const string ApiPageTermsLink = "https://archistar.ai/terms";
        public const string ApiPageUrlPrefix = "swagger";
        public const string ApiPageV1Title = "API Server";
        public const string ApiPageV1Description = "API Server runs on DotNetCore Light-Weight WebServer Kestrel and the background jobs runs through Quartz HostedJobs on Kestrel. Jobs are still be running with same experience of modes using command-line arguments provided as well as through an API using HTTP request.";
        public const string ApiPageV1Version = "v1";
        public const string ApiPageV1JsonPath = "v1/swagger.json";
        public const string ApiPageContactName = "Archistar TechTeam";
        public const string ApiPageContactUrl = "https://archistar.ai";
        public const string ApiPageContactEmail = "support@archistar.ai";

        public const string RequestHeaderNameOrigin = "Origin";
        public const string RequestHeaderNameId = "X-Id";
        public const string RequestHeaderNameCorrelationId = "X-Correlation-Id";

        public const string ResponseHeaderNameId = "X-Id";
        public const string ResponseHeaderNameCorrelationId = "X-Correlation-Id";
        public const string ResponseHeaderNameProcessingTime = "X-Processing-Time";
        public const string ResponseHeaderNameHostIpAddresses = "X-Host-IP-Addresses";
        public const string ResponseHeaderNameHostNames = "X-Host-Names";

        public const string ResponseHeaderNameForwardedToServer = "X-Forwarded-To-Server";
        public const string ResponseHeaderNameForwardedToPath = "X-Forwarded-To-Path";

        public const string ResponseHeaderNameContentDisposition = "Content-Disposition";

        public const string ResponseHeaderNameAccessControlExposeHeaders = "Access-Control-Expose-Headers";
        public const string ResponseHeaderNameAccessControlAllowOrigin = "Access-Control-Allow-Origin";

        public const string RequestQueryParamNameAccessToken = "accessToken";
        public const string RequestHeaderNameAccessToken = "X-Access-Token";
        public const string RequestHeaderValueAccessToken = "aa2a368c03044b15a228dfc6bc639efe";

        public const string RequestHeaderNameAsCountryCode = "X-AS-Country-Code";
        public const string RequestHeaderValueAsCountryCode = "AU";

        public const string RequestHeaderName2AsCountryCode = "AS-Country-Code";
        public const string RequestHeaderValue2AsCountryCode = "AU";
        
        public const string RequestHeaderNameHost = "Host";
        public static readonly string[] RequestHeaderValuesHost = new[] { "gen.dev.au.archistar.io", "gen.prod.au.archistar.ai" };

        public const string RequestHeaderNameAuthorization = "Authorization";

        public const string RequestHeaderNameDataAccessToken = "Q4o8AnQkloD884f6ErfCHp3Dc";

        public const string LogConsoleLogsOutputTemplate = "{NewLine}[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}Properties: {Properties}{NewLine}{Exception}";
        public const string LogDebugLogsOutputTemplate = "{NewLine}[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}Properties: {Properties}{NewLine}{Exception}";
    }
}