using HannaUIDemo.Core.Constants;
using HannaUIDemo.Theme;

namespace HannaUIDemo.Features.Instruments.Halo2.Logs;

/// <summary>Dual-axis pH / temperature chart for saved Halo 2 log sessions.</summary>
public sealed class Halo2LogHistoryChartDrawable(Func<IReadOnlyList<Halo2LogChartPoint>> getPoints) : IDrawable
{
	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		var points = getPoints();
		if (points.Count == 0)
			return;

		var plot = new RectF(48, 16, Math.Max(120, dirtyRect.Width - 96), Math.Max(100, dirtyRect.Height - 56));
		canvas.FillColor = ThemeColors.LabGraphPlotFill;
		canvas.FillRoundedRectangle(plot, 6);

		canvas.StrokeColor = ThemeColors.Divider;
		canvas.StrokeSize = 1f;
		for (var i = 0; i <= 7; i++)
		{
			var ph = i * 2f;
			var y = plot.Bottom - ph / 14f * plot.Height;
			canvas.DrawLine(plot.Left, y, plot.Right, y);
		}

		for (var i = 0; i <= 6; i++)
		{
			var temp = -10 + i * 20;
			var y = plot.Bottom - (temp + 10) / 120f * plot.Height;
			canvas.DrawLine(plot.Right, y, plot.Right + 8, y);
		}

		DrawPhSeries(canvas, plot, points);
		DrawTempSeries(canvas, plot, points);

		canvas.FontColor = ThemeColors.OnSurfaceVariant;
		canvas.FontSize = 9;
		for (var i = 0; i < points.Count; i++)
		{
			var x = plot.Left + i * plot.Width / Math.Max(1, points.Count - 1);
			canvas.DrawString(points[i].TimeLabel, x - 32, plot.Bottom + 6, 64, 14,
				HorizontalAlignment.Center, VerticalAlignment.Top);
		}

		DrawAxisTitle(canvas, "pH", 16, plot.Center.Y);
		DrawAxisTitle(canvas, "°C", dirtyRect.Width - 14, plot.Center.Y);
		canvas.FontSize = 10;
		canvas.DrawString("Time", plot.Center.X - 24, dirtyRect.Bottom - 2, 48, 14,
			HorizontalAlignment.Center, VerticalAlignment.Bottom);
	}

	static void DrawPhSeries(ICanvas canvas, RectF plot, IReadOnlyList<Halo2LogChartPoint> points)
	{
		var path = new PathF();
		for (var i = 0; i < points.Count; i++)
		{
			var x = plot.Left + i * plot.Width / Math.Max(1, points.Count - 1);
			var y = plot.Bottom - (float)(points[i].Ph / 14.0) * plot.Height;
			if (i == 0)
				path.MoveTo(x, y);
			else
				path.LineTo(x, y);
		}

		canvas.StrokeColor = Color.FromArgb("#0F766E");
		canvas.StrokeSize = 2.5f;
		canvas.DrawPath(path);
	}

	static void DrawTempSeries(ICanvas canvas, RectF plot, IReadOnlyList<Halo2LogChartPoint> points)
	{
		var path = new PathF();
		for (var i = 0; i < points.Count; i++)
		{
			var x = plot.Left + i * plot.Width / Math.Max(1, points.Count - 1);
			var y = plot.Bottom - (float)((points[i].Temp + 10) / 120.0) * plot.Height;
			if (i == 0)
				path.MoveTo(x, y);
			else
				path.LineTo(x, y);
		}

		canvas.StrokeColor = AppConstants.Primary;
		canvas.StrokeSize = 2f;
		canvas.DrawPath(path);
	}

	static void DrawAxisTitle(ICanvas canvas, string title, float centerX, float centerY)
	{
		canvas.SaveState();
		canvas.Rotate(-90, centerX, centerY);
		canvas.DrawString(title, centerX - 30, centerY - 8, 60, 16,
			HorizontalAlignment.Center, VerticalAlignment.Center);
		canvas.RestoreState();
	}
}
