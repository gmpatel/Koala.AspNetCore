using Koala.Core;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class XTroubleshootController : AbstractController<XTroubleshootController>
    {
        [HttpGet("EnvVar")]
        public IActionResult ErrorStackTraceOverride([FromQuery] string envVarName)
        {
            return Ok(envVarName.GetEnvVarValue());
        }
    }
}