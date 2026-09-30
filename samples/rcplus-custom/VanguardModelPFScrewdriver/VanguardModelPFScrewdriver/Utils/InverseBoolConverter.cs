// -----------------------------------------------------------------------
// <copyright file="InverseBoolConverter.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Value converter that inverts a <see cref="bool"/> value.
    /// Typically used in bindings such as IsEnabled="{Binding IsBusy, Converter={StaticResource InverseBoolConverter}}".
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        /// <summary>
        /// Converts a source <see cref="bool"/> value to its inverted value.
        /// </summary>
        /// <param name="value">The source value produced by the binding.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter (not used).</param>
        /// <param name="culture">The culture to use in the converter (not used).</param>
        /// <returns>
        /// The inverted value when <paramref name="value"/> is a <see cref="bool"/>;
        /// otherwise <see cref="DependencyProperty.UnsetValue"/>.
        /// </returns>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }

            // The value is not a bool, so let the binding use its fallback/default value.
            return DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// Converts a target <see cref="bool"/> value back to its inverted source value.
        /// The conversion is symmetric with <see cref="Convert"/>.
        /// </summary>
        /// <param name="value">The value produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter (not used).</param>
        /// <param name="culture">The culture to use in the converter (not used).</param>
        /// <returns>
        /// The inverted value when <paramref name="value"/> is a <see cref="bool"/>;
        /// otherwise <see cref="DependencyProperty.UnsetValue"/>.
        /// </returns>
        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }

            // The value is not a bool, so let the binding use its fallback/default value.
            return DependencyProperty.UnsetValue;
        }
    }
}
