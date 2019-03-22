using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WalletIntegration.Base
{
	public abstract class RequestBase
	{
		public abstract string BaseUrl { get; }

		public static HttpClient Client;
		public static Logger Logger = LogManager.GetCurrentClassLogger();
		private string _fileDir = ConfigurationManager.AppSettings["FileDir"];

		public RequestBase()
		{
			Client = new HttpClient();
		}

		public abstract void PrepareRequest();

		public abstract void ProcessResponse(string response, HttpRequestMessage request);

		public Uri GetRequestURL(Base.ApiRequest requestOptions)
		{
			var uriBuilder = new UriBuilder(requestOptions.Url);
			if (requestOptions.Method == HttpMethod.Get)
			{
				var query = HttpUtility.ParseQueryString(uriBuilder.Query);
				foreach (string key in requestOptions.Parameters.Keys)
				{
					query[key] = requestOptions.Parameters[key];
				}
				uriBuilder.Query = query.ToString();
			}
			return uriBuilder.Uri;
		}

		public async Task MakeRequestAsync(Base.ApiRequest requestOptions)
		{

			Uri requestUrl = GetRequestURL(requestOptions);
			using (HttpRequestMessage request = new HttpRequestMessage { Method = requestOptions.Method, RequestUri = requestUrl })
			{
				foreach (string key in requestOptions.Headers.Keys)
				{
					if (key.ToLower() == "content-type")
					{
						Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(requestOptions.Headers[key]));
						continue;
					}
					request.Headers.Add(key, requestOptions.Headers[key]);
				}

				//Send content only for POST request
				if (requestOptions.Method == HttpMethod.Post)
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
				catch (HttpRequestException ex)
				{
					Util.HandleError(ex);
				}
				catch (ArgumentNullException ex)
				{
					Util.HandleError(ex);
				}
				catch (InvalidOperationException ex)
				{
					Util.HandleError(ex);
				}
				catch (TaskCanceledException ex)
				{
					Util.HandleError(ex);
				}
				catch (AggregateException ex)
				{
					Util.HandleError(ex);
				}
				catch (Exception ex)
				{
					Util.HandleError(ex);
				}
			}
		}
	}
}
