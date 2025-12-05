// *****************************************************************************
// Header file for Bridge module (JSON communication bridge)
// *****************************************************************************

#ifndef BRIDGE_HPP
#define BRIDGE_HPP

#include "APIEnvir.h"
#include "ACAPinc.h"

// -----------------------------------------------------------------------------
// Handle JSON request from JavaScript/Grasshopper
// 
// Input: JSON string with format:
//   {
//     "command": "Ping" | "GetDimensions" | "CreateLinearDimension",
//     "payload": { ... }
//   }
//
// Output: JSON string with format:
//   {
//     "ok": true/false,
//     "error": "error message or empty",
//     "result": { ... }
//   }
// -----------------------------------------------------------------------------

GS::UniString HandleJsonRequest (const GS::UniString& jsonRequest);

#endif // BRIDGE_HPP

