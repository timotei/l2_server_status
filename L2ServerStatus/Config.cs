/*
   Copyright (C) 2012 by Timotei Dolean <timotei21@gmail.com>

   Part of the Lineage 2 Server Status Project https://github.com/timotei/l2_server_status

   This program is free software; you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation; either version 2 of the License, or
   (at your option) any later version.
   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY.

   See the COPYING file for more details.
*/
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
