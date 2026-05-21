namespace HannaUIDemo.Features.Measure;

/// <summary>Simple vector icons for photometer quick actions (matches Halo 2 GraphicsView pattern).</summary>
public enum PhotometerActionIconKind
{
	QuickBolt,
	DailySun,
	WeeklyCalendar,
	AllMethodsGrid,
}

public sealed class PhotometerActionIconDrawable(Func<Color> strokeColor, PhotometerActionIconKind kind) : IDrawable
{
	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		var s = strokeColor();
		canvas.StrokeColor = s;
		canvas.FillColor = s;
		canvas.StrokeLineCap = LineCap.Round;
		canvas.StrokeLineJoin = LineJoin.Round;
		canvas.StrokeSize = Math.Max(1.6f, dirtyRect.Width * 0.085f);

		var cx = dirtyRect.Center.X;
		var cy = dirtyRect.Center.Y;
		var r = Math.Min(dirtyRect.Width, dirtyRect.Height) * 0.42f;

		switch (kind)
		{
			case PhotometerActionIconKind.QuickBolt:
				canvas.StrokeSize = Math.Max(1.8f, dirtyRect.Width * 0.1f);
				var path = new PathF(cx - r * 0.35f, cy - r * 0.85f);
				path.LineTo(cx + r * 0.05f, cy - r * 0.15f);
				path.LineTo(cx - r * 0.1f, cy - r * 0.12f);
				path.LineTo(cx + r * 0.45f, cy + r * 0.9f);
				path.LineTo(cx + r * 0.08f, cy + r * 0.02f);
				path.LineTo(cx + r * 0.28f, cy);
				path.Close();
				canvas.DrawPath(path);
				break;

			case PhotometerActionIconKind.DailySun:
				canvas.DrawCircle(cx, cy, r * 0.38f);
				for (var i = 0; i < 8; i++)
				{
					var a = (float)(i * Math.PI / 4);
					var x1 = cx + MathF.Cos(a) * r * 0.52f;
					var y1 = cy + MathF.Sin(a) * r * 0.52f;
					var x2 = cx + MathF.Cos(a) * r * 0.88f;
					var y2 = cy + MathF.Sin(a) * r * 0.88f;
					canvas.DrawLine(x1, y1, x2, y2);
				}
				break;

			case PhotometerActionIconKind.WeeklyCalendar:
				var w = r * 1.45f;
				var h = r * 1.25f;
				var left = cx - w / 2;
				var top = cy - h / 2;
				canvas.DrawRoundedRectangle(left, top, w, h, 3);
				canvas.DrawLine(left, top + h * 0.32f, left + w, top + h * 0.32f);
				canvas.DrawLine(cx, top, cx, top + h * 0.32f);
				var cellW = w / 7.2f;
				var baseY = top + h * 0.42f;
				for (var d = 0; d < 7; d++)
				{
					var px = left + cellW * d + cellW * 0.35f;
					canvas.FillColor = s.MultiplyAlpha(0.35f);
					canvas.FillRoundedRectangle(px, baseY + h * 0.12f, cellW * 0.55f, h * 0.22f, 1);
				}
				break;

			case PhotometerActionIconKind.AllMethodsGrid:
				var g = r * 0.38f;
				for (var row = 0; row < 3; row++)
				for (var col = 0; col < 3; col++)
				{
					var gx = cx - g * 1.6f + col * g * 1.05f;
					var gy = cy - g * 1.6f + row * g * 1.05f;
					canvas.DrawRoundedRectangle(gx, gy, g * 0.85f, g * 0.85f, 1.5f);
				}
				break;
		}
	}
}

public static class PhotometerActionIcons
{
	public static GraphicsView Create(PhotometerActionIconKind kind, Func<Color> stroke, double size = 24) => new()
	{
		WidthRequest = size,
		HeightRequest = size,
		HorizontalOptions = LayoutOptions.Center,
		VerticalOptions = LayoutOptions.Center,
		Drawable = new PhotometerActionIconDrawable(stroke, kind)
	};
}
