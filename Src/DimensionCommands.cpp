// *****************************************************************************
// Source code for Dimension Commands
// *****************************************************************************

#include "DimensionCommands.hpp"
#include "ObjectState.hpp"
#include "DimensionHelper.hpp"

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

// =============================================================================
// DimensionManager - track created dimensions to avoid duplicates
// Forward declaration - implementation is after CreateLinearDimensionCommand
// =============================================================================

namespace DimensionManager {
	API_Guid FindExistingDimension(const API_Guid& hotspot1, const API_Guid& hotspot2);
	void AddDimension(const API_Guid& hotspot1, const API_Guid& hotspot2, const API_Guid& dimensionGuid);
}

// =============================================================================
// CreateLinearDimensionCommand implementation
// =============================================================================

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
	// Disable schema validation to avoid issues with GS::ObjectState serialization
	return GS::NoValue;
}

GS::ObjectState CreateLinearDimensionCommand::Execute (const GS::ObjectState& parameters, GS::ProcessControl& /*processControl*/) const
{
	// Extract point1 and point2 from parameters
	API_Coord pt1 = {};
	API_Coord pt2 = {};
	bool hasPoint1 = false;
	bool hasPoint2 = false;
	API_Guid elementGuid1 = APINULLGuid;
	API_Guid elementGuid2 = APINULLGuid;

	// Try to get point1
	if (parameters.Contains ("point1")) {
		GS::ObjectState point1Obj;
		if (parameters.Get ("point1", point1Obj)) {
			double x = 0.0, y = 0.0;
			if (point1Obj.Get ("x", x) && point1Obj.Get ("y", y)) {
				pt1.x = x;
				pt1.y = y;
				hasPoint1 = true;
			}
		}
	}

	// Try to get point2
	if (parameters.Contains ("point2")) {
		GS::ObjectState point2Obj;
		if (parameters.Get ("point2", point2Obj)) {
			double x = 0.0, y = 0.0;
			if (point2Obj.Get ("x", x) && point2Obj.Get ("y", y)) {
				pt2.x = x;
				pt2.y = y;
				hasPoint2 = true;
			}
		}
	}

	// Try to get hotspot GUIDs first (preferred method)
	// If hotspot GUID is provided, get the element GUID from the hotspot
	API_Guid hotspotGuid1 = APINULLGuid;
	API_Guid hotspotGuid2 = APINULLGuid;
	
	if (parameters.Contains ("hotspotGuid1")) {
		GS::UniString guidStr1;
		if (parameters.Get ("hotspotGuid1", guidStr1) && !guidStr1.IsEmpty()) {
			hotspotGuid1 = APIGuidFromString(guidStr1.ToCStr().Get());
			// Get hotspot element to find the element it's attached to
			API_Element hotspot = {};
			hotspot.header.guid = hotspotGuid1;
			if (ACAPI_Element_Get(&hotspot) == NoError && hotspot.header.type == API_HotspotID) {
				// Find element at hotspot position
				API_ElemSearchPars searchPars = {};
				searchPars.type = API_ZombieElemID;
				searchPars.loc.x = hotspot.hotspot.pos.x;
				searchPars.loc.y = hotspot.hotspot.pos.y;
				searchPars.z = 1.00E6;
				searchPars.filterBits = APIFilt_OnVisLayer | APIFilt_OnActFloor;
				API_Guid foundGuid = APINULLGuid;
				if (ACAPI_Element_SearchElementByCoord(&searchPars, &foundGuid) == NoError) {
					elementGuid1 = foundGuid;
				}
			}
		}
	}

	if (parameters.Contains ("hotspotGuid2")) {
		GS::UniString guidStr2;
		if (parameters.Get ("hotspotGuid2", guidStr2) && !guidStr2.IsEmpty()) {
			hotspotGuid2 = APIGuidFromString(guidStr2.ToCStr().Get());
			// Get hotspot element to find the element it's attached to
			API_Element hotspot = {};
			hotspot.header.guid = hotspotGuid2;
			if (ACAPI_Element_Get(&hotspot) == NoError && hotspot.header.type == API_HotspotID) {
				// Find element at hotspot position
				API_ElemSearchPars searchPars = {};
				searchPars.type = API_ZombieElemID;
				searchPars.loc.x = hotspot.hotspot.pos.x;
				searchPars.loc.y = hotspot.hotspot.pos.y;
				searchPars.z = 1.00E6;
				searchPars.filterBits = APIFilt_OnVisLayer | APIFilt_OnActFloor;
				API_Guid foundGuid = APINULLGuid;
				if (ACAPI_Element_SearchElementByCoord(&searchPars, &foundGuid) == NoError) {
					elementGuid2 = foundGuid;
				}
			}
		}
	}

	// Legacy: Try to get element GUIDs directly (fallback)
	if (elementGuid1 == APINULLGuid && parameters.Contains ("elementGuid1")) {
		GS::UniString guidStr1;
		if (parameters.Get ("elementGuid1", guidStr1) && !guidStr1.IsEmpty()) {
			elementGuid1 = APIGuidFromString(guidStr1.ToCStr().Get());
		}
	}

	if (elementGuid2 == APINULLGuid && parameters.Contains ("elementGuid2")) {
		GS::UniString guidStr2;
		if (parameters.Get ("elementGuid2", guidStr2) && !guidStr2.IsEmpty()) {
			elementGuid2 = APIGuidFromString(guidStr2.ToCStr().Get());
		}
	}

	// Validate that we have both points
	if (!hasPoint1 || !hasPoint2) {
		GS::ObjectState response;
		response.Add ("success", false);
		GS::ObjectState errorOS;
		errorOS.Add ("code", -1);
		if (!hasPoint1 && !hasPoint2) {
			errorOS.Add ("message", "Missing both point1 and point2 in parameters");
		} else if (!hasPoint1) {
			errorOS.Add ("message", "Missing or invalid point1 in parameters");
		} else {
			errorOS.Add ("message", "Missing or invalid point2 in parameters");
		}
		response.Add ("error", errorOS);
		return response;
	}

	// Validate that points are not the same
	double dx = pt2.x - pt1.x;
	double dy = pt2.y - pt1.y;
	double distance = std::hypot (dx, dy);
	if (distance < 1e-6) {
		GS::ObjectState response;
		response.Add ("success", false);
		GS::ObjectState errorOS;
		errorOS.Add ("code", -2);
		errorOS.Add ("message", "Points are too close (distance < 1e-6)");
		response.Add ("error", errorOS);
		return response;
	}

	// Extract optional offset
	double offset = 0.0;
	if (parameters.Contains ("offset")) {
		double offsetValue = 0.0;
		if (parameters.Get ("offset", offsetValue)) {
			offset = offsetValue;
		}
	}

	// Check if dimension already exists for this hotspot pair
	API_Guid existingDimensionGuid = APINULLGuid;
	if (hotspotGuid1 != APINULLGuid && hotspotGuid2 != APINULLGuid) {
		existingDimensionGuid = DimensionManager::FindExistingDimension(hotspotGuid1, hotspotGuid2);
		if (existingDimensionGuid != APINULLGuid) {
			// Dimension already exists - return success without creating new one
			// The dimension will update automatically when hotspots move
			GS::ObjectState response;
			response.Add ("success", true);
			response.Add ("distance", distance);
			response.Add ("dimensionGuid", APIGuidToString(existingDimensionGuid));
			response.Add ("message", "Dimension already exists for this hotspot pair");
			return response;
		}
	}

	// Create dimension with optional hotspot attachments (preferred) or element attachments (fallback)
	const API_Guid* hotspotGuid1Ptr = (hotspotGuid1 != APINULLGuid) ? &hotspotGuid1 : nullptr;
	const API_Guid* hotspotGuid2Ptr = (hotspotGuid2 != APINULLGuid) ? &hotspotGuid2 : nullptr;
	const API_Guid* elementGuid1Ptr = (elementGuid1 != APINULLGuid) ? &elementGuid1 : nullptr;
	const API_Guid* elementGuid2Ptr = (elementGuid2 != APINULLGuid) ? &elementGuid2 : nullptr;
	
	API_Guid createdDimensionGuid = APINULLGuid;
	bool success = DimensionHelper::CreateLinearDimension (pt1, pt2, &createdDimensionGuid, hotspotGuid1Ptr, hotspotGuid2Ptr, elementGuid1Ptr, elementGuid2Ptr, GS::EmptyUniString, GS::EmptyUniString, GS::EmptyUniString, offset);

	GS::ObjectState response;
	if (success && createdDimensionGuid != APINULLGuid) {
		// Register dimension in tracking system
		if (hotspotGuid1 != APINULLGuid && hotspotGuid2 != APINULLGuid) {
			DimensionManager::AddDimension(hotspotGuid1, hotspotGuid2, createdDimensionGuid);
		}
		response.Add ("success", true);
		response.Add ("distance", distance);
		response.Add ("dimensionGuid", APIGuidToString(createdDimensionGuid));
	} else {
		GS::ObjectState errorOS;
		errorOS.Add ("code", -3);
		errorOS.Add ("message", "Failed to create dimension in Archicad. Check Archicad report window for details.");
		response.Add ("success", false);
		response.Add ("error", errorOS);
	}
	return response;
}

