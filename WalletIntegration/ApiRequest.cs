using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletIntegration.PayTM.Model;

namespace WalletIntegration
{
	public class ApiRequest
	{
		public string Url { get; set; }
		public Dictionary<string, string> Headers { get; set; }
		public QRRequest Parameters { get; set; }
	}
}
