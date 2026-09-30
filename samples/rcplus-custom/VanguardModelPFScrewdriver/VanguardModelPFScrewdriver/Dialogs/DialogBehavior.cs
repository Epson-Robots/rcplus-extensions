// -----------------------------------------------------------------------
// <copyright file="DialogBehavior.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows;

namespace VanguardModelPFScrewdriver.Dialogs
{
    /// <summary>
    /// Attached behavior that exposes <see cref="Window.DialogResult"/> as an attached property
    /// so that a data-bound value can close the dialog when it is set.
    /// </summary>
    public static class DialogBehavior
    {
        /// <summary>
        /// Identifies the attached DialogResult dependency property.
        /// Setting this property closes the owning <see cref="Window"/>.
        /// </summary>
        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached(
                "DialogResult",
                typeof(bool?),
                typeof(DialogBehavior),
                new PropertyMetadata(new PropertyChangedCallback(OnDialogResultChanged))
            );

        /// <summary>
        /// Gets the current value of the attached DialogResult property.
        /// </summary>
        /// <param name="target">The object the property is attached to.</param>
        /// <returns>The stored dialog result, or <c>null</c> if not set.</returns>
        public static bool? GetDialogResult(
            DependencyObject target
        )
        {
            return (bool?)target.GetValue(DialogResultProperty);
        }

        /// <summary>
        /// Sets the value of the attached DialogResult property.
        /// </summary>
        /// <param name="target">The object the property is attached to.</param>
        /// <param name="value">The dialog result to apply to the window.</param>
        public static void SetDialogResult(
            DependencyObject target,
            bool? value
        )
        {
            target.SetValue(DialogResultProperty, value);
        }

        /// <summary>
        /// Handles changes of the attached DialogResult property and closes the window.
        /// </summary>
        /// <param name="d">The object whose property changed; expected to be a <see cref="Window"/>.</param>
        /// <param name="ev">Event data containing the new dialog result.</param>
        private static void OnDialogResultChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs ev
        )
        {
            // Only loaded windows can accept a DialogResult; otherwise an exception is thrown.
            if (d is Window window)
            {
                if (window.IsLoaded)
                {
                    window.DialogResult = (bool?)ev.NewValue;
                    window.Close();
                }
            }
        }
    }
}
