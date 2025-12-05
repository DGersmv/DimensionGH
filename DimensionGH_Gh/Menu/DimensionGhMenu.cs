using Grasshopper.Kernel;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;

namespace DimensionGhGh.Menu
{
	/// <summary>
	/// Custom menu registration for DimensionGh plugin
	/// </summary>
	public class DimensionGhMenu : GH_AssemblyPriority
	{
		public override GH_LoadingInstruction PriorityLoad()
		{
			try
			{
				// Register custom category "Info227" with symbol 'I'
				if (Grasshopper.Instances.ComponentServer != null)
				{
					Grasshopper.Instances.ComponentServer.AddCategorySymbolName("Info227", 'I');
					
					// Try to set category icon
					var icon = LoadCategoryIcon();
					if (icon != null)
					{
						try
						{
							// Try to set category icon using reflection (method may not exist in all versions)
							var componentServer = Grasshopper.Instances.ComponentServer;
							var method = componentServer.GetType().GetMethod("AddCategoryIcon", 
								System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
							if (method != null)
							{
								method.Invoke(componentServer, new object[] { "Info227", icon });
							}
						}
						catch
						{
							// Icon will be set from component instead - this is normal
						}
					}
				}
			}
			catch (Exception ex)
			{
				// Log error but continue - category will be created when components are registered
				System.Diagnostics.Debug.WriteLine($"Error registering category: {ex.Message}");
			}
			
			return GH_LoadingInstruction.Proceed;
		}

		/// <summary>
		/// Load category icon from embedded resources
		/// </summary>
		private Bitmap LoadCategoryIcon()
		{
			try
			{
				var assembly = Assembly.GetExecutingAssembly();
				var resourceName = "DimensionGhGh.Resources.Info227Icon.svg";
				
				using (var stream = assembly.GetManifestResourceStream(resourceName))
				{
					if (stream == null)
					{
						// Try to load from file if not embedded
						var iconPath = Path.Combine(Path.GetDirectoryName(assembly.Location), "Resources", "Info227Icon.svg");
						if (File.Exists(iconPath))
						{
							return CreateIconFromSvg(File.ReadAllText(iconPath));
						}
						return CreateDefaultIcon();
					}
					
					using (var reader = new StreamReader(stream))
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
		/// Create icon from SVG content (simple implementation)
		/// For production, consider using a proper SVG library
		/// </summary>
		private Bitmap CreateIconFromSvg(string svgContent)
		{
			// Create a simple bitmap representation
			// For a proper implementation, use a library like Svg.NET
			// For now, create a simple icon based on the SVG design
			var bitmap = new Bitmap(24, 24);
			using (var g = Graphics.FromImage(bitmap))
			{
				g.Clear(Color.Transparent);
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				
				// Draw a simple representation based on the SVG (dimension lines)
				using (var pen = new Pen(Color.Black, 2))
				{
					// Draw dimension arrow/line representation
					g.DrawLine(pen, 2, 12, 22, 12); // Main line
					g.DrawLine(pen, 2, 8, 2, 16);   // Left marker
					g.DrawLine(pen, 22, 8, 22, 16); // Right marker
					
					// Draw arrow heads
					g.DrawLine(pen, 2, 8, 4, 12);
					g.DrawLine(pen, 2, 16, 4, 12);
					g.DrawLine(pen, 22, 8, 20, 12);
					g.DrawLine(pen, 22, 16, 20, 12);
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
	}
}