void CreateLinearDimensionCommand::OnResponseValidationFailed (const GS::ObjectState& /*response*/) const
{
}

// =============================================================================
// HotspotManager implementation
// =============================================================================

namespace HotspotManager {
	static GS::Array<API_Guid> g_createdHotspots;
	// Map: rhinoPointGuid (string) -> hotspotGuid (API_Guid)
	static GS::HashTable<GS::UniString, API_Guid> g_rhinoToHotspotMap;
	
	void AddHotspot(const API_Guid& hotspotGuid, const GS::UniString& rhinoPointGuid)
	{
		if (hotspotGuid != APINULLGuid) {
			g_createdHotspots.Push(hotspotGuid);
			if (!rhinoPointGuid.IsEmpty()) {
				g_rhinoToHotspotMap.Add(rhinoPointGuid, hotspotGuid);
			}
		}
	}
	
	void RemoveHotspot(const API_Guid& hotspotGuid)
	{
		for (UIndex i = 0; i < g_createdHotspots.GetSize(); ++i) {
			if (g_createdHotspots[i] == hotspotGuid) {
				g_createdHotspots.Delete(i);
				// Remove from map
				for (auto it = g_rhinoToHotspotMap.Begin(); it != g_rhinoToHotspotMap.End(); ++it) {
					if (it->value == hotspotGuid) {
						g_rhinoToHotspotMap.Delete(it->key);
						break;
					}
				}
				break;
			}
		}
	}
	
