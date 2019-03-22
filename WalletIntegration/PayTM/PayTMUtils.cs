using paytm;
using System;
using System.Configuration;

namespace WalletIntegration.PayTM
{
	public static class PayTMUtils
	{
		public static string GetCheckSum(string jsonString)
		{
			String merchantKey = ConfigurationManager.AppSettings["paytm.mkey"];
			string checksum = CheckSum.generateCheckSumByJson(merchantKey, jsonString);
			return checksum;
		}
	}
}
