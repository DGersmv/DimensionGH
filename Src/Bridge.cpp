// *****************************************************************************
// Source code for Bridge module (JSON communication bridge)
// *****************************************************************************

#include "Bridge.hpp"

// -----------------------------------------------------------------------------
// Simple JSON parser helpers
// -----------------------------------------------------------------------------

namespace {
	// Extract string value from JSON (simple implementation)
	GS::UniString ExtractJsonStringValue (const GS::UniString& json, const GS::UniString& key)
	{
		GS::UniString searchKey = "\"" + key + "\"";
		Int32 keyPos = json.FindFirst (searchKey);
		if (keyPos == MaxInt32)
			return GS::EmptyUniString;

		// Find colon after key
		Int32 colonPos = json.FindFirst (":", keyPos);
		if (colonPos == MaxInt32)
			return GS::EmptyUniString;

		// Find opening quote
		Int32 quoteStart = json.FindFirst ("\"", colonPos);
		if (quoteStart == MaxInt32)
			return GS::EmptyUniString;

		// Find closing quote
		Int32 quoteEnd = json.FindFirst ("\"", quoteStart + 1);
		if (quoteEnd == MaxInt32)
			return GS::EmptyUniString;

		return json.GetSubstring (quoteStart + 1, quoteEnd);
	}

	// Create JSON response
	GS::UniString CreateJsonResponse (bool ok, const GS::UniString& error, const GS::UniString& result)
	{
		GS::UniString response = "{";
		response += "\"ok\":" + (ok ? GS::UniString ("true") : GS::UniString ("false"));
		
		if (!error.IsEmpty ()) {
			response += ",\"error\":\"" + error + "\"";
		} else {
			response += ",\"error\":\"\"";
		}

		if (!result.IsEmpty ()) {
			response += ",\"result\":" + result;
		} else {
			response += ",\"result\":{}";
		}

		response += "}";
		return response;
	}

	// Handle Ping command
	GS::UniString HandlePingCommand (const GS::UniString& /*payload*/)
	{
		GS::UniString result = "{\"message\":\"Dimension_Gh alive\"}";
		return CreateJsonResponse (true, GS::EmptyUniString, result);
	}

	// Handle GetDimensions command (stub)
	GS::UniString HandleGetDimensionsCommand (const GS::UniString& /*payload*/)
	{
		GS::UniString result = "{\"dimensions\":[]}";
		return CreateJsonResponse (true, GS::EmptyUniString, result);
	}

	// Handle CreateLinearDimension command (stub)
	GS::UniString HandleCreateLinearDimensionCommand (const GS::UniString& /*payload*/)
	{
		// TODO: Implement actual dimension creation
		GS::UniString result = "{\"success\":true}";
		return CreateJsonResponse (true, GS::EmptyUniString, result);
	}
}

// -----------------------------------------------------------------------------
// Main JSON request handler
// -----------------------------------------------------------------------------

GS::UniString HandleJsonRequest (const GS::UniString& jsonRequest)
{
	if (jsonRequest.IsEmpty ()) {
		return CreateJsonResponse (false, "Empty request", GS::EmptyUniString);
	}

	// Extract command from JSON
	GS::UniString command = ExtractJsonStringValue (jsonRequest, "command");
	if (command.IsEmpty ()) {
		return CreateJsonResponse (false, "Missing 'command' field", GS::EmptyUniString);
	}

	// Extract payload (if present) - simple extraction
	GS::UniString payload;
	Int32 payloadStart = jsonRequest.FindFirst ("\"payload\"");
	if (payloadStart != MaxInt32) {
		Int32 colonPos = jsonRequest.FindFirst (":", payloadStart);
		if (colonPos != MaxInt32) {
			// Skip whitespace after colon
			Int32 valueStart = colonPos + 1;
			USize jsonLength = jsonRequest.GetLength ();
			while (valueStart < (Int32)jsonLength && 
				   (jsonRequest[valueStart] == ' ' || jsonRequest[valueStart] == '\t')) {
				valueStart++;
			}
			
			// Check if it's an object
			if (valueStart < (Int32)jsonLength && jsonRequest[valueStart] == '{') {
				// Find matching closing brace
				Int32 braceCount = 1;
				Int32 pos = valueStart + 1;
				while (pos < (Int32)jsonLength && braceCount > 0) {
					if (jsonRequest[pos] == '{')
						braceCount++;
					else if (jsonRequest[pos] == '}')
						braceCount--;
					pos++;
				}
				if (braceCount == 0) {
					payload = jsonRequest.GetSubstring (valueStart, pos);
				}
			} else if (valueStart < (Int32)jsonLength && jsonRequest[valueStart] == '[') {
				// Array - find matching bracket
				Int32 bracketCount = 1;
				Int32 pos = valueStart + 1;
				while (pos < (Int32)jsonLength && bracketCount > 0) {
					if (jsonRequest[pos] == '[')
						bracketCount++;
					else if (jsonRequest[pos] == ']')
						bracketCount--;
					pos++;
				}
				if (bracketCount == 0) {
					payload = jsonRequest.GetSubstring (valueStart, pos);
				}
			} else {
				// Simple value (string, number, boolean, null) - find until comma or closing brace
				Int32 pos = valueStart;
				while (pos < (Int32)jsonLength) {
					if (jsonRequest[pos] == ',' || jsonRequest[pos] == '}' || jsonRequest[pos] == ']')
						break;
					pos++;
				}
				if (pos > valueStart) {
					payload = jsonRequest.GetSubstring (valueStart, pos);
				}
			}
		}
	}

	// Route to command handler
	if (command == "Ping") {
		return HandlePingCommand (payload);
	} else if (command == "GetDimensions") {
		return HandleGetDimensionsCommand (payload);
	} else if (command == "CreateLinearDimension") {
		return HandleCreateLinearDimensionCommand (payload);
	} else {
		return CreateJsonResponse (false, "Unknown command: " + command, GS::EmptyUniString);
	}
}

