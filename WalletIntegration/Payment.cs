using Newtonsoft.Json;
using NLog;
using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using WalletIntegration.PayTM;
using WalletIntegration.PayTM.Model;

namespace WalletIntegration
{
	public class Payment
	{

		private static Logger logger = LogManager.GetCurrentClassLogger();
		private StatusService statusService;
		private string _orderId = string.Empty;
		private int _timerSeconds = 0;
		private string _fileDir = ConfigurationManager.AppSettings["FileDir"];
		private int _qrTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["QRTimeout"]);
		private string _price = string.Empty;
		private int _isCheckingForPaymentCount = -1;

		public Payment()
		{
			_timerSeconds = _qrTimeout;
			Util.ConsoleLog("Getting QR Code started");
			CreateQRCode();
			//Test.Run();
		}

		public void CreateQRCode()
		{
			try
			{
				Util.ConsoleLog("Reading Price from File");
				_price = File.ReadAllText(Path.Combine(_fileDir, "price.txt"));
				if (string.IsNullOrEmpty(_price))
				{
					Util.HandleError("Invlid amount. Please check price.txt");
					return;
				}
				_price = Util.CleanPrice(_price);
				if (!Util.ValidatePrice(_price))
				{
					Util.HandleError("Invlid amount. It should be like 10.50 or 11.00");
					return;
				}
				Util.ConsoleLog("Getting QR Code");
				QRCode qrCodeMgr = new QRCode(_price);
				qrCodeMgr.OnQRSuccess += ShowQRImage;
				qrCodeMgr.Run();
			}
			catch (Exception e)
			{
				Util.HandleError(e.Message);
			}
		}

		public void ShowQRImage(object sender, QRResponse qrRes)
		{
			Util.ConsoleLog("QR Code Response recived");
			_orderId = qrRes.OrderId;
			
			Database.ExecuteQuery(string.Format("Insert into TransactionLog (OrderId, Amount, Detail, CreatedOn) Values ('{0}', '{1}', '', Date('now'))", _orderId, _price));

			var pic = Convert.FromBase64String(qrRes.Response.Path);
			using (MemoryStream ms = new MemoryStream(pic))
			{
				Util.ConsoleLog("QR Image Generated");
				using (FileStream file = new FileStream(Path.Combine(_fileDir, "QRCode.jpeg"), FileMode.Create, FileAccess.ReadWrite))
				{
					ms.WriteTo(file);
					file.Close();
				}

				int wait = 10 * 1000;
				_isCheckingForPaymentCount++;
				Task t = Task.Run(async () => {
					do
					{
						DoStatusCheck();
						await Task.Delay(wait);
					} while (_isCheckingForPaymentCount < (_qrTimeout/ 10) && _isCheckingForPaymentCount > -1);
				});
				t.Wait();
			}
		}

		public void DoStatusCheck()
		{
			_isCheckingForPaymentCount++;
			if (statusService == null)
			{
				Util.ConsoleLog("Initialize payment service");
				statusService = new StatusService(_orderId);
				statusService.OnStatusSuccess += OnStatusResponse;
			}

			if (!statusService.IsRunning)
			{
				if(!File.Exists(Path.Combine(_fileDir, "usercancel.txt")))
				{
					Util.ConsoleLog("Waiting for payment - " + _isCheckingForPaymentCount.ToString());
					statusService.CheckStatus();
				}
				else
				{
					// Close application when user cancel it
					Util.CloseApplication();
				}
			}

			GC.Collect();
		}


		public void OnStatusResponse(object sender, StatusResponse statusResponse)
		{
			try
			{
				//_statusTimer.Stop();

				Util.ConsoleLog("Payment successfull. Closing the application");
				TxnResponse txn = statusResponse.Response.TxnList[0];
				Database.ExecuteQuery(string.Format("UPDATE TransactionLog SET TxnGuid = '{0}' WHERE OrderId = '{1}'", txn.TxnGuid, _orderId));

				File.AppendAllText(Path.Combine(_fileDir, "success.txt"), JsonConvert.SerializeObject(statusResponse));
				File.Delete(Path.Combine(_fileDir, "price.txt"));

				//Auto close the program once payment is completed
				Thread.Sleep(3000);
				_isCheckingForPaymentCount = -1;
				Util.CloseApplication();
			}
			catch (Exception ex)
			{
				Util.HandleError(ex);
			}
		}

		public void Log(string msg)
		{
			Console.Write(msg);
		}

		private void onTransactionCancelled(string msg)
		{
			File.AppendAllText(Path.Combine(_fileDir, "cancel.txt"), msg);
			Util.CloseApplication();
		}
	}
}