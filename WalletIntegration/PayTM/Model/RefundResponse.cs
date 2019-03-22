using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletIntegration.PayTM.Model
{
	public class ResponseRR
	{
		[JsonProperty("refundTxnGuid")]
		public string RefundTxnGuid { get; set; }
		[JsonProperty("refundTxnStatus")]
		public string RefundTxnStatus { get; set; }
	}
	public class RefundResponse
	{
		[JsonProperty("type")]
		public string Type { get; set; }
		[JsonProperty("requestGuid")]
		public string RequestGuid { get; set; }
		[JsonProperty("orderId")]
		public string OrderId { get; set; }
		[JsonProperty("status")]
		public string Status { get; set; }
		[JsonProperty("statusCode")]
		public string StatusCode { get; set; }
		[JsonProperty("statusMessage")]
		public string StatusMessage { get; set; }
		[JsonProperty("response")]
		public ResponseRR Response { get; set; }

	}
}
