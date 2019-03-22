using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WalletIntegration.PayTM.Model
{
	public class QRRequest
	{
		[JsonProperty("request")]
		public Dictionary<string, string> Request { get; set; }
		[JsonProperty("platformName")]
		public string PlatformName { get; set; }
		[JsonProperty("ipAddress")]
		public string IpAddress { get; set; }
		[JsonProperty("operationType")]
		public string OperationType { get; set; }
	}
}
