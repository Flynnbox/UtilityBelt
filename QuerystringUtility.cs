using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace UtilityBelt
{
	public class QuerystringUtility
	{
		#region Fields
		//private NameValueCollection mQuerystring;
		#endregion

		#region Properties
		#endregion

		#region Lifecycle
		static QuerystringUtility()
		{

		}
		#endregion

		#region Methods
		/// <summary>
		/// Returns a querystring formatted string from a NameValueCollection
		/// </summary>
		/// <param name="collection"></param>
		/// <param name="urlEncodeValues"></param>
		/// <returns></returns>
		public static string GetQuerystring(NameValueCollection collection, bool urlEncodeValues)
		{
			StringBuilder querystring = new StringBuilder();
			querystring.Append("?");
			foreach (string key in collection.Keys)
			{
				//querystring.Append(UrlEncode ? HttpUtility.UrlEncode(key) : key);
				querystring.Append(key);
				querystring.Append("=");
				querystring.Append(urlEncodeValues ? HttpUtility.UrlEncode(collection[key]) : collection[key]);
				querystring.Append("&");
			}
			//remove final "&" from end of querystring
			querystring.Remove(querystring.Length - 1, 1);
			return querystring.ToString();
		}
		#endregion
	}
}
