using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletIntegration.PayTM.Model
{
	class RefundRequest : QRRequest
	{
		[JsonProperty("channel")]
		public string Channel { get; set; }
		[JsonProperty("version")]
		public string Version { get; set; }
	}
}
