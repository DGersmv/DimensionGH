using Newtonsoft.Json.Linq;

namespace DimensionGhGh.Protocol
{
	/// <summary>
	/// JSON response from Archicad API
	/// </summary>
	public class JsonResponse
	{
		// Archicad returns response in format:
		// {
		//   "result": { "addOnCommandResponse": {...} },
		//   "error": { "message": "..." }
		// }
		// We don't have a direct "succeeded" field, we check if result exists
		public JObject Result { get; set; }
		public JObject Error { get; set; }
		
		/// <summary>
		/// Check if request succeeded (has result and no error)
		/// </summary>
		public bool Succeeded
		{
			get
			{
				return Result != null && Error == null;
			}
		}

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
			
			// Try to get error from addOnCommandResponse if present
			var addOnResponse = GetAddOnCommandResponse();
			if (addOnResponse != null)
			{
				var errorObj = addOnResponse["error"];
				if (errorObj != null && errorObj["message"] != null)
				{
					return errorObj["message"].ToString();
				}
			}
			
			return "Unknown error";
		}
	}
}

