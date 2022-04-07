using Microsoft.AspNetCore.Constants;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog.ThrowContext;

namespace Microsoft.AspNetCore.Extensions
{
    public static class SerilogExtensions
    {
        public static Logger GetLogger(this object input, out LoggingLevelSwitch loggingLevelSwitch)
        {
            loggingLevelSwitch = input.GetDefaultLoggingLevelSwitch();

            var logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(loggingLevelSwitch)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.With<ThrowContextEnricher>()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: SetConstants.LogConsoleLogsOutputTemplate, theme: AnsiConsoleTheme.Literate)
                .CreateLogger();

            Log.Logger = logger;

            return logger;
        }

        public static LoggingLevelSwitch GetDefaultLoggingLevelSwitch(this object input)
        {
            return new LoggingLevelSwitch
            {
                MinimumLevel = true ? LogEventLevel.Debug : LogEventLevel.Information //input.IsDebugMode()
            };
        }
    }
}