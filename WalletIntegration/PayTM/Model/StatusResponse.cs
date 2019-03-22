using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletIntegration.PayTM.Model
{
	public class TxnResponse
	{
		[JsonProperty("txnGuid")]
		public string TxnGuid { get; set; }

		[JsonProperty("txnAmount")]
		public string TxnAmount { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("txnErrorCode")]
		public string TxnErrorCode { get; set; }

		[JsonProperty("txnType")]
		public string TxnType { get; set; }

		[JsonProperty("merchantOrderId")]
		public string MerchantOrderId { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }
	}

	public class SResponse
	{
		[JsonProperty("txnList")]
		public List<TxnResponse> TxnList { get; set; }
	}

	public class StatusResponse
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
		public SResponse Response { get; set; }
	}
}
