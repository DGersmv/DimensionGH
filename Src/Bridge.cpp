// *****************************************************************************
// Source code for Bridge module (JSON communication bridge)
// *****************************************************************************

#include "Bridge.hpp"
#include "DimensionHelper.hpp"
#include <cmath>
#include <cstdlib>

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

	// Extract double value from JSON
	double ExtractJsonDoubleValue (const GS::UniString& json, const GS::UniString& key)
	{
		GS::UniString searchKey = "\"" + key + "\"";
		Int32 keyPos = json.FindFirst (searchKey);
		if (keyPos == MaxInt32)
			return 0.0;

		// Find colon after key
		Int32 colonPos = json.FindFirst (":", keyPos);
		if (colonPos == MaxInt32)
			return 0.0;

		// Skip whitespace
		Int32 valueStart = colonPos + 1;
		USize jsonLength = json.GetLength ();
		while (valueStart < (Int32)jsonLength && 
			   (json[valueStart] == ' ' || json[valueStart] == '\t')) {
			valueStart++;
		}

		// Find end of number (comma, closing brace, or bracket)
		Int32 valueEnd = valueStart;
		while (valueEnd < (Int32)jsonLength) {
			char c = json[valueEnd];
			if (c == ',' || c == '}' || c == ']' || c == ' ' || c == '\t')
				break;
			valueEnd++;
		}

		if (valueEnd > valueStart) {
			GS::UniString numStr = json.GetSubstring (valueStart, valueEnd);
			// Convert UniString to double using std::atof
			return std::atof (numStr.ToCStr ().Get ());
		}

		return 0.0;
	}

	// Extract point from JSON object (point1 or point2)
	bool ExtractPointFromJson (const GS::UniString& json, const GS::UniString& pointKey, API_Coord& coord)
	{
		// Find point object: "point1": { "x": ..., "y": ..., "z": ... }
		GS::UniString searchKey = "\"" + pointKey + "\"";
		Int32 keyPos = json.FindFirst (searchKey);
		if (keyPos == MaxInt32)
			return false;

		// Find opening brace
		Int32 braceStart = json.FindFirst ("{", keyPos);
		if (braceStart == MaxInt32)
			return false;

		// Find closing brace
		Int32 braceCount = 1;
		Int32 pos = braceStart + 1;
		USize jsonLength = json.GetLength ();
		while (pos < (Int32)jsonLength && braceCount > 0) {
			if (json[pos] == '{')
				braceCount++;
			else if (json[pos] == '}')
				braceCount--;
			pos++;
		}
		if (braceCount != 0)
			return false;

		GS::UniString pointJson = json.GetSubstring (braceStart, pos);

		// Extract x, y (API_Coord only has x and y, no z)
		coord.x = ExtractJsonDoubleValue (pointJson, "x");
		coord.y = ExtractJsonDoubleValue (pointJson, "y");
		// Note: z coordinate is ignored as API_Coord is 2D

		return true;
	}

	// Extract GUID from JSON string value
	bool ExtractGuidFromJson (const GS::UniString& json, const GS::UniString& guidKey, API_Guid& guid)
	{
		// Find GUID key: "elementGuid1": "guid-string"
		GS::UniString searchKey = "\"" + guidKey + "\"";
		Int32 keyPos = json.FindFirst (searchKey);
		if (keyPos == MaxInt32)
			return false;

		// Find colon
		Int32 colonPos = json.FindFirst (":", keyPos);
		if (colonPos == MaxInt32)
			return false;

		// Find opening quote
		Int32 quoteStart = json.FindFirst ("\"", colonPos);
		if (quoteStart == MaxInt32)
			return false;

		// Find closing quote
		Int32 quoteEnd = json.FindFirst ("\"", quoteStart + 1);
		if (quoteEnd == MaxInt32)
			return false;

		GS::UniString guidStr = json.GetSubstring (quoteStart + 1, quoteEnd);
		if (guidStr.IsEmpty ())
			return false;

		guid = APIGuidFromString (guidStr.ToCStr ().Get ());
		return (guid != APINULLGuid);
	}

	// Handle CreateLinearDimension command
	GS::UniString HandleCreateLinearDimensionCommand (const GS::UniString& payload)
	{
		if (payload.IsEmpty ()) {
			return CreateJsonResponse (false, "Empty payload", GS::EmptyUniString);
		}

		API_Coord pt1 = {};
		API_Coord pt2 = {};
		API_Guid elementGuid1 = APINULLGuid;
		API_Guid elementGuid2 = APINULLGuid;
		double offset = 0.0;

		// Extract point1 and point2 from payload
		if (!ExtractPointFromJson (payload, "point1", pt1)) {
			return CreateJsonResponse (false, "Missing or invalid 'point1' in payload", GS::EmptyUniString);
		}

		if (!ExtractPointFromJson (payload, "point2", pt2)) {
			return CreateJsonResponse (false, "Missing or invalid 'point2' in payload", GS::EmptyUniString);
		}

		// Extract optional element GUIDs
		ExtractGuidFromJson (payload, "elementGuid1", elementGuid1);
		ExtractGuidFromJson (payload, "elementGuid2", elementGuid2);

		// Extract optional offset
		offset = ExtractJsonDoubleValue (payload, "offset");

		// Create dimension using DimensionHelper with optional element attachments
		API_Guid createdDimensionGuid = APINULLGuid;
		const API_Guid* guid1Ptr = (elementGuid1 != APINULLGuid) ? &elementGuid1 : nullptr;
		const API_Guid* guid2Ptr = (elementGuid2 != APINULLGuid) ? &elementGuid2 : nullptr;
		bool success = DimensionHelper::CreateLinearDimension (pt1, pt2, &createdDimensionGuid, nullptr, nullptr, guid1Ptr, guid2Ptr, GS::EmptyUniString, GS::EmptyUniString, GS::EmptyUniString, offset);

		if (success) {
			GS::UniString result = "{\"created\":true}";
			return CreateJsonResponse (true, GS::EmptyUniString, result);
		} else {
			return CreateJsonResponse (false, "Failed to create dimension", GS::EmptyUniString);
		}
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

