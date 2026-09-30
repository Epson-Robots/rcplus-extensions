// -----------------------------------------------------------------------
// <copyright file="TorqueValueConverter.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Globalization;
using System.Windows;
using System.Windows.Data;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Preference;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Converter that expresses a torque value stored in mN&#183;m in the torque unit
    /// selected by the user. The unit is supplied by the second binding of the multi-binding.
    /// </summary>
    public class TorqueValueConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts a torque value from mN&#183;m into kgf&#183;cm.
        /// </summary>
        /// <param name="value">The value in mN&#183;m.</param>
        /// <returns>The value in kgf&#183;cm.</returns>
        public static float MNM2KGFCM(
            float value)
        {
            return value * 0.010197f;
        }

        /// <summary>
        /// Converts a torque value from kgf&#183;cm into mN&#183;m.
        /// </summary>
        /// <param name="value">The value in kgf&#183;cm.</param>
        /// <returns>The value in mN&#183;m.</returns>
        public static float KGFCM2MNM(
            float value
        )
        {
            return value * 98.0665f;
        }

        /// <summary>
        /// Converts a torque value in mN&#183;m into the specified torque unit.
        /// </summary>
        /// <param name="values">The bound values; [0] is the value in mN&#183;m, [1] is the <see cref="TorqueUnitKind"/>.</param>
        /// <param name="targetType">The type of the binding target property, either <see cref="string"/> or <see cref="float"/>.</param>
        /// <param name="parameter">The converter parameter to use. Not used.</param>
        /// <param name="culture">The culture to use in the converter. Not used.</param>
        /// <returns>
        /// The converted value formatted for the target type, or
        /// <see cref="DependencyProperty.UnsetValue"/> when the input is not supported.
        /// </returns>
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (
                values.Length >= 2
                && values[0] is float value
                && values[1] is TorqueUnitKind torqueUnit
            )
            {
                // The internal unit is mN.m, so only kgf.cm requires a conversion.
                float? convertedValue = torqueUnit switch
                {
                    TorqueUnitKind.MNM => value,
                    TorqueUnitKind.KGFCM => MNM2KGFCM(value),
                    _ => null
                };

                // Each unit has its own number of decimal places.
                string format = torqueUnit switch
                {
                    TorqueUnitKind.MNM => "f1",
                    TorqueUnitKind.KGFCM => "f3",
                    _ => string.Empty
                };

                if (convertedValue.HasValue)
                {
                    // The caller decides whether the display text or the raw value is needed.
                    if (targetType == typeof(string))
                    {
                        return convertedValue.Value.ToString(format);
                    }
                    else if (targetType.IsAssignableFrom(typeof(float)))
                    {
                        return convertedValue;
                    }
                }
            }

            return DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// Not supported. The conversion is one-way only.
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

        /// <summary>
        /// Gets the number of digits displayed after the decimal point for the specified torque unit.
        /// </summary>
        /// <param name="torqueUnit">The torque unit.</param>
        /// <returns>The number of decimal places.</returns>
        public static int DecimalPlaces(
            TorqueUnitKind torqueUnit
        )
        {
            return torqueUnit switch
            {
                TorqueUnitKind.KGFCM => 3,
                _ => 1,
            };
        }
    }
}
