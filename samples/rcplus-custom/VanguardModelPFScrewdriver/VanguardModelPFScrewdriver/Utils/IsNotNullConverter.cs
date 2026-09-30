// -----------------------------------------------------------------------
// <copyright file="IsNotNullConverter.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Converter that converts a value to a boolean indicating whether the value is not null.
    /// Used in WPF data binding to enable/disable or show/hide UI elements based on null checks.
    /// </summary>
    public class IsNotNullConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value to a boolean indicating whether the value is not null.
        /// </summary>
        /// <param name="value">The value to check for null.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>True if the value is not null; otherwise, false.</returns>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            return value != null;
        }

        /// <summary>
        /// Converts a boolean value back to the original value.
        /// This operation is not supported for this converter.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>DependencyProperty.UnsetValue to indicate conversion is not supported.</returns>
        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            return DependencyProperty.UnsetValue;
        }
    }
}

