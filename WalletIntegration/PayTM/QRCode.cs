
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Timers;
using WalletIntegration.PayTM.Model;

namespace WalletIntegration.PayTM
{
	class QRCode : RequestBase
	{
		public string OrderId { get; set; }

		private string _price = string.Empty;

		public EventHandler<QRResponse> OnQRSuccess;

		public QRCode(string price)
		{
			_price = price;
		}

		public void Run()
		{
			PrepareRequestAsync();
		}

		public override async void PrepareRequestAsync()
		{
			Random rand = new Random();
			String merchantGuid = ConfigurationManager.AppSettings["paytm.mGuid"];
			String mid = ConfigurationManager.AppSettings["paytm.mid"];
			String posId = ConfigurationManager.AppSettings["posId"];
			String merchentPhone = ConfigurationManager.AppSettings["merchantContactNo"];
			OrderId = (ConfigurationManager.AppSettings["OrderPre"] ?? "") + "ORDER" + rand.Next();

			Dictionary<string, string> parameters = new Dictionary<string, string>
			{
				{ "requestType", "QR_ORDER" },
				{ "merchantContactNO", merchentPhone },
				{ "posId", posId },
				{ "channelId", "POS" },
				{ "amount", _price },
				{ "currency", "INR" },
				{ "merchantGuid", merchantGuid },
				{ "orderId", OrderId },
				{ "orderDetails", "Order" },
				{ "validity", "1" },
				{ "industryType", "RETAIL" }
			};

			QRRequest qrReq = new QRRequest() { Request = parameters, IpAddress = "192.168.1.1", OperationType = "QR_CODE", PlatformName = "PayTM" };

			string checkSumHash = PayTMUtils.GetCheckSum(JsonConvert.SerializeObject(qrReq));

			Dictionary<string, string> requestHeader = new Dictionary<string, string>
			{
				{ "Content-Type", "application/json" },
				{ "merchantGuid", merchantGuid },
				{ "mid", mid },
				{ "checksumhash", checkSumHash }
			};

			ApiRequest reqOptions = new ApiRequest() { Headers = requestHeader, Parameters = qrReq, Url = string.Format("{0}{1}", baseUrl, "wallet-merchant/v2/createQRCode") };

			MakeRequestAsync(reqOptions).Wait();
		}

		public override void ProcessResponse(string response, HttpRequestMessage request)
		{
			Logger.Trace(response);
			QRResponse qrRes = JsonConvert.DeserializeObject<QRResponse>(response);
			if (qrRes.Status.Equals("SUCCESS"))
			{
				qrRes.OrderId = OrderId;
				OnQRSuccess?.Invoke(this, qrRes);
			}
			else
			{
				var result = request.Content.ReadAsStringAsync();
				Logger.Error(string.Format("{0} - {1}", "QR Code Error in getting", result));
			}
		}
	}
}
