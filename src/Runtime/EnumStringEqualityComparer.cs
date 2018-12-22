using System;
using System.Collections.Generic;


namespace IronVelocity.Runtime
{
	public static class StringEnumComparer
	{
		public static bool? AreEqual<T>(T enumeration, string str)
			where T : struct
		{
			T parsedEnum;
			if (Enum.TryParse(str, true, out parsedEnum))
				return EqualityComparer<T>.Default.Equals(parsedEnum, enumeration);

			return null;
		}

	}
}
