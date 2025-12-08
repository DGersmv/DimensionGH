using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using DimensionGhGh.Protocol;
using Rhino.Geometry;

namespace DimensionGhGh.Components
{
	/// <summary>
	/// Component for extracting point pairs from two curves
	/// Curve1 is the reference curve. Finds intersection points along Curve1 at Step intervals
	/// using direction vector from start of Curve1 to start of Curve2
	/// Returns list of PointPair for use with Dim_CreateDimension
	/// </summary>
	public class DimCreateFromLinesComponent : GH_Component
	{
		// Static dictionary to store stable GUIDs for curve points
		// Key: component instance GUID + input index + point index
		// Value: stable GUID for this point
		// This ensures GUID remains stable even when curve geometry changes
		private static System.Collections.Generic.Dictionary<string, Guid> _pointGuidCache = 
			new System.Collections.Generic.Dictionary<string, Guid>();
		
		public DimCreateFromLinesComponent()
			: base("Dim_FromLines", "DimLines",
				"Extract point pairs from two curves. Curve1 is reference. Finds intersections at Step intervals using direction from Curve1 start to Curve2 start.",
				"Info227", "Input")
		{
		}
		
		// Get or create stable GUID for a point on a line
		// Uses component instance GUID + input index + point index to ensure stability
		// This GUID will remain the same even when line geometry changes
		private Guid GetStablePointGuid(int inputIndex, int pointIndex)
		{
			// Create a stable key from component instance GUID, input index, and point index
			// Component instance GUID is unique per component instance and doesn't change
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
			pManager.AddCurveParameter("Curve1", "C1", "Reference curve (step will be measured along this curve)", GH_ParamAccess.item);
			pManager.AddCurveParameter("Curve2", "C2", "Second curve (intersections will be found on this curve)", GH_ParamAccess.item);
			pManager.AddNumberParameter("Step", "S", "Step distance along Curve1 (0 = only start points)", GH_ParamAccess.item, 0.0);
		}

