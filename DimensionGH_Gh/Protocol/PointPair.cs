using System;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;

namespace DimensionGhGh.Protocol
{
	/// <summary>
	/// Represents a pair of points for dimension creation
	/// Wrapped in GH_Goo for Grasshopper compatibility
	/// </summary>
	public class PointPair : GH_Goo<PointPair>
	{
		public Point3d Point1 { get; set; }
		public Point3d Point2 { get; set; }
		
		// GUID of Rhino point for point 1 (for tracking changes)
		public Guid? RhinoPointGuid1 { get; set; }
		
		// GUID of Rhino point for point 2 (for tracking changes)
		public Guid? RhinoPointGuid2 { get; set; }
		
		// GUID of Archicad hotspot for point 1 (set after creation)
		public string ArchicadHotspotGuid1 { get; set; }
		
		// GUID of Archicad hotspot for point 2 (set after creation)
		public string ArchicadHotspotGuid2 { get; set; }
		
		// Previous coordinates for tracking changes (used for update mode)
		private Point3d? _previousPoint1;
		private Point3d? _previousPoint2;
		
		// Check if coordinates have changed since last update
		public bool HasCoordinatesChanged()
		{
			if (!_previousPoint1.HasValue || !_previousPoint2.HasValue)
				return true; // First time, consider as changed
			
			const double tolerance = 1e-6;
			return Point1.DistanceTo(_previousPoint1.Value) > tolerance ||
			       Point2.DistanceTo(_previousPoint2.Value) > tolerance;
		}
		
		// Update stored previous coordinates
		public void UpdatePreviousCoordinates()
		{
			_previousPoint1 = Point1;
			_previousPoint2 = Point2;
		}
		
		// Legacy: Optional GUID of element to attach dimension point 1 to (deprecated, use hotspots)
		public string ElementGuid1 { get; set; }
		
		// Legacy: Optional GUID of element to attach dimension point 2 to (deprecated, use hotspots)
		public string ElementGuid2 { get; set; }

		public PointPair()
		{
			Point1 = Point3d.Unset;
			Point2 = Point3d.Unset;
			RhinoPointGuid1 = null;
			RhinoPointGuid2 = null;
			ArchicadHotspotGuid1 = null;
			ArchicadHotspotGuid2 = null;
			ElementGuid1 = null;
			ElementGuid2 = null;
		}

		public PointPair(Point3d pt1, Point3d pt2)
		{
			Point1 = pt1;
			Point2 = pt2;
			RhinoPointGuid1 = null;
			RhinoPointGuid2 = null;
			ArchicadHotspotGuid1 = null;
			ArchicadHotspotGuid2 = null;
			ElementGuid1 = null;
			ElementGuid2 = null;
		}
		
		public PointPair(Point3d pt1, Point3d pt2, Guid? rhinoGuid1, Guid? rhinoGuid2)
		{
			Point1 = pt1;
			Point2 = pt2;
			RhinoPointGuid1 = rhinoGuid1;
			RhinoPointGuid2 = rhinoGuid2;
			ArchicadHotspotGuid1 = null;
			ArchicadHotspotGuid2 = null;
			ElementGuid1 = null;
			ElementGuid2 = null;
		}
		
		// Legacy constructor
		public PointPair(Point3d pt1, Point3d pt2, string guid1, string guid2)
		{
			Point1 = pt1;
			Point2 = pt2;
			RhinoPointGuid1 = null;
			RhinoPointGuid2 = null;
			ArchicadHotspotGuid1 = null;
			ArchicadHotspotGuid2 = null;
			ElementGuid1 = guid1;
			ElementGuid2 = guid2;
		}

		// GH_Goo implementation - override IsValid property
		public override bool IsValid => Point1.IsValid && Point2.IsValid && Point1.DistanceTo(Point2) > 1e-6;

		public override string TypeName => "PointPair";

		public override string TypeDescription => "A pair of points for dimension creation";

		public override IGH_Goo Duplicate()
		{
			var duplicate = new PointPair(Point1, Point2, RhinoPointGuid1, RhinoPointGuid2)
			{
				ArchicadHotspotGuid1 = ArchicadHotspotGuid1,
				ArchicadHotspotGuid2 = ArchicadHotspotGuid2,
				ElementGuid1 = ElementGuid1,
				ElementGuid2 = ElementGuid2
			};
			return duplicate;
		}

		public override string ToString()
		{
			if (IsValid)
				return $"PointPair: P1({Point1.X:F2}, {Point1.Y:F2}, {Point1.Z:F2}) -> P2({Point2.X:F2}, {Point2.Y:F2}, {Point2.Z:F2})";
			return "Invalid PointPair";
		}

		public override bool CastFrom(object source)
		{
			if (source is PointPair pp)
			{
				Point1 = pp.Point1;
				Point2 = pp.Point2;
				RhinoPointGuid1 = pp.RhinoPointGuid1;
				RhinoPointGuid2 = pp.RhinoPointGuid2;
				ArchicadHotspotGuid1 = pp.ArchicadHotspotGuid1;
				ArchicadHotspotGuid2 = pp.ArchicadHotspotGuid2;
				ElementGuid1 = pp.ElementGuid1;
				ElementGuid2 = pp.ElementGuid2;
				return true;
			}
			return false;
		}

		// CastTo is already implemented in GH_Goo<T> base class
		// No need to override it
	}
}

