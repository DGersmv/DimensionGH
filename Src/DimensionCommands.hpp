// *****************************************************************************
// Header file for Dimension Commands (using Archicad's built-in command system)
// *****************************************************************************

#ifndef DIMENSIONCOMMANDS_HPP
#define DIMENSIONCOMMANDS_HPP

#include "APIEnvir.h"
#include "ACAPinc.h"

// -----------------------------------------------------------------------------
// GetPort Command - returns HTTP connection port
// -----------------------------------------------------------------------------

class GetPortCommand : public API_AddOnCommand {
public:
	virtual GS::String							GetName () const override;
	virtual GS::String							GetNamespace () const override;
	virtual GS::Optional<GS::UniString>			GetSchemaDefinitions () const override;
	virtual GS::Optional<GS::UniString>			GetInputParametersSchema () const override;
	virtual GS::Optional<GS::UniString>			GetResponseSchema () const override;
	
	virtual API_AddOnCommandExecutionPolicy		GetExecutionPolicy () const override { return API_AddOnCommandExecutionPolicy::ScheduleForExecutionOnMainThread; }
	virtual bool								IsProcessWindowVisible () const override { return false; }

	virtual GS::ObjectState						Execute (const GS::ObjectState& parameters, GS::ProcessControl& processControl) const override;
	virtual void								OnResponseValidationFailed (const GS::ObjectState& response) const override;
};

// -----------------------------------------------------------------------------
// Ping Command - simple test command
// -----------------------------------------------------------------------------

class PingCommand : public API_AddOnCommand {
public:
	virtual GS::String							GetName () const override;
	virtual GS::String							GetNamespace () const override;
	virtual GS::Optional<GS::UniString>			GetSchemaDefinitions () const override;
	virtual GS::Optional<GS::UniString>			GetInputParametersSchema () const override;
	virtual GS::Optional<GS::UniString>			GetResponseSchema () const override;
	
	virtual API_AddOnCommandExecutionPolicy		GetExecutionPolicy () const override { return API_AddOnCommandExecutionPolicy::ScheduleForExecutionOnMainThread; }
	virtual bool								IsProcessWindowVisible () const override { return false; }

	virtual GS::ObjectState						Execute (const GS::ObjectState& parameters, GS::ProcessControl& processControl) const override;
	virtual void								OnResponseValidationFailed (const GS::ObjectState& response) const override;
};

// -----------------------------------------------------------------------------
// GetDimensions Command - get all dimensions from project
// -----------------------------------------------------------------------------

class GetDimensionsCommand : public API_AddOnCommand {
public:
	virtual GS::String							GetName () const override;
	virtual GS::String							GetNamespace () const override;
	virtual GS::Optional<GS::UniString>			GetSchemaDefinitions () const override;
	virtual GS::Optional<GS::UniString>			GetInputParametersSchema () const override;
	virtual GS::Optional<GS::UniString>			GetResponseSchema () const override;
	
	virtual API_AddOnCommandExecutionPolicy		GetExecutionPolicy () const override { return API_AddOnCommandExecutionPolicy::ScheduleForExecutionOnMainThread; }
	virtual bool								IsProcessWindowVisible () const override { return false; }

	virtual GS::ObjectState						Execute (const GS::ObjectState& parameters, GS::ProcessControl& processControl) const override;
	virtual void								OnResponseValidationFailed (const GS::ObjectState& response) const override;
};

// -----------------------------------------------------------------------------
// CreateLinearDimension Command - create linear dimension
// -----------------------------------------------------------------------------

class CreateLinearDimensionCommand : public API_AddOnCommand {
public:
	virtual GS::String							GetName () const override;
	virtual GS::String							GetNamespace () const override;
	virtual GS::Optional<GS::UniString>			GetSchemaDefinitions () const override;
	virtual GS::Optional<GS::UniString>			GetInputParametersSchema () const override;
	virtual GS::Optional<GS::UniString>			GetResponseSchema () const override;
	
	virtual API_AddOnCommandExecutionPolicy		GetExecutionPolicy () const override { return API_AddOnCommandExecutionPolicy::ScheduleForExecutionOnMainThread; }
	virtual bool								IsProcessWindowVisible () const override { return false; }

	virtual GS::ObjectState						Execute (const GS::ObjectState& parameters, GS::ProcessControl& processControl) const override;
	virtual void								OnResponseValidationFailed (const GS::ObjectState& response) const override;
};

#endif // DIMENSIONCOMMANDS_HPP

