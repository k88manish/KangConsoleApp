using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using WalletIntegration.Base;
using WalletIntegration.PayTM.Model;

namespace WalletIntegration.PayTM
{
	public class StatusService : RequestBase
	{
		private bool _isRunning = false;

		private string checkStatusUrl = ConfigurationManager.AppSettings["paytm.checkStatusUrl"] ?? baseUrl;

		public bool IsRunning { get { return _isRunning; } }

		public EventHandler<StatusResponse> OnStatusSuccess;

		public string TxnId { get; set; }

		public StatusService(string txnId)
		{
			this.TxnId = txnId;
		}

		public void CheckStatus()
		{
			PrepareRequestAsync();
		}

		public override void PrepareRequestAsync()
		{
			if (string.IsNullOrEmpty(TxnId))
			{
				return;
			}

			_isRunning = true;

			String merchantGuid = ConfigurationManager.AppSettings["paytm.mGuid"];
			String mid = ConfigurationManager.AppSettings["paytm.mid"];
			String posId = ConfigurationManager.AppSettings["posId"];

			Random rand = new Random();
			Dictionary<string, string> parameters = new Dictionary<string, string>
			{
				{ "requestType", "merchantTxnId" },
				{ "txnType", "withdraw" },
				{ "txnId", TxnId },
				{ "merchantGuid", merchantGuid }
			};

			StatusRequest statusReq = new StatusRequest() { Request = parameters, OperationType = "CHECK_TXN_STATUS", PlatformName = "PayTM", Version = "1.1" };

			string checkSumHash = PayTMUtils.GetCheckSum(JsonConvert.SerializeObject(statusReq));

			Dictionary<string, string> requestHeader = new Dictionary<string, string>
			{
				{ "Content-Type", "application/json" },
				{ "mid", merchantGuid },
				{ "checksumhash", checkSumHash }
			};

			ApiRequest reqOptions = new ApiRequest() { Headers = requestHeader, Parameters = statusReq, Url = string.Format("{0}{1}", checkStatusUrl, "wallet-web/checkStatus") };

			MakeRequestAsync(reqOptions).Wait();
		}

		public override void ProcessResponse(string response, HttpRequestMessage request)
		{
			//Logger.Trace(response);
			StatusResponse statusRes = JsonConvert.DeserializeObject<StatusResponse>(response);
			if (statusRes.Status.Equals("SUCCESS") && statusRes.StatusCode.Equals("SS_001"))
			{
				if (statusRes.Response.TxnList.Count > 0)
				{
					TxnResponse txnResponse = statusRes.Response.TxnList[0];
					if (txnResponse.Status.Equals("1") && txnResponse.Message.Equals("SUCCESS") && txnResponse.MerchantOrderId.Equals(TxnId))
					{
						_isRunning = false;
						OnStatusSuccess?.Invoke(this, statusRes);
						return;
					}
				}
			}
			else
			{
				Logger.Info(string.Format("{0} - {1}", "Error in getting Transaction status", statusRes.StatusMessage));
			}
			_isRunning = false;
		}
	}
}
