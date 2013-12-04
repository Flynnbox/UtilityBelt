using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Ihi.Common.Utility
{
	public static class EnumUtility
	{
		/// <summary>
		/// 	Returns a IList composed of KeyValuePairs containing the enumeration values and their DescriptionAttribute values
		/// </summary>
		/// <param name = "enumeration"></param>
		/// <returns></returns>
		public static IList GetList(Type enumeration)
		{
			if (enumeration == null)
			{
				throw new ArgumentNullException("enumeration");
			}

			ArrayList list = new ArrayList();
			Array enumValues = Enum.GetValues(enumeration);

			foreach (Enum enumValue in enumValues)
			{
				list.Add(GetKeyValuePair(enumValue));
			}
			return list;
		}

		/// <summary>
		/// 	Returns a KeyValuePair composed of the enum value and that values DescriptionAttribute
		/// </summary>
		/// <param name = "enumValue"></param>
		/// <returns></returns>
		public static KeyValuePair<Enum, string> GetKeyValuePair(Enum enumValue)
		{
			return new KeyValuePair<Enum, string>(enumValue, GetDescription(enumValue));
		}

		/// <summary>
		/// 	Returns the value of a DescriptionAttribute used to decorate the specified enum value
		/// </summary>
		/// <param name = "fieldInformation"></param>
		/// <returns></returns>
		public static string GetDescription(Enum enumValue)
		{
			string description = enumValue.ToString();

			FieldInfo fieldInformation = enumValue.GetType().GetField(description);
			if (fieldInformation != null)
			{
				DescriptionAttribute[] descriptionAttributes = (DescriptionAttribute[]) fieldInformation.GetCustomAttributes(typeof (DescriptionAttribute), false);
				if (descriptionAttributes != null && descriptionAttributes.Length > 0)
				{
					description = descriptionAttributes[0].Description;
				}
			}
			return description;
		}

		public static T ConvertToEnum<T>(String enumString)
		{
			return (T) Enum.Parse(typeof (T), enumString);
		}

		public static String ConvertToString(Enum enumValue)
		{
			return Enum.GetName(enumValue.GetType(), enumValue);
		}
	}
}