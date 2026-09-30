// -----------------------------------------------------------------------
// <copyright file="TextBoxBehavior.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Provides attached behaviors for <see cref="TextBox"/> controls.
    /// </summary>
    public static class TextBoxBehavior
    {
        /// <summary>
        /// Identifies the IsSelectAllOnFocus attached property.
        /// When set to <c>true</c>, the whole text of the <see cref="TextBox"/>
        /// is selected as soon as the control receives the focus.
        /// </summary>
        public static readonly DependencyProperty IsSelectAllOnFocusProperty =
            DependencyProperty.RegisterAttached(
                "IsSelectAllOnFocus",
                typeof(bool),
                typeof(TextBoxBehavior),
                new PropertyMetadata(false, OnIsSelectAllOnFocusChanged)
            );

        /// <summary>
        /// Gets the value of the IsSelectAllOnFocus attached property.
        /// </summary>
        /// <param name="obj">The target dependency object.</param>
        /// <returns><c>true</c> if the behavior is enabled; otherwise, <c>false</c>.</returns>
        public static bool GetIsSelectAllOnFocus(
            DependencyObject obj
        )
        {
            return (bool)obj.GetValue(IsSelectAllOnFocusProperty);
        }

        /// <summary>
        /// Sets the value of the IsSelectAllOnFocus attached property.
        /// </summary>
        /// <param name="obj">The target dependency object.</param>
        /// <param name="value"><c>true</c> to enable the behavior; otherwise, <c>false</c>.</param>
        public static void SetIsSelectAllOnFocus(
            DependencyObject obj,
            bool value
        )
        {
            obj.SetValue(IsSelectAllOnFocusProperty, value);
        }

        /// <summary>
        /// Called when the IsSelectAllOnFocus attached property changes.
        /// Subscribes to or unsubscribes from the required <see cref="TextBox"/> events.
        /// </summary>
        /// <param name="d">The dependency object the property is attached to.</param>
        /// <param name="ev">The property change event data.</param>
        private static void OnIsSelectAllOnFocusChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs ev
        )
        {
            if (d is TextBox textBox)
            {
                if ((bool)ev.NewValue)
                {
                    // Enable the behavior.
                    textBox.GotFocus += TextBox_GotFocus;
                    textBox.PreviewMouseLeftButtonDown += TextBox_PreviewMouseLeftButtonDown;
                }
                else
                {
                    // Disable the behavior and avoid leaking event handlers.
                    textBox.GotFocus -= TextBox_GotFocus;
                    textBox.PreviewMouseLeftButtonDown -= TextBox_PreviewMouseLeftButtonDown;
                }
            }
        }

        /// <summary>
        /// Selects the entire text when the <see cref="TextBox"/> gets the focus
        /// (for example, by keyboard navigation).
        /// </summary>
        /// <param name="sender">The focused <see cref="TextBox"/>.</param>
        /// <param name="ev">The event data.</param>
        private static void TextBox_GotFocus(
            object sender,
            RoutedEventArgs ev
        )
        {
            if (sender is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }

        /// <summary>
        /// Handles the first mouse click on an unfocused <see cref="TextBox"/>.
        /// Without this, the click would place the caret and clear the selection
        /// made by <see cref="TextBox_GotFocus"/>.
        /// </summary>
        /// <param name="sender">The clicked <see cref="TextBox"/>.</param>
        /// <param name="ev">The mouse event data.</param>
        private static void TextBox_PreviewMouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs ev
        )
        {
            if (sender is TextBox textBox)
            {
                if (!textBox.IsKeyboardFocusWithin)
                {
                    textBox.Focus();
                    textBox.SelectAll();

                    // Suppress the default caret placement so the selection is kept.
                    ev.Handled = true;
                }
            }
        }
    }
}
