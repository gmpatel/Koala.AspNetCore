using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Test.WebApp
{
    public class Program
    {
        public static readonly ApplicationConfiguration Configuration = new ApplicationConfiguration
        {
            AppPort = 7060,
            AppIdentifier = new Guid("ee07f2fe-f27e-436f-a223-e4b2312c3834"),
            AppFilesRoot = new DirectoryInfo("/deployments"),
            AppArgs = new string[] { },
            HeaderAsCountryCodeEnabled = true,
            ForceGarbageCollectAfterEveryApiCall = true,
            AddBearerTokenSecurityDefinition = true,

            AppApiPageGroups = new List<IList<string>>
            {
                new List<string>
                {
                    nameof(Test),
                }
            }
        };

        public static void Main(string[] args)
        {
            Configuration.AppArgs = args;

            var hostBuilder = Configuration.GetHostBuilder<Startup>(
                out var logger,
                out var loggingLevelSwitch,
                out var configuration
            );

            var host = hostBuilder.Build();

            host.Run();
        }
    }

    public class Startup : AbstractStartup
    {
        public IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureBasicServices(Configuration, DefaultLoggingLevelSwitch);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime)
        {
            app.ConfigureBasicApp();
            
            appLifetime.ApplicationStarted.Register(() =>
            {
                if (app.ApplicationServices.GetService(typeof(ILogger<Startup>)) is ILogger<Startup> logger)
                {
                    logger.LogInformation(@"Press Ctrl+C to shut down.");
                }
            });

            appLifetime.ApplicationStopped.Register(() =>
            {
                Console.WriteLine(@"Shutting down...");
            });
        }
    }
}