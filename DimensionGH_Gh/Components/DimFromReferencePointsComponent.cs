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
			pManager.AddGenericParameter("PointPair", "PP", "List of point pairs for dimension creation", GH_ParamAccess.list);
			pManager.AddPointParameter("Point1", "P1", "List of reference points (repeated)", GH_ParamAccess.list);
			pManager.AddPointParameter("Point2", "P2", "List of points from Points input", GH_ParamAccess.list);
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
				return;
			}

			// Create pairs: for each reference point, pair with all points from the list
			var pointPairs = new List<PointPair>();
			var points1 = new List<Point3d>();
			var points2 = new List<Point3d>();

			int pairIndex = 0;
			for (int refIdx = 0; refIdx < referencePoints.Count; refIdx++)
			{
				Point3d refPoint = referencePoints[refIdx];

				// Validate reference point
				if (!refPoint.IsValid)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Invalid reference point at index {refIdx}, skipping");
					continue;
				}

				for (int ptIdx = 0; ptIdx < points.Count; ptIdx++)
				{
					Point3d point = points[ptIdx];

					// Validate point
					if (!point.IsValid)
					{
						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Invalid point at index {ptIdx}, skipping pair");
						continue;
					}

					// Check if points are too close
					if (refPoint.DistanceTo(point) < 1e-6)
					{
						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Reference point {refIdx} and point {ptIdx} are too close, skipping pair");
						continue;
					}

					// Get stable GUIDs
					// Input 0 = ReferencePoints, Input 1 = Points
					Guid guid1 = GetStablePointGuid(0, refIdx);
					Guid guid2 = GetStablePointGuid(1, ptIdx);

					// Create PointPair
					var pair = new PointPair(refPoint, point, guid1, guid2);
					pointPairs.Add(pair);
					points1.Add(refPoint);
					points2.Add(point);
					
					pairIndex++;
				}
			}

			// Output results
			DA.SetDataList(0, pointPairs);
			DA.SetDataList(1, points1);
			DA.SetDataList(2, points2);
		}

		protected override Bitmap Icon
		{
			get
			{
				return CreateDimensionIcon();
			}
		}

		/// <summary>
		/// Create icon for reference points component
		/// </summary>
		private Bitmap CreateDimensionIcon()
		{
			var bitmap = new Bitmap(24, 24);
			using (var g = Graphics.FromImage(bitmap))
			{
				g.Clear(Color.Transparent);
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

				using (var pen = new Pen(Color.Black, 2))
				{
					// Draw reference point (left, larger)
					g.FillEllipse(Brushes.Black, 2, 8, 6, 6);
					
					// Draw points (right, smaller)
					g.FillEllipse(Brushes.Black, 12, 6, 4, 4);
					g.FillEllipse(Brushes.Black, 12, 12, 4, 4);
					g.FillEllipse(Brushes.Black, 12, 18, 4, 4);

					// Draw dimension lines from reference to points
					g.DrawLine(pen, 8, 11, 12, 8);
					g.DrawLine(pen, 8, 11, 12, 14);
					g.DrawLine(pen, 8, 11, 12, 20);
				}
			}
			return bitmap;
		}

		public override Guid ComponentGuid => new Guid("e4f5a6b7-c8d9-0123-efab-345678901234");
	}
}

