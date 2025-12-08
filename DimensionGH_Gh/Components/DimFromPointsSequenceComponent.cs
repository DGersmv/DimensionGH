using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using DimensionGhGh.Protocol;
using Rhino.Geometry;

namespace DimensionGhGh.Components
{
	/// <summary>
	/// Component for creating sequential point pairs from a list of points
	/// Creates pairs: (point[0], point[1]), (point[1], point[2]), (point[2], point[3]), etc.
	/// Returns list of PointPair for use with Dim_CreateDimension
	/// </summary>
	public class DimFromPointsSequenceComponent : GH_Component
	{
		// Static dictionary to store stable GUIDs for points
		// Key: component instance GUID + input index + point index
		// Value: stable GUID for this point
		private static System.Collections.Generic.Dictionary<string, Guid> _pointGuidCache = 
			new System.Collections.Generic.Dictionary<string, Guid>();
		
		public DimFromPointsSequenceComponent()
			: base("Dim_FromPointsSequence", "DimSeq",
				"Create sequential point pairs from a list of points. Pairs: (P0,P1), (P1,P2), (P2,P3), etc.",
				"Info227", "Input")
		{
		}
		
		// Get or create stable GUID for a point
		// Uses component instance GUID + input index + point index to ensure stability
		private Guid GetStablePointGuid(int inputIndex, int pointIndex)
		{
			string componentInstanceId = this.InstanceGuid.ToString();
			string key = $"{componentInstanceId}_{inputIndex}_{pointIndex}";
			
			if (!_pointGuidCache.ContainsKey(key))
			{
				_pointGuidCache[key] = Guid.NewGuid();
			}
			
			return _pointGuidCache[key];
		}

		protected override void RegisterInputParams(GH_InputParamManager pManager)
		{
			pManager.AddPointParameter("Points", "P", "List of points to create sequential pairs from", GH_ParamAccess.list);
		}

		protected override void RegisterOutputParams(GH_OutputParamManager pManager)
		{
			pManager.AddGenericParameter("PointPair", "PP", "List of sequential point pairs for dimension creation (projected to XY plane)", GH_ParamAccess.list);
			pManager.AddPointParameter("Point1", "P1", "List of first points in pairs (projected to XY plane)", GH_ParamAccess.list);
			pManager.AddPointParameter("Point2", "P2", "List of second points in pairs (projected to XY plane)", GH_ParamAccess.list);
			pManager.AddPointParameter("Points_2D", "P2D", "All projected points on XY plane for visualization", GH_ParamAccess.list);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			List<Point3d> points = new List<Point3d>();

			// Get input data
			if (!DA.GetDataList(0, points))
			{
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Points list is required");
				return;
			}

			// Validate input
			if (points == null || points.Count == 0)
			{
				// Empty list - return empty results (do nothing)
				DA.SetDataList(0, new List<PointPair>());
				DA.SetDataList(1, new List<Point3d>());
				DA.SetDataList(2, new List<Point3d>());
				DA.SetDataList(3, new List<Point3d>());
				return;
			}

			if (points.Count == 1)
			{
				// Only one point - show error
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "At least 2 points are required to create pairs. Only 1 point provided.");
				DA.SetDataList(0, new List<PointPair>());
				DA.SetDataList(1, new List<Point3d>());
				DA.SetDataList(2, new List<Point3d>());
				DA.SetDataList(3, new List<Point3d>());
				return;
			}

			// Step 1: Project all points to XY plane
			var points_2d = new List<Point3d>();
			foreach (Point3d pt in points)
			{
				if (pt.IsValid)
				{
					// Project point to XY plane (set Z = 0)
					Point3d pt_2d = new Point3d(pt.X, pt.Y, 0);
					points_2d.Add(pt_2d);
				}
				else
				{
					points_2d.Add(pt); // Keep invalid point as is
				}
			}

			// Create sequential pairs from projected points
			var pointPairs = new List<PointPair>();
			var points1 = new List<Point3d>();
			var points2 = new List<Point3d>();

			// Create pairs: (point_2d[i], point_2d[i+1]) for i from 0 to count-2
			for (int i = 0; i < points_2d.Count - 1; i++)
			{
				Point3d pt1_2d = points_2d[i];
				Point3d pt2_2d = points_2d[i + 1];

				// Validate points
				if (!pt1_2d.IsValid || !pt2_2d.IsValid)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Invalid point at index {i} or {i + 1}, skipping pair");
					continue;
				}

				// Check if points are too close (distance between projected points)
				double distance_2d = pt1_2d.DistanceTo(pt2_2d);
				if (distance_2d < 1e-6)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Points at index {i} and {i + 1} are too close (distance: {distance_2d:F6}), skipping pair");
					continue;
				}

				// Get stable GUIDs (using point index in the list)
				Guid guid1 = GetStablePointGuid(0, i);
				Guid guid2 = GetStablePointGuid(0, i + 1);

				// Create PointPair using projected points (in XY plane)
				var pair = new PointPair(pt1_2d, pt2_2d, guid1, guid2);
				pointPairs.Add(pair);
				points1.Add(pt1_2d);
				points2.Add(pt2_2d);
			}

			// Output results
			DA.SetDataList(0, pointPairs);
			DA.SetDataList(1, points1);
			DA.SetDataList(2, points2);
			DA.SetDataList(3, points_2d); // Output all projected points for visualization
		}

		protected override Bitmap Icon
		{
			get
			{
				return LoadIconFromResources("seq24.png");
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

		public override Guid ComponentGuid => new Guid("d3e4f5a6-b7c8-9012-defa-234567890123");
	}
}

