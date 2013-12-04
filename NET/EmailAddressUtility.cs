using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Ihi.Common.Utility
{
	public class EmailAddressUtility
	{
		public static readonly Regex RegexIsValidEmail = new Regex(@"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$",
		                                                           RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public static readonly Regex RegexContainsValidEmail = new Regex(@"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b",
		                                                                 RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// Returns the first valid email address in the candidate string or String.Empty if no match is found
		/// </summary>
		/// <param name="candidate"></param>
		/// <returns></returns>
		public static string GetFirstEmailFromString(string candidate)
		{
			Match match = RegexContainsValidEmail.Match(candidate.ToLower());
			return match.Success ? match.Value : String.Empty;
		}

		/// <summary>
		/// Returns a list of strings containing all unique valid email addresses found within the candidate string
		/// </summary>
		/// <param name="candidate"></param>
		/// <returns></returns>
		public static List<string> GetAllUniqueEmailsFromString(string candidate)
		{
			List<string> emailList = new List<string>();
			
			if (candidate == null || candidate.Trim().Length == 0)
			{
				return emailList;
			}
			
			MatchCollection matches = RegexContainsValidEmail.Matches(candidate.ToLower());

			foreach (Match match in matches)
			{
				if (!emailList.Contains(match.Value))
				{
					emailList.Add(match.Value);
				}
			}
			return emailList;
		}

		/// <summary>
		/// Returns true if the candidate string is a valid email address in its entirety
		/// </summary>
		/// <param name="candidate"></param>
		/// <returns></returns>
		public static bool IsValidEmailAddress(string candidate)
		{
			return RegexIsValidEmail.IsMatch(candidate);
		}

		/// <summary>
		/// Returns true if the candidate string contains a valid email address
		/// </summary>
		/// <param name="candidate"></param>
		/// <returns></returns>
		public static bool ContainsValidEmailAddress(string candidate)
		{
			return RegexContainsValidEmail.IsMatch(candidate);
		}

		/// <summary>
		/// Parses a group of candidates strings into unique valid emails and unique invalid candidates.
		/// Only looks for the first email within each candidate
		/// </summary>
		/// <param name="candidates"></param>
		/// <param name="validEmails"></param>
		/// <param name="invalidCandidates"></param>
		public static void ParseCandidatesOncePerCandidate(IEnumerable<string> candidates, out List<string> validEmails, out List<string> invalidCandidates)
		{
			validEmails = new List<string>();
			invalidCandidates = new List<string>();

			foreach (string candidate in candidates)
			{
				string parsedEmail = GetFirstEmailFromString(candidate.ToLower());

				if (parsedEmail.Length > 0)
				{
					if (!validEmails.Contains(parsedEmail))
					{
						validEmails.Add(parsedEmail);
					}
				}
				else
				{
					if (!invalidCandidates.Contains(candidate))
					{
						invalidCandidates.Add(candidate);
					}
				}
			}
		}

		/// <summary>
		/// Parses a group of candidates strings into unique valid emails and unique invalid candidates.
		/// Looks for all emails within each candidate
		/// </summary>
		/// <param name="candidates"></param>
		/// <param name="validEmails"></param>
		/// <param name="invalidCandidates"></param>
		public static void ParseCandidates(IEnumerable<string> candidates, out List<string> validEmails, out List<string> invalidCandidates)
		{
			validEmails = new List<string>();
			invalidCandidates = new List<string>();

			foreach (string candidate in candidates)
			{
				List<string> emails = GetAllUniqueEmailsFromString(candidate.ToLower());

				if (emails.Count > 0)
				{
					foreach (var email in emails)
					{
						if (!validEmails.Contains(email))
						{
							validEmails.Add(email);
						}
					}
				}
				else
				{
					if (!invalidCandidates.Contains(candidate))
					{
						invalidCandidates.Add(candidate);
					}
				}
			}
		}

	}
}