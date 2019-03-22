using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletIntegration.PayTM;

namespace WalletIntegration
{
	class Program
	{

		private static Logger logger = LogManager.GetCurrentClassLogger();
		private static string _fileDir = ConfigurationManager.AppSettings["FileDir"];

		static void Main(string[] args)
		{
			ConfigurationManager.RefreshSection("appSettings");
			string executable = System.Reflection.Assembly.GetExecutingAssembly().Location;
			string path = (System.IO.Path.GetDirectoryName(executable));
			path = System.IO.Path.Combine(path, "db");
			AppDomain.CurrentDomain.SetData("DataDirectory", path);
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(HandleException);


			DirectoryInfo di = Directory.CreateDirectory(_fileDir);
			Util.PurgeOldLogs();

			//Test.Run();
			//return;

			if (args.Length > 0)
			{
				string action = args[0];
				switch (action)
				{
					case "refund":
						if (args.Length == 2 && !string.IsNullOrEmpty(args[1]))
						{
							string price = args[1];
							string detail = args.Length > 2 ? args[2] : string.Empty;
							Refund refund = new Refund(price, detail);
						}
						else
						{
							Util.ConsoleLog("Not able to get amount to be refunded.");
						}
						break;
					default:
						Util.ConsoleLog("Invalid command");
						break;
				}

			}
			else
			{
				Payment payment = new Payment();
			}
		}

		static void HandleException(object sender, UnhandledExceptionEventArgs args)
		{
			logger.Error(args.ExceptionObject.ToString());
		}
	}
}
