using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Net.Http;
using System.Threading;
using WalletIntegration.PayTM.Model;

namespace WalletIntegration.PayTM
{
	public class Refund : RequestBase
	{
		private string _fileDir = ConfigurationManager.AppSettings["FileDir"];
		private string _amount = string.Empty;
		private string _txnId = string.Empty;
		private string _refundRefId = string.Empty;
		private string _detail = string.Empty;

		public SQLiteDataReader GetLastTransaction()
		{
			return Database.ExecuteDataReader("SELECT * FROM TransactionLog Order By Id DESC LIMIT 1");
		}

		public Refund(string amount, string detail)
		{
			_amount = amount;
			_detail = detail;
			PrepareRequestAsync();
		}

		public override void PrepareRequestAsync()
		{
			String merchantGuid = ConfigurationManager.AppSettings["paytm.mGuid"];
			String mid = ConfigurationManager.AppSettings["paytm.mid"];
			string orderId = string.Empty;

			using (SQLiteDataReader dr = GetLastTransaction())
			{

				if (!dr.HasRows)
				{
					HandleError("No Transaction found");
					return;
				}

				dr.Read();
				_txnId = dr["TxnGuid"].ToString();

				if (string.IsNullOrEmpty(_txnId))
				{
					HandleError("No Transaction found");
					return;
				}
				if (IsRefundDone(_txnId))
				{
					HandleError("Refund has already has been started for this transaction " + _txnId);
					return;
				}

				_refundRefId = string.Format("RF-{0}", Guid.NewGuid());
				orderId = dr["OrderId"].ToString();
				// price = dr["Amount"].ToString();

			}

			Dictionary<string, string> parameters = new Dictionary<string, string>
				{
					{ "txnGuid", _txnId },
					{ "amount", _amount },
					{ "refundRefId", _refundRefId },
					{ "currencyCode", "INR" },
					{ "merchantGuid", merchantGuid },
					{ "merchantOrderId", orderId }
				};

			RefundRequest refundReq = new RefundRequest() { Request = parameters, IpAddress = "192.168.1.1", OperationType = "REFUND_MONEY", PlatformName = "PayTM", Version = "1.0", Channel = "WEB" };

				string checkSumHash = PayTMUtils.GetCheckSum(JsonConvert.SerializeObject(refundReq));

				Dictionary<string, string> requestHeader = new Dictionary<string, string>
				{
					{ "Content-Type", "application/json" },
					{ "merchantGuid", merchantGuid },
					{ "mid", merchantGuid },
					{ "checksumhash", checkSumHash }
				};
			ApiRequest reqOptions = new ApiRequest() { Headers = requestHeader, Parameters = refundReq, Url = string.Format("{0}{1}", baseUrl, "wallet-web/refundWalletTxn") };
			MakeRequestAsync(reqOptions).Wait();
			
		}

		public override void ProcessResponse(string response, HttpRequestMessage request)
		{
			RefundResponse refundRes = JsonConvert.DeserializeObject<RefundResponse>(response);
			if (refundRes.Status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
			{
				File.AppendAllText(Path.Combine(_fileDir, "refundSuccess.txt"), JsonConvert.SerializeObject(refundRes));
				Database.ExecuteQuery(string.Format("Insert into RefundLog (TxnGuid, Amount, RefId, Detail, CreatedOn) Values ('{0}', '{1}', '{2}', '{3}', DateTime('now'))", _txnId, _amount, _refundRefId, _detail));
			}
			else
			{
				HandleError(string.Format("{0} - {1}", "Error during refund", refundRes.StatusMessage));
				File.AppendAllText(Path.Combine(_fileDir, "refundFailure.txt"), JsonConvert.SerializeObject(refundRes));

			}
			Thread.Sleep(2000);
			Environment.Exit(1);
		}

		public bool IsRefundDone(string txnGuid)
		{
			bool toReturn = false;
			using (SQLiteDataReader dr = Database.ExecuteDataReader(string.Format("SELECT * FROM RefundLog where TxnGuid = '{0}'", txnGuid)))
			{
				toReturn = dr.Read();
			}
			return toReturn;
		}

		public void HandleError(string error)
		{
			Logger.Error(error);
			File.AppendAllText(Path.Combine(_fileDir, "error.txt"), error);
			Thread.Sleep(2000);
			CloseApplication();
			return;
		}

		public void CloseApplication()
		{
			Environment.Exit(1);
		}
	}
}
