using Newtonsoft.Json;
using NLog;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WalletIntegration.Base;

namespace WalletIntegration.PayTM
{
	public abstract class RequestBase
	{
		public static HttpClient Client;
		public static Logger Logger = LogManager.GetCurrentClassLogger();
		public static string baseUrl = ConfigurationManager.AppSettings["paytm.baseUrl"];
		private string _fileDir = ConfigurationManager.AppSettings["FileDir"];

		public RequestBase()
		{
			Client = new HttpClient();
		}

		public abstract void PrepareRequestAsync();

		public abstract void ProcessResponse(string response, HttpRequestMessage request);

		public async Task MakeRequestAsync(ApiRequest requestOptions)
		{
			HttpRequestMessage request = new HttpRequestMessage
			{
				Method = HttpMethod.Post,
				RequestUri = new Uri(requestOptions.Url)
			};

			foreach (string key in requestOptions.Headers.Keys)
			{
				if (key.ToLower() == "content-type")
				{
					Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(requestOptions.Headers[key]));
					continue;
				}
				request.Headers.Add(key, requestOptions.Headers[key]);
			}

			request.Content = new StringContent(JsonConvert.SerializeObject(requestOptions.Parameters), Encoding.UTF8, "application/json");
			Logger.Info(JsonConvert.SerializeObject(requestOptions.Parameters));

			try
			{
				var response = await Client.SendAsync(request);
				string res = await response.Content.ReadAsStringAsync();
				if (!response.IsSuccessStatusCode)
				{
					Util.HandleError(res);
					return;
				}
				else if (response.StatusCode == HttpStatusCode.OK)
				{
					Logger.Info(res);
					ProcessResponse(res, request);
				}
			}
			catch(HttpRequestException ex)
			{
				Util.HandleError(ex);
			}
		}
	}
}
