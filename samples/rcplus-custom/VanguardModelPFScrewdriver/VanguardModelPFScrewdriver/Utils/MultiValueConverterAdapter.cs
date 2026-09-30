// -----------------------------------------------------------------------
// <copyright file="MultiValueConverterAdapter.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Adapts an <see cref="IValueConverter"/> to a multi-binding so that the converter
    /// parameter can be supplied by a binding instead of a static value.
    /// The first bound value is converted and the second one is passed as the parameter.
    /// </summary>
    public class MultiValueConverterAdapter : IMultiValueConverter
    {
        /// <summary>
        /// Gets or sets the converter the calls are forwarded to.
        /// </summary>
        public IValueConverter? Converter { get; set; }

        /// <summary>
        /// Forwards the conversion to <see cref="Converter"/> using the bound values.
        /// </summary>
        /// <param name="values">The values produced by the bindings; [0] is the value, [1] is the parameter.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use. Not used.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// The converted value, or <see cref="DependencyProperty.UnsetValue"/> when
        /// no converter is set or the bound values are insufficient.
        /// </returns>
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (Converter != null && values != null && values.Length >= 2)
            {
                // The second bound value takes the role of the converter parameter.
                return Converter.Convert(values[0], targetType, values[1], culture);
            }

            return DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// Forwards the backward conversion to <see cref="Converter"/>.
        /// Only the first binding of the multi-binding receives a value.
        /// </summary>
        /// <param name="value">The value produced by the binding target.</param>
        /// <param name="targetTypes">The types to convert back to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// An array holding the converted value, or <c>null</c> when no converter is set
        /// or no target type is available.
        /// </returns>
        public object[] ConvertBack(
            object value,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture
        )
        {
            if (Converter != null && targetTypes != null && targetTypes.Length >= 1)
            {
                return [Converter.ConvertBack(value, targetTypes[0], parameter, culture)];
            }

            return null!;
        }
    }
}
