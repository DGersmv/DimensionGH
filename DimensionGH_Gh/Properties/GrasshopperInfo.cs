using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace DimensionGhGh.Properties
{
	/// <summary>
	/// Assembly info for Grasshopper plugin registration
	/// This class is automatically discovered by Grasshopper to register the plugin
	/// IMPORTANT: This is NOT a Python plugin - it's a C# plugin
	/// </summary>
	public class GrasshopperInfo : GH_AssemblyInfo
	{
		public override string Name => "DimensionGH_Gh";
		public override Bitmap Icon => null; // Icon will be set by category
		public override string Description => "Dimension Gh – Archicad dimensions bridge for Grasshopper (C# Plugin)";
		public override Guid Id => new Guid("0731dcb2-d9d8-40a6-8721-2b0d39ab073f");
		public override string AuthorName => "";
		public override string AuthorContact => "";
		public override string Version => "1.0.0.0";
	}
}

