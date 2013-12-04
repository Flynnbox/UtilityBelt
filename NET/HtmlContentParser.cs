using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.Configuration;

namespace UtilityBelt
{
  public class HtmlContentParser
  {
    //TODO: Move list of replacement strings into .config
		private static readonly Dictionary<string, string> appSiteReplacements = new Dictionary<string, string>();
		private static readonly Dictionary<string, string> publicSiteReplacements = new Dictionary<string, string>();
    private static readonly Regex appSiteCombinedRegex;
		private static readonly Regex publicSiteCombinedRegex;

    static HtmlContentParser()
    {
			PopulateDictionaryWithCharacterCodes(appSiteReplacements);
			PopulateDictionaryWithAppSiteSpecialCases(appSiteReplacements);
			appSiteCombinedRegex = new Regex(String.Join("|", appSiteReplacements.Keys), RegexOptions.Compiled);

			PopulateDictionaryWithCharacterCodes(publicSiteReplacements);
			PopulateDictionaryWithPublicSiteSpecialCases(publicSiteReplacements);
			publicSiteCombinedRegex = new Regex(String.Join("|", publicSiteReplacements.Keys), RegexOptions.Compiled);
    }
	
		public static string Parse(string content)
		{
			return string.IsNullOrEmpty(content) ? content : appSiteCombinedRegex.Replace(content, m => appSiteReplacements[m.Value]);
		}

		public static string ParseForEnvironment(string content, bool usePublicSiteSettings)
		{
			if (usePublicSiteSettings)
			{
				return string.IsNullOrEmpty(content) ? content : publicSiteCombinedRegex.Replace(content, m => publicSiteReplacements[m.Value]);
			}
			return string.IsNullOrEmpty(content) ? content : appSiteCombinedRegex.Replace(content, m => appSiteReplacements[m.Value]);
		}

    /// <summary>
    /// Populates with reference to character codes from: http://ascii.cl/htmlcodes.htm
    /// </summary>
    /// <param name="dictionary"></param>
    private static void PopulateDictionaryWithCharacterCodes(Dictionary<string, string> dictionary)
    {
      //add standard set
      for(int i = 160; i < 256; i++)
      {
        AddCharacterCodeToDictionary(dictionary, i);
      }

      //add some specific codes
      AddCharacterCodeToDictionary(dictionary, 338);
      AddCharacterCodeToDictionary(dictionary, 339);
      AddCharacterCodeToDictionary(dictionary, 352);
      AddCharacterCodeToDictionary(dictionary, 353);
      AddCharacterCodeToDictionary(dictionary, 376);
      AddCharacterCodeToDictionary(dictionary, 402);
      AddCharacterCodeToDictionary(dictionary, 8211);
      AddCharacterCodeToDictionary(dictionary, 8212);
      AddCharacterCodeToDictionary(dictionary, 8216);
      AddCharacterCodeToDictionary(dictionary, 8217);
      AddCharacterCodeToDictionary(dictionary, 8218);
      AddCharacterCodeToDictionary(dictionary, 8220);
      AddCharacterCodeToDictionary(dictionary, 8221);
      AddCharacterCodeToDictionary(dictionary, 8222);
      AddCharacterCodeToDictionary(dictionary, 8224);
      AddCharacterCodeToDictionary(dictionary, 8225);
      AddCharacterCodeToDictionary(dictionary, 8226);
      AddCharacterCodeToDictionary(dictionary, 8230);
      AddCharacterCodeToDictionary(dictionary, 8240);
      AddCharacterCodeToDictionary(dictionary, 8364);
      AddCharacterCodeToDictionary(dictionary, 8482);
    }

    private static void AddCharacterCodeToDictionary(Dictionary<string, string> dictionary, int i)
    {
      dictionary.Add(Convert.ToChar(i).ToString(CultureInfo.InvariantCulture), GetHtmlVersionOfCharacterCode(i));
    }

    private static string GetHtmlVersionOfCharacterCode(int code)
    {
      return string.Format("&#{0};", code);
    }

    private static void PopulateDictionaryWithAppSiteSpecialCases(Dictionary<string, string> dictionary)
    {
      //replace src attributes with relative urls to use absolute urls
			dictionary.Add("src=\"/", "src=\"" + WebConfigurationManager.AppSettings["SiteUrl"] + "/");
    }

		private static void PopulateDictionaryWithPublicSiteSpecialCases(Dictionary<string, string> dictionary)
		{
			//replace src attributes with relative urls to use absolute urls
			dictionary.Add("src=\"/", "src=\"" + WebConfigurationManager.AppSettings["PublicSiteUrl"] + "/");
		}
  }
}