using System.Collections.Generic;
using Microsoft.AspNetCore.Constants;
using Koala.Core;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Microsoft.AspNetCore.Filters
{
    public class ActionHidingConvention : IActionModelConvention
    {
        public static bool? HiddenControllersOverride { get; set; }

        public static List<string> HiddenControllerNames = new List<string>
        {
            "XHealth", "XInfo", "XSwitch", "XTroubleshoot"
        };

        public void Apply(ActionModel action)
        {
            if (HiddenControllersOverride ?? (string.IsNullOrWhiteSpace(EnvVarNames.SHOW_FULL_SWAGGER.GetEnvVarValue()) && new {}.IsReleaseMode()))
            {
                if (HiddenControllerNames.Contains(action.Controller.ControllerName))
                {
                    action.ApiExplorer.IsVisible = false;
                }
            }
        }
    }
}