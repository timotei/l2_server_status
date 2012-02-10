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
using System.Windows.Media;

namespace L2ServerStatus
{
	public class ServerStatus
	{
		public string ServerName { get; set; }
		public Color Status { get; set; }
		public int Ping { get; set; }
	}
}
