using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DimensionGhGh.Protocol;

namespace DimensionGhGh.Client
{
	/// <summary>
	/// HTTP client for communicating with Dimension_Gh add-on in Archicad
	/// </summary>
	public class DimensionGhClient
	{
		private readonly HttpClient httpClient;
		private readonly string baseUrl;

		public string Host { get; set; } = "127.0.0.1";
		public int Port { get; set; } = 19723; // Default port, can be changed
		public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(3);

		public DimensionGhClient()
		{
			httpClient = new HttpClient
			{
				Timeout = Timeout
			};
			UpdateBaseUrl();
		}

		public DimensionGhClient(int port) : this()
		{
			Port = port;
			UpdateBaseUrl();
		}

		private void UpdateBaseUrl()
		{
			baseUrl = $"http://{Host}:{Port}/";
		}

		/// <summary>
		/// Send JSON request to Archicad
		/// </summary>
		public async Task<JsonResponse> SendAsync(JsonRequest request)
		{
			try
			{
				var json = JsonConvert.SerializeObject(request);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await httpClient.PostAsync(baseUrl, content);
				var responseString = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var jsonResponse = JsonConvert.DeserializeObject<JsonResponse>(responseString);
					return jsonResponse ?? new JsonResponse { Succeeded = false };
				}
				else
				{
					return new JsonResponse
					{
						Succeeded = false,
						Error = new Newtonsoft.Json.Linq.JObject
						{
							["message"] = $"HTTP {response.StatusCode}: {responseString}"
						}
					};
				}
			}
			catch (TaskCanceledException)
			{
				return new JsonResponse
				{
					Succeeded = false,
					Error = new Newtonsoft.Json.Linq.JObject
					{
						["message"] = "Request timeout"
					}
				};
			}
			catch (Exception ex)
			{
				return new JsonResponse
				{
					Succeeded = false,
					Error = new Newtonsoft.Json.Linq.JObject
					{
						["message"] = ex.Message
					}
				};
			}
		}

		/// <summary>
		/// Synchronous version of Send
		/// </summary>
		public JsonResponse Send(JsonRequest request)
		{
			return SendAsync(request).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Update port and recreate base URL
		/// </summary>
		public void SetPort(int port)
		{
			Port = port;
			UpdateBaseUrl();
		}

		public void Dispose()
		{
			httpClient?.Dispose();
		}
	}
}

