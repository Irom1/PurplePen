// A value converter that converts a string to a double, for use in
// CommandParameter bindings where the XAML literal is a string but
// the RelayCommand expects a double parameter.
//
// Usage in AXAML:
//   <MenuItem Command="{Binding SetZoomCommand}"
//             CommandParameter="{Binding Source=0.5,
//                 Converter={x:Static avutil:StringToDoubleConverter.Instance}}" />

using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AvUtil
{
    /// <summary>
    /// Converts a string (or boxed numeric value) to a double.
    /// Primarily used for CommandParameter values that need to arrive
    /// at a RelayCommand&lt;double&gt; as the correct type.
    /// </summary>
    public class StringToDoubleConverter : IValueConverter
    {
        /// <summary>
        /// Singleton instance for use in AXAML via x:Static.
        /// </summary>
        public static readonly StringToDoubleConverter Instance = new StringToDoubleConverter();

        /// <summary>
        /// Converts the value to a double. Accepts strings and numeric types.
        /// </summary>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double d) {
                return d;
            }
            if (value is string s && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double result)) {
                return result;
            }
            if (value != null) {
                try {
                    return System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
                }
                catch {
                    // Fall through to default.
                }
            }
            return 0.0;
        }

        /// <summary>
        /// Converts a double back to a string (for two-way binding scenarios).
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double d) {
                return d.ToString(CultureInfo.InvariantCulture);
            }
            return value?.ToString() ?? "";
        }
    }
}
