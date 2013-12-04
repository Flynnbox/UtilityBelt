using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Configuration;

namespace UtilityBelt
{
  public class UrlContentParser
  {
    private static readonly Regex urlRegex;
		private static readonly string appSiteReplacement;
		private static readonly string publicSiteReplacement;

		static UrlContentParser()
    {
			urlRegex = new Regex("^/", RegexOptions.Compiled);
			appSiteReplacement = WebConfigurationManager.AppSettings["SiteUrl"] + "/";
			publicSiteReplacement = WebConfigurationManager.AppSettings["PublicSiteUrl"] + "/";
    }
	
		public static string Parse(string content)
		{
			return string.IsNullOrEmpty(content) ? content : urlRegex.Replace(content, m => appSiteReplacement);
		}

		public static string ParseForEnvironment(string content, bool usePublicSiteSettings)
		{
			if (usePublicSiteSettings)
			{
				return string.IsNullOrEmpty(content) ? content : urlRegex.Replace(content, m => publicSiteReplacement);
			}
			return string.IsNullOrEmpty(content) ? content : urlRegex.Replace(content, m => appSiteReplacement);
		}
  }
}