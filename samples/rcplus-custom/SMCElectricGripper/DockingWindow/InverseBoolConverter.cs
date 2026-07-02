// -----------------------------------------------------------------------
// <copyright file="InverseBoolConverter.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SMCElectricGripper.DockingWindow
{
    /// <summary>
    /// Boolean value converter that inverts the input value
    /// </summary>
    internal class InverseBoolConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to its inverse
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool b)
            {
                return DependencyProperty.UnsetValue;
            }

            return !b;
        }

        /// <summary>
        /// Converts back by applying the same inversion logic
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
}
