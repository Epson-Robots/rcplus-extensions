// -----------------------------------------------------------------------
// <copyright file="DecimalPlacesLimiter.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Xaml.Behaviors;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Behavior that restricts a <see cref="TextBox"/> to decimal input with a limited
    /// number of digits after the decimal point.
    /// </summary>
    public class DecimalPlacesLimiter : Behavior<TextBox>
    {
        /// <summary>
        /// Identifies the <see cref="MaxDecimalPlaces"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaxDecimalPlacesProperty =
            DependencyProperty.Register(
                nameof(MaxDecimalPlaces),
                typeof(int),
                typeof(DecimalPlacesLimiter),
                new PropertyMetadata(1, OnMaxDecimalPlacesChanged)
            );

        /// <summary>
        /// Gets or sets the maximum number of digits allowed after the decimal point.
        /// </summary>
        public int MaxDecimalPlaces
        {
            get => (int)GetValue(MaxDecimalPlacesProperty);
            set => SetValue(MaxDecimalPlacesProperty, value);
        }

        /// <summary>
        /// Subscribes to the input events of the associated <see cref="TextBox"/>
        /// and normalizes its initial text.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreviewTextInput += OnPreviewTextInput;
            DataObject.AddPastingHandler(AssociatedObject, OnPaste);

            CoerceCurrentText();
        }

        /// <summary>
        /// Unsubscribes from the input events of the associated <see cref="TextBox"/>.
        /// </summary>
        protected override void OnDetaching()
        {
            AssociatedObject.PreviewTextInput -= OnPreviewTextInput;
            DataObject.RemovePastingHandler(AssociatedObject, OnPaste);

            base.OnDetaching();
        }

        /// <summary>
        /// Rejects keyboard input that would make the text an invalid decimal value.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="ev">The text composition event data.</param>
        private void OnPreviewTextInput(
            object sender,
            TextCompositionEventArgs ev
        )
        {
            var proposedText = GetPropsedText(ev.Text);

            // Suppress the input when the resulting text would be invalid.
            ev.Handled = !IsValidDecimal(proposedText);
        }

        /// <summary>
        /// Cancels a paste operation when the pasted content is not text
        /// or would make the text an invalid decimal value.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="ev">The pasting event data.</param>
        private void OnPaste(
            object sender,
            DataObjectPastingEventArgs ev
        )
        {
            if (!ev.DataObject.GetDataPresent(DataFormats.Text))
            {
                // Only plain text can be pasted into the text box.
                ev.CancelCommand();
            }
            else
            {
                if (ev.DataObject.GetData(DataFormats.Text) is string text)
                {
                    var proposedText = GetPropsedText(text);

                    if (!IsValidDecimal(proposedText))
                    {
                        ev.CancelCommand();
                    }
                }
            }
        }

        /// <summary>
        /// Re-applies the limit to the current text when <see cref="MaxDecimalPlaces"/> changes.
        /// </summary>
        /// <param name="d">The object whose property value changed.</param>
        /// <param name="ev">The property change event data.</param>
        private static void OnMaxDecimalPlacesChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs ev
        )
        {
            if (d is DecimalPlacesLimiter behavior && behavior.AssociatedObject != null)
            {
                behavior.CoerceCurrentText();
            }
        }

        /// <summary>
        /// Builds the text that would result from replacing the current selection
        /// with the specified text.
        /// </summary>
        /// <param name="newText">The text to be inserted.</param>
        /// <returns>The resulting text of the text box.</returns>
        private string GetPropsedText(
            string newText
        )
        {
            var currentText = AssociatedObject.Text ?? string.Empty;
            var selectionStart = AssociatedObject.SelectionStart;
            var selectionLength = AssociatedObject.SelectionLength;

            // Replace the selected range with the new text.
            return currentText.Remove(selectionStart, selectionLength).Insert(selectionStart, newText);
        }

        /// <summary>
        /// Determines whether the specified text is a decimal value within
        /// the allowed number of decimal places.
        /// </summary>
        /// <param name="text">The text to validate.</param>
        /// <returns><c>true</c> if the text is acceptable; otherwise, <c>false</c>.</returns>
        private bool IsValidDecimal(
            string text
        )
        {
            // Allow intermediate states such as an empty string or a lone minus sign.
            if (string.IsNullOrEmpty(text) || text == "-")
            {
                return true;
            }

            // Optional sign, digits and up to MaxDecimalPlaces digits after the decimal point.
            var pattern = $@"^-?\d*(\.\d{{0,{MaxDecimalPlaces}}})?$";

            return Regex.IsMatch(text, pattern);
        }

        /// <summary>
        /// Truncates the current text of the associated <see cref="TextBox"/>
        /// so that it satisfies the decimal places limit.
        /// </summary>
        private void CoerceCurrentText()
        {
            var currentText = AssociatedObject.Text;

            if (string.IsNullOrEmpty(currentText))
            {
                return;
            }

            if (!IsValidDecimal(currentText))
            {
                int dotIndex = currentText.IndexOf('.');
                if (dotIndex >= 0)
                {
                    var newText = currentText;
                    if (MaxDecimalPlaces == 0)
                    {
                        // Drop the decimal point and every digit that follows it.
                        newText = currentText[0..dotIndex];
                    }
                    else
                    {
                        // Keep only the allowed number of digits after the decimal point.
                        var length = dotIndex + 1 + MaxDecimalPlaces;
                        if (length <= currentText.Length)
                        {
                            newText = currentText[0..length];
                        }
                    }

                    AssociatedObject.Text = newText;
                }
            }
        }
    }
}
