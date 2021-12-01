using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Histalyzer
{
	class HistGraph
	{
		double width, height;
		double xmargin, ymargin;
		double graphWidth, graphHeight;

		Typeface fontFace;
		double fontSize;
		Brush barBrush;
		Pen barPen;
		Brush textBrush;

		public bool inited = false;


		public void init(double width, double height)
		{
			this.width = width;
			this.height = height;
			xmargin = width * 0.04;
			ymargin = height * 0.02;
			graphWidth = width - xmargin * 2;
			graphHeight = height * 0.9 - ymargin * 2.5;

			fontFace = new Typeface("Arial");
			fontSize = height * 0.025;
			barBrush = Brushes.LightGray;
			barPen = new Pen(Brushes.Black, 1);
			textBrush = Brushes.Black;

			inited = true;
		}

		public void drawBar(DrawingContext drawingContext, double vx1, double vy1, double vx2, double vy2)
		{
			double x1 = xmargin + vx1 * graphWidth;
			double x2 = xmargin + vx2 * graphWidth;
			double y1 = ymargin + (1 - vy1) * graphHeight;
			double y2 = ymargin + (1 - vy2) * graphHeight;
			drawingContext.DrawRectangle(barBrush, barPen, new Rect(new Point(x1, y1), new Point(x2, y2)));
		}

		public void drawText(DrawingContext drawingContext, double vx, double vy, string label)
		{
			double x = xmargin + vx * graphWidth + fontSize / 2;
			double y = ymargin * 1.5 + (1 - vy) * graphHeight;
			Point point = new System.Windows.Point(x, y);
			RotateTransform rotate = new RotateTransform(90, x, y);

			drawingContext.PushTransform(rotate);
			drawingContext.DrawText(new FormattedText(label, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, fontFace, fontSize, textBrush), point);
			drawingContext.Pop();
		}

		public double getDataPosition(Point position)
		{
			return (position.X - xmargin) / graphWidth;
		}

	}
}
