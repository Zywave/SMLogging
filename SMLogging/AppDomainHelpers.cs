using System;
using System.Text.RegularExpressions;

namespace SMLogging
{ 
    internal static class AppDomainHelpers
    {
        public static string GetFriendlyName(this AppDomain appDomain)
        {
            var name = appDomain.FriendlyName;
            if (name.StartsWith("/LM/W3SVC/"))
            { 
                var match = _iisAppNameExpressionRegex.Match(name);
                if (match.Success)
                {
                    var site = match.Groups["site"].Value;
                    var path = match.Groups["path"].Success ? match.Groups["path"].Value : String.Empty;
                    path = path.Replace("/", "_");
                    name = $"W3SVC{site}{path}";
                }
            }
            return name;
        }

        private static readonly Regex _iisAppNameExpressionRegex = new Regex(@"^/LM/W3SVC/(?<site>\d+)/ROOT(?<path>.*)?(?:-\d+-\d+)$");
    }
}
