
using System.Collections.Generic;

namespace L2ServerStatus
{
	public class Config
	{
		public const string UrlFormat = "http://{0}.kaifas.lt/nosound";

		public static List<string> ServerNames = new List<string>( )
			{
				"Shilen",
				"Chronos",
				"Naia",
				"Magmeld",
				"Bartz"
			};
	}
}
