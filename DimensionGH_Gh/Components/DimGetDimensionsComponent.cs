using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using DimensionGhGh.Client;
using DimensionGhGh.Protocol;
using Newtonsoft.Json.Linq;

namespace DimensionGhGh.Components
{
	/// <summary>
	/// Component for getting dimensions from Archicad
	/// </summary>
	public class DimGetDimensionsComponent : GH_Component
	{
		public DimGetDimensionsComponent()
			: base("Dim_GetDimensions", "GetDims",
				"Get dimensions from Archicad Dimension_Gh add-on",
				"Info227", "Dimensions")
		{
		}

		protected override void RegisterInputParams(GH_InputParamManager pManager)
		{
			pManager.AddIntegerParameter("Port", "P", "HTTP port number (default: 19723)", GH_ParamAccess.item, 19723);
			pManager.AddTextParameter("FilterLayer", "L", "Filter by layer name (empty = all layers)", GH_ParamAccess.item);
			pManager[pManager.ParamCount - 1].Optional = true;
			pManager.AddBooleanParameter("Run", "R", "Run to fetch dimensions", GH_ParamAccess.item, false);
		}

		protected override void RegisterOutputParams(GH_OutputParamManager pManager)
		{
			pManager.AddCurveParameter("Curves", "C", "Dimension curves", GH_ParamAccess.list);
			pManager.AddTextParameter("Texts", "T", "Dimension text values", GH_ParamAccess.list);
			pManager.AddTextParameter("Layers", "L", "Dimension layers", GH_ParamAccess.list);
			pManager.AddTextParameter("Guids", "G", "Dimension GUIDs", GH_ParamAccess.list);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			int port = 19723;
			string filterLayer = "";
			bool run = false;

			// Port has default value, so GetData will always succeed (uses default if not connected)
			DA.GetData(0, ref port);
			DA.GetData(1, ref filterLayer);
			DA.GetData(2, ref run);

			if (!run)
			{
				// Return empty lists when not running
				DA.SetDataList(0, new List<Curve>());
				DA.SetDataList(1, new List<string>());
				DA.SetDataList(2, new List<string>());
				DA.SetDataList(3, new List<string>());
				return;
			}

			try
			{
				var client = new DimensionGhClient(port)
				{
					Timeout = System.TimeSpan.FromSeconds(10)
				};

				// Build request payload
				var payload = new JObject();
				if (!string.IsNullOrEmpty(filterLayer))
				{
					payload["filterLayer"] = filterLayer;
				}

				var request = JsonRequest.CreateDimensionGhCommand("GetDimensions", payload);
				var response = client.Send(request);

				if (!response.Succeeded)
				{
					var errorMsg = response.GetErrorMessage();
					AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, errorMsg);
					
					// Return empty lists on error
					DA.SetDataList(0, new List<Curve>());
					DA.SetDataList(1, new List<string>());
					DA.SetDataList(2, new List<string>());
					DA.SetDataList(3, new List<string>());
					return;
				}

				var commandResponse = response.GetAddOnCommandResponse();
				if (commandResponse == null)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unexpected response format");
					DA.SetDataList(0, new List<Curve>());
					DA.SetDataList(1, new List<string>());
					DA.SetDataList(2, new List<string>());
					DA.SetDataList(3, new List<string>());
					return;
				}

				// Parse dimensions from response
				var dimensions = commandResponse["dimensions"] as JArray;
				if (dimensions == null || dimensions.Count == 0)
				{
					// No dimensions found - return empty lists
					DA.SetDataList(0, new List<Curve>());
					DA.SetDataList(1, new List<string>());
					DA.SetDataList(2, new List<string>());
					DA.SetDataList(3, new List<string>());
					return;
				}

				var curves = new List<Curve>();
				var texts = new List<string>();
				var layers = new List<string>();
				var guids = new List<string>();

				foreach (var dimToken in dimensions)
				{
					var dim = dimToken as JObject;
					if (dim == null) continue;

					// Extract dimension data
					var guid = dim["guid"]?.ToString() ?? "";
					var type = dim["type"]?.ToString() ?? "linear";
					var layer = dim["layer"]?.ToString() ?? "";
					var text = dim["text"]?.ToString() ?? "";
					var pointsToken = dim["points"] as JArray;

					// Convert points to Rhino Point3d
					var points = new List<Point3d>();
					if (pointsToken != null)
					{
						foreach (var pointToken in pointsToken)
						{
							var pointObj = pointToken as JObject;
							if (pointObj != null)
							{
								var x = pointObj["x"]?.ToObject<double>() ?? 0.0;
								var y = pointObj["y"]?.ToObject<double>() ?? 0.0;
								var z = pointObj["z"]?.ToObject<double>() ?? 0.0;
								points.Add(new Point3d(x, y, z));
							}
						}
					}

					// Create curve from points
					Curve curve = null;
					if (points.Count >= 2)
					{
						if (points.Count == 2)
						{
							// Simple line
							curve = new LineCurve(points[0], points[1]);
						}
						else
						{
							// Polyline for multiple points
							curve = new PolylineCurve(points);
						}
					}

					if (curve != null)
					{
						curves.Add(curve);
						texts.Add(text);
						layers.Add(layer);
						guids.Add(guid);
					}
				}

				DA.SetDataList(0, curves);
				DA.SetDataList(1, texts);
				DA.SetDataList(2, layers);
				DA.SetDataList(3, guids);
			}
			catch (System.Net.Http.HttpRequestException ex)
			{
				AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
					$"Cannot connect to Archicad on port {port}. Make sure Archicad is running with Dimension_Gh add-on loaded.");
				DA.SetDataList(0, new List<Curve>());
				DA.SetDataList(1, new List<string>());
				DA.SetDataList(2, new List<string>());
				DA.SetDataList(3, new List<string>());
			}
			catch (Exception ex)
			{
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
				DA.SetDataList(0, new List<Curve>());
				DA.SetDataList(1, new List<string>());
				DA.SetDataList(2, new List<string>());
				DA.SetDataList(3, new List<string>());
			}
		}

		protected override Bitmap Icon
		{
			get
			{
				// Simple icon: arrow from Archicad to Grasshopper
				var bitmap = new Bitmap(24, 24);
				using (var g = Graphics.FromImage(bitmap))
				{
					g.Clear(Color.Transparent);
					g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
					
					using (var pen = new Pen(Color.Black, 2))
					{
						// Arrow from left to right
						g.DrawLine(pen, 2, 12, 18, 12);
						g.DrawLine(pen, 18, 12, 14, 8);
						g.DrawLine(pen, 18, 12, 14, 16);
						
						// AC box on left
						g.DrawRectangle(pen, 2, 8, 6, 8);
						
						// GH box on right (smaller)
						g.DrawRectangle(pen, 16, 10, 4, 4);
					}
				}
				return bitmap;
			}
		}

		public override Guid ComponentGuid => new Guid("1435ca0a-840c-46df-8ff8-fa4aeaef41d9");
	}
}

