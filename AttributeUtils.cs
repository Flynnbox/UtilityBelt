using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace UtilityBelt
{
	public class AttributeUtils
	{
		#region Fields
		private static Type descriptionAttributeType = typeof(DescriptionAttribute);
		#endregion

		#region Properties
		#endregion

		#region Lifecycle
		#endregion

		#region Methods
		/// <summary>
		/// Returns the description attribute assigned to an object
		/// </summary>
		/// <param name="describedObject"></param>
		/// <returns></returns>
		public static string GetDescription(object describedObject)
		{
			if (describedObject != null)
			{
				//get the description attribute value for the format type
				FieldInfo fieldInfo = describedObject.GetType().GetField(describedObject.ToString());
				DescriptionAttribute[] attributes = (DescriptionAttribute[]) fieldInfo.GetCustomAttributes(descriptionAttributeType, false);
				return (attributes.Length > 0) ? attributes[0].Description : String.Empty;
			}
			return String.Empty;
		}

		/// <summary>
		/// Returns an Enumeration based on its description attribute string
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public static Enum GetEnumByDescription(Type enumType, string enumDescription)
		{
			IEnumerator enumEnumerator = Enum.GetValues(enumType).GetEnumerator();
			while (enumEnumerator.MoveNext())
			{
				if (enumDescription.Equals(AttributeUtils.GetDescription(enumEnumerator.Current), StringComparison.InvariantCultureIgnoreCase))
				{
					return (Enum) enumEnumerator.Current;
				}
			}
			return null;
		}
		#endregion
	}
}
