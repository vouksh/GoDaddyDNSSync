using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDaddyDNSSync
{
	public class ApiInfo
	{
		public string Key { get; set; }
		public string Secret { get; set; }
		public string BaseDomain { get; set; }
		public string Exclusions { get; set; }
	}
}
