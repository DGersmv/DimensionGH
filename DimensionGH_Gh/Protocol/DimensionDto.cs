using Rhino.Geometry;

namespace DimensionGhGh.Protocol
{
	/// <summary>
	/// Dimension data transfer object
	/// </summary>
	public class DimensionDto
	{
		public string Guid { get; set; }
		public string Type { get; set; }  // "linear", "radial", "angular", etc.
		public Point3d[] Points { get; set; }
		public string Layer { get; set; }
		public string Text { get; set; }
		public string Style { get; set; }

		public DimensionDto()
		{
			Points = new Point3d[0];
		}
	}
}

