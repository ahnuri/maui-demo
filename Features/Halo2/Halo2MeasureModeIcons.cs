namespace HannaUIDemo.Features.Halo2;

/// <summary>Theme-aware toolbar icons for the Halo 2 measure UI.</summary>
public enum HaloMeasureModeIconKind { Table, Chart, Calibration, Settings }

public sealed class HaloMeasureModeIconDrawable(Func<Color> getStroke, HaloMeasureModeIconKind kind) : IDrawable
{
	public void Draw(ICanvas canvas, RectF rect)
	{
		if (rect.Width <= 0 || rect.Height <= 0)
			return;

		var stroke = getStroke();
		canvas.StrokeColor = stroke;
		canvas.FillColor = stroke;
		canvas.StrokeSize = Math.Max(1.6f, rect.Width * 0.09f);
		canvas.StrokeLineCap = LineCap.Round;
		canvas.StrokeLineJoin = LineJoin.Round;

		switch (kind)
		{
			case HaloMeasureModeIconKind.Table:
				DrawTable(canvas, rect);
				break;
			case HaloMeasureModeIconKind.Chart:
				DrawChart(canvas, rect);
				break;
			case HaloMeasureModeIconKind.Calibration:
				DrawCalibration(canvas, rect);
				break;
			case HaloMeasureModeIconKind.Settings:
				DrawSettings(canvas, rect);
				break;
		}
	}

	static void DrawTable(ICanvas canvas, RectF rect)
	{
		var dotR = rect.Width * 0.07f;
		var left = rect.Left + rect.Width * 0.14f;
		var barLeft = rect.Left + rect.Width * 0.34f;
		var barRight = rect.Right - rect.Width * 0.1f;

		foreach (var centerY in new[] { 0.26f, 0.52f, 0.78f })
		{
			var y = rect.Top + rect.Height * centerY;
			canvas.FillCircle(left, y, dotR);
			canvas.DrawLine(barLeft, y, barRight, y);
		}
	}

	static void DrawChart(ICanvas canvas, RectF rect)
	{
		var axisLeft = rect.Left + rect.Width * 0.16f;
		var axisBottom = rect.Bottom - rect.Height * 0.14f;
		var axisTop = rect.Top + rect.Height * 0.18f;
		var axisRight = rect.Right - rect.Width * 0.1f;

		canvas.DrawLine(axisLeft, axisTop, axisLeft, axisBottom);
		canvas.DrawLine(axisLeft, axisBottom, axisRight, axisBottom);

		var path = new PathF();
		path.MoveTo(rect.Left + rect.Width * 0.24f, rect.Top + rect.Height * 0.62f);
		path.LineTo(rect.Left + rect.Width * 0.4f, rect.Top + rect.Height * 0.44f);
		path.LineTo(rect.Left + rect.Width * 0.56f, rect.Top + rect.Height * 0.54f);
		path.LineTo(rect.Left + rect.Width * 0.78f, rect.Top + rect.Height * 0.3f);
		canvas.DrawPath(path);
	}

	static void DrawCalibration(ICanvas canvas, RectF rect)
	{
		var cx = rect.Center.X;
		var neckTop = rect.Top + rect.Height * 0.1f;
		var neckBottom = rect.Top + rect.Height * 0.36f;
		var baseY = rect.Bottom - rect.Height * 0.12f;
		var left = rect.Left + rect.Width * 0.26f;
		var right = rect.Right - rect.Width * 0.26f;
		var liquidY = rect.Top + rect.Height * 0.56f;

		canvas.DrawLine(cx, neckTop, cx, neckBottom);
		canvas.DrawLine(cx, neckBottom, left, baseY);
		canvas.DrawLine(left, baseY, right, baseY);
		canvas.DrawLine(right, baseY, cx, neckBottom);
		canvas.DrawLine(left + rect.Width * 0.06f, liquidY, right - rect.Width * 0.06f, liquidY);
	}

	static void DrawSettings(ICanvas canvas, RectF rect)
	{
		var cx = rect.Center.X;
		var cy = rect.Center.Y;
		var outer = Math.Min(rect.Width, rect.Height) * 0.4f;

		canvas.DrawCircle(cx, cy, outer);
		canvas.DrawCircle(cx, cy, outer * 0.42f);

		for (var i = 0; i < 8; i++)
		{
			var angle = i * Math.PI / 4;
			var tx = cx + (float)(outer * 1.08 * Math.Cos(angle));
			var ty = cy + (float)(outer * 1.08 * Math.Sin(angle));
			canvas.FillCircle(tx, ty, outer * 0.19f);
		}
	}
}

public static class Halo2MeasureModeIcons
{
	public static GraphicsView Create(HaloMeasureModeIconKind kind, Func<Color> getStroke, double size = 20) => new()
	{
		WidthRequest = size,
		HeightRequest = size,
		Drawable = new HaloMeasureModeIconDrawable(getStroke, kind)
	};
}
