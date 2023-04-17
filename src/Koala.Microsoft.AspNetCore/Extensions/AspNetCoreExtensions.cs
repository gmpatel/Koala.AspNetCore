using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Constants;
using Microsoft.AspNetCore.Filters;
using Microsoft.AspNetCore.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Core;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Filters;
using Newtonsoft.Json;
using Koala.Core;
using Microsoft.AspNetCore.Exceptions;
using System.Net;

namespace Microsoft.AspNetCore.Extensions
{
    public static class AspNetCoreExtensions
    {
        public static IList<IList<string>> SwaggerDocsGroups { get; set; } = new List<IList<string>>();

        public static string RedisConnectionStringName { get; set; } = "RedisInstance";

        public static string[] AllowedOrigins { get; set; } = new[]
        {
            "https://develop.app.archistar.io",
            "https://staging.app.archistar.io",
            "https://app.archistar.ai",
            "https://localhost",
            "http://localhost"
        };

        private static string CorsPolicyName { get; set; } = "ArchiSharpAspNetCorsPolicy";

        public static IHostBuilder GetHostBuilder<TStartup>(this ApplicationConfiguration appConfiguration, out ILogger logger, out LoggingLevelSwitch loggingLevelSwitch, out IConfiguration configuration) where TStartup : AbstractStartup
        {
            AbstractStartup.AppConfiguration = appConfiguration;
            AbstractStartup.AppGuid = appConfiguration.AppIdentifier;

            KoalaGlobals.AppIdentifier = appConfiguration.AppIdentifier.ToString();
            KoalaGlobals.AppFilesRoot = appConfiguration.AppFilesRoot == null ? "/tmp" : appConfiguration.AppFilesRoot.FullName;

            if (appConfiguration.AppApiPageGroups != null && appConfiguration.AppApiPageGroups.Any())
            {
                foreach (var appApiPageGroup in appConfiguration.AppApiPageGroups)
                {
                    if (appApiPageGroup != null && appApiPageGroup.Count > 0 && !string.IsNullOrWhiteSpace(appApiPageGroup[0]))
                    {
                        AspNetCoreExtensions.SwaggerDocsGroups.Add(appApiPageGroup);
                    }
                }

                if (AspNetCoreExtensions.SwaggerDocsGroups.Count <= 0)
                {
                    AspNetCoreExtensions.SwaggerDocsGroups.Add(new List<string> { string.Empty.GetVersion() });
                }
            }

            if (appConfiguration.AppAllowedOrigins != null && appConfiguration.AppAllowedOrigins.Length > 0)
            {
                AspNetCoreExtensions.AllowedOrigins = appConfiguration.AppAllowedOrigins;
            }

            foreach (var exceptHiddenControllerName in appConfiguration.AppExceptHiddenControllerNames ?? new List<string>())
            {
                if (exceptHiddenControllerName.Equals("*"))
                {
                    ActionHidingConvention.SystemAlwaysHiddenControllerNames.Clear();
                    ActionHidingConvention.AlwaysHiddenControllerNames.Clear();
                    ActionHidingConvention.ReleaseBuildHiddenControllerNames.Clear();
                    ActionHidingConvention.HiddenControllerNames.Clear();
                }
                else
                {
                    ActionHidingConvention.SystemAlwaysHiddenControllerNames.RemoveAll(n => n.Equals(exceptHiddenControllerName, StringComparison.InvariantCultureIgnoreCase));
                    ActionHidingConvention.AlwaysHiddenControllerNames.RemoveAll(n => n.Equals(exceptHiddenControllerName, StringComparison.InvariantCultureIgnoreCase));
                    ActionHidingConvention.ReleaseBuildHiddenControllerNames.RemoveAll(n => n.Equals(exceptHiddenControllerName, StringComparison.InvariantCultureIgnoreCase));
                    ActionHidingConvention.HiddenControllerNames.RemoveAll(n => n.Equals(exceptHiddenControllerName, StringComparison.InvariantCultureIgnoreCase));
                }
            }

            foreach (var alwaysHiddenControllerName in appConfiguration.AppAppendAlwaysHiddenControllerNames ?? new List<string>())
            {
                ActionHidingConvention.AlwaysHiddenControllerNames.Add(alwaysHiddenControllerName);
            }

            foreach (var releaseBuildHiddenControllerName in appConfiguration.AppAppendReleaseBuildHiddenControllerNames ?? new List<string>())
            {
                ActionHidingConvention.ReleaseBuildHiddenControllerNames.Add(releaseBuildHiddenControllerName);
            }

            foreach (var hiddenControllerName in appConfiguration.AppAppendHiddenControllerNames ?? new List<string>())
            {
                ActionHidingConvention.HiddenControllerNames.Add(hiddenControllerName);
            }

            foreach (var exceptExemptedRequestAuthorizationPath in appConfiguration.AppExceptExemptedRequestAuthorizationPaths ?? new List<string>())
            {
                if (exceptExemptedRequestAuthorizationPath.Equals("*"))
                {
                    MiddlewareConstants.ExemptedPathsFromRequestAuthorization.Clear();
                }
                else
                {
                    MiddlewareConstants.ExemptedPathsFromRequestAuthorization.RemoveAll(n => n.Contains(exceptExemptedRequestAuthorizationPath, StringComparison.InvariantCultureIgnoreCase));
                }
            }

            foreach (var exemptedRequestAuthorizationPath in appConfiguration.AppAppendExemptedRequestAuthorizationPaths ?? new List<string>())
            {
                MiddlewareConstants.ExemptedPathsFromRequestAuthorization.Add(exemptedRequestAuthorizationPath);
            }

            RequestHeadersRequireSwaggerFilter.HeaderXIdEnabled = appConfiguration.HeaderXIdEnabled ?? true;
            RequestHeadersRequireSwaggerFilter.HeaderAccessTokenHeaderEnabled = appConfiguration.HeaderAccessTokenHeaderEnabled ?? true;
            RequestHeadersRequireSwaggerFilter.HeaderAsCountryCodeEnabled = appConfiguration.HeaderAsCountryCodeEnabled ?? false;
            RequestHeadersRequireSwaggerFilter.HeadersCustom = appConfiguration.HeadersCustom;

            MiddlewareConstants.ForceGarbageCollectBeforeEveryApiCall = appConfiguration.ForceGarbageCollectBeforeEveryApiCall;
            MiddlewareConstants.ForceGarbageCollectAfterEveryApiCall = appConfiguration.ForceGarbageCollectAfterEveryApiCall;

            MiddlewareConstants.LogApiCallExecuting = appConfiguration.LogApiCallExecuting;
            MiddlewareConstants.LogApiCallExecuted = appConfiguration.LogApiCallExecuted;

            var resolvedConfiguration = appConfiguration.AppIdentifier.GetAppConfiguration();
            var resolvedLogger = appConfiguration.AppIdentifier.GetLogger(resolvedConfiguration, out var resolvedLoggingLevelSwitch, out var resolvedLoggerConfiguration);

            configuration = resolvedConfiguration;
            logger = resolvedLogger;
            loggingLevelSwitch = resolvedLoggingLevelSwitch;

            AbstractStartup.DefaultLogger = resolvedLogger;
            AbstractStartup.DefaultLoggingLevelSwitch = resolvedLoggingLevelSwitch;
            AbstractStartup.DefaultLoggerConfiguration = resolvedLoggerConfiguration;
            
            var serverPortEnvVarVal = "APP_SERVER_PORT".GetEnvVarValue();
            var portToBeUsed =
                !string.IsNullOrWhiteSpace(serverPortEnvVarVal) && Int32.TryParse(serverPortEnvVarVal, out var portNumberFromEnvVar)
                    ? portNumberFromEnvVar
                    : appConfiguration.AppPort;

            logger.Information($"APP_ID = '{appConfiguration.AppIdentifier}'");
            logger.Information($"APP_SERVER_PORT = '{serverPortEnvVarVal}', portUsed = {portToBeUsed}");
            logger.Information($"APP_ENVIRONMENT = '{"APP_ENVIRONMENT".GetEnvVarValue()}'");
            logger.Information($"LOG_EVENT_LEVEL = '{"LOG_EVENT_LEVEL".GetEnvVarValue()}', loggingLevelSwitch = {loggingLevelSwitch.MinimumLevel}");

            var webHostBuilder = Host.CreateDefaultBuilder(appConfiguration.AppArgs)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseUrls($"http://*:{appConfiguration.AppPort}")
                        .UseKestrel(options =>
                        {
                            options.Limits.MaxRequestBodySize = long.MaxValue;
                        })
                        .UseConfiguration(resolvedConfiguration)
                        .UseSerilog(resolvedLogger)
                        .UseStartup(typeof(TStartup));
                });

