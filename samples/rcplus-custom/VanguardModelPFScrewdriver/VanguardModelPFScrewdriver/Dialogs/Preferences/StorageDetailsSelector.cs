// -----------------------------------------------------------------------
// <copyright file="StorageDetailsSelector.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Preference;

namespace VanguardModelPFScrewdriver.Dialogs.Preferences
{
    /// <summary>
    /// Selects the <see cref="DataTemplate"/> used to display storage details
    /// according to the bound <see cref="StorageKind"/> value.
    /// </summary>
    public class StorageDetailsSelector : DataTemplateSelector
    {
        /// <summary>
        /// Gets or sets the template used when the storage is located on the PC.
        /// </summary>
        public DataTemplate? TemplatePC { get; set; }

        /// <summary>
        /// Gets or sets the template used when the storage is a USB device connected to the controller.
        /// </summary>
        public DataTemplate? TemplateControllerUSB { get; set; }

        /// <summary>
        /// Gets or sets the template used when the storage is the controller's internal flash memory.
        /// </summary>
        public DataTemplate? TemplateControllerFlash { get; set; }

        /// <summary>
        /// Returns the template that matches the specified item.
        /// </summary>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound object.</param>
        /// <returns>
        /// The template corresponding to the <see cref="StorageKind"/> of <paramref name="item"/>,
        /// or the base implementation's result when the item is not a <see cref="StorageKind"/>
        /// or the kind is unknown.
        /// </returns>
        public override DataTemplate? SelectTemplate(
            object item,
            DependencyObject container
        )
        {
            // Only StorageKind values are handled by this selector.
            if (item is StorageKind kind)
            {
                // Map each storage kind to its dedicated template.
                return kind switch
                {
                    StorageKind.PC => TemplatePC,
                    StorageKind.ControllerUSB => TemplateControllerUSB,
                    StorageKind.ControllerFlash => TemplateControllerFlash,
                    _ => base.SelectTemplate(item, container)
                };
            }
            else
            {
                // Fall back to the default selection logic for unsupported item types.
                return base.SelectTemplate(item, container);
            }
        }
    }
}
