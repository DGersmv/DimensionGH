// *****************************************************************************
// Source code for Dimension Helper module
// Based on MarkupHelper::CreateDimensionBetweenPoints from BrowserReplInt
// *****************************************************************************

#include "DimensionHelper.hpp"
#include "APICommon.h"
#include <limits>
#include <cmath>

namespace DimensionHelper {

	// Helper function to find nearest hotspot on element by coordinate
	// Returns true if found, and sets neig, coord, and elementType
	// This matches the pattern from Element_Snippets.cpp where hotspotArray[i].Get(neig, coord) is used
	static bool FindNearestHotspot(const API_Guid& elementGuid, const API_Coord& targetCoord, API_Neig& neig, API_Coord& hotspotCoord, API_ElemType& elementType)
	{
		// Get element to determine its type
		API_Element element = {};
		element.header.guid = elementGuid;
		if (ACAPI_Element_Get(&element) != NoError) {
			return false;
		}
		elementType = element.header.type;

		// Get element hotspots using ACAPI_Element_GetHotspots
		// This returns hotspots that are already attached to the element
		GS::Array<API_ElementHotspot> hotspotArray;
		if (ACAPI_Element_GetHotspots(elementGuid, &hotspotArray) != NoError) {
			return false;
		}

		if (hotspotArray.IsEmpty()) {
			return false;
		}

		// Find nearest hotspot - use the same pattern as Element_Snippets.cpp
		double minDist = std::numeric_limits<double>::max();
		Int32 nearestIdx = -1;
		API_Neig nearestNeig = {};
		API_Coord nearestCoord = {};

		for (Int32 i = 0; i < (Int32)hotspotArray.GetSize(); ++i) {
			API_Neig currentNeig;
			API_Coord3D coord;
			hotspotArray[i].Get(currentNeig, coord);  // Get neig and coord from hotspot
			
			double dx = coord.x - targetCoord.x;
			double dy = coord.y - targetCoord.y;
			double dist = std::hypot(dx, dy);
			if (dist < minDist) {
				minDist = dist;
				nearestIdx = i;
				nearestNeig = currentNeig;  // Save the entire neig structure
				nearestCoord.x = coord.x;
				nearestCoord.y = coord.y;
			}
		}

		// Если ближайшая точка дальше 10 см, не привязываемся
		if (nearestIdx < 0 || minDist > 0.1) {
			return false;
		}

		// Return the neig and coord from the nearest hotspot
		neig = nearestNeig;
		hotspotCoord = nearestCoord;
		return true;
	}

