using System.Globalization;

namespace HannaUIDemo.Core.Converters;

/// <summary>Inverts a boolean (e.g. show card when IsSection is false).</summary>
public sealed class InvertedBoolConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		value is bool b && !b;

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		value is bool b && !b;
}
