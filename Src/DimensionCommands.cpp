// *****************************************************************************
// Source code for Dimension Commands
// *****************************************************************************

#include "DimensionCommands.hpp"
#include "ObjectState.hpp"

// -----------------------------------------------------------------------------
// GetPortCommand implementation
// -----------------------------------------------------------------------------

GS::String GetPortCommand::GetName () const
{
	return "GetPort";
}

GS::String GetPortCommand::GetNamespace () const
{
	return "DimensionGh";
}

GS::Optional<GS::UniString> GetPortCommand::GetSchemaDefinitions () const
{
	return GS::NoValue;
}

GS::Optional<GS::UniString> GetPortCommand::GetInputParametersSchema () const
{
	return GS::NoValue;
}

GS::Optional<GS::UniString> GetPortCommand::GetResponseSchema () const
{
	return R"(
		{
			"type": "object",
			"properties": {
				"port": {
					"type": "integer"
				}
			},
			"additionalProperties": false,
			"required": ["port"]
		}
	)";
}

GS::ObjectState GetPortCommand::Execute (const GS::ObjectState& /*parameters*/, GS::ProcessControl& /*processControl*/) const
{
	UShort port = 0;
	GSErrCode err = ACAPI_Command_GetHttpConnectionPort (&port);
	if (err == NoError && port > 0) {
		return GS::ObjectState ("port", (Int32)port);
	}
	// If port cannot be retrieved, return error in standard format
	GS::ObjectState errorOS;
	errorOS.Add ("code", (Int32)err);
	errorOS.Add ("message", "Failed to get HTTP connection port.");
	return GS::ObjectState ("error", errorOS);
}

void GetPortCommand::OnResponseValidationFailed (const GS::ObjectState& /*response*/) const
{
}

// -----------------------------------------------------------------------------
// PingCommand implementation
// -----------------------------------------------------------------------------

GS::String PingCommand::GetName () const
{
	return "Ping";
}

GS::String PingCommand::GetNamespace () const
{
	return "DimensionGh";
}

GS::Optional<GS::UniString> PingCommand::GetSchemaDefinitions () const
{
	return GS::NoValue;
}

GS::Optional<GS::UniString> PingCommand::GetInputParametersSchema () const
{
	return GS::NoValue;
}

GS::Optional<GS::UniString> PingCommand::GetResponseSchema () const
{
	return R"(
		{
			"type": "object",
			"properties": {
				"message": {
					"type": "string"
				}
			},
			"additionalProperties": false,
			"required": ["message"]
		}
	)";
}

GS::ObjectState PingCommand::Execute (const GS::ObjectState& /*parameters*/, GS::ProcessControl& /*processControl*/) const
{
	return GS::ObjectState ("message", "Dimension_Gh alive");
}

void PingCommand::OnResponseValidationFailed (const GS::ObjectState& /*response*/) const
{
}

// -----------------------------------------------------------------------------
// GetDimensionsCommand implementation
// -----------------------------------------------------------------------------

GS::String GetDimensionsCommand::GetName () const
{
	return "GetDimensions";
}

GS::String GetDimensionsCommand::GetNamespace () const
{
	return "DimensionGh";
}

GS::Optional<GS::UniString> GetDimensionsCommand::GetSchemaDefinitions () const
{
	return GS::NoValue;
}

GS::Optional<GS::UniString> GetDimensionsCommand::GetInputParametersSchema () const
{
	return GS::NoValue;
}

GS::Optional<GS::UniString> GetDimensionsCommand::GetResponseSchema () const
{
	return R"(
		{
			"type": "object",
			"properties": {
				"dimensions": {
					"type": "array",
					"items": {
						"type": "object"
					}
				}
			},
			"additionalProperties": false,
			"required": ["dimensions"]
		}
	)";
}

GS::ObjectState GetDimensionsCommand::Execute (const GS::ObjectState& /*parameters*/, GS::ProcessControl& /*processControl*/) const
{
	// TODO: Implement actual dimension retrieval
	GS::ObjectState result;
	GS::Array<GS::ObjectState> dimensions; // Empty array for now
	result.Add ("dimensions", dimensions);
	return result;
}

void GetDimensionsCommand::OnResponseValidationFailed (const GS::ObjectState& /*response*/) const
{
}

// -----------------------------------------------------------------------------
// CreateLinearDimensionCommand implementation
// -----------------------------------------------------------------------------

GS::String CreateLinearDimensionCommand::GetName () const
{
	return "CreateLinearDimension";
}

GS::String CreateLinearDimensionCommand::GetNamespace () const
{
	return "DimensionGh";
}

GS::Optional<GS::UniString> CreateLinearDimensionCommand::GetSchemaDefinitions () const
{
	return GS::NoValue;
}

GS::Optional<GS::UniString> CreateLinearDimensionCommand::GetInputParametersSchema () const
{
	return GS::NoValue;
}

GS::Optional<GS::UniString> CreateLinearDimensionCommand::GetResponseSchema () const
{
	return R"(
		{
			"type": "object",
			"properties": {
				"success": {
					"type": "boolean"
				}
			},
			"additionalProperties": false,
			"required": ["success"]
		}
	)";
}

GS::ObjectState CreateLinearDimensionCommand::Execute (const GS::ObjectState& /*parameters*/, GS::ProcessControl& /*processControl*/) const
{
	// TODO: Implement actual dimension creation
	return GS::ObjectState ("success", true);
}

void CreateLinearDimensionCommand::OnResponseValidationFailed (const GS::ObjectState& /*response*/) const
{
}

