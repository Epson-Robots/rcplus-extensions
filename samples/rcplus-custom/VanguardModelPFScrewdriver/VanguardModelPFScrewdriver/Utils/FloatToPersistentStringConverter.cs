// -----------------------------------------------------------------------
// <copyright file="FloatToPersistentStringConverter.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Converter between <see cref="float"/> and its string representation that preserves
    /// the exact text typed by the user (for example "1." or "1.50") instead of reformatting it.
    /// </summary>
    public class FloatToPersistentStringConverter : IValueConverter
    {
        /// <summary>
        /// Maps a converter parameter alias to the actual numeric format string.
        /// </summary>
        public static readonly Dictionary<string, string> SpecialFormat = [];

        /// <summary>
        /// The text most recently parsed by <see cref="ConvertBack"/>, used once to keep
        /// the user input untouched on the following conversion.
        /// </summary>
        private string? _lastConvertBackString;

        /// <summary>
        /// Converts a <see cref="float"/> into its string representation.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">A format string or a key registered in <see cref="SpecialFormat"/>.</param>
        /// <param name="culture">The culture to use in the converter. Not used.</param>
        /// <returns>
        /// The user input when it is still pending; otherwise the formatted value.
        /// Returns <see cref="DependencyProperty.UnsetValue"/> when the value is not a <see cref="float"/>.
        /// </returns>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (value is float floatValue)
            {
                string format = string.Empty;
                if (parameter is string specified)
                {
                    // Resolve the alias if it is registered, otherwise use the parameter as-is.
                    if (SpecialFormat.TryGetValue(specified, out var actualFormat))
                    {
                        format = actualFormat;
                    }
                    else
                    {
                        format = specified;
                    }
                }

                // Give priority to the original user input so that editing is not disturbed.
                string stringValue = _lastConvertBackString ?? floatValue.ToString(format);
                _lastConvertBackString = null;

                return stringValue;
            }

            return DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// Parses the text of the binding target into a <see cref="float"/>.
        /// </summary>
        /// <param name="value">The value produced by the binding target.</param>
        /// <param name="targetType">The type to convert back to.</param>
        /// <param name="parameter">The converter parameter to use. Not used.</param>
        /// <param name="culture">The culture to use in the converter. Not used.</param>
        /// <returns>
        /// The parsed value, or <see cref="DependencyProperty.UnsetValue"/> when the text is not a valid number.
        /// </returns>
        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (value is string stringValue)
            {
                if (float.TryParse(stringValue, out var floatValue))
                {
                    // Remember the raw text so the next Convert call can return it unchanged.
                    _lastConvertBackString = stringValue;
                    return floatValue;
                }
            }

            return DependencyProperty.UnsetValue;
        }
    }
}
