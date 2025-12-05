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
				"Dimension Gh", "Connection")
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

			if (!DA.GetData(0, ref port)) return;
			DA.GetData(1, ref ping);

			if (!ping)
			{
				DA.SetData(0, false);
				DA.SetData(1, "Idle - set Ping to true to test connection");
				return;
			}

			try
			{
				var client = new DimensionGhClient(port);
				var request = JsonRequest.CreateDimensionGhCommand("Ping");
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
					DA.SetData(1, response.GetErrorMessage());
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, response.GetErrorMessage());
				}
			}
			catch (Exception ex)
			{
				DA.SetData(0, false);
				DA.SetData(1, $"Error: {ex.Message}");
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
			}
		}

		protected override Bitmap Icon => null; // TODO: Add icon

		public override Guid ComponentGuid => new Guid("D1M2N3S4-5678-90AB-CDEF-1234567890AB");
	}
}

