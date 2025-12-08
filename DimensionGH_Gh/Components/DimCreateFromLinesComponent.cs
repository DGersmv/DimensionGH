using System;
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

			// Get start points of both curves
			Point3d start1 = curve1.PointAtStart;
			Point3d start2 = curve2.PointAtStart;

			if (!start1.IsValid || !start2.IsValid)
			{
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid start points on curves");
				return;
			}

			// Calculate direction vector from start1 to start2
			Vector3d direction = start2 - start1;
			if (direction.Length < 1e-6)
			{
				AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Start points are too close, direction vector is zero");
				return;
			}
			direction.Unitize(); // Normalize direction vector

			// Lists for output
			var pointPairs = new System.Collections.Generic.List<PointPair>();
			var points1 = new System.Collections.Generic.List<Point3d>();
			var points2 = new System.Collections.Generic.List<Point3d>();

			// If Step = 0, use only start points (original behavior)
			if (step <= 0.0)
			{
				Guid pointGuid1 = GetStablePointGuid(0, 0);
				Guid pointGuid2 = GetStablePointGuid(1, 0);
				var pointPair = new PointPair(start1, start2, pointGuid1, pointGuid2);
				
				pointPairs.Add(pointPair);
				points1.Add(start1);
				points2.Add(start2);
			}
			else
			{
				// Step > 0: Find intersections along Curve1 at Step intervals
				double curveLength = curve1.GetLength();
				double currentLength = 0.0;
				int pointIndex = 0;

				// Always include the first pair (start points)
				Guid pointGuid1 = GetStablePointGuid(0, pointIndex);
				Guid pointGuid2 = GetStablePointGuid(1, pointIndex);
				var firstPair = new PointPair(start1, start2, pointGuid1, pointGuid2);
				pointPairs.Add(firstPair);
				points1.Add(start1);
				points2.Add(start2);
				pointIndex++;

				// Continue finding intersections along Curve1
				currentLength = step;
				while (currentLength < curveLength)
				{
					// Find point on Curve1 at currentLength distance from start
					double t;
					if (!curve1.LengthParameter(currentLength, out t))
					{
						// Cannot find parameter for this length, stop
						break;
					}

					Point3d pointOnCurve1 = curve1.PointAt(t);
					if (!pointOnCurve1.IsValid)
					{
						break;
					}

					// Create a line from pointOnCurve1 in direction of direction vector
					// Use a long enough line to ensure intersection with Curve2
					double searchDistance = curve2.GetLength() * 2.0; // Use 2x curve length for safety
					Point3d lineEnd = pointOnCurve1 + direction * searchDistance;
					Line searchLine = new Line(pointOnCurve1, lineEnd);

					// Find intersection between search line and Curve2
					var intersectionEvents = Rhino.Geometry.Intersect.Intersection.CurveLine(curve2, searchLine, 0.001, 0.001);
					
					if (intersectionEvents != null && intersectionEvents.Count > 0)
					{
						// Use the first intersection point
						var intersection = intersectionEvents[0];
						Point3d pointOnCurve2 = intersection.PointA; // Point on Curve2

						// Create PointPair
						Guid guid1 = GetStablePointGuid(0, pointIndex);
						Guid guid2 = GetStablePointGuid(1, pointIndex);
						var pair = new PointPair(pointOnCurve1, pointOnCurve2, guid1, guid2);

						pointPairs.Add(pair);
						points1.Add(pointOnCurve1);
						points2.Add(pointOnCurve2);

						pointIndex++;
					}
					else
					{
						// No intersection found - stop searching
						break;
					}

					// Move to next step
					currentLength += step;
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

