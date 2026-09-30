// -----------------------------------------------------------------------
// <copyright file="BindingProxy.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Freezable helper that makes a data context reachable from elements which are
    /// outside of the visual tree (for example <see cref="System.Windows.Controls.DataGridColumn"/>
    /// or <see cref="System.Windows.Controls.ContextMenu"/>).
    /// Declare it as a resource and bind through its <see cref="Data"/> property.
    /// </summary>
    /// <remarks>
    /// <see cref="Freezable"/> is used because it inherits the data context
    /// through the resource lookup, which a plain <see cref="DependencyObject"/> does not.
    /// </remarks>
    public class BindingProxy : Freezable
    {
        /// <summary>
        /// Identifies the <see cref="Data"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(
                "Data",
                typeof(object),
                typeof(BindingProxy),
                new PropertyMetadata(null)
            );

        /// <summary>
        /// Gets or sets the object (usually the view model) exposed to the bindings.
        /// </summary>
        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BindingProxy"/> class.
        /// Required by the <see cref="Freezable"/> infrastructure.
        /// </summary>
        /// <returns>A new <see cref="BindingProxy"/> instance.</returns>
        protected override Freezable CreateInstanceCore() => new BindingProxy();
    }
}
