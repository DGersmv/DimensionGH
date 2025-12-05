using Newtonsoft.Json.Linq;

namespace DimensionGhGh.Protocol
{
	/// <summary>
	/// JSON response from Archicad API
	/// </summary>
	public class JsonResponse
	{
		public bool Succeeded { get; set; }
		public JObject Result { get; set; }
		public JObject Error { get; set; }

		/// <summary>
		/// Get add-on command response from result
		/// </summary>
		public JToken GetAddOnCommandResponse()
		{
			if (Result != null && Result["addOnCommandResponse"] != null)
			{
				return Result["addOnCommandResponse"];
			}
			return null;
		}

		/// <summary>
		/// Get error message
		/// </summary>
		public string GetErrorMessage()
		{
			if (Error != null && Error["message"] != null)
			{
				return Error["message"].ToString();
			}
			return "Unknown error";
		}
	}
}

