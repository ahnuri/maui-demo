using System.Globalization;

namespace HannaUIDemo.Core.Converters;

/// <summary>Returns true when a string is non-null and non-whitespace (for IsVisible bindings).</summary>
public sealed class StringNotEmptyConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		value is string s && !string.IsNullOrWhiteSpace(s);

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		throw new NotSupportedException();
}
