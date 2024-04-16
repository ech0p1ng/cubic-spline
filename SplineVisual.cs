using System.Windows.Media;

namespace SplineVisualizationWPF
{
	internal class SplineVisual
	{
		public Brush brush { get; set; }
		public Spline spline { get; set; }
		public string title { get; set; }

		public bool isLineVisible {  get; set; }

		public SplineVisual(Spline spline, Brush brush, string title, bool isLineVisible)
		{
			this.spline = spline;
			this.brush = brush;
			this.title = title;
			this.isLineVisible = isLineVisible;
		}
	}
}
