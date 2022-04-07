using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;

namespace Microsoft.AspNetCore.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class XSwitchController : AbstractController<XSwitchController>
    {
        private LoggingLevelSwitch loggingLevelSwitch { get; set; }

        public XSwitchController(LoggingLevelSwitch loggingLevelSwitch, ILogger<XSwitchController> logger) : base(logger)
        {
            this.loggingLevelSwitch = loggingLevelSwitch;
        }

        [HttpGet("Error/StackTrace/{overrideFlag}")]
        public IActionResult ErrorStackTraceOverride(bool overrideFlag)
        {
            return Ok(new { stackTrace = (XErrorController.StackTraceOverride = overrideFlag) });
        }

        /// <summary>
        /// Overrides the default LogEventLevel for Serilog ILogger injection for this instance
        /// </summary>
        /// <param name="overrideFlag">OverrideFlag</param>      
        [HttpGet("ILogger/LoggingLevel")]
        public IActionResult LoggingLevelSwitchOverride([FromQuery] LogEventLevel? overrideFlag)
        {
            if (overrideFlag != null)
            {
                this.loggingLevelSwitch.MinimumLevel = overrideFlag.Value;
                return Ok(new { updatedLoggingLevelSwitch = this.loggingLevelSwitch });
            }

            return Ok(new { currentLoggingLevelSwitch = loggingLevelSwitch });
        }
    }
}