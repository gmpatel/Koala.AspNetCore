using System.Collections.Generic;
using Microsoft.AspNetCore.Constants;
using Koala.Core;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Microsoft.AspNetCore.Filters
{
    public class ActionHidingConvention : IActionModelConvention
    {
        public static readonly List<string> SystemAlwaysHiddenControllerNames = new List<string>
        {
            "XError", "XHealth", "XInfo", "XRedirect", "XSwitch", "XTroubleshoot"
        };

        public static readonly List<string> AlwaysHiddenControllerNames = new List<string>
        {
            "Health"
        };

        public static readonly List<string> ReleaseBuildHiddenControllerNames = new List<string>
        {
            "TeamTools"
        };

        public static readonly List<string> HiddenControllerNames = new List<string>
        {
            "Helper", "Info", "Switch", "Troubleshoot"
        };

        public void Apply(ActionModel action)
        {
            if (SystemAlwaysHiddenControllerNames.Contains(action.Controller.ControllerName))
            {
                action.ApiExplorer.IsVisible = false;
            }

            if (AlwaysHiddenControllerNames.Contains(action.Controller.ControllerName))
            {
                action.ApiExplorer.IsVisible = false;
            }

            var showFullSwagger = !string.IsNullOrWhiteSpace(EnvVarNames.SHOW_FULL_SWAGGER.GetEnvVarValue());

            if (showFullSwagger == false && this.IsReleaseMode() && ReleaseBuildHiddenControllerNames.Contains(action.Controller.ControllerName))
            {
                action.ApiExplorer.IsVisible = false;
            }

            if (showFullSwagger == false && HiddenControllerNames.Contains(action.Controller.ControllerName))
            {
                action.ApiExplorer.IsVisible = false;
            }
        }
    }
}