using System;
using Serilog;
using Serilog.Core;

namespace Microsoft.AspNetCore
{
    public abstract class AbstractStartup
    {
        public static Guid AppGuid { get; set; }

        public static ApplicationConfiguration AppConfiguration { get; set; }

        public static ILogger DefaultLogger { get; set; }

        public static LoggingLevelSwitch DefaultLoggingLevelSwitch { get; set; }

        public static LoggerConfiguration DefaultLoggerConfiguration { get; set; }
    }
}