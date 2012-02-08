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
