using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletIntegration.PayTM.Model
{
	public class StatusRequest : QRRequest
	{
		[JsonProperty("version")]
		public string Version { get; set; }
	}
}
