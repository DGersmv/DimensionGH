// *****************************************************************************
// Header file for Dimension Helper module
// *****************************************************************************

#ifndef DIMENSIONHELPER_HPP
#define DIMENSIONHELPER_HPP

#include "APIEnvir.h"
#include "ACAPinc.h"

namespace DimensionHelper {

	// -----------------------------------------------------------------------------
	// Create linear dimension between two points
	// Optionally attach to hotspot elements by GUID (preferred) or to elements by GUID (fallback)
	// Returns dimension GUID via output parameter
	// -----------------------------------------------------------------------------
	bool CreateLinearDimension(
		const API_Coord& pt1,
		const API_Coord& pt2,
		API_Guid* outDimensionGuid,  // Output: GUID of created dimension
		const API_Guid* hotspotGuid1 = nullptr,  // Optional: attach point 1 to this hotspot element (API_HotspotID)
		const API_Guid* hotspotGuid2 = nullptr,  // Optional: attach point 2 to this hotspot element (API_HotspotID)
		const API_Guid* elementGuid1 = nullptr,  // Optional: fallback - attach point 1 to this element
		const API_Guid* elementGuid2 = nullptr,  // Optional: fallback - attach point 2 to this element
		const GS::UniString& layerName = GS::EmptyUniString,
		const GS::UniString& styleName = GS::EmptyUniString,
		const GS::UniString& textOverride = GS::EmptyUniString,
		double offset = 0.0  // Optional: dimension line offset distance (perpendicular to dimension direction)
	);

} // namespace DimensionHelper

#endif // DIMENSIONHELPER_HPP