	// Find hotspot by rhinoPointGuid
	API_Guid FindHotspotByRhinoGuid(const GS::UniString& rhinoPointGuid)
	{
		if (rhinoPointGuid.IsEmpty()) {
			return APINULLGuid;
		}
		
		API_Guid* foundGuid = g_rhinoToHotspotMap.GetPtr(rhinoPointGuid);
		if (foundGuid != nullptr && *foundGuid != APINULLGuid) {
			// Verify hotspot still exists
			API_Element hotspot = {};
			hotspot.header.guid = *foundGuid;
			if (ACAPI_Element_Get(&hotspot) == NoError && hotspot.header.type == API_HotspotID) {
				return *foundGuid;
			} else {
				// Hotspot was deleted, remove from map
				g_rhinoToHotspotMap.Delete(rhinoPointGuid);
				return APINULLGuid;
			}
		}
		return APINULLGuid;
	}
	
	GS::Array<API_Guid> GetAllHotspots()
	{
		return g_createdHotspots;
	}
	
	void ClearAllHotspots()
	{
		g_createdHotspots.Clear();
		g_rhinoToHotspotMap.Clear();
	}
	
	void DeleteAllTrackedHotspots()
	{
		if (g_createdHotspots.IsEmpty()) {
			return;
		}
		// ACAPI_Element_Delete requires GS::Array<API_Guid>
		ACAPI_Element_Delete(g_createdHotspots);
		g_createdHotspots.Clear();
		g_rhinoToHotspotMap.Clear();
	}
}

// =============================================================================
// DimensionManager - track created dimensions to avoid duplicates
// Forward declaration - implementation is after CreateLinearDimensionCommand
// =============================================================================

namespace DimensionManager {
	API_Guid FindExistingDimension(const API_Guid& hotspot1, const API_Guid& hotspot2);
	void AddDimension(const API_Guid& hotspot1, const API_Guid& hotspot2, const API_Guid& dimensionGuid);
}

