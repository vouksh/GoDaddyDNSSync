using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDaddyDNSSync
{
	public class Domain
	{
		public Record[] Records { get; set; }
	}

	public class Record
	{
		public string Data { get; set; }
		public string Name { get; set; }
		public int Ttl { get; set; }
		public string Type { get; set; }
	}
}
