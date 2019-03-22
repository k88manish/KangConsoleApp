using NLog;
using System;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace WalletIntegration
{
	public static class Util
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private static string _fileDir = ConfigurationManager.AppSettings["FileDir"];
		private static string _logFileDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

		public static string CleanPrice(string price)
		{
			return Regex.Replace(price, @"\t|\n|\r", "");
		}

		public static bool ValidatePrice(string price)
		{
			return Regex.IsMatch(price, @"^[0-9]{1,5}\.[0-9]{2}$");
		}

		public static void HandleError(Exception ex)
		{
			logger.Error(ex);
			Console.WriteLine(ex.Message);
			File.AppendAllText(Path.Combine(_fileDir, "error.txt"), ex.Message);
			Thread.Sleep(2000);
			CloseApplication();
		}

		public static void HandleError(string error)
		{
			logger.Error(error);
			Console.WriteLine(error);
			File.AppendAllText(Path.Combine(_fileDir, "error.txt"), error);
			Thread.Sleep(2000);
			CloseApplication();
		}

		public static void CloseApplication()
		{
			Environment.Exit(1);
		}

		public static void ConsoleLog(string msg)
		{
			Console.WriteLine(msg);
		}

		public static void PurgeOldLogs()
		{
			if(Directory.Exists(_logFileDir))
			{
				string[] files = Directory.GetFiles(_logFileDir);
				foreach (string file in files)
				{
					FileInfo fi = new FileInfo(file);
					if (fi.LastAccessTime < DateTime.Now.AddMonths(-1))
					{
						fi.Delete();
					}
				}
			}
		}
	}
}