		protected override void RegisterOutputParams(GH_OutputParamManager pManager)
		{
			pManager.AddGenericParameter("PointPair", "PP", "List of point pairs for dimension creation", GH_ParamAccess.list);
			pManager.AddPointParameter("Point1", "P1", "List of points on Curve1", GH_ParamAccess.list);
			pManager.AddPointParameter("Point2", "P2", "List of points on Curve2", GH_ParamAccess.list);
			pManager.AddCurveParameter("Curve1_2D", "C1", "Projected Curve1 on XY plane (step is measured along this curve)", GH_ParamAccess.item);
			pManager.AddCurveParameter("Curve2_2D", "C2", "Projected Curve2 on XY plane", GH_ParamAccess.item);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			Curve curve1 = null;
			Curve curve2 = null;
			double step = 0.0;

			// Get input data
			if (!DA.GetData(0, ref curve1))
			{
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Curve1 is required");
				return;
			}

			if (!DA.GetData(1, ref curve2))
			{
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Curve2 is required");
				return;
			}

			DA.GetData(2, ref step);

			// Validate curves
			if (curve1 == null || !curve1.IsValid)
			{
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Curve1 is invalid");
				return;
			}

			if (curve2 == null || !curve2.IsValid)
			{
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Curve2 is invalid");
				return;
			}

			// Step 1: Project both curves to XY plane
			Plane xyPlane = Plane.WorldXY;
			Curve curve1_2d = Curve.ProjectToPlane(curve1, xyPlane);
			Curve curve2_2d = Curve.ProjectToPlane(curve2, xyPlane);

			if (curve1_2d == null || !curve1_2d.IsValid || curve2_2d == null || !curve2_2d.IsValid)
			{
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to project curves to XY plane");
				return;
			}

			// Step 2: Find start points on projected curves (closest to origin 0,0)
			Point3d origin = new Point3d(0, 0, 0);
			
			// Get start and end points of projected curves
			Point3d start1_2d = curve1_2d.PointAtStart;
			Point3d end1_2d = curve1_2d.PointAtEnd;
			Point3d start2_2d = curve2_2d.PointAtStart;
			Point3d end2_2d = curve2_2d.PointAtEnd;
			
			// For Curve1: choose start or end point that is closer to origin (0,0)
			if (end1_2d.DistanceTo(origin) < start1_2d.DistanceTo(origin))
			{
				// End point is closer - reverse curve so that end becomes start
				start1_2d = end1_2d;
				curve1_2d.Reverse();
			}
			// Otherwise start1_2d is already correct
			
			// For Curve2: choose start or end point that is closer to origin (0,0)
			if (end2_2d.DistanceTo(origin) < start2_2d.DistanceTo(origin))
			{
				// End point is closer - reverse curve so that end becomes start
				start2_2d = end2_2d;
				curve2_2d.Reverse();
			}
			// Otherwise start2_2d is already correct

			// Lists for output
			var pointPairs = new System.Collections.Generic.List<PointPair>();
			var points1 = new System.Collections.Generic.List<Point3d>();
			var points2 = new System.Collections.Generic.List<Point3d>();

			// Step 3: Calculate direction vector from first two points (in XY plane)
			Vector3d direction = start2_2d - start1_2d;
			if (direction.Length < 1e-6)
			{
				AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Start points are too close, direction vector is zero");
				return;
			}
			direction.Unitize(); // Normalize direction vector

			// Step 4: If Step = 0, use only first two points (from projected curves in XY plane)
			if (step <= 0.0)
			{
				// Use projected points directly (in XY plane) for dimensions
				Guid guid1 = GetStablePointGuid(0, 0);
				Guid guid2 = GetStablePointGuid(1, 0);
				var pair = new PointPair(start1_2d, start2_2d, guid1, guid2);
				
				pointPairs.Add(pair);
				points1.Add(start1_2d);
				points2.Add(start2_2d);
			}
			else
			{
				// Step > 0: Find points along projected Curve1 at Step intervals
				double curve1Length_2d = curve1_2d.GetLength();
				double currentLength = 0.0;
				int pointIndex = 0;
				int maxIterations = 1000;
				int iterations = 0;

				// Always include the first pair (start points from projected curves in XY plane)
				Guid guid1 = GetStablePointGuid(0, pointIndex);
				Guid guid2 = GetStablePointGuid(1, pointIndex);
				var firstPair = new PointPair(start1_2d, start2_2d, guid1, guid2);
				pointPairs.Add(firstPair);
				points1.Add(start1_2d);
				points2.Add(start2_2d);
				pointIndex++;

				// Continue finding intersections along projected Curve1
				currentLength = step;
				while (currentLength < curve1Length_2d && iterations < maxIterations)
				{
					iterations++;

					// Find point on projected Curve1 at currentLength distance from start
					double t_2d;
					if (!curve1_2d.LengthParameter(currentLength, out t_2d))
					{
						break;
					}

					Point3d pointOnCurve1_2d = curve1_2d.PointAt(t_2d);
					if (!pointOnCurve1_2d.IsValid)
					{
						break;
					}

					// Create a line from pointOnCurve1_2d in direction of direction vector (in XY plane)
					double searchDistance = Math.Max(curve2_2d.GetLength() * 2.0, 1000.0);
					Point3d lineEnd_2d = pointOnCurve1_2d + direction * searchDistance;
					Line searchLine = new Line(pointOnCurve1_2d, lineEnd_2d);

					// Find intersection between search line and projected Curve2
					double intersectionTolerance = 0.001;
					var intersectionEvents = Rhino.Geometry.Intersect.Intersection.CurveLine(curve2_2d, searchLine, intersectionTolerance, intersectionTolerance);

					if (intersectionEvents != null && intersectionEvents.Count > 0)
					{
						// Use the first intersection point
						var intersection = intersectionEvents[0];
						Point3d pointOnCurve2_2d = intersection.PointA; // Point on projected Curve2

						if (pointOnCurve2_2d.IsValid)
						{
							// Use projected points directly (in XY plane) for dimensions
							Guid guid1_current = GetStablePointGuid(0, pointIndex);
							Guid guid2_current = GetStablePointGuid(1, pointIndex);
							var pair = new PointPair(pointOnCurve1_2d, pointOnCurve2_2d, guid1_current, guid2_current);

							pointPairs.Add(pair);
							points1.Add(pointOnCurve1_2d);
							points2.Add(pointOnCurve2_2d);

							pointIndex++;
						}
					}

					// Move to next step
					currentLength += step;
				}
			}

			// Output results
			DA.SetDataList(0, pointPairs);
			DA.SetDataList(1, points1);
			DA.SetDataList(2, points2);
			DA.SetData(3, curve1_2d); // Output projected Curve1 for visualization
			DA.SetData(4, curve2_2d); // Output projected Curve2 for visualization
		}

		protected override Bitmap Icon
		{
			get
			{
				return LoadIconFromResources("curves24.png");
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

		public override Guid ComponentGuid => new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
	}
}