            Console.OutputEncoding = Encoding.UTF8;

            return webHostBuilder;
        }
        
        public static IServiceCollection ConfigureBasicServices(this IServiceCollection services, IConfiguration configuration, LoggingLevelSwitch loggingLevelSwitch, IList<Assembly> assemblies = null)
        {
            services.AddSingleton(configuration);
            services.AddSingleton(loggingLevelSwitch);
            
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                        .SetIsOriginAllowed(
                            origin => AllowedOrigins.Contains(
                                origin,
                                StringComparer.InvariantCultureIgnoreCase
                            )
                        )
                        .AllowCredentials()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });

                options.AddPolicy(CorsPolicyName, builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
            
            services.AddResponseCompression();
            
            services
                .AddControllers(options =>
                {
                    options.Conventions.Add(new ActionHidingConvention());
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.GetOrMergeDefaultJsonSerializerSettings();
                });

            services
                .AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.GetOrMergeDefaultJsonSerializerSettings();
                });

            services
                .AddRazorPages()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.GetOrMergeDefaultJsonSerializerSettings();
                });

            services
                .AddMvc()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.GetOrMergeDefaultJsonSerializerSettings();
                });

            var redisConfigurationOptions = configuration.GetRedisConfigurationOptions(RedisConnectionStringName);

            if (redisConfigurationOptions != null)
            {
                services
                    .AddStackExchangeRedisCache(options =>
                    {
                        options.ConfigurationOptions = redisConfigurationOptions;
                    });

                services
                    .AddSingleton<IConnectionMultiplexer>(
                        ConnectionMultiplexer.Connect(redisConfigurationOptions)
                    );
            }

            services.AddMemoryCache();

            services.AddHttpContextAccessor();

            services.AddSwaggerGen(c =>
            {
                try
                {
                    var xmlFile = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
                    var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                    foreach (var swaggerDocsGroup in SwaggerDocsGroups)
                    {
                        var groupName = swaggerDocsGroup.Count > 0 ? $"{swaggerDocsGroup[0]}".Trim() : string.Empty;
                        var groupDescription = swaggerDocsGroup.Count > 1 ? $"{swaggerDocsGroup[1]} {SetConstants.ApiPageV1Description}".Trim() : SetConstants.ApiPageV1Description;

                        if (string.IsNullOrWhiteSpace(groupName))
                            throw new InvalidDataException("Provided SwaggerDocsGroup configuration is invalid");

                        c.SwaggerDoc(groupName, new OpenApiInfo
                        {
                            Version = services.GetVersion(),
                            Title = GetSwaggerPageTitle(groupName),
                            Description = $"v{new { }.GetVersion()} [.Net 6.0] [ASP.Net Core]<br><br>{groupDescription}<br><br>{new { }.GetContainerIPsString()}<br>{new { }.GetHostNamesString()}",
                            TermsOfService = new Uri(SetConstants.ApiPageTermsLink),
                            Contact = new OpenApiContact
                            {
                                Name = SetConstants.ApiPageContactName,
                                Url = new Uri(SetConstants.ApiPageContactUrl),
                                Email = SetConstants.ApiPageContactEmail
                            }
                        });
                    }

                    //c.SwaggerDoc(SetConstants.ApiPageV1Version, new OpenApiInfo
                    //{
                    //    Version = services.GetVersion(),
                    //    Title = SwaggerPageTitle ?? SetConstants.ApiPageV1Title,
                    //    Description = $"v{new{}.GetVersion()}<br><br>{SwaggerPageDescription ?? SetConstants.ApiPageV1Description}<br><br>{new {}.GetContainerIPsString()}<br>{new {}.GetHostNamesString()}",
                    //    TermsOfService = new Uri(SetConstants.ApiPageTermsLink),
                    //    Contact = new OpenApiContact
                    //    {
                    //        Name = SetConstants.ApiPageContactName,
                    //        Url = new Uri(SetConstants.ApiPageContactUrl),
                    //        Email = SetConstants.ApiPageContactEmail
                    //    }
                    //});

                    c.ExampleFilters();

                    c.SchemaFilter<SchemaFilter>();
                    c.DocumentFilter<SwaggerPathPrefixFilter>();
                    c.OperationFilter<UploadFileOperationFilter>();
                    
                    if (AbstractStartup.AppConfiguration?.AddBearerTokenSecurityDefinition ?? false)
                    {
                        c.AddSecurityDefinition("BearerAuth", new OpenApiSecurityScheme
                        {
                            Type = SecuritySchemeType.Http,
                            Scheme = "bearer",
                            BearerFormat = "JWT",
                            Description = "Authorization header using the Bearer scheme"
                        });

                        c.AddSecurityRequirement(new OpenApiSecurityRequirement
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "BearerAuth" }
                                },
                                AbstractStartup.AppConfiguration?.DefaultBearerSecurityTokenResolver == null
                                    ? new string[] {}
                                    : new List<string>{ AbstractStartup.AppConfiguration.DefaultBearerSecurityTokenResolver() }.ToArray()
                            }
                        });
                    }

                    if (File.Exists(xmlFilePath))
                    {
                        c.IncludeXmlComments(xmlFilePath);
                    }

                    c.OperationFilter<RequestHeadersRequireSwaggerFilter>();
                }
                catch(Exception ex)
                {
                    Log.Logger.Error(ex, ex.Message);
                }
            });

            // Register Swagger Newtonsoft Support
            services.AddSwaggerGenNewtonsoftSupport();

            // Swagger Request Examples
            if (assemblies != null && assemblies.Any())
            {
                services.AddSwaggerExamplesFromAssemblies(assemblies.ToArray());
            }
            else
            {
                services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());
            }
            
            return services;
        }

        public static IApplicationBuilder ConfigureBasicApp(this IApplicationBuilder app)
        {
            var rewriteOptions = new RewriteOptions()
                .AddRewrite("/api(.*)", "/api$1", true);

            app.UseRewriter(rewriteOptions);
            app.UseExceptionHandler($"/api/xerror/{new {}.IsDebugMode()}");
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors(CorsPolicyName);
            app.UseAuthorization();
            app.UseResponseCompression();
            
            app.UseMiddleware(typeof(RequestAuthorizationMiddleware));
            app.UseMiddleware(typeof(DefaultHeadersMiddleware));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers()
                    .RequireCors(
                        CorsPolicyName
                    );
            });
            
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                try
                {
                    foreach (var swaggerDocsGroup in SwaggerDocsGroups)
                    {
                        var groupName = swaggerDocsGroup.FirstOrDefault();

                        if (string.IsNullOrWhiteSpace(groupName))
                            throw new InvalidDataException("Provided SwaggerDocsGroup configuration is invalid");

                        c.SwaggerEndpoint($"{groupName}/{SetConstants.ApiPageUrlPrefix}.json", groupName);
                        c.RoutePrefix = SetConstants.ApiPageUrlPrefix;
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, ex.Message);
                }
            });

            return app;
        }

        public static ConfigurationOptions GetRedisConfigurationOptions(this IConfiguration configuration, string name)
        {
            var redisConnection = configuration.GetConnectionString(name);
            var redisEndpoint = default(string);
            var redisPassword = default(string);

            if (!string.IsNullOrWhiteSpace(redisConnection))
            {
                var redisConnectionInfo = redisConnection.Split(";").ForEach(x => x.Trim());

                redisEndpoint = redisConnectionInfo.Count >= 1
                    ? redisConnectionInfo[0]
                    : null;

                redisPassword = redisConnectionInfo.Count >= 2
                    ? redisConnectionInfo[1]
                    : null;
            }

            if (!string.IsNullOrWhiteSpace(redisEndpoint))
            {
                return new ConfigurationOptions
                {
                    EndPoints = { redisEndpoint },
                    Password = redisPassword,
                    AbortOnConnectFail = false,
                    ConnectTimeout = 1000,
                };
            }

            return null;
        }

        public static IConfigurationRoot GetAppConfiguration(this object input)
        {
            var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", true)
                    .AddJsonFile($"appsettings.{GetAppEnvironment()}.json", true)
                    .AddEnvironmentVariables()
                    .Build();

            return configuration;
        }

        public static readonly string SpecialToken = string.Empty.GenerateRandom(36);

        public static bool ValidateSpecialToken(this string specialToken, bool? throwOnFailure = default)
        {
            var result = $"{specialToken}".Equals(SpecialToken, StringComparison.InvariantCulture) || $"{specialToken}".Equals(Properties.Resources.AccessToken, StringComparison.InvariantCulture);

            if ((throwOnFailure ?? true) && !result)
            {
                throw new HttpStatusException(HttpStatusCode.Unauthorized, SetConstants.ExceptionMessageSpecialTokenUnauthorized);
            }

            return result;
        }

        private static string GetSwaggerPageTitle(string groupName)
        {
            return $"{groupName} Service - {GetAppEnvironment()}";
        }

        private static string GetSwaggerPageDescription(string groupName, string groupDescription)
        {
            return $"{groupName} Service - {GetAppEnvironment()}";
        }

        private static string GetAppEnvironment()
        {
            return "APP_ENVIRONMENT".GetEnvVarValue(null, new {}.IsDebugMode() ? "Debug" : "Release");
        }
    }
}