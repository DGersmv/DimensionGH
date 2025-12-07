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

// -----------------------------------------------------------------------------
// CreateHotspot Command - create hotspot on element by coordinates
// -----------------------------------------------------------------------------

class CreateHotspotCommand : public API_AddOnCommand {
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
// UpdateHotspot Command - update hotspot position by GUID
// -----------------------------------------------------------------------------

class UpdateHotspotCommand : public API_AddOnCommand {
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
// DeleteHotspot Command - delete hotspot by GUID
// -----------------------------------------------------------------------------

class DeleteHotspotCommand : public API_AddOnCommand {
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
// DeleteAllHotspots Command - delete all hotspots created by this add-on
// -----------------------------------------------------------------------------

class DeleteAllHotspotsCommand : public API_AddOnCommand {
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
// Global storage for created hotspots (for cleanup on disconnect)
// -----------------------------------------------------------------------------

namespace HotspotManager {
	// Add hotspot GUID to the list (with optional rhinoPointGuid for mapping)
	void AddHotspot(const API_Guid& hotspotGuid, const GS::UniString& rhinoPointGuid = GS::EmptyUniString);
	
	// Remove hotspot GUID from the list
	void RemoveHotspot(const API_Guid& hotspotGuid);
	
	// Find hotspot by rhinoPointGuid
	API_Guid FindHotspotByRhinoGuid(const GS::UniString& rhinoPointGuid);
	
	// Get all hotspot GUIDs
	GS::Array<API_Guid> GetAllHotspots();
	
	// Clear all hotspots
	void ClearAllHotspots();
	
	// Delete all tracked hotspots from Archicad
	void DeleteAllTrackedHotspots();
}

#endif // DIMENSIONCOMMANDS_HPP

