// EnumToIndexConverter.cs
//
// A value converter that converts between an enum value and its integer index,
// for binding enum ViewModel properties to ComboBox.SelectedIndex in AXAML.
// This works for any enum whose values are sequential starting from 0.
//
// Usage in AXAML:
//   <ComboBox SelectedIndex="{Binding MyEnumProp, Converter={x:Static resx:EnumToIndexConverter.Instance}}" />

using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AvPurplePen
{
    /// <summary>
    /// Converts between an enum value and its integer index (ordinal position).
    /// Works for enums whose underlying values are 0, 1, 2, etc.
    /// </summary>
    public class EnumToIndexConverter : IValueConverter
    {
        /// <summary>
        /// Singleton instance for use in AXAML via x:Static.
        /// </summary>
        public static readonly EnumToIndexConverter Instance = new EnumToIndexConverter();

        /// <summary>
        /// Converts an enum value to its integer index.
        /// </summary>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Enum enumValue) {
                return System.Convert.ToInt32(enumValue);
            }
            return 0;
        }

        /// <summary>
        /// Converts an integer index back to an enum value.
        /// The target enum type is inferred from the targetType parameter.
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int index && targetType.IsEnum) {
                return Enum.ToObject(targetType, index);
            }
            return value;
        }
    }
}