// =============================================================================
// DimensionManager implementation
// =============================================================================

namespace DimensionManager {
	// Structure to store hotspot pair and corresponding dimension GUID
	struct HotspotPair {
		API_Guid hotspot1;
		API_Guid hotspot2;
		
		bool operator==(const HotspotPair& other) const {
			return (hotspot1 == other.hotspot1 && hotspot2 == other.hotspot2) ||
			       (hotspot1 == other.hotspot2 && hotspot2 == other.hotspot1); // Order doesn't matter
		}
	};
	
	static GS::Array<HotspotPair> g_dimensionPairs;
	static GS::Array<API_Guid> g_dimensionGuids;
	
	// Check if dimension already exists for this hotspot pair
	API_Guid FindExistingDimension(const API_Guid& hotspot1, const API_Guid& hotspot2)
	{
		if (hotspot1 == APINULLGuid || hotspot2 == APINULLGuid) {
			return APINULLGuid;
		}
		
		HotspotPair pair = {hotspot1, hotspot2};
		for (UIndex i = 0; i < g_dimensionPairs.GetSize(); ++i) {
			if (g_dimensionPairs[i] == pair) {
				// Check if dimension still exists
				API_Element dim = {};
				dim.header.guid = g_dimensionGuids[i];
				if (ACAPI_Element_Get(&dim) == NoError && dim.header.type == API_DimensionID) {
					return g_dimensionGuids[i];
				} else {
					// Dimension was deleted, remove from tracking
					g_dimensionPairs.Delete(i);
					g_dimensionGuids.Delete(i);
					return APINULLGuid;
				}
			}
		}
		return APINULLGuid;
	}
	
	// Register a new dimension for hotspot pair
	void AddDimension(const API_Guid& hotspot1, const API_Guid& hotspot2, const API_Guid& dimensionGuid)
	{
		if (hotspot1 == APINULLGuid || hotspot2 == APINULLGuid || dimensionGuid == APINULLGuid) {
			return;
		}
		
		// Check if already exists
		if (FindExistingDimension(hotspot1, hotspot2) != APINULLGuid) {
			return; // Already tracked
		}
		
		HotspotPair pair = {hotspot1, hotspot2};
		g_dimensionPairs.Push(pair);
		g_dimensionGuids.Push(dimensionGuid);
	}
	
	// Clear all tracked dimensions
	void ClearAllDimensions()
	{
		g_dimensionPairs.Clear();
		g_dimensionGuids.Clear();
	}
}

// =============================================================================
// CreateHotspotCommand implementation
// =============================================================================

GS::String CreateHotspotCommand::GetName () const
{
	return "CreateHotspot";
}

GS::String CreateHotspotCommand::GetNamespace () const
{
	return "DimensionGh";
}

GS::Optional<GS::UniString> CreateHotspotCommand::GetSchemaDefinitions () const
{
	return GS::NoValue;
}

GS::Optional<GS::UniString> CreateHotspotCommand::GetInputParametersSchema () const
{
	return GS::NoValue;
}

GS::Optional<GS::UniString> CreateHotspotCommand::GetResponseSchema () const
{
	return GS::NoValue;
}

