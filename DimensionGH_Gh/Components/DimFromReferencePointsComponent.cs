using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using DimensionGhGh.Protocol;
using Rhino.Geometry;

namespace DimensionGhGh.Components
{
	/// <summary>
	/// Component for creating point pairs from reference points and a list of points
	/// For each reference point, creates pairs with all points from the list
	/// Returns list of PointPair for use with Dim_CreateDimension
	/// </summary>
	public class DimFromReferencePointsComponent : GH_Component
	{
		// Static dictionary to store stable GUIDs for points
		// Key: component instance GUID + input index + point index
		// Value: stable GUID for this point
		private static System.Collections.Generic.Dictionary<string, Guid> _pointGuidCache = 
			new System.Collections.Generic.Dictionary<string, Guid>();
		
		public DimFromReferencePointsComponent()
			: base("Dim_FromReferencePoints", "DimRef",
				"Create point pairs from reference points and a list of points. Each reference point pairs with all points from the list.",
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
			pManager.AddPointParameter("ReferencePoints", "RP", "List of reference points (each will pair with all points from Points list)", GH_ParamAccess.list);
			pManager.AddPointParameter("Points", "P", "List of points to pair with reference points", GH_ParamAccess.list);
		}

		protected override void RegisterOutputParams(GH_OutputParamManager pManager)
		{
			pManager.AddGenericParameter("PointPair", "PP", "List of point pairs for dimension creation (projected to XY plane)", GH_ParamAccess.list);
			pManager.AddPointParameter("Point1", "P1", "List of reference points (repeated, projected to XY plane)", GH_ParamAccess.list);
			pManager.AddPointParameter("Point2", "P2", "List of points from Points input (projected to XY plane)", GH_ParamAccess.list);
			pManager.AddPointParameter("ReferencePoints_2D", "RP2D", "All projected reference points on XY plane for visualization", GH_ParamAccess.list);
			pManager.AddPointParameter("Points_2D", "P2D", "All projected points on XY plane for visualization", GH_ParamAccess.list);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			List<Point3d> referencePoints = new List<Point3d>();
			List<Point3d> points = new List<Point3d>();

			// Get input data
			if (!DA.GetDataList(0, referencePoints))
			{
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "ReferencePoints list is required");
				return;
			}

			if (!DA.GetDataList(1, points))
			{
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Points list is required");
				return;
			}

			// Validate input - if either list is empty, do nothing (return empty results)
			if (referencePoints == null || referencePoints.Count == 0 || points == null || points.Count == 0)
			{
				DA.SetDataList(0, new List<PointPair>());
				DA.SetDataList(1, new List<Point3d>());
				DA.SetDataList(2, new List<Point3d>());
				DA.SetDataList(3, new List<Point3d>());
				DA.SetDataList(4, new List<Point3d>());
				return;
			}

			// Step 1: Project all points to XY plane
			var referencePoints_2d = new List<Point3d>();
			var points_2d = new List<Point3d>();

			foreach (Point3d pt in referencePoints)
			{
				if (pt.IsValid)
				{
					// Project point to XY plane (set Z = 0)
					Point3d pt_2d = new Point3d(pt.X, pt.Y, 0);
					referencePoints_2d.Add(pt_2d);
				}
				else
				{
					referencePoints_2d.Add(pt); // Keep invalid point as is
				}
			}

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

			// Step 2: Create pairs from projected points: for each reference point, pair with all points from the list
			var pointPairs = new List<PointPair>();
			var points1 = new List<Point3d>();
			var points2 = new List<Point3d>();

			int pairIndex = 0;
			for (int refIdx = 0; refIdx < referencePoints_2d.Count; refIdx++)
			{
				Point3d refPoint_2d = referencePoints_2d[refIdx];

				// Validate reference point
				if (!refPoint_2d.IsValid)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Invalid reference point at index {refIdx}, skipping");
					continue;
				}

				for (int ptIdx = 0; ptIdx < points_2d.Count; ptIdx++)
				{
					Point3d point_2d = points_2d[ptIdx];

					// Validate point
					if (!point_2d.IsValid)
					{
						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Invalid point at index {ptIdx}, skipping pair");
						continue;
					}

					// Check if points are too close (distance between projected points)
					double distance_2d = refPoint_2d.DistanceTo(point_2d);
					if (distance_2d < 1e-6)
					{
						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Reference point {refIdx} and point {ptIdx} are too close (distance: {distance_2d:F6}), skipping pair");
						continue;
					}

					// Get stable GUIDs
					// Input 0 = ReferencePoints, Input 1 = Points
					Guid guid1 = GetStablePointGuid(0, refIdx);
					Guid guid2 = GetStablePointGuid(1, ptIdx);

					// Create PointPair using projected points (in XY plane)
					var pair = new PointPair(refPoint_2d, point_2d, guid1, guid2);
					pointPairs.Add(pair);
					points1.Add(refPoint_2d);
					points2.Add(point_2d);
					
					pairIndex++;
				}
			}

			// Output results
			DA.SetDataList(0, pointPairs);
			DA.SetDataList(1, points1);
			DA.SetDataList(2, points2);
			DA.SetDataList(3, referencePoints_2d); // Output all projected reference points for visualization
			DA.SetDataList(4, points_2d); // Output all projected points for visualization
		}

		protected override Bitmap Icon
		{
			get
			{
				return LoadIconFromResources("ref24.png");
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

		public override Guid ComponentGuid => new Guid("e4f5a6b7-c8d9-0123-efab-345678901234");
	}
}

