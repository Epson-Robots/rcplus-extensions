// -----------------------------------------------------------------------
// <copyright file="BoolsAndConverter.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Globalization;
using System.Windows.Data;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Multi-value converter that performs a logical AND over the bound boolean values.
    /// </summary>
    public class BoolsAndConverter : IMultiValueConverter
    {
        /// <summary>
        /// Returns <c>true</c> only when every bound boolean value is <c>true</c>.
        /// </summary>
        /// <param name="values">The values produced by the bindings in the multi-binding.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use. Not used.</param>
        /// <param name="culture">The culture to use in the converter. Not used.</param>
        /// <returns><c>true</c> if no value is <c>false</c>; otherwise, <c>false</c>.</returns>
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            // Short-circuit as soon as a false boolean value is found.
            foreach (var value in values)
            {
                if (value is bool boolValue && !boolValue)
                {
                    return false;
                }
            }

            // All values are true (non-boolean values are ignored).
            return true;
        }

        /// <summary>
        /// Not supported. The AND operation cannot be reversed unambiguously.
        /// </summary>
        /// <param name="value">The value produced by the binding target.</param>
        /// <param name="targetTypes">The types to convert back to.</param>
        /// <param name="parameter">The converter parameter to use. Not used.</param>
        /// <param name="culture">The culture to use in the converter. Not used.</param>
        /// <returns>Always <c>null</c>.</returns>
        public object[] ConvertBack(
            object value,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture
        )
        {
            return null!;
        }
    }
}