GS::ObjectState CreateHotspotCommand::Execute (const GS::ObjectState& parameters, GS::ProcessControl& /*processControl*/) const
{
	// Extract coordinates and optional rhinoPointGuid
	API_Coord coord = {};
	bool hasCoord = false;
	GS::UniString rhinoPointGuid;

	// Try to get coordinates
	if (parameters.Contains("x") && parameters.Contains("y")) {
		double x = 0.0, y = 0.0;
		if (parameters.Get("x", x) && parameters.Get("y", y)) {
			coord.x = x;
			coord.y = y;
			hasCoord = true;
		}
	}

	if (!hasCoord) {
		GS::ObjectState response;
		response.Add("success", false);
		GS::ObjectState errorOS;
		errorOS.Add("code", -1);
		errorOS.Add("message", "Missing or invalid coordinates (x, y)");
		response.Add("error", errorOS);
		return response;
	}

	// Try to get optional rhinoPointGuid
	if (parameters.Contains("rhinoPointGuid")) {
		parameters.Get("rhinoPointGuid", rhinoPointGuid);
	}
	
	// Check if hotspot already exists for this rhinoPointGuid
	if (!rhinoPointGuid.IsEmpty()) {
		API_Guid existingHotspotGuid = HotspotManager::FindHotspotByRhinoGuid(rhinoPointGuid);
		if (existingHotspotGuid != APINULLGuid) {
			// Hotspot already exists - update its position and return
			API_Element hotspot = {};
			hotspot.header.guid = existingHotspotGuid;
			if (ACAPI_Element_Get(&hotspot) == NoError && hotspot.header.type == API_HotspotID) {
				// Update coordinates
				hotspot.hotspot.pos.x = coord.x;
				hotspot.hotspot.pos.y = coord.y;
				
				API_Element mask = {};
				ACAPI_ELEMENT_MASK_CLEAR(mask);
				ACAPI_ELEMENT_MASK_SET(mask, API_HotspotType, pos);
				
				GSErrCode err = ACAPI_CallUndoableCommand("UpdateHotspot", [&]() -> GSErrCode {
					return ACAPI_Element_Change(&hotspot, &mask, nullptr, 0, true);
				});
				
				if (err == NoError) {
					GS::ObjectState response;
					response.Add("success", true);
					GS::UniString hotspotGuidStr = APIGuidToString(existingHotspotGuid);
					response.Add("hotspotGuid", hotspotGuidStr);
					response.Add("rhinoPointGuid", rhinoPointGuid);
					response.Add("message", "Hotspot updated (already existed for this Rhino point)");
					return response;
				}
			}
		}
	}

	// Try to find nearest element by coordinate (optional - hotspot can exist without element)
	API_Guid elementGuid = APINULLGuid;
	API_ElemSearchPars searchPars = {};
	searchPars.type = API_ZombieElemID;  // Search for any element
	searchPars.loc.x = coord.x;
	searchPars.loc.y = coord.y;
	searchPars.z = 1.00E6;  // Large Z range
	searchPars.filterBits = APIFilt_OnVisLayer | APIFilt_OnActFloor;

	GSErrCode err = ACAPI_Element_SearchElementByCoord(&searchPars, &elementGuid);
	// Note: We continue even if no element is found - hotspot can be created standalone

	// Create hotspot element
	API_Element hotspot = {};
	hotspot.header.type = API_HotspotID;
	err = ACAPI_Element_GetDefaults(&hotspot, nullptr);
	if (err != NoError) {
		GS::ObjectState response;
		response.Add("success", false);
		GS::ObjectState errorOS;
		errorOS.Add("code", -4);
		errorOS.Add("message", "Failed to get hotspot defaults");
		response.Add("error", errorOS);
		return response;
	}

	hotspot.hotspot.pos.x = coord.x;
	hotspot.hotspot.pos.y = coord.y;
	// Note: API_Coord is 2D only, no z coordinate

	// Create hotspot
	err = ACAPI_CallUndoableCommand("CreateHotspot", [&]() -> GSErrCode {
		return ACAPI_Element_Create(&hotspot, nullptr);
	});

	if (err != NoError) {
		GS::ObjectState response;
		response.Add("success", false);
		GS::ObjectState errorOS;
		errorOS.Add("code", -5);
		errorOS.Add("message", "Failed to create hotspot");
		response.Add("error", errorOS);
		return response;
	}

	// Track the created hotspot with rhinoPointGuid mapping
	HotspotManager::AddHotspot(hotspot.header.guid, rhinoPointGuid);

	// Return success with hotspot GUID and optional element GUID
	GS::ObjectState response;
	response.Add("success", true);
	GS::UniString hotspotGuidStr = APIGuidToString(hotspot.header.guid);
	response.Add("hotspotGuid", hotspotGuidStr);
	if (elementGuid != APINULLGuid) {
		GS::UniString elementGuidStr = APIGuidToString(elementGuid);
		response.Add("elementGuid", elementGuidStr);  // GUID элемента, на котором создан hotspot (если найден)
	}
	if (!rhinoPointGuid.IsEmpty()) {
		response.Add("rhinoPointGuid", rhinoPointGuid);
	}
	return response;
}

void CreateHotspotCommand::OnResponseValidationFailed (const GS::ObjectState& /*response*/) const
{
}

// =============================================================================
// UpdateHotspotCommand implementation
// =============================================================================

