using System;
using System.Text.RegularExpressions;

namespace UtilityBelt
{
	public static class GuidUtility
	{
		//Slight performance optimization by using only lowercase letters in regular expression and performing ToLower() operation on string prior to match
		private static readonly Regex isGuidRegex =
			new Regex(@"^(\{){0,1}[0-9a-f]{8}\-[0-9a-f]{4}\-[0-9a-f]{4}\-[0-9a-f]{4}\-[0-9a-f]{12}(\}){0,1}$", RegexOptions.Compiled);

		/// <summary>
		/// Converts a string to a guid; if null or invalid string returns Guid.Empty
		/// </summary>
		/// <param name="guidString"></param>
		/// <returns></returns>
		public static Guid ToGuid(string guidString)
		{
			return (Guid) ToGuid(guidString, false);
		}

		/// <summary>
		/// Converts an object to a guid; if null, invalid object type, or invalid string representation, returns Guid.Empty
		/// </summary>
		/// <param name="guidCandidate"></param>
		/// <returns></returns>
		public static Guid ToGuid(object guidCandidate)
		{
			if (guidCandidate == null)
			{
				return Guid.Empty;
			}
			return guidCandidate is Guid ? (Guid) guidCandidate : ToGuid(guidCandidate.ToString());
		}

		/// <summary>
		/// Converts a string to a guid; if null or invalid string returns null
		/// </summary>
		/// <param name="guidString"></param>
		/// <returns></returns>
		public static Object ToGuidOrNull(string guidString)
		{
			return ToGuid(guidString, true);
		}

		/// <summary>
		/// Converts an object to a Nullable Guid; if null, invalid object type, or invalid string representation, returns null
		/// </summary>
		/// <param name="guidCandidate"></param>
		/// <returns></returns>
		public static Guid? ToNullableGuid(object guidCandidate)
		{
			if (guidCandidate == null)
			{
				return null;
			}
			return guidCandidate is Guid ? (Guid)guidCandidate : ToNullableGuid(guidCandidate.ToString());
		}

		/// <summary>
		/// Converts a string to a Nullable Guid; if null or invalid string returns null
		/// </summary>
		/// <param name="guidString"></param>
		/// <returns></returns>
		public static Guid? ToNullableGuid(string guidString)
		{
			if (!IsGuid(guidString))
			{
				return null;
			}
			return new Guid(guidString);
		}

		/// <summary>
		/// Converts a string to a guid; if null, invalid, or empty string it returns 
		/// either a Guid.Empty or null object (depending on the boolean flag)
		/// </summary>
		/// <param name="guidString"></param>
		/// <param name="defaultToNull"></param>
		/// <returns></returns>
		private static Object ToGuid(string guidString, bool defaultToNull)
		{
			if (!IsGuid(guidString))
			{
				if (defaultToNull)
				{
					return null;
				}
				else
				{
					return Guid.Empty;
				}
			}
			return new Guid(guidString);
		}

		/// <summary>
		/// Checks if an object, or its string representation, is non-null, non-Guid.Empty Guid value.
		/// </summary>
		/// <param name="guidCandidate"></param>
		/// <returns></returns>
		public static bool IsGuid(object guidCandidate)
		{
			if (guidCandidate == null)
			{
				return false;
			}
			return (guidCandidate is Guid) ? IsGuid((Guid) guidCandidate) : IsGuid(guidCandidate.ToString());
		}

		/// <summary>
		/// Checks if a Guid is non-null, non-Guid.Empty Guid value.
		/// </summary>
		/// <param name="guid"></param>
		/// <returns></returns>
		public static bool IsGuid(Guid guid)
		{
			return !IsNullOrEmpty(guid);
		}

		/// <summary>
		/// Checks string against a regular expression to see if it is a validly formatted GUID string
		/// and not an Empty GUID
		/// </summary>
		/// <param name="guidString"></param>
		/// <returns></returns>
		public static bool IsGuid(string guidString)
		{
			return IsGuid(guidString, false);
		}

		/// <summary>
		/// Checks string against a regular expression to see if it is a validly formatted GUID string;
		/// boolean value indicates whether a Guid.Empty value is considered valid
		/// </summary>
		/// <param name="guidString"></param>
		/// <param name="isEmptyGuidValid"></param>
		/// <returns></returns>
		public static bool IsGuid(string guidString, bool isEmptyGuidValid)
		{
			bool isValid = false;
			if (!IsNullOrEmpty(guidString))
			{
				if (isGuidRegex.IsMatch(guidString.ToLower()))
				{
					isValid = isEmptyGuidValid || !guidString.Equals(Guid.Empty.ToString(), StringComparison.InvariantCultureIgnoreCase);
				}
			}
			return isValid;
		}

		/// <summary>
		/// Checks string against a regular expression to see if the candidate is a valid GUID string;
		/// Output parameter is Guid.Empty if match fails, or Guid if successsful
		/// </summary>
		/// <param name="guidString"></param>
		/// <param name="outputGuid"></param>
		/// <returns></returns>
		public static bool TryParse(string guidString, out Guid outputGuid)
		{
			bool isValid = IsGuid(guidString);
			outputGuid = isValid ? new Guid(guidString) : Guid.Empty;
			return isValid;
		}

		/// <summary>
		/// Returns true if the Guid is equal to null or to Guid.Empty
		/// </summary>
		/// <param name="guid"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty(Guid guid)
		{
			return guid == null || guid.Equals(Guid.Empty);
		}

		/// <summary>
		/// Returns true if the guid string is equal to null or is a zero-length string after a Trim() operation
		/// </summary>
		/// <param name="guidString"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty(string guidString)
		{
			return guidString == null || guidString.Trim().Length == 0;
		}

		/// <summary>
		/// Returns true if the guid string is equal to null or is a zero-length string after a Trim() operation
		/// </summary>
		/// <param name="guidString"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty(object guidCandidate)
		{
			return guidCandidate == null || guidCandidate.ToString().Trim().Length == 0;
		}

		/// <summary>
		/// Returns true if the guid string is equal to null or equal to Guid.Empty string
		/// </summary>
		/// <param name="guidString"></param>
		/// <returns></returns>
		public static bool IsNullOrGuidEmpty(string guidString)
		{
			return guidString == null || guidString.Equals(Guid.Empty.ToString());
		}

		/// <summary>
		/// Returns the ToString() results of a object if it is a Guid, else returns String.Empty
		/// </summary>
		/// <param name="guidCandidate"></param>
		/// <returns></returns>
		public static string GetGuidString(Object guidCandidate)
		{
			return (guidCandidate is Guid) ? guidCandidate.ToString() : String.Empty;
		}
	}
}