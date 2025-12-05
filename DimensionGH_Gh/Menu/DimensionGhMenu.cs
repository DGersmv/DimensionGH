using Grasshopper.Kernel;
using System;

namespace DimensionGhGh.Menu
{
	/// <summary>
	/// Custom menu registration for DimensionGh plugin
	/// </summary>
	public class DimensionGhMenu : GH_AssemblyPriority
	{
		public override GH_LoadingInstruction PriorityLoad()
		{
			// Register custom category "227info"
			Grasshopper.Instances.ComponentServer.AddCategorySymbolName("227info", 'D');
			
			return GH_LoadingInstruction.Proceed;
		}
	}
}