GS::String UpdateHotspotCommand::GetName () const
{
	return "UpdateHotspot";
}

GS::String UpdateHotspotCommand::GetNamespace () const
{
	return "DimensionGh";
}

GS::Optional<GS::UniString> UpdateHotspotCommand::GetSchemaDefinitions () const
{
	return GS::NoValue;
}

GS::Optional<GS::UniString> UpdateHotspotCommand::GetInputParametersSchema () const
{
	return GS::NoValue;
}

GS::Optional<GS::UniString> UpdateHotspotCommand::GetResponseSchema () const
{
	return GS::NoValue;
}

GS::ObjectState UpdateHotspotCommand::Execute (const GS::ObjectState& parameters, GS::ProcessControl& /*processControl*/) const
{
	// Extract hotspot GUID and new coordinates
	GS::UniString hotspotGuidStr;
	API_Coord newCoord = {};
	bool hasGuid = false;
	bool hasCoord = false;

	if (parameters.Contains("hotspotGuid")) {
		if (parameters.Get("hotspotGuid", hotspotGuidStr) && !hotspotGuidStr.IsEmpty()) {
			hasGuid = true;
		}
	}

	if (parameters.Contains("x") && parameters.Contains("y")) {
		double x = 0.0, y = 0.0;
		if (parameters.Get("x", x) && parameters.Get("y", y)) {
			newCoord.x = x;
			newCoord.y = y;
			hasCoord = true;
		}
	}

	if (!hasGuid) {
		GS::ObjectState response;
		response.Add("success", false);
		GS::ObjectState errorOS;
		errorOS.Add("code", -1);
		errorOS.Add("message", "Missing or invalid hotspotGuid");
		response.Add("error", errorOS);
		return response;
	}

	if (!hasCoord) {
		GS::ObjectState response;
		response.Add("success", false);
		GS::ObjectState errorOS;
		errorOS.Add("code", -2);
		errorOS.Add("message", "Missing or invalid coordinates (x, y)");
		response.Add("error", errorOS);
		return response;
	}

	API_Guid hotspotGuid = APIGuidFromString(hotspotGuidStr.ToCStr().Get());
	if (hotspotGuid == APINULLGuid) {
		GS::ObjectState response;
		response.Add("success", false);
		GS::ObjectState errorOS;
		errorOS.Add("code", -3);
		errorOS.Add("message", "Invalid hotspot GUID format");
		response.Add("error", errorOS);
		return response;
	}

	// Get hotspot element
	API_Element hotspot = {};
	hotspot.header.guid = hotspotGuid;
	GSErrCode err = ACAPI_Element_Get(&hotspot);
	if (err != NoError || hotspot.header.type != API_HotspotID) {
		GS::ObjectState response;
		response.Add("success", false);
		GS::ObjectState errorOS;
		errorOS.Add("code", -4);
		errorOS.Add("message", "Hotspot not found");
		response.Add("error", errorOS);
		return response;
	}

	// Update coordinates
	hotspot.hotspot.pos.x = newCoord.x;
	hotspot.hotspot.pos.y = newCoord.y;

	// Update hotspot using ACAPI_Element_Change
	// mask is of type API_Element (same structure as element, used to specify which fields to change)
	API_Element mask = {};
	ACAPI_ELEMENT_MASK_CLEAR(mask);
	ACAPI_ELEMENT_MASK_SET(mask, API_HotspotType, pos);  // pos is the field name in API_HotspotType

	err = ACAPI_CallUndoableCommand("UpdateHotspot", [&]() -> GSErrCode {
		return ACAPI_Element_Change(&hotspot, &mask, nullptr, 0, true);
	});

	if (err != NoError) {
		GS::ObjectState response;
		response.Add("success", false);
		GS::ObjectState errorOS;
		errorOS.Add("code", -5);
		errorOS.Add("message", "Failed to update hotspot");
		response.Add("error", errorOS);
		return response;
	}

	GS::ObjectState response;
	response.Add("success", true);
	return response;
}

void UpdateHotspotCommand::OnResponseValidationFailed (const GS::ObjectState& /*response*/) const
{
}

// =============================================================================
// DeleteHotspotCommand implementation
// =============================================================================

GS::String DeleteHotspotCommand::GetName () const
{
	return "DeleteHotspot";
}

