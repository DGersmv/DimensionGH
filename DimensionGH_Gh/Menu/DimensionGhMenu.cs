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
		/// Load category icon from embedded resources (16x16 for menu)
		/// </summary>
		private Bitmap LoadCategoryIcon()
		{
			try
			{
				var assembly = Assembly.GetExecutingAssembly();
				var resourceName = "DimensionGhGh.Resources.ac16.png";
				
				using (var stream = assembly.GetManifestResourceStream(resourceName))
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
			var bitmap = new Bitmap(16, 16);
			using (var g = Graphics.FromImage(bitmap))
			{
				g.Clear(Color.Transparent);
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				
				using (var pen = new Pen(Color.Black, 1))
				{
					g.DrawLine(pen, 2, 8, 14, 8);
					g.DrawLine(pen, 2, 6, 2, 10);
					g.DrawLine(pen, 14, 6, 14, 10);
				}
			}
			return bitmap;
		}
	}
}

