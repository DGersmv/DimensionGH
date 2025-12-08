using Newtonsoft.Json.Linq;

namespace DimensionGhGh.Protocol
{
	/// <summary>
	/// JSON request for Archicad API.ExecuteAddOnCommand
	/// </summary>
	public class JsonRequest
	{
		[Newtonsoft.Json.JsonProperty("command")]
		public string Command { get; set; }
		
		[Newtonsoft.Json.JsonProperty("parameters")]
		public JObject Parameters { get; set; }

		public JsonRequest()
		{
			Command = "API.ExecuteAddOnCommand";
			Parameters = new JObject();
		}

		/// <summary>
		/// Create request for DimensionGh command
		/// </summary>
		public static JsonRequest CreateDimensionGhCommand(string commandName, JObject commandParameters = null)
		{
			var request = new JsonRequest();
			
			var addOnCommandId = new JObject
			{
				["commandNamespace"] = "DimensionGh",
				["commandName"] = commandName
			};

			request.Parameters["addOnCommandId"] = addOnCommandId;
			request.Parameters["addOnCommandParameters"] = commandParameters ?? new JObject();

			return request;
		}
	}
}