GS::String DeleteHotspotCommand::GetNamespace () const
{
	return "DimensionGh";
}

GS::Optional<GS::UniString> DeleteHotspotCommand::GetSchemaDefinitions () const
{
	return GS::NoValue;
}

GS::Optional<GS::UniString> DeleteHotspotCommand::GetInputParametersSchema () const
{
	return GS::NoValue;
}

GS::Optional<GS::UniString> DeleteHotspotCommand::GetResponseSchema () const
{
	return GS::NoValue;
}

GS::ObjectState DeleteHotspotCommand::Execute (const GS::ObjectState& parameters, GS::ProcessControl& /*processControl*/) const
{
	GS::UniString hotspotGuidStr;
	if (!parameters.Contains("hotspotGuid") || !parameters.Get("hotspotGuid", hotspotGuidStr) || hotspotGuidStr.IsEmpty()) {
		GS::ObjectState response;
		response.Add("success", false);
		GS::ObjectState errorOS;
		errorOS.Add("code", -1);
		errorOS.Add("message", "Missing or invalid hotspotGuid");
		response.Add("error", errorOS);
		return response;
	}

	API_Guid hotspotGuid = APIGuidFromString(hotspotGuidStr.ToCStr().Get());
	if (hotspotGuid == APINULLGuid) {
		GS::ObjectState response;
		response.Add("success", false);
		GS::ObjectState errorOS;
		errorOS.Add("code", -2);
		errorOS.Add("message", "Invalid hotspot GUID format");
		response.Add("error", errorOS);
		return response;
	}

	// Get hotspot element
	API_Element hotspot = {};
	hotspot.header.guid = hotspotGuid;
	GSErrCode err = ACAPI_Element_Get(&hotspot);
	if (err != NoError || hotspot.header.type != API_HotspotID) {
		GS::ObjectState response;
		response.Add("success", false);
		GS::ObjectState errorOS;
		errorOS.Add("code", -3);
		errorOS.Add("message", "Hotspot not found");
		response.Add("error", errorOS);
		return response;
	}

	// Delete hotspot - ACAPI_Element_Delete requires GS::Array<API_Guid>
	GS::Array<API_Guid> guidsToDelete;
	guidsToDelete.Push(hotspotGuid);
	err = ACAPI_CallUndoableCommand("DeleteHotspot", [&]() -> GSErrCode {
		return ACAPI_Element_Delete(guidsToDelete);
	});

	if (err != NoError) {
		GS::ObjectState response;
		response.Add("success", false);
		GS::ObjectState errorOS;
		errorOS.Add("code", -4);
		errorOS.Add("message", "Failed to delete hotspot");
		response.Add("error", errorOS);
		return response;
	}

	// Remove from tracking
	HotspotManager::RemoveHotspot(hotspotGuid);

	GS::ObjectState response;
	response.Add("success", true);
	return response;
}

void DeleteHotspotCommand::OnResponseValidationFailed (const GS::ObjectState& /*response*/) const
{
}

// =============================================================================
// DeleteAllHotspotsCommand implementation
// =============================================================================

GS::String DeleteAllHotspotsCommand::GetName () const
{
	return "DeleteAllHotspots";
}

GS::String DeleteAllHotspotsCommand::GetNamespace () const
{
	return "DimensionGh";
}

GS::Optional<GS::UniString> DeleteAllHotspotsCommand::GetSchemaDefinitions () const
{
	return GS::NoValue;
}

GS::Optional<GS::UniString> DeleteAllHotspotsCommand::GetInputParametersSchema () const
{
	return GS::NoValue;
}

GS::Optional<GS::UniString> DeleteAllHotspotsCommand::GetResponseSchema () const
{
	return GS::NoValue;
}

GS::ObjectState DeleteAllHotspotsCommand::Execute (const GS::ObjectState& /*parameters*/, GS::ProcessControl& /*processControl*/) const
{
	// Delete all tracked hotspots
	HotspotManager::DeleteAllTrackedHotspots();

	GS::ObjectState response;
	response.Add("success", true);
	response.Add("deletedCount", (Int32)0);  // Count is 0 after deletion
	return response;
}

void DeleteAllHotspotsCommand::OnResponseValidationFailed (const GS::ObjectState& /*response*/) const
{
}

