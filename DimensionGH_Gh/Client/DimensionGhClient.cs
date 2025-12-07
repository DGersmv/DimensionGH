using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DimensionGhGh.Protocol;

namespace DimensionGhGh.Client
{
	/// <summary>
	/// HTTP client for communicating with Dimension_Gh add-on in Archicad
	/// </summary>
	public class DimensionGhClient
	{
		private readonly HttpClient httpClient;
        private string baseUrl;


		public string Host { get; set; } = "127.0.0.1";
		public int Port { get; set; } = 19723; // Default port, can be changed
		public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10); // Increased timeout to prevent hanging

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
				// Serialize with JsonProperty attributes (command, parameters)
				// Don't use CamelCase resolver as we already have JsonProperty attributes
				var json = JsonConvert.SerializeObject(request);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await httpClient.PostAsync(baseUrl, content);
				var responseString = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					// Archicad returns response in format: { "result": {...}, "error": {...} }
					// Parse as JObject first to handle the structure correctly
					var responseObj = JObject.Parse(responseString);
					var jsonResponse = new JsonResponse
					{
						Result = responseObj["result"] as JObject,
						Error = responseObj["error"] as JObject
					};
					return jsonResponse;
				}
				else
				{
					return new JsonResponse
					{
						Result = null,
						Error = new JObject
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
					Result = null,
					Error = new JObject
					{
						["message"] = "Request timeout"
					}
				};
			}
			catch (Exception ex)
			{
				return new JsonResponse
				{
					Result = null,
					Error = new JObject
					{
						["message"] = ex.Message
					}
				};
			}
		}

		/// <summary>
		/// Synchronous version of Send
		/// Uses Task.Run to avoid blocking UI thread in Grasshopper
		/// </summary>
		public JsonResponse Send(JsonRequest request)
		{
			try
			{
				// Use Task.Run to execute async operation in background thread
				// This prevents blocking the Grasshopper UI thread
				return Task.Run(async () => await SendAsync(request)).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				return new JsonResponse
				{
					Result = null,
					Error = new JObject
					{
						["message"] = $"Connection error: {ex.Message}"
					}
				};
			}
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

