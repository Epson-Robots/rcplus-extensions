// -----------------------------------------------------------------------
// <copyright file="EnumLocalizer.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Globalization;
using System.Windows;
using System.Windows.Data;
using static VanguardModelPFScrewdriver.Constants;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Preference;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Program.TighteningProgram;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Base value converter that turns an enumeration value into its localized caption.
    /// Derived classes map each enumeration value to a caption number.
    /// </summary>
    /// <typeparam name="T">The enumeration type to localize.</typeparam>
    public class EnumLocalizer<T> : IValueConverter where T : Enum
    {
        /// <summary>
        /// Maps the specified enumeration value to the caption number used by <see cref="Main.Captions"/>.
        /// </summary>
        /// <param name="enumValue">The enumeration value to map.</param>
        /// <returns>The caption number, or a negative value when no caption is defined.</returns>
        protected virtual int ToCaptionNumber(
            T enumValue
        )
        {
            return -1;
        }

        /// <summary>
        /// Converts an enumeration value to its localized display text.
        /// </summary>
        /// <param name="value">The source value produced by the binding.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter (not used).</param>
        /// <param name="culture">The culture to use in the converter (not used).</param>
        /// <returns>
        /// The localized caption, the raw enumeration name when no caption is defined,
        /// or <see cref="DependencyProperty.UnsetValue"/> when the value is not of type <typeparamref name="T"/>.
        /// </returns>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (value is T enumValue)
            {
                var captionNumber = ToCaptionNumber(enumValue);

                if (captionNumber >= 0)
                {
                    return Main.Captions![captionNumber];
                }

                // No caption is assigned to this value, so fall back to its name.
                return enumValue.ToString();
            }

            return DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// Not supported. The conversion is one-way only.
        /// </summary>
        /// <param name="value">The value produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter (not used).</param>
        /// <param name="culture">The culture to use in the converter (not used).</param>
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

    /// <summary>
    /// Localizer for the <see cref="ModeKind"/> enumeration (tightening mode).
    /// </summary>
    public class ModeKindLocalizer : EnumLocalizer<ModeKind>
    {
        /// <inheritdoc/>
        protected override int ToCaptionNumber(
            ModeKind enumValue
        )
        {
            return enumValue switch
            {
                ModeKind.Normal => Caption.NormalMode,
                ModeKind.Tapping => Caption.TappingMode,
                ModeKind.Fast => Caption.FastMode,
                _ => -1
            };
        }
    }

    /// <summary>
    /// Localizer for the <see cref="StorageKind"/> enumeration (log storage destination).
    /// </summary>
    public class StorageKindLocalizer : EnumLocalizer<StorageKind>
    {
        /// <inheritdoc/>
        protected override int ToCaptionNumber(
            StorageKind enumValue
        )
        {
            return enumValue switch
            {
                StorageKind.PC => Caption.LogStoragePC,
                StorageKind.ControllerUSB => Caption.LogStorageUSB,
                StorageKind.ControllerFlash => Caption.LogStorageFlash,
                _ => -1
            };
        }
    }

    /// <summary>
    /// Localizer for the <see cref="TorqueUnitKind"/> enumeration (torque unit).
    /// </summary>
    public class TorqueUnitLocalizer : EnumLocalizer<TorqueUnitKind>
    {
        /// <inheritdoc/>
        protected override int ToCaptionNumber(
            TorqueUnitKind enumValue
        )
        {
            return enumValue switch
            { 
                TorqueUnitKind.MNM => Caption.TorqueUnitMNM,
                TorqueUnitKind.KGFCM => Caption.TorqueUnitKGFCM,
                _ => -1
            };
        }
    }
}
