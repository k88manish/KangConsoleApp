using System.Collections.Generic;
using System.Net.Http;
using WalletIntegration.PayTM.Model;

namespace WalletIntegration.Base
{
	public class ApiRequest
	{
		public string Url { get; set; }
		public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
		public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
		public HttpMethod Method { get; set; } = HttpMethod.Post;
	}
}
