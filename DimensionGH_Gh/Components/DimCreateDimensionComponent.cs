using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Attributes;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using DimensionGhGh.Client;
using DimensionGhGh.Protocol;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace DimensionGhGh.Components
{
	/// <summary>
	/// Custom attributes for DimCreateDimensionComponent with Update button
	/// </summary>
	public class DimCreateDimensionComponentAttributes : GH_ComponentAttributes
	{
		public DimCreateDimensionComponentAttributes(DimCreateDimensionComponent owner) : base(owner)
		{
		}

		protected override void Layout()
		{
			base.Layout();
			
			// Get the base layout bounds - let Grasshopper calculate proper size
			var baseBounds = Bounds;
			
			// Extend bounds to include button at the bottom
			var extendedBounds = baseBounds;
			extendedBounds.Height += 22; // Add space for button (20px + 2px margin)
			Bounds = extendedBounds;
			
			// Position button at the bottom of the component
			var buttonBounds = new RectangleF(
				Bounds.Left + 1,
				Bounds.Bottom - 21,
				Bounds.Width - 2,
				20
			);
			
			// Store button bounds for click detection
			ButtonBounds = buttonBounds;
		}

		private RectangleF ButtonBounds { get; set; }

		protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
		{
			// Draw background in Wires channel (before base rendering)
			if (channel == GH_CanvasChannel.Wires)
			{
				// Draw component background with rounded corners and Archicad gray color
				var componentRect = Bounds;
				var cornerRadius = 4.0f; // Rounded corners radius
				
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				
				// Create rounded rectangle path
				using (var path = new GraphicsPath())
				{
					// Top-left corner
					path.AddArc(componentRect.X, componentRect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
					// Top-right corner
					path.AddArc(componentRect.Right - cornerRadius * 2, componentRect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
					// Bottom-right corner
					path.AddArc(componentRect.Right - cornerRadius * 2, componentRect.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
					// Bottom-left corner
					path.AddArc(componentRect.X, componentRect.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
					path.CloseFigure();
					
					// Fill with Archicad gray color (#E0E0E0)
					using (var brush = new SolidBrush(Color.FromArgb(255, 224, 224, 224))) // Archicad gray
					{
						graphics.FillPath(brush, path);
					}
					
					// Draw border
					using (var pen = new Pen(Color.FromArgb(255, 192, 192, 192), 1)) // Slightly darker gray border
					{
						graphics.DrawPath(pen, path);
					}
				}
			}
			
			// Render base component (icon, inputs, outputs) - this will render on top of background
			base.Render(canvas, graphics, channel);
			
			// Draw button in Objects channel (after base rendering)
			if (channel == GH_CanvasChannel.Objects)
			{
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				
				var buttonRect = ButtonBounds;
				var buttonRadius = 2.0f; // Match Archicad style (2px radius)
				
				using (var buttonPath = new GraphicsPath())
				{
					buttonPath.AddArc(buttonRect.X, buttonRect.Y, buttonRadius * 2, buttonRadius * 2, 180, 90);
					buttonPath.AddArc(buttonRect.Right - buttonRadius * 2, buttonRect.Y, buttonRadius * 2, buttonRadius * 2, 270, 90);
					buttonPath.AddArc(buttonRect.Right - buttonRadius * 2, buttonRect.Bottom - buttonRadius * 2, buttonRadius * 2, buttonRadius * 2, 0, 90);
					buttonPath.AddArc(buttonRect.X, buttonRect.Bottom - buttonRadius * 2, buttonRadius * 2, buttonRadius * 2, 90, 90);
					buttonPath.CloseFigure();
					
					// Button background - Archicad style gradient: #ffffff -> #f2f2f2 -> #e2e2e2
					using (var brush = new LinearGradientBrush(
						new PointF(buttonRect.Left, buttonRect.Top),
						new PointF(buttonRect.Left, buttonRect.Bottom),
						Color.FromArgb(255, 255, 255, 255),      // #ffffff top
						Color.FromArgb(255, 226, 226, 226)))    // #e2e2e2 bottom (approximate middle #f2f2f2)
					{
						// Set middle color for gradient
						var blend = new ColorBlend(3);
						blend.Colors = new Color[] {
							Color.FromArgb(255, 255, 255, 255),      // #ffffff 0%
							Color.FromArgb(255, 242, 242, 242),      // #f2f2f2 50%
							Color.FromArgb(255, 226, 226, 226)      // #e2e2e2 100%
						};
						blend.Positions = new float[] { 0f, 0.5f, 1f };
						brush.InterpolationColors = blend;
						
						graphics.FillPath(brush, buttonPath);
					}
					
					// Button border - rgba(0,0,0,0.25) = #404040 with alpha
					using (var pen = new Pen(Color.FromArgb(64, 0, 0, 0), 1)) // rgba(0,0,0,0.25)
					{
						graphics.DrawPath(pen, buttonPath);
					}
					
					// Inner highlight shadow - 0 1px 0 #ffffff inset
					using (var highlightPen = new Pen(Color.FromArgb(128, 255, 255, 255), 1))
					{
						var highlightRect = buttonRect;
						highlightRect.Height = 1;
						graphics.DrawLine(highlightPen, highlightRect.Left + buttonRadius, highlightRect.Top, 
							highlightRect.Right - buttonRadius, highlightRect.Top);
					}
					
					// Button text - #2f2f2f (dark gray, not black)
					using (var font = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Bold))
					using (var brush = new SolidBrush(Color.FromArgb(255, 47, 47, 47))) // #2f2f2f
					{
						var textRect = buttonRect;
						var format = new StringFormat
						{
							Alignment = StringAlignment.Center,
							LineAlignment = StringAlignment.Center
						};
						graphics.DrawString("Update", font, brush, textRect, format);
					}
				}
			}
		}

		public override bool IsPickRegion(PointF point)
		{
			// Check if point is within button bounds
			if (ButtonBounds.Contains(point))
			{
				return true;
			}
			return base.IsPickRegion(point);
		}

		public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, Grasshopper.GUI.GH_CanvasMouseEvent e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				// Check if click is within button bounds (using canvas coordinates)
				if (ButtonBounds.Contains(e.CanvasLocation))
				{
					var component = Owner as DimCreateDimensionComponent;
					if (component != null)
					{
						component.OnUpdateButtonClick();
						return GH_ObjectResponse.Handled;
					}
				}
			}
			return base.RespondToMouseDown(sender, e);
		}
	}

	/// <summary>
	/// Main component for creating dimensions in Archicad
	/// Takes port number and list of point pairs, checks connection and creates dimensions
	/// </summary>
	public class DimCreateDimensionComponent : GH_Component
	{
		private bool _updateButtonPressed = false;
		
		// Static dictionary to store hotspot GUIDs by point coordinates (as string key)
		// This allows us to preserve GUIDs between component solves
		private static System.Collections.Generic.Dictionary<string, string> _hotspotGuidCache = 
			new System.Collections.Generic.Dictionary<string, string>();
		
		// Helper method to create a unique key from point coordinates
		private static string GetPointKey(Point3d point, Guid? rhinoGuid)
		{
			// Use Rhino GUID if available, otherwise use coordinates
			if (rhinoGuid.HasValue)
			{
				return $"Rhino_{rhinoGuid.Value}";
			}
			// Use coordinates rounded to 1mm precision as key
			return $"Coord_{Math.Round(point.X, 3)}_{Math.Round(point.Y, 3)}_{Math.Round(point.Z, 3)}";
		}

		/// <summary>
		/// Convert coordinate from Rhino units to meters (ArchiCAD units)
		/// </summary>
		private double ConvertToMeters(double valueInRhinoUnits)
		{
			var doc = Rhino.RhinoDoc.ActiveDoc;
			if (doc == null)
			{
				// Default: assume millimeters if doc is not available
				return valueInRhinoUnits / 1000.0;
			}

			var unitSystem = doc.ModelUnitSystem;
			switch (unitSystem)
			{
				case Rhino.UnitSystem.Millimeters:
					return valueInRhinoUnits / 1000.0;
				case Rhino.UnitSystem.Centimeters:
					return valueInRhinoUnits / 100.0;
				case Rhino.UnitSystem.Meters:
					return valueInRhinoUnits;
				case Rhino.UnitSystem.Inches:
					return valueInRhinoUnits * 0.0254;
				case Rhino.UnitSystem.Feet:
					return valueInRhinoUnits * 0.3048;
				default:
					// Default: assume millimeters for unknown unit systems
					return valueInRhinoUnits / 1000.0;
			}
		}

		public DimCreateDimensionComponent()
			: base("Dim_CreateDimension", "DimCreate",
				"Main component: Check connection to Archicad and create linear dimensions from point pairs. Click Update button to sync hotspots.",
				"Info227", "AC")
		{
		}

		public override void CreateAttributes()
		{
			m_attributes = new DimCreateDimensionComponentAttributes(this);
		}

		public void OnUpdateButtonClick()
		{
			_updateButtonPressed = true;
			ExpireSolution(true);
		}

		protected override void RegisterInputParams(GH_InputParamManager pManager)
		{
			pManager.AddIntegerParameter("Port", "P", "HTTP port number", GH_ParamAccess.item, 19723);
			pManager.AddGenericParameter("PointPairs", "PP", "List of point pairs to create dimensions", GH_ParamAccess.list);
			pManager.AddBooleanParameter("Connected", "C", "Connection status to Archicad (true = connected)", GH_ParamAccess.item, false);
			pManager.AddNumberParameter("Offset", "O", "Dimension line offset distance (0 = no offset)", GH_ParamAccess.item, 0.0);
		}

		protected override void RegisterOutputParams(GH_OutputParamManager pManager)
		{
			pManager.AddBooleanParameter("Success", "S", "Success status for each dimension", GH_ParamAccess.list);
			pManager.AddTextParameter("Messages", "M", "Messages for each dimension", GH_ParamAccess.list);
			pManager.AddCurveParameter("DimensionLines", "DL", "Dimension lines for visualization in Grasshopper", GH_ParamAccess.list);
			pManager.AddPointParameter("Points", "P", "Dimension points (start and end)", GH_ParamAccess.list);
			pManager.AddTextParameter("DimensionTexts", "DT", "Dimension text values", GH_ParamAccess.list);
			pManager.AddGeometryParameter("TextDots", "TD", "Text dots for displaying dimension values in viewport", GH_ParamAccess.list);
		}


		protected override void SolveInstance(IGH_DataAccess DA)
		{
			int port = 19723;
			bool connected = false;
			double offset = 0.0;

			// Get input data
			if (!DA.GetData(0, ref port))
			{
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Port is required");
				return;
			}

			DA.GetData(2, ref connected);

			// Only process if Update button was pressed
			if (!_updateButtonPressed)
			{
				DA.SetDataList(0, new List<bool>());
				DA.SetDataList(1, new List<string> { "Click Update button (right-click on component) to sync hotspots and create dimensions" });
				_updateButtonPressed = false;
				return;
			}

			// Save flag before resetting (needed later in the code)
			bool updateButtonWasPressed = _updateButtonPressed;
			_updateButtonPressed = false; // Reset flag
			
			// Read offset parameter (item parameter - same value for all pairs)
			// Read it here after early returns to ensure it's available in the loop
			if (!DA.GetData(3, ref offset))
			{
				offset = 0.0; // Default to 0 if not provided
			}

			// Get PointPairs - try multiple methods to extract them
			List<PointPair> pointPairs = new List<PointPair>();
			
			// Method 1: Try as IGH_Goo list (for GH_Goo types like PointPair)
			var gooList = new List<IGH_Goo>();
			if (DA.GetDataList(1, gooList) && gooList.Count > 0)
			{
				foreach (var goo in gooList)
				{
					if (goo == null) continue;
					
					PointPair pp = null;
					
					// Direct cast
					if (goo is PointPair)
					{
						pp = goo as PointPair;
					}
					// Try CastTo
					else
					{
						PointPair casted;
						if (goo.CastTo<PointPair>(out casted))
						{
							pp = casted;
						}
						// Try ScriptVariable if CastTo failed
						else
						{
							try
							{
								var scriptVar = goo.ScriptVariable();
								if (scriptVar is PointPair)
								{
									pp = scriptVar as PointPair;
								}
							}
							catch
							{
								// ScriptVariable may throw, ignore
							}
						}
					}
					
					if (pp != null && pp.IsValid)
					{
						pointPairs.Add(pp);
					}
					else
					{
						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, 
							$"Invalid PointPair from input. Type: {goo.GetType().Name}, Valid: {pp?.IsValid ?? false}");
					}
				}
			}
			
			// Method 2: Fallback - try as generic object list
			if (pointPairs.Count == 0)
			{
				List<object> pointPairsInput = new List<object>();
				if (DA.GetDataList(1, pointPairsInput) && pointPairsInput.Count > 0)
				{
					foreach (var item in pointPairsInput)
					{
						if (item == null) continue;
						
						PointPair pp = null;
						
						// Direct cast
						if (item is PointPair)
						{
							pp = item as PointPair;
						}
						// Try as IGH_Goo
						else if (item is IGH_Goo goo)
						{
							if (goo is PointPair)
							{
								pp = goo as PointPair;
							}
							else
							{
								PointPair casted;
								if (goo.CastTo<PointPair>(out casted))
								{
									pp = casted;
								}
								// Try ScriptVariable if CastTo failed
								else
								{
									try
									{
										var scriptVar = goo.ScriptVariable();
										if (scriptVar is PointPair)
										{
											pp = scriptVar as PointPair;
										}
									}
									catch
									{
										// Ignore
									}
								}
							}
						}
						
						if (pp != null && pp.IsValid)
						{
							pointPairs.Add(pp);
						}
					}
				}
			}
			
			// Check if we have any point pairs to process
			if (pointPairs.Count == 0)
			{
				DA.SetDataList(0, new List<bool>());
				DA.SetDataList(1, new List<string> { "No point pairs to process. Connect Dim_FromLines output to PointPairs input." });
				AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No point pairs found. Make sure Dim_FromLines is connected.");
				return;
			}

			// Check connection status from input
			if (!connected)
			{
				DA.SetDataList(0, new List<bool>());
				DA.SetDataList(1, new List<string> { "Not connected to Archicad. Set Connected input to true." });
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Not connected to Archicad");
				return;
			}

			// Create dimensions for each point pair
			List<bool> successes = new List<bool>();
			List<string> messages = new List<string>();
			
			// Lists for Grasshopper visualization output
			List<Curve> dimensionLines = new List<Curve>();
			List<Point3d> dimensionPoints = new List<Point3d>();
			List<string> dimensionTexts = new List<string>();
			List<GeometryBase> textDots = new List<GeometryBase>();

			// Read offset once for all pairs (it's an item parameter, same value for all)
			// Offset is already read at line 219, but ensure it's available in the loop scope
			double currentOffset = offset;

			foreach (var pair in pointPairs)
			{
				// Check validity
				if (pair == null || !pair.IsValid)
				{
					successes.Add(false);
					messages.Add($"Invalid point pair: IsValid={pair?.IsValid}");
					continue;
				}

				try
				{
					var client = new DimensionGhClient(port)
					{
						Timeout = System.TimeSpan.FromSeconds(10)
					};

					// Step 1: Create or update hotspots for both points
					// Try to restore GUIDs from cache if not in PointPair
					string pointKey1 = GetPointKey(pair.Point1, pair.RhinoPointGuid1);
					string pointKey2 = GetPointKey(pair.Point2, pair.RhinoPointGuid2);
					
					string hotspotGuid1 = pair.ArchicadHotspotGuid1;
					string hotspotGuid2 = pair.ArchicadHotspotGuid2;
					
					// Restore from cache if not in PointPair
					if (string.IsNullOrEmpty(hotspotGuid1) && _hotspotGuidCache.ContainsKey(pointKey1))
					{
						hotspotGuid1 = _hotspotGuidCache[pointKey1];
						pair.ArchicadHotspotGuid1 = hotspotGuid1;
					}
					if (string.IsNullOrEmpty(hotspotGuid2) && _hotspotGuidCache.ContainsKey(pointKey2))
					{
						hotspotGuid2 = _hotspotGuidCache[pointKey2];
						pair.ArchicadHotspotGuid2 = hotspotGuid2;
					}
					
					bool coordinatesChanged = pair.HasCoordinatesChanged();
					bool shouldUpdate = coordinatesChanged || updateButtonWasPressed; // Update if coordinates changed OR button pressed

					// Handle hotspot 1: Create new or update existing
					if (string.IsNullOrEmpty(hotspotGuid1))
					{
						// Create new hotspot - always include rhinoPointGuid
						// Convert coordinates from Rhino units to meters (ArchiCAD units)
						var hotspotPayload1 = new JObject
						{
							["x"] = ConvertToMeters(pair.Point1.X),
							["y"] = ConvertToMeters(pair.Point1.Y)
						};
						// Always include rhinoPointGuid if available - this is the key for finding existing hotspot
						if (pair.RhinoPointGuid1.HasValue)
						{
							hotspotPayload1["rhinoPointGuid"] = pair.RhinoPointGuid1.Value.ToString();
						}

						var hotspotRequest1 = JsonRequest.CreateDimensionGhCommand("CreateHotspot", hotspotPayload1);
						var hotspotResponse1 = client.Send(hotspotRequest1);

						// Check response - try to get hotspotGuid even if Succeeded is false
						// (Archicad may return success=true in addOnCommandResponse even if there's a warning)
						var hotspotResult1 = hotspotResponse1.GetAddOnCommandResponse();
						if (hotspotResult1 != null)
						{
							// Check if command itself succeeded (field "success" in response)
							var successToken = hotspotResult1["success"];
							bool commandSucceeded = (successToken != null && successToken.Type == JTokenType.Boolean && successToken.Value<bool>());
							
							if (commandSucceeded && hotspotResult1["hotspotGuid"] != null)
							{
								hotspotGuid1 = hotspotResult1["hotspotGuid"].ToString();
								pair.ArchicadHotspotGuid1 = hotspotGuid1;
								// Cache the GUID
								_hotspotGuidCache[pointKey1] = hotspotGuid1;
							}
							else
							{
								// Command failed or hotspotGuid missing
								string errorMsg = hotspotResponse1.GetErrorMessage();
								if (string.IsNullOrEmpty(errorMsg) && hotspotResult1["error"] != null)
								{
									var errorObj = hotspotResult1["error"];
									if (errorObj["message"] != null)
									{
										errorMsg = errorObj["message"].ToString();
									}
								}
								messages.Add($"Failed to create hotspot 1: {(string.IsNullOrEmpty(errorMsg) ? "Unknown error" : errorMsg)}");
							}
						}
						else
						{
							// No response at all
							string errorMsg = hotspotResponse1.GetErrorMessage();
							messages.Add($"Failed to create hotspot 1: {(string.IsNullOrEmpty(errorMsg) ? "No response from Archicad" : errorMsg)}");
						}
					}
					else if (shouldUpdate)
					{
						// Update existing hotspot if coordinates changed or button pressed
						// Convert coordinates from Rhino units to meters (ArchiCAD units)
						var updatePayload1 = new JObject
						{
							["hotspotGuid"] = hotspotGuid1,
							["x"] = ConvertToMeters(pair.Point1.X),
							["y"] = ConvertToMeters(pair.Point1.Y)
						};

						var updateRequest1 = JsonRequest.CreateDimensionGhCommand("UpdateHotspot", updatePayload1);
						var updateResponse1 = client.Send(updateRequest1);

						// Check if update succeeded by checking the response
						var updateResult1 = updateResponse1.GetAddOnCommandResponse();
						bool updateSucceeded = false;
						if (updateResult1 != null)
						{
							var successToken = updateResult1["success"];
							updateSucceeded = (successToken != null && successToken.Type == JTokenType.Boolean && successToken.Value<bool>());
						}

						if (!updateSucceeded)
						{
							// If update failed, try to create new hotspot
								hotspotGuid1 = null;
								pair.ArchicadHotspotGuid1 = null;
								// Convert coordinates from Rhino units to meters (ArchiCAD units)
								var hotspotPayload1 = new JObject
								{
									["x"] = ConvertToMeters(pair.Point1.X),
									["y"] = ConvertToMeters(pair.Point1.Y)
								};
							if (pair.RhinoPointGuid1.HasValue)
							{
								hotspotPayload1["rhinoPointGuid"] = pair.RhinoPointGuid1.Value.ToString();
							}
							var hotspotRequest1 = JsonRequest.CreateDimensionGhCommand("CreateHotspot", hotspotPayload1);
							var hotspotResponse1 = client.Send(hotspotRequest1);
							
							var hotspotResult1 = hotspotResponse1.GetAddOnCommandResponse();
							if (hotspotResult1 != null)
							{
								var successToken = hotspotResult1["success"];
								bool createSucceeded = (successToken != null && successToken.Type == JTokenType.Boolean && successToken.Value<bool>());
								if (createSucceeded && hotspotResult1["hotspotGuid"] != null)
								{
									hotspotGuid1 = hotspotResult1["hotspotGuid"].ToString();
									pair.ArchicadHotspotGuid1 = hotspotGuid1;
									// Cache the GUID
									_hotspotGuidCache[pointKey1] = hotspotGuid1;
								}
							}
						}
					}
					// If coordinates didn't change and hotspot exists, keep it (no update needed)

					// Handle hotspot 2: Create new or update existing
					if (string.IsNullOrEmpty(hotspotGuid2))
					{
						// Create new hotspot - always include rhinoPointGuid
						// Convert coordinates from Rhino units to meters (ArchiCAD units)
						var hotspotPayload2 = new JObject
						{
							["x"] = ConvertToMeters(pair.Point2.X),
							["y"] = ConvertToMeters(pair.Point2.Y)
						};
						// Always include rhinoPointGuid if available - this is the key for finding existing hotspot
						if (pair.RhinoPointGuid2.HasValue)
						{
							hotspotPayload2["rhinoPointGuid"] = pair.RhinoPointGuid2.Value.ToString();
						}

						var hotspotRequest2 = JsonRequest.CreateDimensionGhCommand("CreateHotspot", hotspotPayload2);
						var hotspotResponse2 = client.Send(hotspotRequest2);

						// Check response - try to get hotspotGuid even if Succeeded is false
						// (Archicad may return success=true in addOnCommandResponse even if there's a warning)
						var hotspotResult2 = hotspotResponse2.GetAddOnCommandResponse();
						if (hotspotResult2 != null)
						{
							// Check if command itself succeeded (field "success" in response)
							var successToken = hotspotResult2["success"];
							bool commandSucceeded = (successToken != null && successToken.Type == JTokenType.Boolean && successToken.Value<bool>());
							
							if (commandSucceeded && hotspotResult2["hotspotGuid"] != null)
							{
								hotspotGuid2 = hotspotResult2["hotspotGuid"].ToString();
								pair.ArchicadHotspotGuid2 = hotspotGuid2;
								// Cache the GUID
								_hotspotGuidCache[pointKey2] = hotspotGuid2;
							}
							else
							{
								// Command failed or hotspotGuid missing
								string errorMsg = hotspotResponse2.GetErrorMessage();
								if (string.IsNullOrEmpty(errorMsg) && hotspotResult2["error"] != null)
								{
									var errorObj = hotspotResult2["error"];
									if (errorObj["message"] != null)
									{
										errorMsg = errorObj["message"].ToString();
									}
								}
								messages.Add($"Failed to create hotspot 2: {(string.IsNullOrEmpty(errorMsg) ? "Unknown error" : errorMsg)}");
							}
						}
						else
						{
							// No response at all
							string errorMsg = hotspotResponse2.GetErrorMessage();
							messages.Add($"Failed to create hotspot 2: {(string.IsNullOrEmpty(errorMsg) ? "No response from Archicad" : errorMsg)}");
						}
					}
					else if (shouldUpdate)
					{
						// Update existing hotspot if coordinates changed or button pressed
						// Convert coordinates from Rhino units to meters (ArchiCAD units)
						var updatePayload2 = new JObject
						{
							["hotspotGuid"] = hotspotGuid2,
							["x"] = ConvertToMeters(pair.Point2.X),
							["y"] = ConvertToMeters(pair.Point2.Y)
						};

						var updateRequest2 = JsonRequest.CreateDimensionGhCommand("UpdateHotspot", updatePayload2);
						var updateResponse2 = client.Send(updateRequest2);

						// Check if update succeeded by checking the response
						var updateResult2 = updateResponse2.GetAddOnCommandResponse();
						bool updateSucceeded = false;
						if (updateResult2 != null)
						{
							var successToken = updateResult2["success"];
							updateSucceeded = (successToken != null && successToken.Type == JTokenType.Boolean && successToken.Value<bool>());
						}

						if (!updateSucceeded)
						{
							// If update failed, try to create new hotspot
								hotspotGuid2 = null;
								pair.ArchicadHotspotGuid2 = null;
								// Convert coordinates from Rhino units to meters (ArchiCAD units)
								var hotspotPayload2 = new JObject
								{
									["x"] = ConvertToMeters(pair.Point2.X),
									["y"] = ConvertToMeters(pair.Point2.Y)
								};
							if (pair.RhinoPointGuid2.HasValue)
							{
								hotspotPayload2["rhinoPointGuid"] = pair.RhinoPointGuid2.Value.ToString();
							}
							var hotspotRequest2 = JsonRequest.CreateDimensionGhCommand("CreateHotspot", hotspotPayload2);
							var hotspotResponse2 = client.Send(hotspotRequest2);
							
							var hotspotResult2 = hotspotResponse2.GetAddOnCommandResponse();
							if (hotspotResult2 != null)
							{
								var successToken = hotspotResult2["success"];
								bool createSucceeded = (successToken != null && successToken.Type == JTokenType.Boolean && successToken.Value<bool>());
								if (createSucceeded && hotspotResult2["hotspotGuid"] != null)
								{
									hotspotGuid2 = hotspotResult2["hotspotGuid"].ToString();
									pair.ArchicadHotspotGuid2 = hotspotGuid2;
									// Cache the GUID
									_hotspotGuidCache[pointKey2] = hotspotGuid2;
								}
							}
						}
					}
					// If coordinates didn't change and hotspot exists, keep it (no update needed)
					
					// Update stored coordinates after processing (if coordinates changed or button pressed)
					if (shouldUpdate)
					{
						pair.UpdatePreviousCoordinates();
						// Update cache keys if coordinates changed (remove old keys, new keys will be created on next solve)
						// Note: We keep the GUID in cache with new coordinates, so it will be found on next solve
						if (coordinatesChanged)
						{
							// Update cache with new coordinates
							string newPointKey1 = GetPointKey(pair.Point1, pair.RhinoPointGuid1);
							string newPointKey2 = GetPointKey(pair.Point2, pair.RhinoPointGuid2);
							if (!string.IsNullOrEmpty(hotspotGuid1) && newPointKey1 != pointKey1)
							{
								_hotspotGuidCache.Remove(pointKey1);
								_hotspotGuidCache[newPointKey1] = hotspotGuid1;
							}
							if (!string.IsNullOrEmpty(hotspotGuid2) && newPointKey2 != pointKey2)
							{
								_hotspotGuidCache.Remove(pointKey2);
								_hotspotGuidCache[newPointKey2] = hotspotGuid2;
							}
						}
					}

					// Step 2: Create dimension using hotspots (always create, dimensions attached to hotspots will update automatically)
					bool dimensionCreated = false;
					string dimensionMessage = "";
					
					// Always create dimension if hotspots are available
					if (!string.IsNullOrEmpty(hotspotGuid1) && !string.IsNullOrEmpty(hotspotGuid2))
					{
						// Create payload with two points and hotspot GUIDs
						// Convert coordinates from Rhino units to meters (ArchiCAD units)
						var payload = new JObject
						{
							["point1"] = new JObject
							{
								["x"] = ConvertToMeters(pair.Point1.X),
								["y"] = ConvertToMeters(pair.Point1.Y),
								["z"] = ConvertToMeters(pair.Point1.Z)
							},
							["point2"] = new JObject
							{
								["x"] = ConvertToMeters(pair.Point2.X),
								["y"] = ConvertToMeters(pair.Point2.Y),
								["z"] = ConvertToMeters(pair.Point2.Z)
							},
							["offset"] = ConvertToMeters(currentOffset)
						};
						
						// Add hotspot GUIDs if available (preferred method - dimensions will be attached to hotspots)
						if (!string.IsNullOrEmpty(hotspotGuid1))
						{
							payload["hotspotGuid1"] = hotspotGuid1;
						}
						
						if (!string.IsNullOrEmpty(hotspotGuid2))
						{
							payload["hotspotGuid2"] = hotspotGuid2;
						}
						
						// Legacy: Add element GUIDs if provided (fallback)
						if (!string.IsNullOrEmpty(pair.ElementGuid1))
						{
							payload["elementGuid1"] = pair.ElementGuid1;
						}
						
						if (!string.IsNullOrEmpty(pair.ElementGuid2))
						{
							payload["elementGuid2"] = pair.ElementGuid2;
						}

						// Create request for CreateLinearDimension command
						var request = JsonRequest.CreateDimensionGhCommand("CreateLinearDimension", payload);

						// Send request
						var response = client.Send(request);

						// Check response - need to check both direct error and addOnCommandResponse
						bool requestSucceeded = response.Succeeded;
						
						// Try to get result from addOnCommandResponse
						var addOnResponse = response.GetAddOnCommandResponse();
						if (addOnResponse != null)
						{
							var successToken = addOnResponse["success"];
							if (successToken != null && successToken.Type == JTokenType.Boolean)
							{
								requestSucceeded = successToken.Value<bool>();
								if (requestSucceeded)
								{
									var distanceToken = addOnResponse["distance"];
									if (distanceToken != null)
									{
										dimensionMessage = $"Dimension created successfully (distance: {distanceToken.Value<double>():F2})";
									}
									else
									{
										dimensionMessage = "Dimension created successfully";
									}
								}
								else
								{
									// Check for error in response
									var errorObj = addOnResponse["error"];
									if (errorObj != null && errorObj["message"] != null)
									{
										dimensionMessage = errorObj["message"].ToString();
									}
									else
									{
										dimensionMessage = "Failed to create dimension (unknown error)";
									}
								}
							}
						}
						
						if (requestSucceeded && string.IsNullOrEmpty(dimensionMessage))
						{
							dimensionMessage = "Dimension created successfully";
						}
						
						if (!requestSucceeded && string.IsNullOrEmpty(dimensionMessage))
						{
							dimensionMessage = response.GetErrorMessage();
						}
						
						dimensionCreated = requestSucceeded;
					}
					else
					{
						// Hotspots not available - cannot create dimension
						dimensionCreated = false;
						string hotspotStatus = "";
						if (string.IsNullOrEmpty(hotspotGuid1) && string.IsNullOrEmpty(hotspotGuid2))
						{
							hotspotStatus = "Both hotspots failed to create";
						}
						else if (string.IsNullOrEmpty(hotspotGuid1))
						{
							hotspotStatus = "Hotspot 1 failed to create";
						}
						else if (string.IsNullOrEmpty(hotspotGuid2))
						{
							hotspotStatus = "Hotspot 2 failed to create";
						}
						dimensionMessage = $"Failed to create/update hotspots ({hotspotStatus}) - cannot create dimension. Check Archicad for errors.";
					}

					successes.Add(dimensionCreated);
					messages.Add(dimensionMessage ?? "Unknown status");
					
					// Create visualization geometry for Grasshopper (only if dimension was created)
					if (dimensionCreated)
					{
						CreateGrasshopperDimensionGeometry(pair.Point1, pair.Point2, dimensionLines, dimensionPoints, dimensionTexts, textDots);
					}
					
					if (!dimensionCreated)
					{
						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Failed to create dimension: {dimensionMessage}");
					}
				}
				catch (System.Net.Http.HttpRequestException ex)
				{
					successes.Add(false);
					messages.Add($"Connection error: {ex.Message}");
					AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Connection failed: {ex.Message}");
				}
				catch (Exception ex)
				{
					successes.Add(false);
					messages.Add($"Error: {ex.Message}");
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
				}
			}

			// Always output results
			if (successes.Count == 0 && pointPairs.Count > 0)
			{
				// This shouldn't happen, but add debug info
				AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Processed {pointPairs.Count} point pairs but got {successes.Count} results. Check for errors above.");
			}
			
			DA.SetDataList(0, successes);
			DA.SetDataList(1, messages);
			DA.SetDataList(2, dimensionLines);
			DA.SetDataList(3, dimensionPoints);
			DA.SetDataList(4, dimensionTexts);
			DA.SetDataList(5, textDots);
			
			// Debug output
			if (pointPairs.Count > 0)
			{
				AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"Processed {pointPairs.Count} point pairs. Success: {successes.Count(s => s)}, Failed: {successes.Count(s => !s)}");
			}
		}

		/// <summary>
		/// Create dimension geometry for visualization in Grasshopper
		/// </summary>
		private void CreateGrasshopperDimensionGeometry(Point3d pt1, Point3d pt2, 
			List<Curve> dimensionLines, List<Point3d> dimensionPoints, List<string> dimensionTexts, List<GeometryBase> textDots)
		{
			// Calculate distance between points
			double distance = pt1.DistanceTo(pt2);

			// Create dimension line between the points
			Line dimensionLine = new Line(pt1, pt2);
			dimensionLines.Add(new LineCurve(dimensionLine));

			// Add points to output
			dimensionPoints.Add(pt1);
			dimensionPoints.Add(pt2);

			// Calculate midpoint and direction for text placement
			Point3d midPoint = (pt1 + pt2) / 2.0;
			Vector3d direction = pt2 - pt1;
			if (direction.Length > 1e-6)
			{
				direction.Unitize();
			}
			else
			{
				direction = Vector3d.XAxis; // Default direction if points are too close
			}
			
			// Perpendicular vector for offset
			Vector3d offset = new Vector3d(-direction.Y, direction.X, 0);
			offset *= (distance * 0.1); // 10% of distance as offset
			
			Point3d textPoint = midPoint + offset;
			dimensionPoints.Add(textPoint); // Add text point for visualization
			
			// Format distance text
			// distance is already in Rhino units (mm if Rhino is set to millimeters)
			// We just need to format it correctly without any conversion
			var doc = Rhino.RhinoDoc.ActiveDoc;
			var unitSystem = doc != null ? doc.ModelUnitSystem : Rhino.UnitSystem.Millimeters;
			string unitName = "units";
			double displayDistance = distance; // Use distance as-is, it's already in correct units
			
			if (unitSystem == Rhino.UnitSystem.Meters)
			{
				unitName = "m";
				displayDistance = distance; // distance is already in meters
			}
			else if (unitSystem == Rhino.UnitSystem.Millimeters)
			{
				unitName = "mm";
				displayDistance = distance; // distance is already in millimeters, no conversion needed
			}
			else if (unitSystem == Rhino.UnitSystem.Centimeters)
			{
				unitName = "cm";
				displayDistance = distance; // distance is already in centimeters
			}
			
			string distanceText = unitSystem == Rhino.UnitSystem.Millimeters 
				? $"{displayDistance:F0} {unitName}" 
				: $"{displayDistance:F2} {unitName}";
			
			dimensionTexts.Add(distanceText);
			
			// Create TextDot for displaying text in viewport
			// TextDot displays text at a point location in the viewport
			TextDot textDot = new TextDot(distanceText, textPoint);
			textDots.Add(textDot);

			// Create extension lines for visualization
			Vector3d perp = new Vector3d(-direction.Y, direction.X, 0);
			if (perp.Length > 1e-6)
			{
				perp.Unitize();
			}
			perp *= (distance * 0.05);

			// Extension line at point1
			Line extLine1 = new Line(pt1 - perp, pt1 + perp);
			dimensionLines.Add(new LineCurve(extLine1));

			// Extension line at point2
			Line extLine2 = new Line(pt2 - perp, pt2 + perp);
			dimensionLines.Add(new LineCurve(extLine2));
		}

		protected override Bitmap Icon
		{
			get
			{
				return LoadIconFromResources("ac24.PNG");
			}
		}

		/// <summary>
		/// Load icon from embedded resources
		/// </summary>
		private Bitmap LoadIconFromResources(string resourceName)
		{
			try
			{
				var assembly = System.Reflection.Assembly.GetExecutingAssembly();
				var fullResourceName = $"DimensionGhGh.Resources.{resourceName}";
				
				using (var stream = assembly.GetManifestResourceStream(fullResourceName))
				{
					if (stream != null)
					{
						return new Bitmap(stream);
					}
				}
			}
			catch
			{
				// Fallback to default icon if loading fails
			}
			
			// Return a simple default icon if resource not found
			var bitmap = new Bitmap(24, 24);
			using (var g = Graphics.FromImage(bitmap))
			{
				g.Clear(Color.Transparent);
			}
			return bitmap;
		}

		public override Guid ComponentGuid => new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901");
	}
}

