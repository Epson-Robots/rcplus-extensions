// -----------------------------------------------------------------------
// <copyright file="TighteningErrorDetailConverter.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Globalization;
using System.Windows;
using System.Windows.Data;
using static VanguardModelPFScrewdriver.Constants;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Converter that translates a tightening error detail code into its localized caption text.
    /// </summary>
    public class TighteningErrorDetailConverter : IValueConverter
    {
        /// <summary>
        /// Converts a tightening error detail code into the corresponding localized caption.
        /// </summary>
        /// <param name="value">The error detail code produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use. Not used.</param>
        /// <param name="culture">The culture to use in the converter. Not used.</param>
        /// <returns>
        /// The localized caption when a matching caption exists; otherwise, the original value.
        /// </returns>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (value is string errorDetail)
            {
                // Caption names follow the "ErrorDetail<code>" naming convention.
                var captionName = $"ErrorDetail{errorDetail}";
                var captionId = Caption.NameToNumber(captionName);
                if (captionId.HasValue)
                {
                    return Main.Captions![captionId.Value];
                }
            }

            // Fall back to the original value when no caption is defined.
            return value;
        }

        /// <summary>
        /// Not supported. The conversion is one-way only.
        /// </summary>
        /// <param name="value">The value produced by the binding target.</param>
        /// <param name="targetType">The type to convert back to.</param>
        /// <param name="parameter">The converter parameter to use. Not used.</param>
        /// <param name="culture">The culture to use in the converter. Not used.</param>
        /// <returns>Always <see cref="DependencyProperty.UnsetValue"/>.</returns>
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