	bool CreateLinearDimension(
		const API_Coord& pt1,
		const API_Coord& pt2,
		API_Guid* outDimensionGuid,
		const API_Guid* hotspotGuid1,
		const API_Guid* hotspotGuid2,
		const API_Guid* elementGuid1,
		const API_Guid* elementGuid2,
		const GS::UniString& /*layerName*/,
		const GS::UniString& /*styleName*/,
		const GS::UniString& /*textOverride*/,
		double offset)
	{
		const double dx = pt2.x - pt1.x;
		const double dy = pt2.y - pt1.y;
		const double len = std::hypot(dx, dy);
		if (len < 1e-6) return false; // точки совпали

		API_Element dim = {};
		dim.header.type = API_DimensionID;

		// Get defaults - this will use last used dimension properties (style, colors, arrows, etc.)
		GSErrCode err = ACAPI_Element_GetDefaults(&dim, nullptr);
		if (err != NoError) return false;

		// Only set the geometry (base line and direction) - keep all other properties from defaults
		// Базовая линия проходит через A и B
		dim.dimension.refC.x = pt1.x;
		dim.dimension.refC.y = pt1.y;
		dim.dimension.direction.x = dx;   // направление A→B
		dim.dimension.direction.y = dy;
		
		// Apply offset if specified (perpendicular to dimension direction)
		// Offset is applied to the dimension line position (refC)
		// Perpendicular vector to direction (dx, dy) is (-dy, dx) normalized
		if (std::abs(offset) > 1e-6) {
			double dirLen = len;
			if (dirLen > 1e-6) {
				// Normalized perpendicular vector
				double perpX = -dy / dirLen;
				double perpY = dx / dirLen;
				// Apply offset to refC
				dim.dimension.refC.x += perpX * offset;
				dim.dimension.refC.y += perpY * offset;
			}
		}

		// Узлы размерной цепочки: кладём ТУДА ЖЕ, без проекций
		API_ElementMemo memo = {};
		BNZeroMemory(&memo, sizeof(API_ElementMemo));
		dim.dimension.nDimElem = 2;

		memo.dimElems = reinterpret_cast<API_DimElem**>(
			BMAllocateHandle(2 * sizeof(API_DimElem), ALLOCATE_CLEAR, 0)
		);
		if (memo.dimElems == nullptr) return false;

		API_DimElem& e1 = (*memo.dimElems)[0];
		
		// Priority 1: Try to attach to hotspot element if hotspot GUID provided
		if (hotspotGuid1 != nullptr && *hotspotGuid1 != APINULLGuid) {
			// Get hotspot element
			API_Element hotspot = {};
			hotspot.header.guid = *hotspotGuid1;
			if (ACAPI_Element_Get(&hotspot) == NoError && hotspot.header.type == API_HotspotID) {
				// Привязываемся к hotspot элементу напрямую
				e1.base.loc = hotspot.hotspot.pos;  // Координата из hotspot элемента
				e1.base.base.type = API_HotspotID;  // Тип - hotspot элемент
				e1.base.base.guid = *hotspotGuid1;  // GUID hotspot элемента
				e1.base.base.inIndex = 0;  // Для hotspot элементов обычно 0
				e1.base.base.line = false;
				e1.base.base.special = false;
				// pos устанавливается после base.loc
				e1.pos.x = e1.base.loc.x;
				e1.pos.y = dim.dimension.refC.y;  // Y берется из refC
			} else {
				// Hotspot не найден - используем только координаты
				e1.base.loc = pt1;
				e1.base.base.line = false;
				e1.base.base.special = false;
				e1.pos = pt1;
			}
		}
		// Priority 2: Try to attach to element if element GUID provided (fallback)
		else if (elementGuid1 != nullptr && *elementGuid1 != APINULLGuid) {
			API_Neig neig = {};
			API_Coord hotspotCoord = {};
			API_ElemType elementType = {};
			if (FindNearestHotspot(*elementGuid1, pt1, neig, hotspotCoord, elementType)) {
				// Привязываемся к элементу - используем данные из hotspot (как в примере)
				e1.base.loc = hotspotCoord;  // Координата из hotspot
				e1.base.base.type = elementType;
				e1.base.base.guid = *elementGuid1;  // GUID элемента (instance GUID)
				e1.base.base.inIndex = neig.inIndex;  // inIndex из neig (как в примере)
				e1.base.base.line = false;
				e1.base.base.special = false;
				// pos устанавливается после base.loc, как в примере
				e1.pos.x = e1.base.loc.x;
				e1.pos.y = dim.dimension.refC.y;  // Y берется из refC, как в примере
			} else {
				// Не удалось найти точку привязки - используем только координаты
				e1.base.loc = pt1;
				e1.base.base.line = false;
				e1.base.base.special = false;
				e1.pos = pt1;
			}
		} else {
			// No attachment - just coordinates
			e1.base.loc = pt1;
			e1.base.base.line = false;
			e1.base.base.special = false;
			e1.pos = pt1;
		}

		API_DimElem& e2 = (*memo.dimElems)[1];
		
		// Priority 1: Try to attach to hotspot element if hotspot GUID provided
		if (hotspotGuid2 != nullptr && *hotspotGuid2 != APINULLGuid) {
			// Get hotspot element
			API_Element hotspot = {};
			hotspot.header.guid = *hotspotGuid2;
			if (ACAPI_Element_Get(&hotspot) == NoError && hotspot.header.type == API_HotspotID) {
				// Привязываемся к hotspot элементу напрямую
				e2.base.loc = hotspot.hotspot.pos;  // Координата из hotspot элемента
				e2.base.base.type = API_HotspotID;  // Тип - hotspot элемент
				e2.base.base.guid = *hotspotGuid2;  // GUID hotspot элемента
				e2.base.base.inIndex = 0;  // Для hotspot элементов обычно 0
				e2.base.base.line = false;
				e2.base.base.special = false;
				// pos устанавливается после base.loc
				e2.pos.x = e2.base.loc.x;
				e2.pos.y = dim.dimension.refC.y;  // Y берется из refC
			} else {
				// Hotspot не найден - используем только координаты
				e2.base.loc = pt2;
				e2.base.base.line = false;
				e2.base.base.special = false;
				e2.pos = pt2;
			}
		}
		// Priority 2: Try to attach to element if element GUID provided (fallback)
		else if (elementGuid2 != nullptr && *elementGuid2 != APINULLGuid) {
			API_Neig neig = {};
			API_Coord hotspotCoord = {};
			API_ElemType elementType = {};
			if (FindNearestHotspot(*elementGuid2, pt2, neig, hotspotCoord, elementType)) {
				// Привязываемся к элементу - используем данные из hotspot (как в примере)
				e2.base.loc = hotspotCoord;  // Координата из hotspot
				e2.base.base.type = elementType;
				e2.base.base.guid = *elementGuid2;  // GUID элемента (instance GUID)
				e2.base.base.inIndex = neig.inIndex;  // inIndex из neig (как в примере)
				e2.base.base.line = false;
				e2.base.base.special = false;
				// pos устанавливается после base.loc, как в примере
				e2.pos.x = e2.base.loc.x;
				e2.pos.y = dim.dimension.refC.y;  // Y берется из refC, как в примере
			} else {
				// Не удалось найти точку привязки - используем только координаты
				e2.base.loc = pt2;
				e2.base.base.line = false;
				e2.base.base.special = false;
				e2.pos = pt2;
			}
		} else {
			// No attachment - just coordinates
			e2.base.loc = pt2;
			e2.base.base.line = false;
			e2.base.base.special = false;
			e2.pos = pt2;
		}

		// Use ACAPI_CallUndoableCommand for proper undo support
		err = ACAPI_CallUndoableCommand("CreateLinearDimension", [&]() -> GSErrCode {
			GSErrCode createErr = ACAPI_Element_Create(&dim, &memo);
			if (createErr != NoError) {
				// Log error for debugging
				ACAPI_WriteReport("DimensionHelper::CreateLinearDimension failed with error: %d", false, createErr);
			} else if (outDimensionGuid != nullptr) {
				// Return created dimension GUID
				*outDimensionGuid = dim.header.guid;
			}
			return createErr;
		});
		
		ACAPI_DisposeElemMemoHdls(&memo);

		return (err == NoError);
	}

} // namespace DimensionHelper

