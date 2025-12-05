using System;
using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using DimensionGhGh.Client;
using DimensionGhGh.Protocol;
using Newtonsoft.Json.Linq;

namespace DimensionGhGh.Components
{
	/// <summary>
	/// Component for testing connection to Archicad Dimension_Gh add-on
	/// </summary>
	public class DimConnectComponent : GH_Component
	{
		public DimConnectComponent()
			: base("Dim_Connect", "DimConnect",
				"Test connection to Archicad Dimension_Gh add-on",
				"Info227", "Connection")
		{
		}

		protected override void RegisterInputParams(GH_InputParamManager pManager)
		{
			pManager.AddIntegerParameter("Port", "P", "HTTP port number (default: 19723)", GH_ParamAccess.item, 19723);
			var pingParam = pManager.AddBooleanParameter("Ping", "Ping", "Send ping to test connection", GH_ParamAccess.item);
			pManager[pingParam].Optional = true;
		}

		protected override void RegisterOutputParams(GH_OutputParamManager pManager)
		{
			pManager.AddBooleanParameter("Connected", "C", "Connection status", GH_ParamAccess.item);
			pManager.AddTextParameter("Message", "M", "Response message", GH_ParamAccess.item);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			int port = 19723;
			bool ping = false;

			// Port has default value, so GetData will always succeed (uses default if not connected)
			DA.GetData(0, ref port);
			DA.GetData(1, ref ping);

			if (!ping)
			{
				DA.SetData(0, false);
				DA.SetData(1, "Idle - set Ping to true to test connection");
				return;
			}

			try
			{
				// Create client with timeout to prevent hanging
				var client = new DimensionGhClient(port)
				{
					Timeout = System.TimeSpan.FromSeconds(10) // 10 second timeout
				};

				var request = JsonRequest.CreateDimensionGhCommand("Ping");
				
				// Send request with timeout handling
				var response = client.Send(request);

				if (response.Succeeded)
				{
					var commandResponse = response.GetAddOnCommandResponse();
					if (commandResponse != null && commandResponse["message"] != null)
					{
						DA.SetData(0, true);
						DA.SetData(1, commandResponse["message"].ToString());
					}
					else
					{
						DA.SetData(0, false);
						DA.SetData(1, "Unexpected response format");
					}
				}
				else
				{
					DA.SetData(0, false);
					var errorMsg = response.GetErrorMessage();
					DA.SetData(1, errorMsg);
					
					// Only show error if it's not a timeout (timeout is expected if Archicad is not running)
					var errorMsgLower = errorMsg.ToLowerInvariant();
					if (!errorMsgLower.Contains("timeout") && 
					    !errorMsgLower.Contains("connection refused") &&
					    !errorMsgLower.Contains("no connection"))
					{
						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, errorMsg);
					}
				}
			}
			catch (System.Net.Http.HttpRequestException ex)
			{
				// Connection errors - Archicad might not be running
				DA.SetData(0, false);
				DA.SetData(1, $"Cannot connect to Archicad on port {port}. Make sure Archicad is running with Dimension_Gh add-on loaded.");
				AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, 
					$"Connection failed: {ex.Message}. Check if Archicad is running.");
			}
			catch (Exception ex)
			{
				DA.SetData(0, false);
				DA.SetData(1, $"Error: {ex.Message}");
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
			}
		}

		protected override Bitmap Icon
		{
			get
			{
				return LoadComponentIcon();
			}
		}

		/// <summary>
		/// Load component icon from resources
		/// </summary>
		private Bitmap LoadComponentIcon()
		{
			try
			{
				var assembly = System.Reflection.Assembly.GetExecutingAssembly();
				var resourceName = "DimensionGhGh.Resources.Info227Icon.svg";
				
				using (var stream = assembly.GetManifestResourceStream(resourceName))
				{
					if (stream == null)
					{
						// Try to load from file if not embedded
						var iconPath = System.IO.Path.Combine(
							System.IO.Path.GetDirectoryName(assembly.Location), 
							"Resources", "Info227Icon.svg");
						if (System.IO.File.Exists(iconPath))
						{
							return CreateIconFromSvg(System.IO.File.ReadAllText(iconPath));
						}
						return CreateDefaultIcon();
					}
					
					using (var reader = new System.IO.StreamReader(stream))
					{
						var svgContent = reader.ReadToEnd();
						return CreateIconFromSvg(svgContent);
					}
				}
			}
			catch
			{
				return CreateDefaultIcon();
			}
		}

		/// <summary>
		/// Create icon from SVG content
		/// </summary>
		private Bitmap CreateIconFromSvg(string svgContent)
		{
			// Create a bitmap representation based on the SVG design
			var bitmap = new Bitmap(24, 24);
			using (var g = Graphics.FromImage(bitmap))
			{
				g.Clear(Color.Transparent);
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				
				// Draw dimension lines representation based on SVG
				using (var pen = new Pen(Color.Black, 2))
				{
					// Main horizontal line
					g.DrawLine(pen, 2, 12, 22, 12);
					
					// Left and right markers
					g.DrawLine(pen, 2, 8, 2, 16);
					g.DrawLine(pen, 22, 8, 22, 16);
					
					// Arrow heads
					g.DrawLine(pen, 2, 8, 4, 12);
					g.DrawLine(pen, 2, 16, 4, 12);
					g.DrawLine(pen, 22, 8, 20, 12);
					g.DrawLine(pen, 22, 16, 20, 12);
					
					// Extension lines (vertical lines at top and bottom)
					g.DrawLine(pen, 1, 4, 1, 8);
					g.DrawLine(pen, 23, 4, 23, 8);
					g.DrawLine(pen, 1, 16, 1, 20);
					g.DrawLine(pen, 23, 16, 23, 20);
				}
			}
			return bitmap;
		}

		/// <summary>
		/// Create a default icon if SVG loading fails
		/// </summary>
		private Bitmap CreateDefaultIcon()
		{
			var bitmap = new Bitmap(24, 24);
			using (var g = Graphics.FromImage(bitmap))
			{
				g.Clear(Color.Transparent);
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				
				using (var pen = new Pen(Color.Black, 2))
				{
					g.DrawLine(pen, 2, 12, 22, 12);
					g.DrawLine(pen, 2, 8, 2, 16);
					g.DrawLine(pen, 22, 8, 22, 16);
				}
			}
			return bitmap;
		}

		public override Guid ComponentGuid => new Guid("3165ca30-9747-4a2e-b551-4d26af1a2dc6");
	}
}

