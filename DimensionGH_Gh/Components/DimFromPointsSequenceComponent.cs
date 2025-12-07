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
			pManager.AddGenericParameter("PointPair", "PP", "List of sequential point pairs for dimension creation", GH_ParamAccess.list);
			pManager.AddPointParameter("Point1", "P1", "List of first points in pairs", GH_ParamAccess.list);
			pManager.AddPointParameter("Point2", "P2", "List of second points in pairs", GH_ParamAccess.list);
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
				return;
			}

			if (points.Count == 1)
			{
				// Only one point - show error
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "At least 2 points are required to create pairs. Only 1 point provided.");
				DA.SetDataList(0, new List<PointPair>());
				DA.SetDataList(1, new List<Point3d>());
				DA.SetDataList(2, new List<Point3d>());
				return;
			}

			// Create sequential pairs
			var pointPairs = new List<PointPair>();
			var points1 = new List<Point3d>();
			var points2 = new List<Point3d>();

			// Create pairs: (point[i], point[i+1]) for i from 0 to count-2
			for (int i = 0; i < points.Count - 1; i++)
			{
				Point3d pt1 = points[i];
				Point3d pt2 = points[i + 1];

				// Validate points
				if (!pt1.IsValid || !pt2.IsValid)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Invalid point at index {i} or {i + 1}, skipping pair");
					continue;
				}

				// Check if points are too close
				if (pt1.DistanceTo(pt2) < 1e-6)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Points at index {i} and {i + 1} are too close, skipping pair");
					continue;
				}

				// Get stable GUIDs (using point index in the list)
				Guid guid1 = GetStablePointGuid(0, i);
				Guid guid2 = GetStablePointGuid(0, i + 1);

				// Create PointPair
				var pair = new PointPair(pt1, pt2, guid1, guid2);
				pointPairs.Add(pair);
				points1.Add(pt1);
				points2.Add(pt2);
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
		/// Create icon for sequential point pairs
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
					// Draw sequential points
					g.FillEllipse(Brushes.Black, 2, 10, 4, 4);
					g.FillEllipse(Brushes.Black, 8, 10, 4, 4);
					g.FillEllipse(Brushes.Black, 14, 10, 4, 4);
					g.FillEllipse(Brushes.Black, 20, 10, 4, 4);

					// Draw dimension lines between points
					g.DrawLine(pen, 6, 12, 8, 12);
					g.DrawLine(pen, 12, 12, 14, 12);
					g.DrawLine(pen, 18, 12, 20, 12);
				}
			}
			return bitmap;
		}

		public override Guid ComponentGuid => new Guid("d3e4f5a6-b7c8-9012-defa-234567890123");
	}
}

